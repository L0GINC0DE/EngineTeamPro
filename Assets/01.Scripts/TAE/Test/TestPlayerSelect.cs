using System;
using _01.Scripts.TAE.Players;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.TAE.Test
{
    public class TestPlayerSelect : MonoBehaviour
    {
        public PlayerInputReader InputCompo { get; private set; }

        [SerializeField] private TestUI testUI;

        private void Awake()
        {
            InputCompo = GetComponent<PlayerInputReader>();
        }

        private void OnEnable()
        {
            InputCompo.OnPrevKeyPressed += HandlePrevKeyPressed;
            InputCompo.OnNextKeyPressed += HandleNextKeyPressed;
        }

        private void OnDisable()
        {
            InputCompo.OnPrevKeyPressed -= HandlePrevKeyPressed;
            InputCompo.OnNextKeyPressed -= HandleNextKeyPressed;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.leftArrowKey.wasPressedThisFrame)
                HandlePrevKeyPressed();
            else if (keyboard.rightArrowKey.wasPressedThisFrame)
                HandleNextKeyPressed();
        }

        private void HandlePrevKeyPressed() => testUI.MoveSelection(-1);
        private void HandleNextKeyPressed() => testUI.MoveSelection(1);
    }
}