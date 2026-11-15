using System;
using System.Collections;
using UnityEngine;

namespace _01.Scripts.KTH
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

        public void SetMoveDir(Vector2 moveDir)
        {
            this.moveDir = moveDir;
        }

        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
        }

        public void Jump()
        {
            if (_coyoteTimer <= 0f) return;
            
            rb.linearVelocityY = 0f;
            rb.AddForceY(jumpForce, ForceMode2D.Impulse);
            _coyoteTimer = 0f;
        }
        
        private void FixedUpdate()
        {
            if (CanManualMove)
            {
                rb.linearVelocityX = moveDir.x * moveSpeed;
            }
        }

        private void Update()
        {
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
                _timeInAir += Time.deltaTime;
                _coyoteTimer -= Time.deltaTime;
            }
            else
            {
                _timeInAir = 0f;
                _coyoteTimer = jumpBufferTime;
            }
        }

        private void ApplyExtraGravity()
        {
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

        public void StopInput(bool isStop)
        {
            rb.linearVelocity = Vector2.zero;
            CanManualMove = !isStop;
        }
    }
}