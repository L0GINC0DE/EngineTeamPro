using UnityEngine;

namespace Systems
{
    public interface IPoolable
    {
        public PoolItemSO Item { get; }
        public GameObject GameObject { get; }

        public void ResetItem();
    }
}