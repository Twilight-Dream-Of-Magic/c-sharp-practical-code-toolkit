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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Twilight_Dream.EventSystem.Framework.Core;

namespace Twilight_Dream.EventSystem.Framework
{
	/// <summary>
	/// Event System Tool (Event Acceessor)
	/// </summary>
	public static class EventSystemTool
	{
		/// <summary>
		/// Event Dispatcher (Event Publisher)
		/// </summary>
		public static EventDispatcher dispatcher = new EventDispatcher();

		/// <summary>
		/// Add Event Listener from other class
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventHandler">This event processor</param>
		public static void AddEventListener(string eventName, EventListener.EventHandler eventHandler)
		{
			dispatcher.AddListener(eventName, eventHandler);
		}

		/// <summary>
		/// Remove Event Listener from other class
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventHandler">This event processor</param>
		public static void RemoveEventListener(string eventName, EventListener.EventHandler eventHandler)
		{
			dispatcher.RemoveListener(eventName, eventHandler);
		}

		/// <summary>
		/// Does it already have the event name?
		/// </summary>
		/// <param name="eventName">Event name</param>
		/// <returns></returns>
		public static bool HasExistListener(string eventName)
		{
			return dispatcher.HasListener(eventName);
		}

		/// <summary>
		/// Dispatch events
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventArgs">The event has the parameter data</param>
		public static void DispatchEvent(string eventName, params object[] eventArgs)
		{
			dispatcher.DispatchEvent(eventName, eventArgs);
		}

		/*
		/// <summary>
		/// An event that is posted from the sender, but notifies the event center that it needs to wait for the receiver to be ready before the event can be dispatched.
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventArgs">The event has the parameter data</param>
		public static async Task<Task> DispatchOneNeedWaitingEvent(string eventName, params object[] eventArgs)
		{
			Func<int> function = () =>
			{
				int statusCode = null;

				TaskCompletionSource<int> TCS = new TaskCompletionSource<int>();

				Task<int> taskFunction = dispatcher.DispatchOneNeedStartWaitingEvent(eventName, eventArgs);

				try
				{
					TCS.SetResult(taskFunction.GetAwaiter().GetResult());
					statusCode = TCS.Task.Result;
				}
				catch (Exception execption)
				{
					TCS.SetException(execption);
				}
				
				return statusCode;
			};


			Action action = () =>
			{
				var result = function.Invoke();

				if (result == null && result != 0)
				{
					throw new System.Exception("EventSystemTool.DispatchOneNeedStartWaitingEvent() No results for asynchronous tasks");
				}
			};

			await Task.Yield();
			var task = Task.Run(action);
			return task;
		}

		/// <summary>
		/// From the recipient, sends a notification to the Event Center that the event is now available for dispatch and ready to be received.
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventHandler">This event processor</param>
		/// <param name="isSingleUseMode">Do the methods of the event delegate need to be removed as soon as they are called?</param>
		public static async Task<Task> PleaseToDispatchEventAndStopWait(string eventName, EventListener.EventHandler eventHandler, bool isSingleUseMode = true)
		{
			Func<bool> function = () =>
			{
				bool statusFlag = false;

				TaskCompletionSource<bool> TCS = new TaskCompletionSource<bool>();

				Task<bool> taskFunction = dispatcher.PleaseToDispatchEventAndStopWait(eventName, eventHandler, isSingleUseMode);

				try
				{
					TCS.SetResult(taskFunction.GetAwaiter().GetResult());
					statusFlag = TCS.Task.Result;
				}
				catch (Exception execption)
				{
					TCS.SetException(execption);
				}

				return statusFlag;
			};

			Action action = () =>
			{
				var result = function.Invoke();

				if (result != true)
				{
					throw new System.Exception("EventSystemTool.PleaseToDispatchEventAndStopWait() No results for asynchronous tasks");
				}
			};

			await Task.Yield();
			var task = Task.Run(action);
			return task;
		}
		*/

		/// <summary>
		/// Remove all Event Listener
		/// </summary>
		public static void RemoveAllEventListener()
		{
			dispatcher.RemoveAllListener();
		}

	}
}
