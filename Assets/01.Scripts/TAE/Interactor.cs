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

        private PlatformPlayer player;

        private void Awake()
        {
            player = GetComponent<PlatformPlayer>();
        }

        public void Interact(UITypes type)
        {
            var hit = Physics2D.OverlapCircle(transform.position, range, whatIsInteractable);

            if (hit != null)
            {
                InteractTest interact = hit.GetComponent<InteractTest>();

                switch (type)
                {
                    case UITypes.Player:
                        if (interact.Data == player.WeaponCompo.data) return;
                        player.SetData(interact.Data);
                        break;
                    case UITypes.Weapon:
                        if (interact.Data == player.data) return;
                        player.WeaponCompo.SetWeapon(interact.Data);
                        break;
                        
                }
            }
        }
        
        

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}