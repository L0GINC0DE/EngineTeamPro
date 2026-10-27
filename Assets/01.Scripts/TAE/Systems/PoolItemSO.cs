using System;
using UnityEngine;

namespace Systems
{
    [CreateAssetMenu(fileName = "Pool Item", menuName = "SO/Pool/Item", order = 0)]
    public class PoolItemSO : ScriptableObject
    {
        public string ItemName;
        public GameObject Prefab;
        public int Count;

        private void OnValidate()
        {
            if (Prefab != null)
            {
                IPoolable item = Prefab.GetComponent<IPoolable>();
                if (item == null)
                {
                    Prefab = null;
                    Debug.LogWarning("Can not find IPoolable component");
                }
            }
        }
    }
}