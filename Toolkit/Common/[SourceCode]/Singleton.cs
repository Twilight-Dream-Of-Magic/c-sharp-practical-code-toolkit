using System;

namespace Twilight_Dream
{
	public class Singleton<T> where T : new()
	{
		private static T _instance;
		private static readonly object SyncObject = new object();

		public static T Instance
		{
			get
			{
				if (_instance == null || _instance.Equals(default(T)))
				{
					lock (SyncObject)
					{
						if (_instance == null)
						{
							_instance = new T();
						}
					}
				}
				return _instance;
			}
		}
	}

	public class SingletonNoPublic<T> where T : class
	{
		private static T _instance;
		private static readonly object SyncObject = new object();

		public static T Instance
		{
			get
			{
				//没有第一重 singleton == null 的话，每一次有线程进入 GetInstance()时，均会执行锁定操作来实现线程同步，
				//非常耗费性能 增加第一重singleton ==null 成立时的情况下执行一次锁定以实现线程同步
				if (_instance == null)
				{
					lock (SyncObject)
					{
						if (_instance == null)//Double-Check Locking 双重检查锁定
						{
							//_instance = new T();
							//需要非公共的无参构造函数，不能使用new T() ,new不支持非公共的无参构造函数 
							//第二个参数防止异常：“没有为该对象定义无参数的构造函数。”
							_instance = (T)Activator.CreateInstance(typeof(T), true); 
						}
					}
				}
				return _instance;
			}
		}
		public static void SetInstance(T value)
		{
			_instance = value;
		}
	}
}
