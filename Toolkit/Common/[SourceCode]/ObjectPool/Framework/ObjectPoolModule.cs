// You are currently viewing the code for the Object Pooling module
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
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

using Newtonsoft.Json;

using Twilight_Dream.Security;
using Twilight_Dream.Conversion.DataFormat;
using Twilight_Dream.Extensions.Function;

namespace Twilight_Dream.ObjectPool.Framework
{
	#if Old_Test_ObjectPool
	
	public interface IPoolable_Old
	{
		
	}
	
	public class DataObjectPoolManager : IDisposable
	{
		private static Dictionary<Type, DataObjectPool_ForTransitStation<IPoolable_Old>> dataObjectPoolList = null;

		public DataObjectPool_ForTransitStation<ThisObjectType> GetPoolInstance<ThisObjectType>() where ThisObjectType : class, IPoolable_Old
		{
			Type typeKey = typeof(ThisObjectType);
			DataObjectPool_ForTransitStation<IPoolable> Value = null;
			if (!dataObjectPoolList.ContainsKey(typeKey))
			{
				var poolStatus = CreatePool<ThisObjectType>();
				if(poolStatus)
				{
					Value = dataObjectPoolList[typeKey];
					Value.SpecialInstance = Value;
					return Value.SpecialInstance as DataObjectPool_ForTransitStation<ThisObjectType>;
				}
				else
				{
					return null;
				}
			}
			Value = dataObjectPoolList[typeKey];
			return Value.SpecialInstance as DataObjectPool_ForTransitStation<ThisObjectType>;
		}

		public DataObjectPoolManager()
		{
			dataObjectPoolList = new Dictionary<Type, DataObjectPool_ForTransitStation<IPoolable>>();
		}

		public class DataObjectPool_ForTransitStation<PooledObjectType> : IPoolable, IDisposable, ICloneable, IEqualityComparer<PooledObjectType> where PooledObjectType : class, IPoolable_Old
		{
			protected static readonly object SyncObject = new object();

			protected static Queue<PooledObjectType> pooledObjectQueque = new Queue<PooledObjectType>();
			protected static Dictionary<string, object> pooledObjectMultiMapping = new Dictionary<string, object>();

			protected static List<PooledObjectType> pooledObjectList_available = new List<PooledObjectType>();
			protected static List<PooledObjectType> pooledObjectList_inUse = new List<PooledObjectType>();

			private DataObjectPool_ForTransitStation<PooledObjectType> _instance = null;

			public DataObjectPool_ForTransitStation()
			{

			}

			public void Dispose()
			{

			}

			public DataObjectPool_ForTransitStation<PooledObjectType> SpecialInstance
			{
				get
				{
					if (_instance is null || _instance.Equals(default(DataObjectPool_ForTransitStation<PooledObjectType>)))
					{
						lock (SyncObject)
						{
							if (_instance is null)
							{
								_instance = new DataObjectPool_ForTransitStation<PooledObjectType>();
							}
						}
					}
					return _instance;
				}
				internal set
				{
					if(_instance is not null)
					{
						_instance = value;
					}
				}
			}

			internal bool isPoolEquals(Object A, Object B)
			{
				byte[] binary = ObjectExchangeBinary.ObjectSerializeToByteArray(A, ObjectExchangeBinary.ObjectExchangeBinaryWithDataFormatMode.JSON);
				byte[] binary2 = ObjectExchangeBinary.ObjectSerializeToByteArray(B, ObjectExchangeBinary.ObjectExchangeBinaryWithDataFormatMode.JSON);

				string Base64Code = Convert.ToBase64String(binary);
				string Base64Code2 = Convert.ToBase64String(binary2);

				return Base64Code == Base64Code2;
			}

			internal bool isPoolEquals<PoolType>(PoolType A, PoolType B) where PoolType : class, IPoolable
			{
				HashHelper hashHelper = new HashHelper();

				string json = JsonConvert.SerializeObject(A);
				string json2 = JsonConvert.SerializeObject(B);

				string HashCode = hashHelper.HashString(json, Encoding.UTF8);
				string HashCode2 = hashHelper.HashString(json2, Encoding.UTF8);

				return HashCode == HashCode2;
			}

			public object Clone()
			{
				return this.CloneObjectByReflection();
			}

			public bool Equals(PooledObjectType x, PooledObjectType y)
			{
				return this.GetHashCode(x) == this.GetHashCode(y);
			}

			public int GetHashCode(PooledObjectType obj)
			{
				HashHelper hashHelper = new HashHelper();
				byte[] binary = ObjectExchangeBinary.ObjectSerializeToByteArray(obj, ObjectExchangeBinary.ObjectExchangeBinaryWithDataFormatMode.JSON);
				string Base64Code = Convert.ToBase64String(binary);
				string HashCode = hashHelper.HashString(Base64Code, Encoding.UTF8);

				int HashCode_Integer = 0;
				bool flag = int.TryParse(HashCode, out HashCode_Integer);
				if(flag)
				{
					return HashCode_Integer;
				}
				else
				{
					return Convert.ToInt32(HashCode);
				}
			}

			internal virtual void SetObjectToMapping(string Name, object myObject)
			{
				pooledObjectMultiMapping.Add(Name, myObject);
			}

			internal virtual object GetObjectFromMapping(string Name)
			{
				object myObject = null;
				if (pooledObjectMultiMapping.TryGetValue(Name, out myObject))
				{
					return myObject;
				}
				return default(object);
			}

			internal virtual bool RemoveObjectFromMapping(string Name)
			{
				return pooledObjectMultiMapping.Remove(Name);
			}

			internal virtual bool ClearObjectFromMapping()
			{
				if (pooledObjectMultiMapping.Count > 0)
				{
					foreach (var kvp in pooledObjectMultiMapping)
					{
						RemoveObjectFromMapping(kvp.Key);
					}
					return true;
				}
				return false;
			}

			internal virtual void EnqueueData(PooledObjectType myObject)
			{
				pooledObjectQueque.Enqueue(myObject);
			}

			internal virtual bool ContainQueueData(PooledObjectType myObject)
			{
				return pooledObjectQueque.Contains(myObject);
			}

			internal virtual PooledObjectType DequeueData()
			{
				return pooledObjectQueque.Dequeue();
			}

			internal virtual void ClearQueueData()
			{
				pooledObjectQueque.Clear();
			}

			public static implicit operator DataObjectPool_ForTransitStation<IPoolable>(DataObjectPool_ForTransitStation<PooledObjectType> value)
			{
				return value as DataObjectPool_ForTransitStation<IPoolable>;
			}

			public static explicit operator DataObjectPool_ForTransitStation<PooledObjectType>(DataObjectPool_ForTransitStation<IPoolable> value)
			{
				Object thisObject = value as object;
				return (DataObjectPool_ForTransitStation<PooledObjectType>)thisObject;
			}
		}

		private ValueTuple<bool, bool> TryPushObject<ThisObjectType>(DataObjectPool_ForTransitStation<ThisObjectType> instancePool, ThisObjectType myObject) where ThisObjectType : class, IPoolable
		{
			Type type = typeof(ThisObjectType);

			if (dataObjectPoolList is not null)
			{
				if(dataObjectPoolList.Count <= 0)
				{
					this.CreatePool<ThisObjectType>();
				}

				if (myObject is null)
				{
					return new ValueTuple<bool, bool>(true, false);
				}

				DataObjectPool_ForTransitStation<ThisObjectType> dataObjectPool = null;
				var Value = dataObjectPool as DataObjectPool_ForTransitStation<IPoolable>;

				if (dataObjectPoolList.ContainsValue(instancePool))
				{
					if (dataObjectPoolList.TryGetValue(type, out Value))
					{
						if (instancePool.isPoolEquals(instancePool, Value))
						{
							instancePool.EnqueueData(myObject);
							return new ValueTuple<bool, bool>(true, true);
						}
					}
				}
			}
			return new ValueTuple<bool, bool>(false, false);
		}

		private ValueTuple<bool, ThisObjectType> TryPullObject<ThisObjectType>(DataObjectPool_ForTransitStation<ThisObjectType> instancePool) where ThisObjectType : class, IPoolable
		{
			Type type = typeof(ThisObjectType);

			if (dataObjectPoolList is not null)
			{
				if(dataObjectPoolList.Count > 0)
				{
					DataObjectPool_ForTransitStation<ThisObjectType> dataObjectPool = null;
					var Value = dataObjectPool as DataObjectPool_ForTransitStation<IPoolable>;

					if (dataObjectPoolList.ContainsValue(instancePool))
					{
						if (dataObjectPoolList.TryGetValue(type, out Value))
						{
							if (instancePool.isPoolEquals(instancePool, Value))
							{
								ThisObjectType myObject = instancePool.DequeueData() as ThisObjectType;
								if (myObject is not null)
								{
									return new ValueTuple<bool, ThisObjectType>(true, myObject);
								}
							}
						}
					}
				}
				else
				{
					this.DeleteOnePool(type, false);
				}
			}
			return new ValueTuple<bool, ThisObjectType>(false, default(ThisObjectType));
		}

		public ValueTuple<bool, bool> TrySetObject<ThisObjectType>(DataObjectPool_ForTransitStation<ThisObjectType> instancePool, string name, object myObject) where ThisObjectType : class, IPoolable
		{
			Type type = typeof(ThisObjectType);

			if (dataObjectPoolList is not null)
			{
				if (dataObjectPoolList.Count <= 0)
				{
					this.CreatePool<ThisObjectType>();
				}

				if (myObject is null)
				{
					return new ValueTuple<bool, bool>(true, false);
				}

				if (dataObjectPoolList.ContainsValue(instancePool))
				{
					DataObjectPool_ForTransitStation<ThisObjectType> dataObjectPool = null;
					var Value = (DataObjectPool_ForTransitStation<IPoolable>)dataObjectPool;

					if (dataObjectPoolList.TryGetValue(type, out Value))
					{
						if(instancePool.isPoolEquals(instancePool, Value))
						{
							instancePool.SetObjectToMapping(name, myObject);
							return new ValueTuple<bool, bool>(true, true);
						}
					}
				}
			}
			return new ValueTuple<bool, bool>(false, false);
		}

		public ValueTuple<bool, object> TryGetObject<ThisObjectType>(DataObjectPool_ForTransitStation<ThisObjectType> instancePool, string name) where ThisObjectType : class, IPoolable
		{
			Type type = typeof(ThisObjectType);

			if (dataObjectPoolList is not null)
			{
				if (dataObjectPoolList.Count > 0)
				{
					if(dataObjectPoolList.ContainsValue(instancePool))
					{
						DataObjectPool_ForTransitStation<ThisObjectType> dataObjectPool = null;
						var Value = (DataObjectPool_ForTransitStation<IPoolable>)dataObjectPool;

						if (dataObjectPoolList.TryGetValue(type, out Value))
						{
							if(instancePool.isPoolEquals(instancePool, Value))
							{
								object myObject = instancePool.GetObjectFromMapping(name);

								if (myObject is not null)
								{
									return new ValueTuple<bool, object>(true, myObject);
								}
							}
						}
					}
				}
				else
				{
					this.DeleteOnePool(type, false);
				}
			}
			return new ValueTuple<bool, object>(false, default(object));
		}

		public bool CreatePool<ThisObjectType>() where ThisObjectType : class, IPoolable
		{
			Type type = typeof(ThisObjectType);

			DataObjectPool_ForTransitStation<ThisObjectType> dataObjectPool = new DataObjectPool_ForTransitStation<ThisObjectType>();

			DataObjectPool_ForTransitStation<IPoolable> Value = (DataObjectPool_ForTransitStation<IPoolable>)dataObjectPool;

			if (dataObjectPoolList is not null && !dataObjectPoolList.ContainsKey(type))
			{
				dataObjectPoolList.Add(type, dataObjectPool);
				return true;
			}
			return false;
		}

		public bool ClearAllDataFromOnePool(Type type)
		{
			if (dataObjectPoolList is not null)
			{
				if (dataObjectPoolList.Count > 0 && dataObjectPoolList.ContainsKey(type))
				{
					var Value = dataObjectPoolList[type];
					Value.Dispose();
					return true;
				}
			}
			return false;
		}

		public bool DeleteOnePool(Type type, bool isClearDataOnly)
		{
			if (dataObjectPoolList != null)
			{
				if(isClearDataOnly)
				{
					this.ClearAllDataFromOnePool(type);
					return true;
				}
				else
				{
					this.ClearAllDataFromOnePool(type);
					dataObjectPoolList.Remove(type);
					return true;
				}
			}
			return false;
		}

		public void DeleteAllPool()
		{
			foreach (var kvp in dataObjectPoolList)
			{
				DeleteOnePool(kvp.Key, true);
			}
		}

		public void ThrowInObjectToPool<ThisObjectType>(DataObjectPool_ForTransitStation<ThisObjectType> instancePool, System.Type dataStructureType, ThisObjectType myObject) where ThisObjectType : class, IPoolable
		{
			Type type = typeof(ThisObjectType);

			if(instancePool is not null)
			{
				if (dataStructureType == typeof(Queue<ThisObjectType>))
				{
					ValueTuple<bool, bool> results = TryPushObject<ThisObjectType>(instancePool, myObject);
					if (results.Item1 != false)
					{
						if (results.Item2 == true)
						{
							return;
						}
						else
						{
							throw new ArgumentNullException(string.Format("Argument myObject:[{0}] must can not is null!", myObject));
						}
					}
					else
					{
						throw new Exception("Oops, Data Object Pool List Is Null!");
					}
				}
			}
			return;
		}

		public ThisObjectType TakeOutObjectFromPool<ThisObjectType>(DataObjectPool_ForTransitStation<ThisObjectType> instancePool, System.Type dataStructureType) where ThisObjectType : class, IPoolable
		{
			Type type = typeof(ThisObjectType);

			if (instancePool is not null)
			{
				if (dataStructureType == typeof(Queue<ThisObjectType>))
				{
					ValueTuple<bool, ThisObjectType> results = TryPullObject<ThisObjectType>(instancePool);

					if (results.Item1)
					{
						return results.Item2;
					}
				}
			}
			return default(ThisObjectType);
		}

		public void ThrowInObjectToPool<ThisObjectType>(DataObjectPool_ForTransitStation<ThisObjectType> instancePool, string name, object myObject) where ThisObjectType : class, IPoolable
		{
			Type type = typeof(ThisObjectType);

			ValueTuple<bool, bool> results = TrySetObject<ThisObjectType>(instancePool, name, myObject);
			if (results.Item1 is not false)
			{
				if (results.Item2 is true)
				{
					return;
				}
				else
				{
					throw new ArgumentNullException(string.Format("Argument myObject:[{0}] must can not is null!", myObject));
				}
			}
			else
			{
				throw new Exception("Oops, Data Object Pool List Is Null!");
			}
		}

		public object TakeOutObjectFromPool<ThisObjectType>(DataObjectPool_ForTransitStation<ThisObjectType> instancePool, string name) where ThisObjectType : class, IPoolable
		{
			Type type = typeof(ThisObjectType);

			ValueTuple<bool, object> results = TryGetObject<ThisObjectType>(instancePool, name);

			if (results.Item1)
			{
				return results.Item2;
			}
			return default(object);
		}

		public void Dispose()
		{
			dataObjectPoolList.Clear();
			dataObjectPoolList = null;
		}
	}
	#endif

	public enum AllocateObjectItemState
	{
		InUse,//使用中
		Recycled //已回收
	}

	public interface IPoolable
	{
		/// <summary>
		/// 这表示一个对象的分配状态, 表示它在对象池里面现在否可以回收？</br>
		/// This indicates the allocation status of an object, indicating whether it can be reclaimed now in the object pool?
		/// </summary>
		AllocateObjectItemState ObjectWithPoolItemState { get; set; }
	}

	public abstract class ABC_PoolableItem : IPoolable
	{
		protected static readonly object _SyncObjectLock = new object();

		private AllocateObjectItemState _objectPoolItemState;
		private DateTime _ObjectPoolItemCreateAt = DateTime.Now;

		public static object SyncObjectLock
		{
			get
			{
				return _SyncObjectLock;
			}
		}

		public AllocateObjectItemState ObjectWithPoolItemState
		{
			get
			{
				return _objectPoolItemState;
			}
			set
			{
				_objectPoolItemState = value;
			}
		}

		public DateTime PoolItemCreateAt
		{
			get
			{
				return _ObjectPoolItemCreateAt;
			}
		}
	}

	public enum AccessMode_PoolContainter
	{
		/// <summary>
		/// Use With Queue
		/// </summary>
		FIFO,
		/// <summary>
		/// Use With Stack
		/// </summary>
		LIFO,
		/// <summary>
		/// Use With Double List
		/// </summary>
		CIRCULAR,
		/// <summary>
		/// Use With Dictionary By KeyValuePair
		/// </summary>
		MAPPING,
	}

	[Serializable()]
	public class DataObjectPool<PooledObjectType> : IDisposable, ICloneable, IEqualityComparer<DataObjectPool<PooledObjectType>> where PooledObjectType : class, IPoolable
	{
		private AccessMode_PoolContainter _AccessMode_Pool = AccessMode_PoolContainter.FIFO;
		public AccessMode_PoolContainter AccessMode_Pool
		{
			get => _AccessMode_Pool;
			private set => _AccessMode_Pool = value;
		}

		internal interface IPoolContainer
		{
			/// <summary>
			/// 表示这个对象池里面有多少对象<br/>
			/// indicates how many objects are in this object pool
			/// </summary>
			int ObjectCount { get; }

			/// <summary>
			/// 表示对象池的容积当前能容纳多少个对象？<br/>
			/// Indicates how many objects the volume of the object pool can currently hold?
			/// </summary>
			int CurrentPoolCapacity { get; set; }
		}

		internal abstract class ABC_PoolContainer : IPoolContainer
		{
			public abstract int ObjectCount { get; }
			public abstract int CurrentPoolCapacity { get; set; }

			/// <summary>
			/// 从对象池中抓取一个对象
			/// </summary>
			/// <returns></returns>
			internal abstract PooledObjectType CaptureObject();

			/// <summary>
			/// 向对象池中投入一个对象
			/// </summary>
			/// <param name="item"></param>
			internal abstract void DropObject(PooledObjectType item);

			/// <summary>
			/// 清空对象池中的所有对象
			/// </summary>
			internal abstract void ClearAllObject();
		}

		internal abstract class ABC_PoolMappingContainer : IPoolContainer
		{
			public abstract int ObjectCount { get; }
			public abstract int CurrentPoolCapacity { get; set; }

			private protected Dictionary<int, string> mappingKeyPool = null;
			private protected Dictionary<string, Object> mappingValuePool = null;
			private protected HashHelper hashHelper = new HashHelper();

			/// <summary>
			/// 从键值映射值的对象池中抓取一个对象
			/// </summary>
			/// <returns></returns>
			internal abstract Object CaptureObject(int ID);

			/// <summary>
			/// 向键值映射值的对象池中投入一个对象
			/// </summary>
			/// <param name="ID"></param>
			/// <param name="name"></param>
			/// <param name="objectItem"></param>
			internal abstract void DropObject(int ID, string name, Object objectItem);

			/// <summary>
			/// 从键值映射值的对象池中删除一个对象
			/// </summary>
			/// <param name="ID"></param>
			/// <returns></returns>
			internal abstract bool RemoveOneObject(int ID);

			/// <summary>
			/// 清空键值映射值的对象池中的所有对象
			/// </summary>
			internal abstract void RemoveAllObject();
		}

		private protected class QueuePoolContainer : ABC_PoolContainer
		{
			private protected ConcurrentQueue<PooledObjectType> pool = null;
			private protected Func<PooledObjectType> objectFactory = null;
			private SpinLock poolLock;

			private int poolCapacity = 0;

			public override int ObjectCount => pool.Count;

			public override int CurrentPoolCapacity
			{ 
				get
				{
					return poolCapacity;
				}
				set
				{
					if(poolCapacity == 0 && value > 0)
					{
						if(poolCapacity >= ObjectCount)
						{
							if(value > ObjectCount)
							{
								poolCapacity = value;
							}
							else
							{
								poolCapacity = poolCapacity * 2;
							}
						}
					}
					else
					{
						poolCapacity = 256;
					}
				}
			}

			internal QueuePoolContainer(int capacity)
			{
				this.pool = new ConcurrentQueue<PooledObjectType>();
				this.CurrentPoolCapacity = capacity;
				this.poolLock = new SpinLock(false);
			}

			internal QueuePoolContainer(Func<PooledObjectType> factory, int capacity)
			{
				this.objectFactory = factory;
				this.pool = new ConcurrentQueue<PooledObjectType>();
				this.CurrentPoolCapacity = capacity;
				this.poolLock = new SpinLock(false);
			}

			internal override PooledObjectType CaptureObject()
			{
				PooledObjectType item = default(PooledObjectType);
				if (pool.IsEmpty || !pool.TryDequeue(out item))
				{
					if(objectFactory is not null)
					{
						item = objectFactory.Invoke();
					}
					else
					{
						return default(PooledObjectType);
					}
				}

				bool lockTaken = false;
				this.poolLock.Enter(ref lockTaken);
				if (lockTaken)
				{
					item.ObjectWithPoolItemState = AllocateObjectItemState.InUse;
					this.poolLock.Exit(false);
				}
				return item;
			}

			internal override void DropObject(PooledObjectType item)
			{
				if (pool is not null)
				{
					if (item.ObjectWithPoolItemState.Equals(AllocateObjectItemState.InUse) && pool.Count < CurrentPoolCapacity)
					{
						pool.Enqueue(item);
					}

					bool lockTaken = false;
					this.poolLock.Enter(ref lockTaken);
					if (lockTaken)
					{
						item.ObjectWithPoolItemState = AllocateObjectItemState.Recycled;
						this.poolLock.Exit(false);
					}
				}
			}

			internal override void ClearAllObject()
			{
				if (pool is not null)
				{
					PooledObjectType item = default(PooledObjectType);
					while (pool.Count > 0)
					{
						if (pool.TryDequeue(out item))
						{
							item = null;
						}
					}
				}
			}
		}

		private protected class StackPoolContainer : ABC_PoolContainer
		{
			private protected ConcurrentStack<PooledObjectType> pool = null;
			private protected Func<PooledObjectType> objectFactory = null;
			private int poolCapacity = 0;
			private SpinLock poolLock;

			public override int ObjectCount => pool.Count;

			public override int CurrentPoolCapacity
			{
				get
				{
					return poolCapacity;
				}
				set
				{
					if (poolCapacity == 0 && value > 0)
					{
						if (poolCapacity >= ObjectCount)
						{
							if (value > ObjectCount)
							{
								poolCapacity = value;
							}
							else
							{
								poolCapacity = poolCapacity * 2;
							}
						}
					}
					else
					{
						poolCapacity = 256;
					}
				}
			}

			internal StackPoolContainer(int capacity)
			{
				this.pool = new ConcurrentStack<PooledObjectType>();
				this.CurrentPoolCapacity = capacity;
				this.poolLock = new SpinLock(false);
			}

			internal StackPoolContainer(Func<PooledObjectType> factory, int capacity)
			{
				this.objectFactory = factory;
				this.pool = new ConcurrentStack<PooledObjectType>();
				this.CurrentPoolCapacity = capacity;
				this.poolLock = new SpinLock(false);
			}

			internal override PooledObjectType CaptureObject()
			{
				PooledObjectType item = default(PooledObjectType);
				if (pool.IsEmpty || !pool.TryPop(out item))
				{
					if (objectFactory is not null)
					{
						item = objectFactory.Invoke();
					}
					else
					{
						return default(PooledObjectType);
					}
				}

				bool lockTaken = false;
				this.poolLock.Enter(ref lockTaken);
				if (lockTaken)
				{
					item.ObjectWithPoolItemState = AllocateObjectItemState.InUse;
					this.poolLock.Exit(false);
				}
				return item;
			}

			internal override void DropObject(PooledObjectType item)
			{
				if (pool is not null)
				{
					if (item.ObjectWithPoolItemState.Equals(AllocateObjectItemState.InUse) && pool.Count < CurrentPoolCapacity)
					{
						pool.Push(item);
					}

					bool lockTaken = false;
					this.poolLock.Enter(ref lockTaken);
					if (lockTaken)
					{
						item.ObjectWithPoolItemState = AllocateObjectItemState.Recycled;
						this.poolLock.Exit(false);
					}
				}
			}

			internal override void ClearAllObject()
			{
				if (pool is not null)
				{
					PooledObjectType item = default(PooledObjectType);
					while (pool.Count > 0)
					{
						if (pool.TryPop(out item))
						{
							item = null;
						}
					}
				}
			}
		}

		private protected class ListPoolContainer : ABC_PoolContainer
		{
			private protected static List<PooledObjectType> availablePool = null;
			private protected List<PooledObjectType> inUsePool = null;
			private protected Func<PooledObjectType> objectFactory = null;
			private int poolCapacity = 0;
			private SpinLock poolLock;

			public override int ObjectCount => availablePool.Count;

			public override int CurrentPoolCapacity
			{
				get
				{
					poolCapacity = availablePool.Capacity;
					return poolCapacity;
				}
				set
				{
					if (value >= availablePool.Count)
					{
						poolCapacity = value;
						availablePool.Capacity = poolCapacity;
						inUsePool.Capacity = poolCapacity;
					}
				}
			}

			internal ListPoolContainer()
			{
				availablePool = new List<PooledObjectType>();
				this.inUsePool = new List<PooledObjectType>();
				this.poolLock = new SpinLock(false);
			}

			internal ListPoolContainer(int capacity)
			{
				availablePool = new List<PooledObjectType>();
				this.inUsePool = new List<PooledObjectType>();
				this.CurrentPoolCapacity = capacity;
				this.poolLock = new SpinLock(false);
			}

			internal ListPoolContainer(Func<PooledObjectType> factory)
			{
				this.objectFactory = factory;
				availablePool = new List<PooledObjectType>();
				this.inUsePool = new List<PooledObjectType>();
				this.poolLock = new SpinLock(false);
			}

			internal ListPoolContainer(Func<PooledObjectType> factory, int capacity)
			{
				this.objectFactory = factory;
				availablePool = new List<PooledObjectType>();
				this.inUsePool = new List<PooledObjectType>();
				this.CurrentPoolCapacity = capacity;
				this.poolLock = new SpinLock(false);
			}

			internal override PooledObjectType CaptureObject()
			{
				if (availablePool.Count != 0 && availablePool.Count < CurrentPoolCapacity)
				{
					PooledObjectType item = availablePool[0];

					bool lockTaken = false;
					this.poolLock.Enter(ref lockTaken);
					if (lockTaken)
					{
						item.ObjectWithPoolItemState = AllocateObjectItemState.InUse;
						inUsePool.Add(item);
						availablePool.RemoveAt(0);
						this.poolLock.Exit(false);
					}
					return item;
				}
				else
				{
					PooledObjectType objectItem = default(PooledObjectType);

					if (objectFactory is not null)
					{
						objectItem = objectFactory.Invoke();

						bool lockTaken = false;
						this.poolLock.Enter(ref lockTaken);
						if (lockTaken)
						{
							objectItem.ObjectWithPoolItemState = AllocateObjectItemState.InUse;
							inUsePool.Add(objectItem);
							this.poolLock.Exit(false);
						}
						return objectItem;
					}
					else
					{
						objectItem = Activator.CreateInstance<PooledObjectType>();

						bool lockTaken = false;
						this.poolLock.Enter(ref lockTaken);
						if (lockTaken)
						{
							objectItem.ObjectWithPoolItemState = AllocateObjectItemState.InUse;
							if(objectItem is not default(PooledObjectType))
							{
								inUsePool.Add(objectItem);
							}
							this.poolLock.Exit(false);
						}
						return objectItem;
					}
				}
			}

			internal override void DropObject(PooledObjectType item)
			{
				if (ObjectCount <= CurrentPoolCapacity)
				{
					if (availablePool.Contains(item) && !inUsePool.Contains(item))
					{
						return;
					}

					bool lockTaken = false;
					this.poolLock.Enter(ref lockTaken);
					if (lockTaken)
					{
						item.ObjectWithPoolItemState = AllocateObjectItemState.Recycled;
						availablePool.Add(item);
						inUsePool.Remove(item);
					}
				}
			}

			internal override void ClearAllObject()
			{
				foreach (var item in availablePool)
				{
					availablePool.Remove(item);
				}

				foreach (var item in inUsePool)
				{
					availablePool.Remove(item);
				}
			}
		}

		private protected class MappingPoolContainer : ABC_PoolMappingContainer
		{
			public override int ObjectCount => mappingValuePool.Count;

			public override int CurrentPoolCapacity
			{
				get
				{
					poolCapacity = mappingKeyPool.Count;
					return poolCapacity;
				}
				set
				{
					if (mappingKeyPool.Count < value)
					{
						poolCapacity = value;
					}
				}
			}

			private SpinLock poolLock;
			private int poolCapacity = 1024;

			internal MappingPoolContainer()
			{
				mappingKeyPool = new Dictionary<int, string>();
				mappingValuePool = new Dictionary<string, Object>();
				poolLock = new SpinLock(false);
			}

			internal override object CaptureObject(int ID)
			{
				object objectItem = default(object);
				if (CurrentPoolCapacity > ObjectCount)
				{
					bool lockTaken = false;
					this.poolLock.Enter(ref lockTaken);

					if(lockTaken)
					{
						if (mappingKeyPool.ContainsKey(ID))
						{
							string hashed_string = mappingKeyPool[ID];
							if (mappingValuePool.TryGetValue(hashed_string, out objectItem))
							{
								if (objectItem is not default(object))
								{
									this.poolLock.Exit();
									return objectItem;
								}
							}
						}
						this.poolLock.Exit();
					}
				}
				return objectItem;
			}

			internal override void DropObject(int ID, string name, object objectItem)
			{
				if (CurrentPoolCapacity > ObjectCount)
				{
					bool lockTaken = false;
					this.poolLock.Enter(ref lockTaken);

					if(lockTaken)
					{
						if (!mappingKeyPool.ContainsKey(ID))
						{
							string hashed_string = hashHelper.HashString(name, Encoding.UTF8);
							mappingKeyPool[ID] = hashed_string;
							if (!mappingValuePool.ContainsValue(objectItem))
							{
								if (!mappingValuePool.ContainsKey(hashed_string))
								{
									mappingValuePool.Add(hashed_string, objectItem);
									this.poolLock.Exit();
									return;
								}
							}
							else
							{
								object temporaryObjectItem = default(object);

								if (mappingValuePool.TryGetValue(hashed_string, out temporaryObjectItem))
								{
									if (temporaryObjectItem is not null)
									{
										if (temporaryObjectItem.Equals(objectItem) && Object.ReferenceEquals(temporaryObjectItem, objectItem))
										{
											this.poolLock.Exit();
											return;
										}
									}
								}
								mappingValuePool.Add(hashed_string, objectItem);
								this.poolLock.Exit();
								return;
							}
						}
						this.poolLock.Exit();
					}
				}
			}

			internal override bool RemoveOneObject(int ID)
			{
				string hashed_string = null;

				if (mappingKeyPool.TryGetValue(ID, out hashed_string))
				{
					if (hashed_string is not null)
					{
						if (mappingValuePool.ContainsKey(hashed_string))
						{
							return mappingValuePool.Remove(hashed_string);
						}
					}
				}
				return false;
			}

			internal override void RemoveAllObject()
			{
				mappingKeyPool.Clear();
				mappingValuePool.Clear();
			}
		}

		private protected QueuePoolContainer queuePoolContainer = null;
		private protected StackPoolContainer stackPoolContainer = null;
		private protected ListPoolContainer listPoolContainer = null;
		private protected MappingPoolContainer mappingPoolContainer = null;

#region QueuePoolContainer Function

		internal void QueuePool_ThrowIn(PooledObjectType pooledObjectItem)
		{
			if (queuePoolContainer is not null)
			{
				queuePoolContainer.DropObject(pooledObjectItem);
			}
		}

		internal PooledObjectType QueuePool_TakeOut()
		{
			if (queuePoolContainer is not null)
			{
				return queuePoolContainer.CaptureObject();
			}
			return default(PooledObjectType);
		}

		internal void QueuePool_ResetObjects()
		{
			if (queuePoolContainer is not null)
			{
				queuePoolContainer.ClearAllObject();
			}
		}

#endregion

#region StackPoolContainer Function

		internal void StackPool_ThrowIn(PooledObjectType pooledObjectItem)
		{
			if (stackPoolContainer is not null)
			{
				stackPoolContainer.DropObject(pooledObjectItem);
			}
		}

		internal PooledObjectType StackPool_TakeOut()
		{
			if (stackPoolContainer is not null)
			{
				return stackPoolContainer.CaptureObject();
			}
			return default(PooledObjectType);
		}

		internal void StackPool_ResetObjects()
		{
			if (stackPoolContainer is not null)
			{
				stackPoolContainer.ClearAllObject();
			}
		}

#endregion

#region ListPoolContainer Function

		internal void ListPool_ThrowIn(PooledObjectType pooledObjectItem)
		{
			if (listPoolContainer is not null)
			{
				listPoolContainer.DropObject(pooledObjectItem);
			}
		}

		internal PooledObjectType ListPool_TakeOut()
		{
			if (listPoolContainer is not null)
			{
				return listPoolContainer.CaptureObject();
			}
			return default(PooledObjectType);
		}

		internal void ListPool_ResetObjects()
		{
			if (listPoolContainer is not null)
			{
				listPoolContainer.ClearAllObject();
			}
		}

#endregion

#region MappingPoolContainer Function

		internal void MappingPool_ThrowIn(int ID, string name, Object objectItem)
		{
			if (mappingPoolContainer is not null)
			{
				mappingPoolContainer.DropObject(ID, name, objectItem);
			}
		}

		internal Object MappingPool_TakeOut(int ID)
		{
			if (mappingPoolContainer is not null)
			{
				return mappingPoolContainer.CaptureObject(ID);
			}
			return default(PooledObjectType);
		}

		internal void MappingPool_ReleaseOneObject(int ID)
		{
			if (mappingPoolContainer is not null)
			{
				mappingPoolContainer.RemoveOneObject(ID);
			}
		}

		internal void MappingPool_ResetObjects()
		{
			if (mappingPoolContainer is not null)
			{
				mappingPoolContainer.RemoveAllObject();
			}
		}

#endregion

		internal DataObjectPool(AccessMode_PoolContainter accessMode_Pool, Func<PooledObjectType> factory, int capacity)
		{
			if(accessMode_Pool != AccessMode_PoolContainter.MAPPING)
			{
				this.AccessMode_Pool = accessMode_Pool;
				if (accessMode_Pool == AccessMode_PoolContainter.FIFO)
				{
					if (capacity <= 0)
					{
						capacity = 256;
					}

					if (factory != null)
					{
						queuePoolContainer = new QueuePoolContainer(factory, capacity);
					}
					else
					{
						queuePoolContainer = new QueuePoolContainer(capacity);
					}
					
				}
				if(accessMode_Pool == AccessMode_PoolContainter.LIFO)
				{
					if(capacity <= 0)
					{
						capacity = 256;
					}
					
					if(factory != null)
					{
						stackPoolContainer = new StackPoolContainer(factory, capacity);
					}
					else
					{
						stackPoolContainer = new StackPoolContainer(capacity);
					}
				}
				if(accessMode_Pool == AccessMode_PoolContainter.CIRCULAR)
				{
					if(factory != null)
					{
						if (capacity == 0)
						{
							listPoolContainer = new ListPoolContainer(factory);
						}

						if (capacity < 0)
						{
							capacity = 256;
						}
						listPoolContainer = new ListPoolContainer(factory, capacity);
					}
					else
					{
						if (capacity == 0)
						{
							listPoolContainer = new ListPoolContainer();
						}

						if (capacity < 0)
						{
							capacity = 256;
						}
						listPoolContainer = new ListPoolContainer(capacity);
					}
				}
			}
			else
			{
				this.AccessMode_Pool = AccessMode_PoolContainter.FIFO;
				queuePoolContainer = new QueuePoolContainer(factory, capacity);
			}
		}

		internal void ClearAllDataWithPool(DataObjectPool<PooledObjectType> instancePool)
		{
			if (instancePool.AccessMode_Pool == AccessMode_PoolContainter.FIFO)
			{
				instancePool.QueuePool_ResetObjects();
			}
			if (instancePool.AccessMode_Pool == AccessMode_PoolContainter.LIFO)
			{
				instancePool.StackPool_ResetObjects();
			}
			if (instancePool.AccessMode_Pool == AccessMode_PoolContainter.CIRCULAR)
			{
				instancePool.ListPool_ResetObjects();
			}
			if (instancePool.AccessMode_Pool == AccessMode_PoolContainter.MAPPING)
			{
				instancePool.MappingPool_ResetObjects();
			}
		}

		internal void ResetPoolInstance(DataObjectPool<PooledObjectType> instancePool)
		{
			this.ClearAllDataWithPool(instancePool);

			queuePoolContainer = null;

			stackPoolContainer = null;

			listPoolContainer = null;

			mappingPoolContainer = null;

			this.Dispose();
		}

		internal DataObjectPool()
		{
			this.AccessMode_Pool = AccessMode_PoolContainter.MAPPING;
			mappingPoolContainer = new MappingPoolContainer();
		}

		public bool isObjectEquals(Object A, Object B)
		{
			byte[] binary = ObjectExchangeBinary.ObjectSerializeToByteArray(A, ObjectExchangeBinary.ObjectExchangeBinaryWithDataFormatMode.JSON);
			byte[] binary2 = ObjectExchangeBinary.ObjectSerializeToByteArray(B, ObjectExchangeBinary.ObjectExchangeBinaryWithDataFormatMode.JSON);

			string Base64Code = Convert.ToBase64String(binary);
			string Base64Code2 = Convert.ToBase64String(binary2);

			return Base64Code == Base64Code2;
		}

		public bool isPoolEquals<PoolType>(PoolType A, PoolType B) where PoolType : class, IPoolable
		{
			HashHelper hashHelper = new HashHelper();

			string json = JsonConvert.SerializeObject(A);
			string json2 = JsonConvert.SerializeObject(B);

			string HashCode = hashHelper.HashString(json, Encoding.UTF8);
			string HashCode2 = hashHelper.HashString(json2, Encoding.UTF8);

			return HashCode == HashCode2;
		}

		public object Clone()
		{
			var a = this.CloneObjectByReflection();
			var b = this.CloneObjectByNewtonSoftJSON();

			if (a is not null)
			{
				return a;
			}
			if (b is not null)
			{
				return b;
			}
			return null;
		}

		public bool Equals(DataObjectPool<PooledObjectType> x, DataObjectPool<PooledObjectType> y)
		{
			return this.GetHashCode(x) == this.GetHashCode(y);
		}

		public int GetHashCode(DataObjectPool<PooledObjectType> obj)
		{
			HashHelper hashHelper = new HashHelper();
			byte[] binary = ObjectExchangeBinary.ObjectSerializeToByteArray(obj, ObjectExchangeBinary.ObjectExchangeBinaryWithDataFormatMode.JSON);
			string Base64Code = Convert.ToBase64String(binary);
			string HashCode = hashHelper.HashString(Base64Code, Encoding.UTF8);

			int HashCode_Integer = 0;
			bool flag = int.TryParse(HashCode, out HashCode_Integer);
			if (flag)
			{
				return HashCode_Integer;
			}
			else
			{
				return Convert.ToInt32(HashCode);
			}
		}

		public void Dispose()
		{
			
		}
	}

	internal class DataObjectPoolBuffer<ObjectType> : IDisposable where ObjectType : class, IPoolable
	{
		protected static readonly object SyncObject = new object();
		private Dictionary<string, DataObjectPool<ObjectType>> dataObjectPools = new Dictionary<string, DataObjectPool<ObjectType>>();
		private static DataObjectPoolBuffer<ObjectType> _instance = null;

		internal static DataObjectPoolBuffer<ObjectType> SpecialInstance
		{
			get
			{
				if (_instance is null || _instance.Equals(default(DataObjectPoolBuffer<ObjectType>)))
				{
					lock (SyncObject)
					{
						if (_instance is null)
						{
							_instance = new DataObjectPoolBuffer<ObjectType>();
						}
					}
				}
				return _instance;
			}
			set
			{
				if (_instance is not null)
				{
					_instance = value;
				}
			}
		}

		internal Dictionary<string, DataObjectPool<ObjectType>> DataObjectPools
		{
			get
			{
				if(dataObjectPools != null)
				{
					return dataObjectPools;
				}
				return new Dictionary<string, DataObjectPool<ObjectType>>();
			}
		}

		public DataObjectPoolBuffer()
		{

		}

		public void Dispose()
		{
			
		}
	}

	public class DataObjectPoolManager: Singleton<DataObjectPoolManager>, IDisposable
	{
		public DataObjectPoolManager()
		{
			
		}

		public DataObjectPool<ObjectType> CreatePoolInstance<ObjectType>() where ObjectType : class, IPoolable
		{
			DataObjectPool<ObjectType> poolInstance = new DataObjectPool<ObjectType>();

			foreach (var kvp in DataObjectPoolBuffer<ObjectType>.SpecialInstance.DataObjectPools)
			{
				if(DataObjectPoolBuffer<ObjectType>.SpecialInstance.DataObjectPools.Count <= 0)
				{
					break;
				}

				var pool = kvp.Value;
				if(pool.Equals(pool, poolInstance))
				{
					poolInstance.ResetPoolInstance(poolInstance);
					poolInstance = null;
					return poolInstance;
				}
			}
			return poolInstance;
		}

		public DataObjectPool<ObjectType> CreatePoolInstance<ObjectType>(AccessMode_PoolContainter accessMode_Pool, Func<ObjectType> factory, int capacity) where ObjectType : class, IPoolable
		{
			DataObjectPool<ObjectType> poolInstance = new DataObjectPool<ObjectType>(accessMode_Pool, factory, capacity);

			foreach (var kvp in DataObjectPoolBuffer<ObjectType>.SpecialInstance.DataObjectPools)
			{
				if (DataObjectPoolBuffer<ObjectType>.SpecialInstance.DataObjectPools.Count <= 0)
				{
					break;
				}

				var pool = kvp.Value;
				if (pool.Equals(pool, poolInstance))
				{
					poolInstance.ResetPoolInstance(poolInstance);
					poolInstance = null;
					return poolInstance;
				}
			}
			return poolInstance;
		}

		public DataObjectPool<ObjectType> GetPoolInstance<ObjectType>(string PoolName) where ObjectType : class, IPoolable
		{
			DataObjectPool<ObjectType> _instancePool = null;
			if(DataObjectPoolBuffer<ObjectType>.SpecialInstance.DataObjectPools.TryGetValue(PoolName, out _instancePool))
			{
				if (_instancePool != null)
				{
					return _instancePool;
				}
			}
			return default(DataObjectPool<ObjectType>);
		}

		public void SetPoolInstance<ObjectType>(DataObjectPool<ObjectType> instancePool, DataObjectPool<ObjectType> instancePoolNew) where ObjectType : class, IPoolable
		{
			if (instancePool != null && instancePoolNew != null)
			{
				if (instancePool.GetType() == instancePoolNew.GetType())
				{
					instancePool = instancePoolNew;
				}
			}
		}

		public bool IsExistPoolInstance<ObjectType>(string PoolName, DataObjectPool<ObjectType> instancePool = null) where ObjectType : class, IPoolable
		{
			if(instancePool != null)
			{
				if (DataObjectPoolBuffer<ObjectType>.SpecialInstance.DataObjectPools.ContainsValue(instancePool))
				{
					return true;
				}
				else
				{
					if (DataObjectPoolBuffer<ObjectType>.SpecialInstance.DataObjectPools.ContainsKey(PoolName))
					{
						var pool = DataObjectPoolBuffer<ObjectType>.SpecialInstance.DataObjectPools[PoolName];
						if (pool == null)
						{
							return false;
						}
						return instancePool.Equals(instancePool, pool);
					}
					return false;
				}
			}

			if (DataObjectPoolBuffer<ObjectType>.SpecialInstance.DataObjectPools.ContainsKey(PoolName))
			{
				var Value = DataObjectPoolBuffer<ObjectType>.SpecialInstance.DataObjectPools[PoolName];
				if(Value == null)
				{
					return false;
				}
				else
				{
					return true;
				}
			}

			return false;
		}

		public void AddPoolInstance<ObjectType>(string PoolName, DataObjectPool<ObjectType> instancePool) where ObjectType : class, IPoolable
		{
			if (instancePool != null)
			{
				if (!IsExistPoolInstance(PoolName, instancePool))
				{
					DataObjectPoolBuffer<ObjectType>.SpecialInstance.DataObjectPools.Add(PoolName, instancePool);
				}
			}
		}

		public void RemovePoolInstance<ObjectType>(string PoolName, DataObjectPool<ObjectType> instancePool) where ObjectType : class, IPoolable
		{
			if (instancePool != null)
			{
				instancePool.ResetPoolInstance(instancePool);

				if (IsExistPoolInstance(PoolName, instancePool))
				{
					DataObjectPoolBuffer<ObjectType>.SpecialInstance.DataObjectPools.Remove(PoolName);
				}
			}
		}

		public void ClearAllData<ThisObjectType>(DataObjectPool<ThisObjectType> instancePool) where ThisObjectType : class, IPoolable
		{
			if (instancePool != null)
			{
				instancePool.ClearAllDataWithPool(instancePool);
			}
		}

		public void ThrowInObjectToPool<ThisObjectType>(DataObjectPool<ThisObjectType> instancePool, ThisObjectType objectItem) where ThisObjectType : class, IPoolable
		{
			if(instancePool != null && objectItem != null)
			{
				if(instancePool.AccessMode_Pool == AccessMode_PoolContainter.FIFO)
				{
					instancePool.QueuePool_ThrowIn(objectItem);
				}
				if(instancePool.AccessMode_Pool == AccessMode_PoolContainter.LIFO)
				{
					instancePool.StackPool_ThrowIn(objectItem);
				}
				if(instancePool.AccessMode_Pool == AccessMode_PoolContainter.CIRCULAR)
				{
					instancePool.ListPool_ThrowIn(objectItem);
				}
			}
		}

		public ThisObjectType TakeOutObjectFromPool<ThisObjectType>(DataObjectPool<ThisObjectType> instancePool) where ThisObjectType : class, IPoolable
		{
			if (instancePool != null)
			{
				if (instancePool.AccessMode_Pool == AccessMode_PoolContainter.FIFO)
				{
					return instancePool.QueuePool_TakeOut();
				}
				if (instancePool.AccessMode_Pool == AccessMode_PoolContainter.LIFO)
				{
					return instancePool.StackPool_TakeOut();
				}
				if (instancePool.AccessMode_Pool == AccessMode_PoolContainter.CIRCULAR)
				{
					return instancePool.ListPool_TakeOut();
				}
			}
			return default(ThisObjectType);
		}

		public void ThrowInObjectToMappingPool<ThisObjectType>(DataObjectPool<ThisObjectType> instancePool, int ID, string name, Object objectItem) where ThisObjectType : class, IPoolable
		{
			if(instancePool != null)
			{
				if(instancePool.AccessMode_Pool != AccessMode_PoolContainter.MAPPING)
				{
					return;
				}
				else
				{
					instancePool.MappingPool_ThrowIn(ID, name, objectItem);
				}
			}
		}

		public Object TakeOutObjectFromMappingPool<ThisObjectType>(DataObjectPool<ThisObjectType> instancePool, int ID) where ThisObjectType : class, IPoolable
		{
			if (instancePool != null)
			{
				if (instancePool.AccessMode_Pool != AccessMode_PoolContainter.MAPPING)
				{
					return default(Object);
				}
				else
				{
					return instancePool.MappingPool_TakeOut(ID);
				}
			}
			return default(Object);
		}

		public void ReleaseOneFromMappingPool<ThisObjectType>(DataObjectPool<ThisObjectType> instancePool, int ID) where ThisObjectType : class, IPoolable
		{
			if (instancePool != null)
			{
				if (instancePool.AccessMode_Pool != AccessMode_PoolContainter.MAPPING)
				{
					return;
				}
				else
				{
					instancePool.MappingPool_ReleaseOneObject(ID);
				}
			}
		}

		public void Dispose()
		{
			
		}
	}

	namespace LiteVersion
	{
		public class ObjectPool<ObjectType>
		{
			private bool canResize;
			private int currentPoolSize;
			private SpinLock poolLock;
			private Dictionary<Type, Stack<ObjectType>> poolCache = null;
			private Func<ObjectType> factory = null;

			private enum Property_ObjectPool
			{
				NotExist = -1,
				NewPool = 0,
				ExistPool = 1,
			};

			public ObjectPool(int poolSize)
			{
				this.canResize = false;
				this.currentPoolSize = poolSize;
				this.poolLock = new SpinLock(false);
				this.poolCache = new Dictionary<Type, Stack<ObjectType>>();
			}

			public ObjectPool(int poolSize, bool resizeFlag)
			{
				this.canResize = resizeFlag;
				this.currentPoolSize = poolSize;
				this.poolLock = new SpinLock(false);
				this.poolCache = new Dictionary<Type, Stack<ObjectType>>();
			}

			public ObjectPool(int poolSize, Func<ObjectType> factory) : this(poolSize)
			{
				this.currentPoolSize = poolSize;
				this.canResize = false;
				this.factory = factory;
			}

			public ObjectPool(int poolSize, bool resizeFlag, Func<ObjectType> factory) : this(poolSize)
			{
				this.currentPoolSize = poolSize;
				this.canResize = resizeFlag;
				this.factory = factory;
			}

			private Stack<ObjectType> TryGetPool(Type type)
			{
				Stack<ObjectType> cachedCollection = null;

				if (isExistPool() == Property_ObjectPool.ExistPool || isExistPool() == Property_ObjectPool.NewPool)
				{
					return poolCache[type];
				}
				else
				{
					if (isExistPool() == Property_ObjectPool.NotExist)
					{
						cachedCollection = new Stack<ObjectType>();
						this.poolCache.Add(type, cachedCollection);
						return cachedCollection;
					}
					return default;
				}
			}

			public WithObjectType PullObject<WithObjectType>() where WithObjectType : ObjectType
			{
				return (WithObjectType)this.PullObject(typeof(WithObjectType));
			}

			public ObjectType PullObject(Type type)
			{
				bool lockTaken = false;
				Stack<ObjectType> cachedCollection = null;
				this.poolLock.Enter(ref lockTaken);

				try
				{
					cachedCollection = TryGetPool(type);
				}
				catch (Exception except)
				{
					throw except;
				}
				finally
				{
					if (lockTaken)
					{
						this.poolLock.Exit(false);
					}
				}

				if (cachedCollection != null && cachedCollection.Count > 0)
				{
					ObjectType instanceObject = cachedCollection.Pop();
					if (instanceObject != null)
					{
						this.poolCache[type] = cachedCollection;
						return instanceObject;
					}
					this.poolCache[type] = cachedCollection;
					return default(ObjectType);
				}

				// New instances don't need to be prepared for re-use, so we just return it.
				if (this.factory == null)
				{
					return (ObjectType)Activator.CreateInstance(type);
				}
				else
				{
					return this.factory();
				}
			}

			public void PullObject<WithObjectType>(WithObjectType instanceObject) where WithObjectType : ObjectType
			{
				this.PullObject(instanceObject);
			}

			public void PushObject(ObjectType instanceObject)
			{
				Stack<ObjectType> cachedCollection = null;
				Type type = typeof(ObjectType);

				bool lockTaken = false;
				this.poolLock.Enter(ref lockTaken);
				try
				{
					cachedCollection = TryGetPool(type);

					if (cachedCollection.Count >= this.currentPoolSize)
					{
						this.Resize(currentPoolSize * 2, true);
					}

					if (cachedCollection != null && instanceObject != null)
					{
						cachedCollection.Push(instanceObject);
					}
				}
				catch (Exception except)
				{
					throw except;
				}
				finally
				{
					if (lockTaken)
					{
						this.poolLock.Exit(false);
					}
				}
			}

			private Property_ObjectPool isExistPool()
			{
				Stack<ObjectType> cachedCollection = null;
				Type type = typeof(ObjectType);

				if (poolCache != null && poolCache.Count > 0)
				{
					if (poolCache.TryGetValue(type, out cachedCollection))
					{
						if (cachedCollection.Count > 0)
						{
							return Property_ObjectPool.ExistPool;
						}
						return Property_ObjectPool.NewPool;
					}
				}
				return Property_ObjectPool.NotExist;
			}

			public void Resize(int newPoolSize, bool mode = true)
			{
				if (canResize && isExistPool() != Property_ObjectPool.NotExist)
				{
					//Expansion
					if (newPoolSize > currentPoolSize && mode)
					{
						if (newPoolSize >= int.MaxValue / 4)
						{
							return;
						}
						else
						{
							currentPoolSize = newPoolSize;
						}
					}

					//Shrink
					if (newPoolSize < currentPoolSize && !mode)
					{
						if (!(newPoolSize <= 0))
						{
							Stack<ObjectType> cachedCollection = null;
							Type type = typeof(ObjectType);

							bool lockTaken = false;
							this.poolLock.Enter(ref lockTaken);
							try
							{
								int loopCount = cachedCollection.Count - newPoolSize;
								while (loopCount >= 0)
								{
									if (newPoolSize < loopCount)
									{
										ObjectType instance = cachedCollection.Pop();
										instance = default(ObjectType);
										--loopCount;
									}
								}
								currentPoolSize = cachedCollection.Count;
							}
							catch (Exception except)
							{
								throw except;
							}
							finally
							{
								if (lockTaken)
								{
									this.poolLock.Exit(false);
								}
							}
						}
					}
				}
			}

			public void ReleasePool(Type type)
			{
				if (poolCache.Count > 0)
				{
					Stack<ObjectType> cachedCollection = TryGetPool(type);

					if (cachedCollection.Count > 0)
					{
						cachedCollection.Clear();
						poolCache[type] = cachedCollection;
						poolCache.Remove(type);
					}
					else
					{
						poolCache.Remove(type);
					}
				}
				return;
			}
		}
	}
}
