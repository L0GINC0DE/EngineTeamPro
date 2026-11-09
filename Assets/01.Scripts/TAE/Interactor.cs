using System;
using _01.Scripts.KTH;
using _01.Scripts.TAE.Test;
using UnityEngine;


namespace _01.Scripts.TAE
{
    public enum UITypes
    {
        Player,
        Weapon
    }
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private float range;
        [SerializeField] private LayerMask whatIsInteractable;

        public void Interact(PlatformPlayer player)
        {
            foreach (var hit in Physics2D.OverlapCircleAll(transform.position, range, whatIsInteractable))
            {
                var target = hit.GetComponentInParent<Interactable>();
                if (target == null) continue;
                target.Interact(player);
                return;
            }
        }

        public bool IsInRange(Interactable target)
        {
            if (target == null) return false;
            foreach (var hit in Physics2D.OverlapCircleAll(transform.position, range, whatIsInteractable))
                if (hit.GetComponentInParent<Interactable>() == target) return true;
            return false;
        }
        
        

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
