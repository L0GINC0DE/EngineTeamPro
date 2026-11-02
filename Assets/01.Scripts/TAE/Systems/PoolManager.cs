using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using UnityEngine;

namespace Systems
{
    
    public class PoolManager : MonoSingleton<PoolManager>
    {
        [SerializeField] private PoolingListSO poolingList;
        private Dictionary<string, Pool> _poolDict;

        protected override void Awake()
        {
            base.Awake();
            
            _poolDict = new Dictionary<string, Pool>();

            foreach (PoolItemSO pair in poolingList.list)
            {
                CreatePool(pair.ItemName, pair.Prefab, pair.Count);
            }
        }

        private void CreatePool(string itemName, GameObject prefab, int count)
        {
            IPoolable poolable = prefab.GetComponent<IPoolable>();
            Debug.Assert(poolable != null, $"GameObject must have IPoolable component : {prefab.gameObject}");
            
            Pool pool = new Pool(poolable, transform, count);
            _poolDict.Add(itemName, pool);
        }

        public IPoolable Pop(string itemName)
        {
            if (_poolDict.TryGetValue(itemName, out Pool pool))
            {
                IPoolable item = pool.Pop();
                item.ResetItem();
                return item;
            }

            return null;
        }

        public void Push(IPoolable item)
        {
            if (_poolDict.TryGetValue(item.Item.ItemName, out Pool pool))
            {
                pool.Push(item);
            }
        }
    }
}