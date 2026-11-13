using _01.Scripts.TAE.Interface;
using _01.Scripts.TAE.UI;
using UnityEngine;

namespace _01.Scripts.TAE
{
    public class Cloth : MonoBehaviour, Interactable
    {
        public void Interact()
        {
            UIManager.Instance.ShowClothUI();
        }
    }
}