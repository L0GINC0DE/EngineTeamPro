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

        [SerializeField] private SpriteRenderer spriteRenderer;

        public float attackRange;

        public LayerMask _whatIsEnemy;

        private int num;

        private void Awake()
        {
            InputCompo = GetComponent<PlayerInputReader>();
            MoveCompo = GetComponentInChildren<AgentMovement>();
            HealthCompo = GetComponent<HealthModule>();
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
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame) HealthCompo.TakeDamage(1);
            FlipController(InputCompo.MoveInput.x);
        }

        private void HandleJumpKeyPressed()
        {
            MoveCompo.Jump();
        }

        private void HandleInteractKeyPressed()
        {
            Debug.Log("상호작용");
        }

        private void HandleAttackKeyPressed()
        {
            Debug.Log("공격");
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
