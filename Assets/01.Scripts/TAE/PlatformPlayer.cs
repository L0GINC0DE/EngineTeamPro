using _01.Scripts.TAE;
using _01.Scripts.TAE.Data;
using _01.Scripts.TAE.Test;
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

        public PlayerData data { get; } = new();
        [SerializeField] private UIData startingBody;
        [SerializeField] private UIData startingWeapon;
        private InteractTest currentWardrobe;
        private GameObject wardrobePanel;
        private bool IsChoosingWardrobe => wardrobePanel != null && wardrobePanel.activeInHierarchy;

        public bool CanUseWardrobe
        {
            get
            {
                if (!isActiveAndEnabled || !IsChoosingWardrobe) return false;
                if (currentWardrobe == null || !currentWardrobe.isActiveAndEnabled) return false;
                return currentWardrobe.Data != null && Interactor != null && Interactor.IsInRange(currentWardrobe);
            }
        }

        [SerializeField] private SpriteRenderer spriteRenderer;

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

            if (spriteRenderer == null)
                foreach (var renderer in GetComponentsInChildren<SpriteRenderer>())
                    if (renderer.GetComponent<Weapon>() == null)
                    {
                        spriteRenderer = renderer;
                        break;
                    }

            data.CurrentBodyData = startingBody;
            if (startingWeapon != startingBody) data.CurrentWeaponData = startingWeapon;
            SetUp();
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
            if (IsChoosingWardrobe)
            {
                MoveCompo.SetMoveDir(Vector2.zero);
                return;
            }

            MoveCompo.SetMoveDir(InputCompo.MoveInput);
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame) HealthCompo.TakeDamage(1);

            Physics2D.OverlapCircle(transform.position, attackRange, _whatIsEnemy);
            FlipController(InputCompo.MoveInput.x);
        }

        private void HandleJumpKeyPressed()
        {
            if (!IsChoosingWardrobe) MoveCompo.Jump();
        }

        private void HandleInteractKeyPressed()
        {
            if (!IsChoosingWardrobe) Interactor?.Interact(this);
        }

        private void HandleAttackKeyPressed()
        {
            if (!IsChoosingWardrobe) Debug.Log("공격");
        }

        internal bool BeginWardrobe(InteractTest wardrobe, GameObject panel)
        {
            currentWardrobe = wardrobe;
            wardrobePanel = panel;
            if (!CanUseWardrobe) return false;
            MoveCompo?.SetMoveDir(Vector2.zero);
            return true;
        }

        public bool ChangeBody()
        {
            if (!CanUseWardrobe || currentWardrobe.Data == data.CurrentWeaponData) return false;
            data.CurrentBodyData = currentWardrobe.Data;
            SetUp();
            return true;
        }

        public bool ChangeWeapon()
        {
            if (!CanUseWardrobe || currentWardrobe.Data == data.CurrentBodyData) return false;
            data.CurrentWeaponData = currentWardrobe.Data;
            SetUp();
            return true;
        }

        private void SetUp()
        {
            if (data.CurrentBodyData != null)
            {
                if (spriteRenderer != null) spriteRenderer.sprite = data.CurrentBodyData.icon;
                MoveCompo?.SetSpeed(data.CurrentBodyData.moveSpeed);
                defendPower = data.CurrentBodyData.defendPower;
            }
            if (data.CurrentWeaponData != null)
            {
                attackRange = data.CurrentWeaponData.attackRange;
                WeaponCompo?.SetWeapon(data.CurrentWeaponData);
            }
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
