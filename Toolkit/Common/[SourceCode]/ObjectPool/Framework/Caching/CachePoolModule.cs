// You are currently viewing the code for the Object Pooling module
// Project Name: C# Practical code toolkit by Twilight-Dream
// Copyright 2050@Twilight-Dream. All rights reserved.
// Github: https://github.com/Twilight-Dream-Of-Magic
// Git Repository: https://github.com/Twilight-Dream-Of-Magic/c-sharp-practical-code-toolkit
// China Bilibili Video Space: https://space.bilibili.com/21974189
// China Bilibili Live Space: https://live.bilibili.com/1210760

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Caching;

using Twilight_Dream.Security;

namespace Twilight_Dream.ObjectPool.Framework
{
	namespace Caching
	{
		public interface ICacheContainer
		{
			/// <summary>
			/// Get cache items<br/>
			/// When there are no cache items, use the value provided by the factory parameter
			/// </summary>
			/// <param name="HASHED_KEY"></param>
			/// <param name="factory"></param>
			/// <returns></returns>
			object GetObjectOrByFactory(string HASHED_KEY, Func<string, object> factory);

			/// <summary>
			/// Get cache items<br/>
			/// When there are no cache items, the default value is returned.<br/>
			/// by default MemoryCache
			/// </summary>
			/// <param name="HASHED_KEY"></param>
			/// <returns></returns>
			object GetObject(string HASHED_KEY);

			/// <summary>
			/// Set cache items<br/>
			/// by default MemoryCache
			/// </summary>
			/// <param name="HASHED_KEY">The only key</param>
			/// <param name="value">object item</param>
			/// <param name="slidingExpireTime">After how long this cache is not accessed, it will expire inaccessible.</param>
			/// <param name="absoluteExpireTime">After this specified time, let this object not exist.</param>
			void SetObject(string HASHED_KEY, object value, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null);

			/// <summary>
			/// Remove cache items<br/>
			/// By default MemoryCache
			/// </summary>
			/// <param name="HASHED_KEY"></param>
			void RemoveObject(string HASHED_KEY);

			/// <summary>
			/// Clear all cache item<br/>
			/// By default MemoryCache
			/// </summary>
			void Reset();
		}

		public abstract class MyCacheBase : ICacheContainer, IDisposable
		{
			protected readonly object SyncLockObject = new object();

			public MyCacheBase()
			{

			}

			public virtual object GetObjectOrByFactory(string hash_key, Func<string, object> factory)
			{
				var myCacheKey = hash_key;
				var myCacheSpace = this.GetObject(hash_key);

				if (myCacheSpace == null)
				{
					lock (this.SyncLockObject)
					{
						myCacheSpace = this.GetObject(hash_key);

						if (myCacheSpace != null)
						{
							return myCacheSpace;
						}

						myCacheSpace = factory(hash_key);
						if (myCacheSpace == null)
						{
							return null;
						}

						this.SetObject(myCacheKey, myCacheSpace);
					}
				}

				return myCacheSpace;
			}

			public abstract object GetObject(string HASHED_KEY);

			public abstract void SetObject(string HASHED_KEY, object value, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null);

			public abstract void RemoveObject(string HASHED_KEY);

			public abstract void Reset();

			public virtual void Dispose() { }
		}

		#region Core Class

		internal class MyCache : MyCacheBase
		{
			private MemoryCache _MemoryCache = null;

			public MemoryCache CurrentCache
			{
				get
				{
					if (_MemoryCache != null)
					{
						return _MemoryCache;
					}
					return null;
				}
				private set
				{
					lock (this.SyncLockObject)
					{
						if (!System.Object.Equals(_MemoryCache, value))
						{
							_MemoryCache = value;
						}
					}
				}
			}

			public MyCache() : base()
			{
				this._MemoryCache = new MemoryCache("MyCacheSpace");
			}

			public MyCache(string name) : base()
			{
				this._MemoryCache = new MemoryCache(name);
			}

			public override object GetObject(string HASHED_KEY)
			{
				return this._MemoryCache.Get(HASHED_KEY);
			}

			internal object GetObject(MemoryCache memoryCacheObject, string HASH_EDKEY)
			{
				if (memoryCacheObject != null)
				{
					this.CurrentCache = memoryCacheObject;
					return this.GetObject(HASH_EDKEY);
				}
				return null;
			}

			public override void SetObject(string HASHED_KEY, object value, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null)
			{
				if (value == null)
				{
					this.RemoveObject(HASHED_KEY);
					return;
				}

				var cachePolicy = new CacheItemPolicy();

				if (absoluteExpireTime == null)
				{
					cachePolicy.AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;
				}
				else if (absoluteExpireTime != null && absoluteExpireTime.Value > TimeSpan.Zero)
				{
					cachePolicy.AbsoluteExpiration = DateTimeOffset.Now.Add(absoluteExpireTime.Value);
				}
				else if (absoluteExpireTime != null && absoluteExpireTime.Value <= TimeSpan.Zero)
				{
					cachePolicy.AbsoluteExpiration = DateTimeOffset.Now.Add(TimeSpan.FromSeconds(120));
				}

				if (slidingExpireTime == null)
				{
					cachePolicy.SlidingExpiration = ObjectCache.NoSlidingExpiration;
				}
				else if (slidingExpireTime != null && slidingExpireTime.Value > TimeSpan.Zero)
				{
					cachePolicy.SlidingExpiration = slidingExpireTime.Value;
				}
				else if (slidingExpireTime != null && slidingExpireTime.Value <= TimeSpan.Zero)
				{
					cachePolicy.SlidingExpiration = TimeSpan.FromSeconds(120);
				}

				this._MemoryCache.Set(HASHED_KEY, value, cachePolicy);
			}

			internal void SetObject(MemoryCache memoryCacheObject, string HASHED_KEY, object value, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null)
			{
				if (memoryCacheObject != null)
				{
					this.CurrentCache = memoryCacheObject;
					this.SetObject(HASHED_KEY, value, slidingExpireTime, absoluteExpireTime);
				}
			}

			public override void RemoveObject(string HASHED_KEY)
			{
				this._MemoryCache.Remove(HASHED_KEY);
			}

			internal MyCache DoReset(MyCache myCacheObject)
			{
				if (myCacheObject != null)
				{
					MemoryCache memoryCacheObject = myCacheObject.CurrentCache;

					if (memoryCacheObject != null)
					{
						string name = memoryCacheObject.Name;
						memoryCacheObject.Dispose();
						memoryCacheObject = new MemoryCache(name);
						myCacheObject.CurrentCache = memoryCacheObject;
						return myCacheObject;
					}
				}
				return null;
			}

			public override void Reset()
			{
				this._MemoryCache.Dispose();
				this._MemoryCache = new MemoryCache("MyCacheSpace");
			}

			public override void Dispose()
			{
				this._MemoryCache.Dispose();
				base.Dispose();
			}
		}

		#endregion

		public class MyCacheManager : Singleton<MyCacheManager>
		{
			private MyCache _MyCache = null;
			private Dictionary<int, MyCache> _MyCaches = new Dictionary<int, MyCache>();

			public MyCacheManager()
			{

			}

			#region Instance Function

			private void AllocationCache(int ID, string CacheSpaceName)
			{
				if (!_MyCaches.ContainsKey(ID))
				{
					if (CacheSpaceName != null && CacheSpaceName.Length > 0)
					{
						_MyCache = new MyCache(CacheSpaceName);
						_MyCaches.Add(ID, _MyCache);
					}
				}
			}

			private object GetObjectByID(int ID, string HASHED_KEY)
			{
				if (_MyCaches == null || _MyCaches != null && _MyCaches.Count == 0)
				{
					return null;
				}

				if (_MyCaches.TryGetValue(ID, out _MyCache))
				{
					var MemoryCacheObject = _MyCache.CurrentCache;
					object MyObject = _MyCache.GetObject(MemoryCacheObject, HASHED_KEY);
					if (MyObject != null)
					{
						return MyObject;
					}
				}
				return null;
			}

			private void SetObjectByID(int ID, string HASHED_KEY, object value, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null)
			{
				if (_MyCaches.TryGetValue(ID, out _MyCache))
				{
					var MemoryCacheObject = _MyCache.CurrentCache;

					_MyCache.SetObject(MemoryCacheObject, HASHED_KEY, value, slidingExpireTime, absoluteExpireTime);
				}
			}

			private void ResetObjectFromMappingWithID(int ID)
			{
				if (_MyCaches == null || _MyCaches != null && _MyCaches.Count == 0)
				{
					return;
				}

				if (_MyCaches.TryGetValue(ID, out _MyCache))
				{
					_MyCache = _MyCache.DoReset(_MyCache);

					if (_MyCache != null)
					{
						_MyCaches[ID] = _MyCache;
					}
				}
			}

			private void ResetObjectFromMappingAll()
			{
				if (_MyCaches == null || _MyCaches != null && _MyCaches.Count == 0)
				{
					return;
				}

				foreach (var kv in _MyCaches)
				{
					if (_MyCaches.ContainsKey(kv.Key))
					{
						ResetObjectFromMappingWithID(kv.Key);
						continue;
					}
					break;
				}
			}

			private void DeallocationCacheFromMappingWithID(int ID)
			{
				if (_MyCaches == null || _MyCaches != null && _MyCaches.Count == 0)
				{
					return;
				}

				if (_MyCaches.TryGetValue(ID, out _MyCache))
				{
					var MemoryCacheObject = _MyCache.CurrentCache;

					if (MemoryCacheObject != null)
					{
						_MyCache.Dispose();
						_MyCaches.Remove(ID);
					}
				}
			}

			private void DeallocationCacheAll()
			{
				if (_MyCaches == null || _MyCaches != null && _MyCaches.Count == 0)
				{
					return;
				}

				foreach (var kv in _MyCaches)
				{
					if (_MyCaches.ContainsKey(kv.Key))
					{
						DeallocationCacheFromMappingWithID(kv.Key);
						continue;
					}
					break;
				}
			}

			#endregion

			#region Static API

			public static void Allocation(int ID, string CacheSpaceName)
			{
				MyCacheManager.Instance.AllocationCache(ID, CacheSpaceName);
			}

			public static void DeallocationWithID(int ID)
			{
				MyCacheManager.Instance.DeallocationCacheFromMappingWithID(ID);
			}

			public static void DeallocationAll()
			{
				MyCacheManager.Instance.DeallocationCacheAll();
			}

			public static object GetByID(int ID, string KEY)
			{
				HashHelper hashHelper = new HashHelper();
				string hashed_string = hashHelper.HashString(KEY, System.Text.Encoding.UTF8);
				return MyCacheManager.Instance.GetObjectByID(ID, hashed_string);
			}

			public static void SetByID(int ID, string KEY, object value, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null)
			{
				HashHelper hashHelper = new HashHelper();
				string hashed_string = hashHelper.HashString(KEY, System.Text.Encoding.UTF8);
				MyCacheManager.Instance.SetObjectByID(ID, hashed_string, value, slidingExpireTime, absoluteExpireTime);
			}

			public static void ResetWithID(int ID)
			{
				MyCacheManager.Instance.ResetObjectFromMappingWithID(ID);
			}

			public static void ResetAll()
			{
				MyCacheManager.Instance.ResetObjectFromMappingAll();
			}

			#endregion
		}
	}
}