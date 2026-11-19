using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.TAE.Players
{
    public class PlayerInputReader : MonoBehaviour, InputSystem_Actions.IPlayerActions
    {
        public event Action OnJumpKeyPressed;
        public event Action OnInteractKeyPressed;
        public event Action OnAttackKeyPressed;
        public event Action OnPrevKeyPressed;
        public event Action OnNextKeyPressed;
        public event Action OnExitKeyPressed;
        public Vector2 MoveInput { get; private set; }

        private InputSystem_Actions _controls;

        private void Awake()
        {
            _controls = new InputSystem_Actions();
            _controls.Player.SetCallbacks(this);
        }

        private void OnEnable()
        {
            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnAttackKeyPressed?.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnInteractKeyPressed?.Invoke();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnJumpKeyPressed?.Invoke();
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnPrevKeyPressed?.Invoke();
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnNextKeyPressed?.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
        }

        public void OnExit(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnExitKeyPressed?.Invoke();
        }
    }
}