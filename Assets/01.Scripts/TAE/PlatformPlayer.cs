using System;
using _01.Scripts.TAE.Agents;
using _01.Scripts.TAE.Combat;
using _01.Scripts.TAE.Data;
using _01.Scripts.TAE.Interface;
using _01.Scripts.TAE.UI;
using Agents;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.KTH
{
    public class PlatformPlayer : MonoBehaviour, IDamageable
    {
        public PlayerInputReader InputCompo { get; private set; }
        public AgentMovement MoveCompo { get; private set; }
        public AgentRenderer RendererCompo { get; private set; }
        public HealthModule HealthCompo { get; private set; }
        public WeaponHolder WeaponCompo { get; private set; }
        public Interactor InteractCompo { get; private set; }

        [SerializeField] private SpriteRenderer spriteRenderer;

        public float attackRange;


        private int num;

        private void Awake()
        {
            InputCompo = GetComponent<PlayerInputReader>();
            MoveCompo = GetComponentInChildren<AgentMovement>();
            RendererCompo = GetComponentInChildren<AgentRenderer>();
            HealthCompo = GetComponent<HealthModule>();
            WeaponCompo = GetComponentInChildren<WeaponHolder>();
            InteractCompo = GetComponentInChildren<Interactor>();
        }

        private void Start()
        {
            RendererCompo.Init();
            WeaponCompo.InitializeWeapons(this);
            InteractCompo.Initialize(this);
            UIManager.Instance.weaponSlotUI.Initialize(WeaponCompo);
            UIManager.Instance.bodySlotUI.Initialize(RendererCompo);
        }

        private void OnEnable()
        {
            InputCompo.OnJumpKeyPressed += HandleJumpKeyPressed;
        }

        private void OnDisable()
        {
            InputCompo.OnJumpKeyPressed -= HandleJumpKeyPressed;
        }

        private void Update()
        {
            MoveCompo.SetMoveDir(InputCompo.MoveInput);
            if (Keyboard.current.tKey.wasPressedThisFrame) HealthCompo.TakeDamage(1);
            RendererCompo.FlipController(InputCompo.MoveInput.x);
        }

        private void HandleJumpKeyPressed()
        {
            MoveCompo.Jump();
        }

        public void ApplyDamage(int dmg, Vector2 kbPower)
        {
            if (HealthCompo.IsDead) return;

            HealthCompo.TakeDamage(dmg);
            MoveCompo.KnockbackPower(kbPower);
        }

        public void ApplyStats(UIDataSO data)
        {
            MoveCompo.SetSpeed(data.moveSpeed);
            HealthCompo.SetMaxHealth(data.health);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
