using UnityEngine;

namespace _01.Scripts.TAE
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private float range;
        [SerializeField] private LayerMask whatIsInteractable;

        public void Interact()
        {
                
        }
        
        

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}