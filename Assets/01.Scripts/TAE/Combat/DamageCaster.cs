using _01.Scripts.TAE.Interface;
using UnityEngine;
using UnityEngine.Events;

namespace _01.Scripts.TAE.Combat
{
    public class DamageCaster : MonoBehaviour
    {
        [SerializeField] private ContactFilter2D targetFilter;
        [SerializeField] private float radius;
        [SerializeField] private int detectCount = 1;

        private Collider2D[] _resultArray;

        private void Awake()
        {
            _resultArray = new Collider2D[detectCount];
        }

        public void SetRange(float range) => radius = range;

        public bool CastDamage(int damage, float kbPower)
        {
            int cnt = Physics2D.OverlapCircle(transform.position, radius, targetFilter, _resultArray);

            for (int i = 0; i < cnt; i++)
            {
                if(!_resultArray[i].TryGetComponent(out IDamageable damageable)) continue;

                Vector2 direction = _resultArray[i].transform.position - transform.position;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, direction.magnitude, targetFilter.layerMask);
                damageable.ApplyDamage(damage, kbPower * direction.normalized);
            }

            return cnt > 0;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}