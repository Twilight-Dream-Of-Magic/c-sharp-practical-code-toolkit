// You are currently viewing the code for the Event System module
// Project Name: C# Practical code toolkit by Twilight-Dream
// Copyright 2050@Twilight-Dream. All rights reserved.
// Github: https://github.com/Twilight-Dream-Of-Magic
// Git Repository: https://github.com/Twilight-Dream-Of-Magic/c-sharp-practical-code-toolkit
// China Bilibili Video Space: https://space.bilibili.com/21974189
// China Bilibili Live Space: https://live.bilibili.com/1210760

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
	}
}
