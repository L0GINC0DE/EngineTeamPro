using System;
using _01.Scripts.TAE;
using _01.Scripts.TAE.Data;
using Agents;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.KTH
{
    public class PlatformPlayer : MonoBehaviour
    {
        public PlayerInputReader InputCompo { get; private set; }
        public AgentMovement MoveCompo { get; private set; }
        public HealthModule HealthCompo { get; private set; }
        public Interactor Interactor { get; private set; }
        public PlayerAttack AttackCompo { get; private set; }
        public Weapon WeaponCompo { get; private set; }

        public float defendPower;

        public TestPlayerData data;

        private SpriteRenderer spriteRenderer;

        public float attackRange;

        public LayerMask _whatIsEnemy;

        private int num;

        private void Awake()
        {
            InputCompo = GetComponent<PlayerInputReader>();
            MoveCompo = GetComponentInChildren<AgentMovement>();
            HealthCompo = GetComponent<HealthModule>();
            Interactor = GetComponent<Interactor>();
            AttackCompo = GetComponentInChildren<PlayerAttack>();
            WeaponCompo = GetComponentInChildren<Weapon>();

            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void OnEnable()
        {
            InputCompo.OnJumpKeyPressed += HandleJumpKeyPressed;
            InputCompo.OnInteractKeyPressed += HandleInteractKeyPressed;
            InputCompo.OnAttackKeyPressed += HandleAttackKeyPressed;
        }
        
        private void OnDisable()
        {
            InputCompo.OnJumpKeyPressed -= HandleJumpKeyPressed;
            InputCompo.OnInteractKeyPressed -= HandleInteractKeyPressed;
            InputCompo.OnAttackKeyPressed -= HandleAttackKeyPressed;
        }

        private void Update()
        {
            MoveCompo.SetMoveDir(InputCompo.MoveInput);

            if (Keyboard.current.digit1Key.wasPressedThisFrame) num = 1;
            if (Keyboard.current.digit2Key.wasPressedThisFrame) num = 2;

            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                HealthCompo.TakeDamage(1);
            }

            Physics2D.OverlapCircle(transform.position, attackRange, _whatIsEnemy);
            
            FlipController(InputCompo.MoveInput.x);
        }

        private void HandleJumpKeyPressed() => MoveCompo.Jump();

        private void HandleInteractKeyPressed()
        {
            Debug.Log("상호작용");

            switch (num)
            {
                case 1:
                    Interactor.Interact(UITypes.Player);
                    break;
                case 2:
                    Interactor.Interact(UITypes.Weapon);
                    break;
            }
            
        }

        private void HandleAttackKeyPressed() => Debug.Log("공격");
        
        public void SetData(TestPlayerData data)
        {
            this.data = data;
            SetUp();
        }

        private void SetUp()
        {
            spriteRenderer.sprite = data.uiSprite;
            MoveCompo.SetSpeed(data.moveSpeed);
            defendPower = data.defendPower;
            attackRange = data.attackRange;
        }

        private void FlipController(float value)
        {
            if (value != 0)
                transform.eulerAngles = value > 0f ? Vector3.zero : new Vector3(0, 180, 0);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}