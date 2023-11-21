// You are currently viewing the code for the Object Pooling module
// Project Name: C# Practical code toolkit by Twilight-Dream
// Copyright 2050@Twilight-Dream. All rights reserved.
// Github: https://github.com/Twilight-Dream-Of-Magic
// Git Repository: https://github.com/Twilight-Dream-Of-Magic/c-sharp-practical-code-toolkit
// China Bilibili Video Space: https://space.bilibili.com/21974189
// China Bilibili Live Space: https://live.bilibili.com/1210760

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twilight_Dream.ObjectPool.Framework2
{
    /// <summary>
    /// Represents a collection of pools for one or more object types.
    /// </summary>
    public class MultiTypePool
    {
        private readonly List<IPoolContainer> _pools;

        public MultiTypePool()
        {
            _pools = new List<IPoolContainer>();
        }

        public void DefineType<ObjectType>(int initialCapacity, SignalTypePool<ObjectType>.Create createHandler, SignalTypePool<ObjectType>.Reset resetHandler)
        {
            SignalTypePool<ObjectType> poolObject = new SignalTypePool<ObjectType>(initialCapacity, createHandler, resetHandler);
            _pools.Add(poolObject);
        }

        public ObjectType PullObject<ObjectType>()
        {
            var poolBase = GetPool(typeof(ObjectType));
            if (poolBase == null)
                throw new Exception(string.Format("Pooler.Get<{0}>() failed; there is no pool for that type.", typeof(ObjectType)));

            return ((SignalTypePool<ObjectType>)poolBase).PullObject();
        }

        public void PushObject(object item)
        {
            var poolBase = GetPool(item.GetType());
            if (poolBase == null)
                throw new Exception(string.Format("Pooler.Get<{0}>() failed; there is no pool for that type.", item.GetType()));

            poolBase.PushObject(item);
        }

        private IPoolContainer GetPool(Type itemType)
        {
            foreach (var poolBase in _pools)
            {
                if (poolBase.ItemType == itemType)
                {
                    return poolBase;
                }
            }

            return null;
        }
    }
}
