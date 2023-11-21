// You are currently viewing the code for the Event System module
// Project Name: C# Practical code toolkit by Twilight-Dream
// Copyright 2050@Twilight-Dream. All rights reserved.
// Gmail: yujiang1187791459@gmail.com
// Github: https://github.com/Twilight-Dream-Of-Magic
// Git Repository: https://github.com/Twilight-Dream-Of-Magic/c-sharp-practical-code-toolkit
// China Tencent QQ Mail: 1187791459@qq.com
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
		public static EventWaitableDispatcher waitableDispatcher = new EventWaitableDispatcher();

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

		#region WaitableDispatcher

		/// <summary>
		/// Add Event Listener from other class
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventHandler">This event processor</param>
		public static void AddEventListenerByAsync(string eventName, EventListener.EventHandler eventHandler)
		{
			waitableDispatcher.AddListener(eventName, eventHandler);
		}

		/// <summary>
		/// Remove Event Listener from other class
		/// </summary>
		/// <param name="eventName">This event name</param>
		/// <param name="eventHandler">This event processor</param>
		public static void RemoveEventListenerByAsync(string eventName, EventListener.EventHandler eventHandler)
		{
			waitableDispatcher.RemoveListener(eventName, eventHandler);
		}

		/// <summary>
		/// Performs an asynchronous “wait–notify” dispatch in a three-party model:<br/>
		/// 1. Subscriber parties register interest via <see cref="AddEventListenerByAsync(string, EventListener.EventHandler)"/>.<br/>
		/// 2. The intermediary (sender) calls this method to enqueue a wait slot:<br/>
		///    - Optionally registers <paramref name="eventHandler"/> as a subscriber callback.<br/>
		///    - Creates an AutoResetEvent (“gate”) plus <c>MyEventArgs</c> and enqueues them.<br/>
		///    - Inside <c>Task.Run</c>, calls <c>gate.WaitOne()</c> to block until signaled,<br/>
		///      thereby freeing the original caller thread.<br/>
		/// 3. The actual actor (receiver) invokes <see cref="NotifyReadyForDispatchOne"/>,<br/>
		///    which dequeues the slot, invokes all subscriber callbacks, sets the gate,<br/>
		///    and thereby unblocks this Task to complete the dispatch.<br/>
		/// <br/>
		/// This ensures that subscribers never talk directly to the receiver;<br/>
		/// all coordination happens via the sender’s agreed protocol.<br/>
		/// </summary>
		/// <param name="eventName">
		/// The unique identifier of the event to wait on and later dispatch.
		/// </param>
		/// <param name="eventHandler">
		/// Optional. A single-use callback to register just before waiting.  
		/// Pass <c>null</c> if registration was done earlier or not needed.
		/// </param>
		/// <param name="eventArgs">
		/// Optional. Payload objects to include with the event.  
		/// If empty, a default <c>MyEventArgs(eventName)</c> is used.
		/// </param>
		/// <returns>
		/// A <see cref="Task"/> that completes only after the receiver calls
		/// <see cref="NotifyReadyForDispatchOne"/>, unblocking the sender and firing callbacks.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown if another sender is already waiting on the same <paramref name="eventName"/> (code 1).
		/// </exception>
		public static async Task DispatchOneWithWaiting(string eventName, EventListener.EventHandler eventHandler = null, params object[] eventArgs)
		{
			// 若提供了 handler，则先挂接
			if (eventHandler != null)
				waitableDispatcher.AddListener(eventName, eventHandler);

			await Task.Run
			(
				() =>
				{
					// 同步调用；SenderStartWaitingEvent 自己会阻塞到接收方放行
					var code = waitableDispatcher.SenderStartWaitingEvent(eventName, eventArgs);
					if (code != 0)
					{
						if (eventHandler != null)   // 异常时解绑 handler，避免遗留
							waitableDispatcher.RemoveListener(eventName, eventHandler);

						throw new InvalidOperationException($"Sender already waiting for '{eventName}'");
					}
				}
			);
		}

		/// <summary>
		/// Called by the receiver to signal completion and release a previously waiting sender.
		/// Process:
		/// 1. Look up the WaitSlot for <paramref name="eventName"/>; return code 3 if none exists.<br/>
		/// 2. Dequeue the next waiting slot (gate + args); return code 2 if the queue is empty.<br/>
		/// 3. If the dequeued args name mismatches <paramref name="eventName"/>,<br/>
		///    set the gate to avoid deadlock, dispose it, and return code 1.<br/>
		/// 4. Invoke all registered subscriber callbacks with the dequeued <c>MyEventArgs</c>.<br/>
		/// 5. Call <c>slot.Gate.Set()</c> to unblock the sender, then dispose the gate.<br/>
		/// 6. In single-use mode, remove the slot entirely when no more waiters or handlers remain.<br/>
		///    Otherwise, leave the slot intact for future use.<br/>
		/// 
		/// Any nonzero return code is wrapped by the caller into an <see cref="InvalidOperationException"/>.
		/// </summary>
		/// <param name="eventName">
		/// The unique identifier of the event being signaled as ready.
		/// </param>
		/// <param name="isSingleUseMode">
		/// If <c>true</c>, the slot is removed after this dispatch when empty;  
		/// if <c>false</c>, the slot and any remaining handlers persist for reuse.
		/// </param>
		/// <returns>
		/// A <see cref="Task"/> that completes after callbacks are invoked and the sender is released.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown for any nonzero status code:
		/// <list type="bullet">
		///   <item><description>1: Argument-name mismatch</description></item>
		///   <item><description>2: No waiting sender to release</description></item>
		///   <item><description>3: No slot (no sender ever waited)</description></item>
		/// </list>
		/// </exception>
		public static async Task NotifyReadyForDispatchOne
		(
			string eventName,
			bool isSingleUseMode = true
		)
		{
			await Task.Run
			(
				() =>
				{
					var code = waitableDispatcher.SenderPleaseStopWaitEvent(eventName, isSingleUseMode);
					switch (code)
					{
						case 0:
							break; // OK
						case 1:
							throw new InvalidOperationException($"EventName mismatch for '{eventName}'.");
						case 2:
							throw new InvalidOperationException($"No waiting argument for '{eventName}'.");
						case 3:
							throw new InvalidOperationException($"No sender waiting for '{eventName}'.");
						default:
							throw new InvalidOperationException($"Unknown status {code}.");
					}
				}
			);
		}

		#endregion

		#region Cleanup all

		/// <summary>
		/// Remove all Event Listener
		/// </summary>
		public static void RemoveAllEventListener()
		{
			dispatcher.RemoveAllListener();
			waitableDispatcher.RemoveAllListener();
			waitableDispatcher.RemoveAllMyEventArgs();
		}

		#endregion
	}
}
