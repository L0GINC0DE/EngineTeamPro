using _01.Scripts.TAE.Agents;
using _01.Scripts.TAE.Combat;
using _01.Scripts.TAE.Data;
using _01.Scripts.TAE.Interface;
using _01.Scripts.TAE.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.TAE.Players
{
    public class PlatformPlayer : MonoBehaviour, IDamageable
    {
        public PlayerInputReader InputCompo { get; private set; }
        public AgentMovement MoveCompo { get; private set; }
        public HealthModule HealthCompo { get; private set; }
        public WeaponHolder WeaponCompo { get; private set; }
        public Interactor InteractCompo { get; private set; }
        public PlayerAnimator AnimCompo { get; private set; }
        public GimmickController GimmickCompo { get; private set; }

        [SerializeField] private SpriteRenderer spriteRenderer;

        [field: SerializeField] public UIDataListSO BodyDataList { get; private set; }
        public UIDataSO CurrentBodyData { get; private set; }

        private void Awake()
        {
            InputCompo = GetComponent<PlayerInputReader>();
            MoveCompo = GetComponentInChildren<AgentMovement>();
            HealthCompo = GetComponent<HealthModule>();
            WeaponCompo = GetComponentInChildren<WeaponHolder>();
            InteractCompo = GetComponentInChildren<Interactor>();
            AnimCompo = GetComponentInChildren<PlayerAnimator>();
            GimmickCompo = GetComponentInChildren<GimmickController>();
        }

        private void Start()
        {
            WeaponCompo.InitializeWeapons(this);
            InteractCompo.Initialize(this);
            AnimCompo.Initialize(this);
            ApplyStats(BodyDataList.uiDataList[0]);
            UIManager.Instance.Initialize(this);
            BindMoveAndAnimator(true);
            GimmickCompo.Initialize(this);
        }

        private void OnEnable()
        {
            InputCompo.OnJumpKeyPressed += HandleJumpKeyPressed;
            InputCompo.OnExitKeyPressed += HandleExitKeyPressed;
        }

        private void OnDisable()
        {
            InputCompo.OnJumpKeyPressed -= HandleJumpKeyPressed;
            InputCompo.OnExitKeyPressed -= HandleExitKeyPressed;
            BindMoveAndAnimator(false);
        }

        private void Update()
        {
            if (!MoveCompo.CanManualMove)
            {
                MoveCompo.SetMoveDir(Vector2.zero);
                return;
            }

            Vector2 moveInput = InputCompo.MoveInput;
            MoveCompo.SetMoveDir(moveInput);
            FlipController(moveInput.x);

            if (Keyboard.current.tKey.wasPressedThisFrame)
                HealthCompo.TakeDamage(1);

            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                GimmickCompo.StartSelect();
            }
        }

        public void ApplyDamage(int dmg, Vector2 kbPower)
        {
            if (HealthCompo.IsDead) return;

            HealthCompo.TakeDamage(dmg);
            MoveCompo.KnockbackPower(kbPower);
        }

        public void ApplyStats(UIDataSO data)
        {
            if (CurrentBodyData != data)
            {
                CurrentBodyData = data;
                AnimCompo.ChangeAgent(CurrentBodyData);
                MoveCompo.SetSpeed(CurrentBodyData.moveSpeed);
                HealthCompo.SetMaxHealth(CurrentBodyData.health);
            }
        }

        private void FlipController(float value)
        {
            if (value != 0)
            {
                transform.root.eulerAngles = value > 0f ? Vector3.zero : new Vector3(0, 180, 0);
                spriteRenderer.sortingOrder = value > 0f ? 0 : 1;
            }
        }

        #region Animations

        private readonly int IsMoveHash = Animator.StringToHash("IsMove");
        private readonly int IsGround = Animator.StringToHash("IsGround");
        
        private void BindMoveAndAnimator(bool isBinding)
        {
            if (MoveCompo == null || AnimCompo == null) return;
            if (isBinding)
            {
                MoveCompo.IsGrounded.OnValueChanged += HandleGroundChange;
                MoveCompo.IsMove.OnValueChanged += HandleIsMoveChange;
            }
            else
            {
                MoveCompo.IsGrounded.OnValueChanged -= HandleGroundChange;
                MoveCompo.IsMove.OnValueChanged -= HandleIsMoveChange;
            }
        }
        
        private void HandleIsMoveChange(bool prev, bool next)
        {
            AnimCompo.SetBoolean(IsMoveHash, next);
        }

        private void HandleGroundChange(bool prev, bool next)
        {
            
        }

        #endregion
        
        private void HandleJumpKeyPressed() => MoveCompo.Jump();
        private void HandleExitKeyPressed() => UIManager.Instance.OnEscape();
        
    }
}
