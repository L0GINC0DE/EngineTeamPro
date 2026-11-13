using System.Collections;
using System.Collections.Generic;
using _01.Scripts.TAE.Interface;
using UnityEngine;
using UnityEngine.Events;

namespace _01.Scripts.TAE.Agents
{
    public class AttackHitBox : MonoBehaviour
    {
        public UnityEvent OnAttackEvent;
        
        [SerializeField] private Collider2D _hitCol;

        private int damage = 10;
        private float knockback = 5f;
        private float facingDir = 1f;

        private readonly HashSet<IDamageable> _hitList = new();

        private void Awake()
        {
            if (_hitCol == null)
                _hitCol = GetComponentInChildren<Collider2D>();

            _hitCol.enabled = false;
        }

        public void Attack(int damage, float knockback, float activeDuration = 0.1f)
        {
            this.damage = damage;
            this.knockback = knockback;
            _hitList.Clear();

            StopAllCoroutines();
            StartCoroutine(CoAttackRoutine(activeDuration));
        }

        private IEnumerator CoAttackRoutine(float duration)
        {
            _hitCol.enabled = true; // 1. 켬!
            yield return new WaitForSeconds(duration);
            _hitCol.enabled = false; // 2. 끔!
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out IDamageable target))
            {
                if (_hitList.Add(target))
                {
                    facingDir = transform.root.eulerAngles.y == 0 ? 1f : -1f;
                    Vector2 kb = new Vector2(facingDir * knockback, 0.5f);
                    target.ApplyDamage(damage, kb);
                    OnAttackEvent?.Invoke();
                }
            }
        }
    }
}