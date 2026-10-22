using System;
using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts.TAE.Managers
{
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance;
        
        private List<Interactable> interactables = new();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void AddInteractable(Interactable interactable)
        {
            interactables.Add(interactable);
        }
        
        public bool TryInteract(Interactable target)
        {
            if (target == null || !interactables.Contains(target)) return false;
            target.Interact();
            return true;
        }
    }
}