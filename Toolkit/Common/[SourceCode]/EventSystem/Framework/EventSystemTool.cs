// You are currently viewing the code for the Object Pooling module
// Project Name: C# Practical code toolkit by Twilight-Dream
// Copyright 2050@Twilight-Dream. All rights reserved.
// Github: https://github.com/Twilight-Dream-Of-Magic
// Git Repository: https://github.com/Twilight-Dream-Of-Magic/c-sharp-practical-code-toolkit
// China Bilibili Video Space: https://space.bilibili.com/21974189
// China Bilibili Live Space: https://live.bilibili.com/1210760

using System;
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


		/// <summary>
		/// An event that is posted from the sender, but notifies the event center that it needs to wait for the receiver to be ready before the event can be dispatched.
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventArgs">The event has the parameter data</param>
		public static async Task DispatchOneNeedWaitingEvent(string eventName, params object[] eventArgs)
		{
			// 使用 TaskCompletionSource 来管理等待状态
			var tcs = new TaskCompletionSource<bool>();

			// 处理异步等待事件的流程
			try
			{
				// 异步请求派发事件，直到接收方准备好
				await Task.Run
				(
					() =>
					{
						// 调用原始的事件调度器方法，等待接收方的准备
						int StatusCode = dispatcher.DispatchOneNeedStartWaitingEvent(eventName, eventArgs);
						if (StatusCode == 1)
						{
							tcs.SetResult(false);
							tcs.SetException(new Exception("EventDispatcher > DispatchOneNeedStartWaitingEvent() | The eventName string must not be equal to a null reference or the eventName already exists!"));
						}
						tcs.SetResult(true); // 在接收方准备好后，通知等待线程继续执行
					}
				);

				// 等待异步操作完成
				await tcs.Task;
			}
			catch (Exception ex)
			{
				tcs.SetException(ex); // 如果发生异常，传递到 TaskCompletionSource
				throw new Exception("Error in DispatchOneNeedWaitingEvent", ex);
			}
		}

		/// <summary>
		/// From the recipient, sends a notification to the Event Center that the event is now available for dispatch and ready to be received.
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventHandler">This event processor</param>
		/// <param name="isSingleUseMode">Do the methods of the event delegate need to be removed as soon as they are called?</param>
		public static async Task PleaseToDispatchEventAndStopWait(string eventName, EventListener.EventHandler eventHandler, bool isSingleUseMode = true)
		{
			var tcs = new TaskCompletionSource<bool>();

			try
			{
				// 执行异步事件的处理逻辑
				await Task.Run
				(
					() =>
					{
						// 调用原始的事件调度器方法，通知事件中心准备分发事件
						var result = dispatcher.PleaseToDispatchEventAndStopWait(eventName, eventHandler, isSingleUseMode);

						switch (result)
						{
							case 3:
								tcs.SetResult(false);
								tcs.SetException(new Exception("Failed to dispatch event in PleaseToDispatchEventAndStopWait, EventDispatcher > PleaseToDispatchEventAndStopWait() The MyEventArgs._eventName string the eventName never is not exists!"));
								break;
							case 2:
								tcs.SetResult(false);
								tcs.SetException( new Exception("Failed to dispatch event in PleaseToDispatchEventAndStopWait, EventDispatcher > PleaseToDispatchEventAndStopWait() The MyEventArgs._eventName string must not be equal to a null reference!"));
								break;
							case 1:
								tcs.SetResult(false);
								tcs.SetException( new Exception("Failed to dispatch event in PleaseToDispatchEventAndStopWait, EventDispatcher > PleaseToDispatchEventAndStopWait() The MyEventArgs object must not be equal to a null reference!"));
								break;
							case 0:
								break;
							default:
								break;
						}

						tcs.SetResult(true);
					}
				);

				// 等待异步事件处理完成
				await tcs.Task;
			}
			catch (Exception ex)
			{
				tcs.SetException(ex); // 传递异常
				throw new Exception("Error in PleaseToDispatchEventAndStopWait", ex);
			}
		}

		/// <summary>
		/// Remove all MyEventArgs
		/// </summary>
		public static void RemoveAllMyEventArgs()
		{
			dispatcher.RemoveAllMyEventArgsAsync();
		}

		/// <summary>
		/// Remove all Event Listener
		/// </summary>
		public static void RemoveAllEventListener()
		{
			dispatcher.RemoveAllListener();
		}

	}
}