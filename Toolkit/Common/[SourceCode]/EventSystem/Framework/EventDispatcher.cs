// You are currently viewing the code for the Object Pooling module
// Project Name: C# Practical code toolkit by Twilight-Dream
// Copyright 2050@Twilight-Dream. All rights reserved.
// Github: https://github.com/Twilight-Dream-Of-Magic
// Git Repository: https://github.com/Twilight-Dream-Of-Magic/c-sharp-practical-code-toolkit
// China Bilibili Video Space: https://space.bilibili.com/21974189
// China Bilibili Live Space: https://live.bilibili.com/1210760

using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Twilight_Dream.EventSystem.Framework.Core;

namespace Twilight_Dream.EventSystem.Framework
{
	public class EventDispatcher
	{
		/// <summary>
		/// Event Mapping Table
		/// </summary>
		public Dictionary<string, EventListener> EventMap = new Dictionary<string, EventListener>();

		private EventWaitHandle Async_EventWaitHandle = new ManualResetEvent(false);

		public Dictionary<string, EventListener> Async_EventMap = new Dictionary<string, EventListener>();
		private HashSet<string> Async_EventHashset = new HashSet<string>();
		private ConcurrentQueue<MyEventArgs> Async_MyEventArgs_Queque = new ConcurrentQueue<MyEventArgs>();
		private ConcurrentQueue<EventListener.EventHandler> Async_EventHandler_Queue = new ConcurrentQueue<EventListener.EventHandler>();

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
		/// <returns>int StatusCode: 1, 0</returns>
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
				Async_EventWaitHandle.WaitOne();
				return StatusCode;
			}
			else
			{
				StatusCode = 1;
				return StatusCode;
			}
		}


		/// <summary>
		/// An event that is posted from the sender, but notifies the event center that it needs to wait for the receiver to be ready before the event can be dispatched.
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventHandler">This event processor</param>
		/// <param name="isSingleUseMode">Do the methods of the event delegate need to be removed as soon as they are called?</param>
		/// <returns>int StatusCode: 3, 2, 1, 0</returns>
		public int PleaseToDispatchEventAndStopWait(string eventName, EventListener.EventHandler eventHandler, bool isSingleUseMode = true)
		{
			EventListener invoker = null;
			MyEventArgs EA_object = null;

			bool isExistEventName = Async_EventMap.TryGetValue(eventName, out invoker);

			if (Async_MyEventArgs_Queque.Count > 0)
			{
				Async_MyEventArgs_Queque.TryDequeue(out EA_object);
			}
			Async_MyEventArgs_Queque.Enqueue(EA_object);

			Async_EventWaitHandle.Set();

			if (isExistEventName == true && Async_EventHashset.Contains(eventName))
			{
				if (EA_object != null)
				{
					if (EA_object._eventName != null)
					{
						if (eventName != EA_object._eventName)
						{
							eventName = EA_object._eventName;

							if (!HasListenerByAsync(eventName))
							{
								return 3;
							}
						}
					}
					else if(EA_object._eventName == null)
					{
						return 2;
					}
					
					invoker.Invoke(EA_object);
					return 0;
				}
				else
				{
					return 1;
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
			return 0;
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
		/// Remove all MyEventArgs in the concurrent queue
		/// </summary>
		public void RemoveAllMyEventArgsAsync()
		{
			MyEventArgs EA_object = null;

			while (Async_MyEventArgs_Queque.Count > 0)
			{
				Async_MyEventArgs_Queque.TryDequeue(out EA_object);
				EA_object = null;
			}
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