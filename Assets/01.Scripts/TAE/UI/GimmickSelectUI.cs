using System;
using _01.Scripts.TAE.Combat;
using _01.Scripts.TAE.Managers;
using _01.Scripts.TAE.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts.TAE.UI
{
    public class GimmickSelectUI : MonoBehaviour
    {
        [SerializeField] private Button[] btns;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            int index = 0;
            
            foreach (InteractPointType type in Enum.GetValues(typeof(InteractPointType)))
            {
                if (type == InteractPointType.None) continue;
                btns[index].onClick.AddListener(() => UIManager.Instance.ApplyGimmick(type));
                index++;
            }
        }

        public void Show(Vector3 worldPosition)
        {
            gameObject.SetActive(true);
            transform.position = Utility.GetPos(worldPosition) + Vector3.up * 100f;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SelectFirstButton()
        {
            btns[0].Select();
        }
    }
}