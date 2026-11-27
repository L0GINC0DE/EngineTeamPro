using System.Collections.Generic;
using _01.Scripts.TAE.Agents;
using _01.Scripts.TAE.Interface;
using _01.Scripts.TAE.Players;
using UnityEngine;

namespace _01.Scripts.TAE.Combat
{
    public class DamageCaster : MonoBehaviour
    {
        [SerializeField] private ContactFilter2D targetFilter;
        [SerializeField] private float radius;
        [SerializeField] private int detectCount = 1;
        [Tooltip("플랫포머 플레이어에게 적중할 때 사용할 패링 설정")]
        [SerializeField] private DLJ_ParryProfile parryProfile = DLJ_ParryProfile.Default;

        private readonly List<Collider2D> targets = new();

        public void SetRange(float range) => radius = range;

        public bool CastDamage(int damage, float kbPower)
        {
            Vector2 origin = transform.position;
            Vector2 forward = transform.right;
            Physics2D.OverlapCircle(origin, radius, targetFilter, targets);

            int hitCount = 0;
            foreach (Collider2D target in targets)
            {
                if (hitCount >= detectCount)
                    break;

                Vector2 toTarget = (Vector2)target.bounds.center - origin;
                if (Vector2.Dot(forward, toTarget) <= 0f)
                    continue;

                if (!target.TryGetComponent(out IDamageable damageable))
                    continue;

                Vector2 knockbackDirection = (Vector2)target.transform.position - origin;
                Vector2 knockback = kbPower * knockbackDirection.normalized;
                if (damageable is PlatformPlayer player)
                    player.ApplyDamage(damage, knockback, parryProfile, GetComponentInParent<HealthModule>(), transform.position);
                else
                    damageable.ApplyDamage(damage, knockback);
                hitCount++;
            }

            return hitCount > 0;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position;
            Vector2 forward = ((Vector2)transform.right).normalized;
            Vector2 sideways = Vector2.Perpendicular(forward);
            Vector3 start = origin - (Vector3)(sideways * radius);
            Vector3 previous = start;

            Gizmos.color = Color.red;
            const int segments = 24;
            for (int segment = 1; segment <= segments; segment++)
            {
                float angle = Mathf.PI * segment / segments;
                Vector2 offset = radius * (forward * Mathf.Sin(angle) - sideways * Mathf.Cos(angle));
                Vector3 point = origin + (Vector3)offset;

                Gizmos.DrawLine(previous, point);
                previous = point;
            }

            Gizmos.DrawLine(previous, start);
            Gizmos.DrawLine(origin, origin + (Vector3)(forward * radius));
        }
    }
}
