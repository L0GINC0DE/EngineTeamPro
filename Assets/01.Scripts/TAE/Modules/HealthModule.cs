using UnityEngine;

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Agents
{
    public class HealthModule : MonoBehaviour
    {
        public UnityEvent OnHitEvent;
        
        public delegate void HealthChanged(int prev, int next);

        public event Action OnDead;
        public event HealthChanged OnHealthChanged;

        [SerializeField] private int maxHealth = 50;
        [SerializeField] private int currentHealth;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public bool IsDead => currentHealth == 0;

        private void Awake()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            ResetHealth();
        }

        public void ResetHealth()
        {
            SetHealth(maxHealth);
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || IsDead) return;
            SetHealth(currentHealth - damage);
            OnHitEvent?.Invoke();
        }

        private void SetHealth(int value)
        {
            int previous = currentHealth;
            int next = Mathf.Clamp(value, 0, maxHealth);

            if (previous == next)
                return;

            currentHealth = next;
            OnHealthChanged?.Invoke(previous, next);

            if (previous > 0 && next == 0)
                OnDead?.Invoke();
        }
    }
}
