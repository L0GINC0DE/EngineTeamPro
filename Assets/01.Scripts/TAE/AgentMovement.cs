using System;
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
        private float _timeInAir;
        
        private void Awake()
        {
            rb = transform.root.GetComponent<Rigidbody2D>();
        }

        public void SetMoveDir(Vector2 moveDir)
        {
            this.moveDir = moveDir;
        }

        public void Jump()
        {
            if (!CheckGround()) return;
            rb.AddForceY(jumpForce, ForceMode2D.Impulse);
        }
        
        private void FixedUpdate()
        {
            rb.linearVelocityX = moveDir.x * moveSpeed;
        }

        private void Update()
        {
            CalculateAirTime();
            ApplyExtraGravity();
        }

        private bool CheckGround()
        {
            return Physics2D.OverlapBox(checker.position, checkerSize, 0f, whatIsGround) != null;
        }

        private void CalculateAirTime()
        {
            if (!CheckGround())
                _timeInAir += Time.deltaTime;
            else
                _timeInAir = 0f;
        }

        private void ApplyExtraGravity()
        {
            if (_timeInAir > gravityDelay)
                rb.AddForceY(-extraGravity);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(checker.position, checkerSize);
        }
    }
}