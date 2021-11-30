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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twilight_Dream.ObjectPool.Framework2
{
	/// <summary>
	/// Allows code to operate on a Pool<T> without casting to an explicit generic type.
	/// </summary>
	public interface IPoolContainer
	{
		Type ItemType { get; }
		void PushObject(object item);
	}

	/// <summary>
	/// A pool of items of the same type.<br/>
	/// 
	/// Items are pull and then later pushed to the pool (generally for reference types) to avoid allocations and the resulting garbage generation.<br/>
	/// 
	/// Any pool must have a way to 'reset' returned items to a canonical state.<br/>
	/// This class delegates that work to the allocator (literally, with a delegate) who probably knows more about the type being pooled.<br/>
	/// </summary>    
	public class SignalTypePool<ObjectType> : IPoolContainer
	{
		public delegate ObjectType Create();
		public readonly Create HandleCreate;

		public delegate void Reset(ref ObjectType item);
		public readonly Reset HandleReset;

		private readonly List<ObjectType> _in;

#if !SHIPPING
		private readonly List<ObjectType> _out;
#endif

		public Type ItemType
		{
			get
			{
				return typeof(ObjectType);
			}
		}

		public SignalTypePool(int initialCapacity, Create createMethod, Reset resetMethod)
		{
			HandleCreate = createMethod;
			HandleReset = resetMethod;

			_in = new List<ObjectType>(initialCapacity);
			for (int count = 0; count < initialCapacity; count++)
			{
				_in.Add(HandleCreate());
			}

#if !SHIPPING
			_out = new List<ObjectType>();
#endif
		}

		public ObjectType PullObject()
		{
			if (_in.Count == 0)
			{
				_in.Add(HandleCreate());
			}

			var item = _in[_in.Count - 1];
			_in.Remove(item);

#if !SHIPPING
			_out.Add(item);
#endif

			return item;
		}

		public void PushObject(ObjectType item)
		{
			HandleReset(ref item);

#if !SHIPPING
			Debug.Assert(!_in.Contains(item), "Returning an Item we already have.");
			Debug.Assert(_out.Contains(item), "Returning an Item we never gave out.");
			_out.Remove(item);
#endif

			_in.Add(item);
		}

		public void PushObject(object item)
		{
			PushObject((ObjectType)item);
		}

#if !SHIPPING
		public void Validate()
		{
			Debug.Assert(_out.Count == 0, "An Item was not returned.");
		}
#endif
	}
}
