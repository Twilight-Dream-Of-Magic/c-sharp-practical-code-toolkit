// You are currently viewing the code for the Event System module
// Project Name: C# Practical code toolkit by Twilight-Dream
// Copyright 2021@Twilight-Dream. All rights reserved.
// Gmail: yujiang1187791459@gmail.com
// Github: https://github.com/Twilight-Dream-Of-Magic
// Git Repository: https://github.com/Twilight-Dream-Of-Magic/c-sharp-practical-code-toolkit
// China Tencent QQ Mail: 1187791459@qq.com
// China Bilibili Video Space: https://space.bilibili.com/21974189
// China Bilibili Live Space: https://live.bilibili.com/1210760

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Twilight_Dream.EventSystem.Framework.Core;

namespace Twilight_Dream.EventSystem.Framework
{
	public class EventDispatcher
	{
		/// <summary>
		/// Event Mapping Table
		/// </summary>
		public Dictionary<string, EventListener> EventMap = new Dictionary<string, EventListener>();

		private EventWaitHandle @EventWaitHandle = new ManualResetEvent(false);
		private EventWaitHandle @EventWaitHandle2 = new ManualResetEvent(false);

		public Dictionary<string, EventListener> Async_EventMap = new Dictionary<string, EventListener>();
		private HashSet<string> Async_EventHashset = new HashSet<string>();
		private Queue<MyEventArgs> Async_MyEventArgs_Queque = new Queue<MyEventArgs>();
		private Queue<EventListener.EventHandler> Async_EventHandler_Queue = new Queue<EventListener.EventHandler>();

		/// <summary>
		/// Add Event Listener from other class
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventHandler">This event processor</param>
		public void AddListener(string eventName, EventListener.EventHandler eventHandler)
		{
			EventListener invoker;
			if (!EventMap.TryGetValue(eventName, out invoker))
			{
				invoker = new EventListener();
				EventMap.Add(eventName, invoker);
			}
			invoker.eventHandler += eventHandler;
		}

		/// <summary>
		/// Remove Event Listener from other class
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventHandler">This event processor</param>
		public void RemoveListener(string eventName, EventListener.EventHandler eventHandler)
		{
			EventListener invoker;
			if (EventMap.TryGetValue(eventName, out invoker))
			{
				invoker.eventHandler -= eventHandler;
				EventMap.Remove(eventName);
			}
		}

		/// <summary>
		/// Does it already have the event name?
		/// </summary>
		/// <param name="eventName">Event name</param>
		/// <returns></returns>
		public bool HasListener(string eventName)
		{
			return EventMap.ContainsKey(eventName);
		}

		/// <summary>
		/// Dispatch events
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventArgs">The event has the parameter data</param>
		public void DispatchEvent(string eventName, params object[] eventArgs)
		{
			EventListener invoker;
			if (EventMap.TryGetValue(eventName, out invoker))
			{
				MyEventArgs EA_object;
				if (eventArgs == null || eventArgs.Length == 0)
				{
					EA_object = new MyEventArgs(eventName);
				}
				else
				{
					EA_object = new MyEventArgs(eventName, eventArgs);
				}
				invoker.Invoke(EA_object);
			}
		}

		/// <summary>
		/// Remove all Event Listener
		/// </summary>
		public void RemoveAllListener()
		{
			foreach (EventListener value in EventMap.Values)
			{
				value.ClearAll();
			}
			EventMap.Clear();
		}

		/* By Async Callback Function*/

		/// <summary>
		/// An event that is posted from the sender, but notifies the event center that it needs to wait for the receiver to be ready before the event can be dispatched.
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventArgs">The event has the parameter data</param>
		public int DispatchOneNeedStartWaitingEvent(string eventName, params object[] eventArgs)
		{
			int StatusCode = 0;

			EventListener invoker = null;

			bool isExistEventName = Async_EventMap.TryGetValue(eventName, out invoker);

			if (isExistEventName == false && !Async_EventHashset.Contains(eventName))
			{
				MyEventArgs EA_object;

				if (eventArgs == null || eventArgs.Length == 0)
				{
					EA_object = new MyEventArgs(eventName);
				}
				else
				{
					EA_object = new MyEventArgs(eventName, eventArgs);
				}

				EventMap.Add(eventName, invoker);
				Async_MyEventArgs_Queque.Enqueue(EA_object);
				@EventWaitHandle.WaitOne();
				return StatusCode;
			}
			else
			{
				StatusCode = 1;
				if(StatusCode == 1) throw new Exception("EventDispatcher > DispatchOneNeedStartWaitingEvent() | The eventName string must not be equal to a null reference or the eventName already exists!");
				return StatusCode;
			}
		}


		/// <summary>
		/// An event that is posted from the sender, but notifies the event center that it needs to wait for the receiver to be ready before the event can be dispatched.
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventHandler">This event processor</param>
		/// <param name="isSingleUseMode">Do the methods of the event delegate need to be removed as soon as they are called?</param>
		public bool PleaseToDispatchEventAndStopWait(string eventName, EventListener.EventHandler eventHandler, bool isSingleUseMode = true)
		{
			const bool StatusFlag = false;

			EventListener invoker = null;
			MyEventArgs EA_object = null;

			bool isExistEventName = Async_EventMap.TryGetValue(eventName, out invoker);
			EA_object = Async_MyEventArgs_Queque.Dequeue();
			Async_MyEventArgs_Queque.Enqueue(EA_object);

			@EventWaitHandle.Set();

			if (isExistEventName == true && Async_EventHashset.Contains(eventName))
			{
				if (EA_object != null)
				{
					if (EA_object._eventName != null)
					{
						if (eventName != EA_object._eventName)
						{
							eventName = EA_object._eventName;

							if (!HasListenerByAsync(eventName) && StatusFlag == false)
							{
								throw new Exception("EventDispatcher > PleaseToDispatchEventAndStopWait() The MyEventArgs._eventName string the eventName never is not exists!");
							}
						}
					}
					else if(EA_object._eventName == null && StatusFlag == false)
					{
						throw new Exception("EventDispatcher > PleaseToDispatchEventAndStopWait() The MyEventArgs._eventName string must not be equal to a null reference!");
					}
					
					invoker.Invoke(EA_object);
					return StatusFlag != true;
				}
				else
				{
					throw new Exception("EventDispatcher > PleaseToDispatchEventAndStopWait() The MyEventArgs object must not be equal to a null reference!");
				}
			}

			if (isExistEventName == true && !Async_EventHashset.Contains(eventName))
			{
				Async_EventHashset.Add(eventName);
			}
			invoker.eventHandler += eventHandler;

			invoker.Invoke(EA_object);
			
			if (isSingleUseMode == true)
			{
				invoker.eventHandler -= eventHandler;
				if(isExistEventName == true)
				{
					Async_EventMap.Remove(eventName);
					Async_EventHashset.Remove(eventName);
				}
			}
			if (isSingleUseMode == false)
			{
				invoker.eventHandler -= eventHandler;
			}
			return StatusFlag != true;
		}

		/// <summary>
		/// Does it already have the event name?
		/// </summary>
		/// <param name="eventName">Event name</param>
		/// <returns></returns>
		public bool HasListenerByAsync(string eventName)
		{
			return Async_EventMap.ContainsKey(eventName);
		}

		/// <summary>
		/// Remove all Event Listener
		/// </summary>
		public void RemoveAllListenerByAsync()
		{
			foreach (EventListener value in Async_EventMap.Values)
			{
				value.ClearAll();
			}
			Async_EventMap.Clear();
		}
	}
}
