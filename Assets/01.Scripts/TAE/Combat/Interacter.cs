using System;
using _01.Scripts.KTH;
using _01.Scripts.TAE.Interface;
using UnityEngine;

namespace _01.Scripts.TAE.Combat
{
    public class Interactor : MonoBehaviour
    {
        public float range;
        public LayerMask whatIsInteractable;

        private PlayerInputReader _playerInput;

        public void Initialize(PlatformPlayer player)
        {
            _playerInput = player.InputCompo;
            _playerInput.OnInteractKeyPressed += HandleInteractKeyPressed;
        }

        private void OnDisable()
        {
            if (_playerInput != null)
                _playerInput.OnInteractKeyPressed -= HandleInteractKeyPressed;
        }

        private void HandleInteractKeyPressed()
        {
            var col = Physics2D.OverlapCircle(transform.position, range, whatIsInteractable);
            if (col != null)
            {
                if (col.TryGetComponent(out Interactable interactable))
                {
                    interactable.Interact();
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}