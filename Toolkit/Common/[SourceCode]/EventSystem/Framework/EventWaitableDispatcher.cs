using System.Threading;
using System.Collections.Concurrent;
using Twilight_Dream.EventSystem.Framework.Core;

namespace Twilight_Dream.EventSystem.Framework
{
	/// <summary>
	/// 线程安全、可等待的事件分发器。
	/// 每个事件名对应一个 WaitSlot：内部自带等待锁、参数队列和回调聚合器。
	/// </summary>
	public class EventWaitableDispatcher
	{
		#region 内部模型
		private class WaitSlot
		{
			internal readonly ConcurrentQueue<(MyEventArgs Args, AutoResetEvent Gate)> WaiterDatas = new ConcurrentQueue<(MyEventArgs Args, AutoResetEvent Gate)>();
			internal readonly EventListener Invoker = new EventListener();               // 回调聚合器
		}
		private readonly ConcurrentDictionary<string, WaitSlot> _slots = new();
		#endregion

		#region 监听器管理（对外 API）
		public void AddListener(string eventName, EventListener.EventHandler handler)
		{
			var slot = _slots.GetOrAdd(eventName, _ => new WaitSlot());
			slot.Invoker.eventHandler += handler;
		}

		public void RemoveListener(string eventName, EventListener.EventHandler handler)
		{
			if (!_slots.TryGetValue(eventName, out var slot)) 
				return;
			slot.Invoker.eventHandler -= handler;
			// 若已无任何回调且无等待者，可回收整个 slot
			if (slot.Invoker.eventHandler == null && slot.WaiterDatas.IsEmpty)
				_slots.TryRemove(eventName, out _);
		}

		public bool HasListener(string eventName) => _slots.TryGetValue(eventName, out var slot) && slot.Invoker.eventHandler != null;
		
		/// <summary>
		/// The sender announces it will wait for the receiver to be ready, then blocks the thread until released.
		/// Return code: 0 = ok, 1 = another sender is already waiting for the same event name.
		/// </summary>
		public int SenderStartWaitingEvent(string eventName, params object[] args)
		{
			var slot = _slots.GetOrAdd(eventName, _ => new WaitSlot());
			
			// 如果想独占，保留下面两行；想支持多发送方，就注释掉
			// if (!slot.WaiterDatas.IsEmpty)
			//     return 1;

			// 每个发送方创建自己的 Gate
			var gate = new AutoResetEvent(false);
			var ea = (args == null || args.Length == 0) ? new MyEventArgs(eventName) : new MyEventArgs(eventName, args);

			slot.WaiterDatas.Enqueue((ea, gate));
			// Block until the receiver calls Set() 
			// 阻塞到接收方调用 Set()
			gate.WaitOne();
			gate.Dispose();
			return 0;
		}
		
		/// <summary>
		/// The receiver is ready, triggers the event, and then releases the waiting sender.
		/// Return code: 0 = ok, 1 = argument does not match event name, 2 = no waiting argument, 3 = no waiter
		/// </summary>
		public int SenderPleaseStopWaitEvent(string eventName, bool singleUse = true)
		{
			// If no slot exists for the event name, return 3 (no waiter).
			// 若无对应事件名的 slot，返回 3（无等待者）
			if (!_slots.TryGetValue(eventName, out var slot))
			{
				return 3;
			}

			// If there is no waiting argument, release the sender to avoid deadlock and return 2.
			// 若无等待参数，放行以避免死锁，返回 2
			if (!slot.WaiterDatas.TryDequeue(out var waiterData))
				return 2;

			// If the event name does not match, release the sender and return 1.
			// 若事件名不符，放行并返回 1
			if (waiterData.Args._eventName != eventName)
			{
				waiterData.Gate.Set();
				waiterData.Gate.Dispose();
				return 1;
			}

			// Invoke all registered callbacks.
			// 执行所有已注册回调
			slot.Invoker.Invoke(waiterData.Args);

			// Release the sender after task completion.
			// 任务完成后释放发送方
			waiterData.Gate.Set();
			waiterData.Gate.Dispose();

			if (singleUse && slot.WaiterDatas.IsEmpty && slot.Invoker.eventHandler == null)
			{
				// Dequeue and remove the slot if single use.
				// 一次性：出队并释放 slot
				_slots.TryRemove(eventName, out _);
			}
			else
			{
				// Not single use: consume the current waiter data, wait for next call.
				// 非一次性：消费当前waiter data，等待下一次调用
				slot.WaiterDatas.TryDequeue(out _);
			}
			return 0;
		}
		#endregion

		#region 清理辅助
		public void RemoveAllMyEventArgs()
		{
			foreach (var slot in _slots.Values)
			{
				while (slot.WaiterDatas.TryDequeue(out _)) { /* discard */ }
			}
		}

		public void RemoveAllListener()
		{
			foreach (var slot in _slots.Values)
			{
				slot.Invoker.ClearAll();
			}
		}
		#endregion
	}
}
