using _01.Scripts.TAE.Agents;
using _01.Scripts.TAE.Combat;
using _01.Scripts.TAE.Data;
using _01.Scripts.TAE.Interface;
using _01.Scripts.TAE.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.TAE.Players
{
    [RequireComponent(typeof(DLJ_Parry))]
    public class PlatformPlayer : MonoBehaviour, IDamageable
    {
        public PlayerInputReader InputCompo { get; private set; }
        public AgentMovement MoveCompo { get; private set; }
        public HealthModule HealthCompo { get; private set; }
        public WeaponHolder WeaponCompo { get; private set; }
        public Interactor InteractCompo { get; private set; }
        public PlayerAnimator AnimCompo { get; private set; }
        public GimmickController GimmickCompo { get; private set; }

        private ShieldGimmick gimmickShield;
        private DLJ_Parry parry;
        public bool HasGimmickShield => gimmickShield != null;
        public float GimmickMoveMultiplier { get; private set; } = 1f;

        public GameObject shieldIcon;

        private SpriteRenderer _spriteRenderer;

        private bool isEquipShield;
        public bool IsEquipShield
        {
            get => isEquipShield;
            private set
            {
                bool isEquip = value != isEquipShield;
                if (isEquip)
                {
                    isEquipShield = value;
                    shieldIcon.SetActive(isEquipShield);
                    _spriteRenderer.color = isEquipShield ? Color.aquamarine : Color.white;
                }
            }
        }

        public void EquipGimmickShield(ShieldGimmick source, float moveMultiplier)
        {
            gimmickShield = source;
            GimmickMoveMultiplier = moveMultiplier;
            IsEquipShield = true;
        }

        public void ReleaseGimmickShield(ShieldGimmick source)
        {
            if (gimmickShield != source)
                return;
            gimmickShield = null;
            GimmickMoveMultiplier = 1f;
            IsEquipShield = false;
        }

        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [field: SerializeField] public UIDataListSO BodyDataList { get; private set; }
        public UIDataSO CurrentBodyData { get; private set; }

        private void Awake()
        {
            InputCompo = GetComponent<PlayerInputReader>();
            MoveCompo = GetComponentInChildren<AgentMovement>();
            HealthCompo = GetComponent<HealthModule>();
            if (!TryGetComponent(out parry)) parry = gameObject.AddComponent<DLJ_Parry>();
            WeaponCompo = GetComponentInChildren<WeaponHolder>();
            InteractCompo = GetComponentInChildren<Interactor>();
            AnimCompo = GetComponentInChildren<PlayerAnimator>();
            GimmickCompo = GetComponentInChildren<GimmickController>();

            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            WeaponCompo.InitializeWeapons(this);
            InteractCompo.Initialize(this);
            AnimCompo.Initialize(this);
            MoveCompo.Initialize(this);
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
                ApplyDamage(1, Vector2.zero, DLJ_ParryProfile.Default, null, transform.position + Vector3.right,
                    applyKnockback: false);

            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                GimmickCompo.StartSelect();
            }
        }

        public void ApplyDamage(int dmg, Vector2 kbPower)
        {
            ApplyDamage(dmg, kbPower, DLJ_ParryProfile.Default, null, transform.position + Vector3.right);
        }

        public void ApplyDamage(int dmg, Vector2 kbPower, DLJ_ParryProfile profile, HealthModule attacker, Vector3 hitFrom,
            bool applyKnockback = true)
        {
            if (HealthCompo.IsDead) return;

            bool attackFromFront = Vector2.Dot(transform.right, kbPower) < 0f;
            if (HasGimmickShield && attackFromFront)
                return;

            parry.JudgeContact(profile, false, grade =>
            {
                bool perfect = grade == DLJ_ParryGrade.Perfect;
                float achievement = grade == DLJ_ParryGrade.Fail ? 0f : perfect ? 1f : DLJ_ParryProfile.GoodWeight;
                var outcome = profile.Resolve(dmg, achievement, perfect);
                float takenCoefficient = TryGetComponent(out KTH_DefenseStats stats) ? stats.TakenCoefficient : 1f;
                int damage = Mathf.RoundToInt(dmg * outcome.damageMultiplier * takenCoefficient);

                if (damage > 0) HealthCompo.TakeDamage(damage, hitFrom);
                if (applyKnockback && !outcome.blocked && !HasGimmickShield)
                    MoveCompo.KnockbackPower(kbPower);
                if (outcome.counterDamage > 0 && attacker != null && attacker != HealthCompo && attacker.isActiveAndEnabled)
                    attacker.TakeDamage(outcome.counterDamage, transform.position);
                if (outcome.succeeded) parry.PlaySuccess();
            });
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
        
        public bool IsFlipX { get; private set; }

        private void FlipController(float value)
        {
            if (value != 0)
            {
                IsFlipX = value > 0f;
                transform.root.eulerAngles = IsFlipX ? Vector3.zero : new Vector3(0, 180, 0);
                spriteRenderer.sortingOrder = IsFlipX ? 0 : 1;
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
        
        private void HandleJumpKeyPressed()
        {
            if (Time.timeScale > 0.1f)
                MoveCompo.Jump();
        }
        private void HandleExitKeyPressed() => UIManager.Instance.OnEscape();
        
    }
}
