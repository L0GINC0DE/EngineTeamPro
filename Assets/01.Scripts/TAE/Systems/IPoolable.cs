using UnityEngine;

namespace _01.Scripts.TAE.Systems
{
    public interface IPoolable
    {
        public PoolItemSO Item { get; }
        public GameObject GameObject { get; }

        public void ResetItem();
    }
}