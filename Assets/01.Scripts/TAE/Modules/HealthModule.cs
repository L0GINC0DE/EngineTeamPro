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

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || IsDead) return;
            SetHealth(currentHealth - damage, true);
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
