using System;
using _01.Scripts.TAE.Data;
using _01.Scripts.KTH;
using _01.Scripts.TAE.Managers;
using UnityEngine;

namespace _01.Scripts.TAE.Test
{
    public class InteractTest : MonoBehaviour, Interactable
    {
        [SerializeField] private UIData data;

        public UIData Data => data;

        public void Interact(PlatformPlayer player)
        {
            if (isActiveAndEnabled && player != null)
                UIManager.Instance.ShowUI(player, this);
        }
    }
}
