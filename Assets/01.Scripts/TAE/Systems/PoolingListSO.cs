using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems
{
    // [Serializable]
    // public struct PoolingPair
    // {
    //     public GameObject Prefab;
    //     public int Count;
    // }
    
    [CreateAssetMenu(fileName = "Pooling list", menuName = "SO/Pool/list", order = 0)]
    public class PoolingListSO : ScriptableObject
    {
        public List<PoolItemSO> list;   
    }
}