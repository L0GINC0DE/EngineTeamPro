using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts.TAE.UI
{
    public class ClothUI : MonoBehaviour
    {
        public readonly Stack<GameObject> panels = new();

        public void Push(GameObject panel)
        {
            panels.Push(panel);
            panel.SetActive(true);
        }

        public void Pop()
        {
            if (panels.Count == 0) return;

            panels.Pop().SetActive(false);
        }
    }
}