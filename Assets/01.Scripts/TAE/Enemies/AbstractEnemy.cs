using System;
using _01.Scripts.KTH;
using _01.Scripts.TAE.Agents;
using _01.Scripts.TAE.Enemies.FSM;
using _01.Scripts.TAE.Interface;
using _01.Scripts.TAE.UI;
using Agents;
using Systems;
using UnityEngine;

namespace _01.Scripts.TAE.Enemies
{
    public abstract class AbstractEnemy : MonoBehaviour, IDamageable, IPoolable
    {
        [SerializeField] private PoolItemSO poolItem;
        
        public float detectRange;
        public float attackRange;
        
        public float lastAttackTime;
        public float attackCoolTime;
        
        [SerializeField] private LayerMask _whatIsPlayer;
        
        public AgentMovement MoveCompo { get; private set; }
        public AgentRenderer Renderer { get; private set; }        
        public HealthModule HealthCompo { get; private set; }
        public Animator AnimCompo { get; private set; }
        public EnemyAnimation EnemyAnimCompo { get; private set; }
        
        public Transform targetTrm;

        protected virtual void Awake()
        {
            MoveCompo = GetComponentInChildren<AgentMovement>();
            HealthCompo = GetComponent<HealthModule>();
            AnimCompo = GetComponentInChildren<Animator>();
            EnemyAnimCompo = GetComponentInChildren<EnemyAnimation>();
            Renderer = GetComponentInChildren<AgentRenderer>();

            HealthCompo.OnDead += () => PoolManager.Instance.Push(this);
        }

        public Collider2D DetectPlayer()
        {
            return Physics2D.OverlapCircle(transform.position, detectRange, _whatIsPlayer);
        }
        
        public void ApplyDamage(int dmg, Vector2 kbPower)
        {
            UIManager.Instance.ShowDamageTMP(transform, dmg);
            HealthCompo.TakeDamage(dmg);
            if (HealthCompo.IsDead) return;
            MoveCompo.KnockbackPower(kbPower);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRange);
        }

        public PoolItemSO Item => poolItem;
        public GameObject GameObject => gameObject;
        public void ResetItem()
        {
            HealthCompo?.ResetHealth();
        }
    }
}