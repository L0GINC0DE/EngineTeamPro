using System.Collections;
using Systems;
using UnityEngine;

namespace _01.Scripts.KTH.Effects
{
    public class EffectPlayer : MonoBehaviour, IPoolable
    {
        [field: SerializeField] public PoolItemSO Item { get; private set; }
        public GameObject GameObject => gameObject;

        private ParticleSystem _particle;
        private float _duration;
        private WaitForSeconds _delaySec;

        private void Awake()
        {
            _particle = GetComponent<ParticleSystem>();
            _duration = _particle.main.duration;
            _delaySec = new WaitForSeconds(_duration);
        }

        public void SetPositionAndPlay(Vector3 position)
        {
            transform.position = position;
            _particle.Play();
            StartCoroutine(DelayAndGotoPool());
        }

        private IEnumerator DelayAndGotoPool()
        {
            yield return _delaySec;
            PoolManager.Instance.Push(this);
        }
        
        public void ResetItem()
        {
            _particle.Stop();
            _particle.Simulate(0);
        }
    }
}