using System;
using UnityEngine;
using UnityEngine.Events;

namespace Agents
{
    public class HealthModule : MonoBehaviour
    {
        public UnityEvent OnHitEvent = new();
        
        public delegate void HealthChanged(int prev, int next);

        public event Action OnDead;
        public event HealthChanged OnHealthChanged;

        [SerializeField] private int maxHealth = 50;
        [SerializeField] private int currentHealth;
        [SerializeField] private KTH_HitEffectSO hitEffect; // 비워두면 피격 연출 없음
        private bool initialized;

        public int MaxHealth => maxHealth;
        public int CurrentHealth
        {
            get
            {
                EnsureInitialized();
                return currentHealth;
            }
        }
        public bool IsDead => CurrentHealth <= 0;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void ResetHealth()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            initialized = true;
            SetHealth(maxHealth);
        }

        private void EnsureInitialized()
        {
            if (!initialized) ResetHealth();
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead) return;
            SetHealth(currentHealth + Mathf.Min(amount, maxHealth - currentHealth));
        }

        // 방향을 모르면 오른쪽에서 맞은 걸로 처리
        public void TakeDamage(int damage)
        {
            if (TryGetComponent(out KTH_DefenseStats defense))
                damage = Mathf.RoundToInt(damage * defense.TakenCoefficient);
            TakeDamage(damage, transform.position + Vector3.right);
        }

        // hitFrom: 공격이 날아온 위치 (넉백 방향 계산용)
        public void TakeDamage(int damage, Vector3 hitFrom)
        {
            if (damage <= 0 || IsDead) return;

            int previous = currentHealth;
            SetHealth(currentHealth - damage, true);

            if (currentHealth != previous && hitEffect != null && KTH_DamageEffectManager.instance != null)
                KTH_DamageEffectManager.instance.PlayHit(hitEffect, transform, hitFrom);
        }

        private void SetHealth(int value, bool isHit = false)
        {
            int previous = currentHealth;
            int next = Mathf.Clamp(value, 0, maxHealth);

            if (previous == next)
                return;

            currentHealth = next;
            OnHealthChanged?.Invoke(previous, next);
            if (isHit) OnHitEvent?.Invoke();

            if (previous > 0 && next == 0)
                OnDead?.Invoke();
        }
    }
}
