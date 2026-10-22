using System;
using _01.Scripts.TAE.Managers;
using UnityEngine;

namespace _01.Scripts.TAE.Test
{
    public class InteractTest : MonoBehaviour, Interactable
    {
        private void Start()
        {
            StageManager.Instance.AddInteractable(this);
        }

        public void Interact()
        {
            Debug.Log("상호작용이 되었습니다");
        }
    }
}