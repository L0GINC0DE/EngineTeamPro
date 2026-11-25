using System.Collections;
using _01.Scripts.TAE.CoreLib;
using _01.Scripts.TAE.Players;
using _01.Scripts.TAE.Test;
using UnityEngine;

namespace _01.Scripts.TAE.Agents
{
    public class AgentMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float jumpForce = 5f;
        private Vector2 moveDir;

        private Rigidbody2D rb;
        
        [Header("Ground Settings")]
        [SerializeField] private Transform checker;
        [SerializeField] private Vector2 checkerSize;
        [SerializeField] private LayerMask whatIsGround;

        [Header("ExtraGravity Settings")]
        [SerializeField] private float extraGravity;
        [SerializeField] private float gravityDelay;

        [Header("Jump Buffer Settings")]
        [SerializeField] private float jumpBufferTime;
        
        private float _timeInAir;
        private float _coyoteTimer;
        
        public bool CanManualMove { get; private set; } = true;
        
        public NotifyValue<bool> IsGrounded = new();
        public NotifyValue<bool> IsMove = new();

        private Vector2 pushDir;
        private float launchVelocity;
        private float launchTime;
        private bool boostedAscent;
        private float boostAscentDeceleration;
        
        public void Bounce(float speed, float ascentDeceleration)
        {
            launchTime = 0f;
            boostedAscent = speed > 0f;
            boostAscentDeceleration = ascentDeceleration;
            rb.linearVelocityY = speed;
            _coyoteTimer = 0f;
            _timeInAir = 0f;
        }

        public void Launch(float horizontalSpeed, float duration)
        {
            boostedAscent = false;
            launchVelocity = horizontalSpeed;
            launchTime = duration;
            rb.linearVelocity = new Vector2(horizontalSpeed, 0f);
        }

        private PlatformPlayer owner;
        
        private void Awake()
        {
            rb = transform.root.GetComponent<Rigidbody2D>();
        }

        private void OnDisable()
        {
            if (_kbCoroutine != null)
            {
                StopCoroutine(_kbCoroutine);
                _kbCoroutine = null;
            }

            CanManualMove = true;
        }

        public void Initialize(PlatformPlayer player)
        {
            owner = player;
        }

        public void SetMoveDir(Vector2 moveDir)
        {
            this.moveDir = moveDir;
            IsMove.Value = moveDir != Vector2.zero;
        }

        public void SetPushDir(Vector2 pushDir, PushDirType type = PushDirType.None)
        {
            var blockDirection = owner.IsFlipX ? PushDirType.Left : PushDirType.Right;
            bool isBlocked = owner.IsEquipShield && type == blockDirection;
            this.pushDir = isBlocked ? Vector2.zero : pushDir;
        }

        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
        }

        public void Jump()
        {
            if (launchTime > 0f || _coyoteTimer <= 0f) return;
            
            rb.linearVelocityY = 0f;
            rb.AddForceY(jumpForce, ForceMode2D.Impulse);
            _coyoteTimer = 0f;
        }
        
        private void FixedUpdate()
        {
            if (launchTime > 0f)
            {
                rb.linearVelocity = new Vector2(launchVelocity, 0f);
                launchTime -= Time.fixedDeltaTime;
                return;
            }

            if (CanManualMove)
            {
                float speedMultiplier = owner != null ? owner.GimmickMoveMultiplier : 1f;
                float push = owner != null && owner.HasGimmickShield ? 0f : pushDir.x;
                rb.linearVelocityX = moveDir.x * moveSpeed * speedMultiplier + push;
            }
            
            CalculateAirTime();
            ApplyExtraGravity();
        }

        private bool CheckGround()
        {
            if (checker != null)
            {
                return Physics2D.OverlapBox(checker.position, checkerSize, 0f, whatIsGround) != null;
            }

            return false;
        }

        private void CalculateAirTime()
        {
            if (!CheckGround())
            {
                _timeInAir += Time.fixedDeltaTime;
                _coyoteTimer -= Time.fixedDeltaTime;
            }
            else
            {
                _timeInAir = 0f;
                _coyoteTimer = jumpBufferTime;
            }
        }

        private void ApplyExtraGravity()
        {
            if (boostedAscent && rb.linearVelocityY > 0f)
            {
                rb.AddForceY(-boostAscentDeceleration * rb.mass);
                return;
            }

            boostedAscent = false;
            if (_timeInAir > gravityDelay)
                rb.AddForceY(-extraGravity);
        }

        private void OnDrawGizmos()
        {
            if (checker == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(checker.position, checkerSize);
        }

        private Coroutine _kbCoroutine;
        public void KnockbackPower(Vector2 force, float knockbackDuration = 0.2f)
        {
            CanManualMove = false;
            _timeInAir = 0f;
            rb.AddForce(force * rb.mass, ForceMode2D.Impulse);
            
            if(_kbCoroutine != null) StopCoroutine(_kbCoroutine);
            _kbCoroutine = StartCoroutine(KnockbackCoroutine(knockbackDuration));
        }

        private IEnumerator KnockbackCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            rb.linearVelocity = Vector2.zero;
            CanManualMove = true;
        }

        public void ResetForStage(Vector2 position)
        {
            if (_kbCoroutine != null)
            {
                StopCoroutine(_kbCoroutine);
                _kbCoroutine = null;
            }

            launchTime = 0f;
            boostedAscent = false;
            _timeInAir = 0f;
            _coyoteTimer = 0f;
            pushDir = Vector2.zero;
            SetMoveDir(Vector2.zero);
            rb.position = position;
            StopInput(false);
        }

        public void StopInput(bool isStop)
        {
            rb.linearVelocity = Vector2.zero;
            CanManualMove = !isStop;
        }
    }
}