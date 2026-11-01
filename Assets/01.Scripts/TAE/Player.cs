using System;
using _01.Scripts.TAE;
using Agents;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.KTH
{
    public class Player : MonoBehaviour
    {
        public PlayerInputReader InputCompo { get; private set; }
        public AgentMovement MoveCompo { get; private set; }
        public HealthModule HealthCompo { get; private set; }
        public Interactor Interactor { get; private set; }
        public PlayerAttack AttackCompo { get; private set; }

        private void Awake()
        {
            InputCompo = GetComponent<PlayerInputReader>();
            MoveCompo = GetComponentInChildren<AgentMovement>();
            HealthCompo = GetComponent<HealthModule>();
            Interactor = GetComponent<Interactor>();
            AttackCompo = GetComponentInChildren<PlayerAttack>();
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

            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                HealthCompo.TakeDamage(1);
            }
        }

        private void HandleJumpKeyPressed() => MoveCompo.Jump();
        private void HandleInteractKeyPressed() => Debug.Log("상호작용");
        private void HandleAttackKeyPressed() => AttackCompo.Attack();
    }
}