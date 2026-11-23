using _01.Scripts.TAE.Interface;
using _01.Scripts.TAE.Managers;
using UnityEngine;

namespace _01.Scripts.TAE.UI.Slots
{
    public class Cloth : MonoBehaviour, Interactable
    {
        public void Interact()
        {
            UIManager.Instance.ShowClothUI();
        }
    }
}