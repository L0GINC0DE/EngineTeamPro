using System;
using UnityEngine;

namespace _01.Scripts.KTH
{
    public class Player : MonoBehaviour
    {
        public PlayerInputReader InputCompo { get; private set; }
        public AgentMovement MoveCompo { get; private set; }

        private void Awake()
        {
            InputCompo = GetComponent<PlayerInputReader>();
            MoveCompo = GetComponentInChildren<AgentMovement>();
        }

        private void OnEnable()
        {
            InputCompo.OnJumpKeyPressed += HandleJumpKeyPressed;
            InputCompo.OnInteractKeyPressed += HandleInteractKeyPressed;
        }
        
        private void OnDisable()
        {
            InputCompo.OnJumpKeyPressed -= HandleJumpKeyPressed;
            InputCompo.OnInteractKeyPressed -= HandleInteractKeyPressed;
        }

        private void Update()
        {
            MoveCompo.SetMoveDir(InputCompo.MoveInput);
        }

        private void HandleJumpKeyPressed() => MoveCompo.Jump();
        private void HandleInteractKeyPressed() => Debug.Log("상호작용");
    }
}