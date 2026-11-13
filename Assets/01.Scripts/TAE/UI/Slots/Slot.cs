using System;
using _01.Scripts.TAE.Data;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts.TAE.UI
{
    public class Slot : MonoBehaviour
    {
        public Transform content;
        public GameObject BtnTemplate;
        
        public void Initialize(UIDataListSO list, Action<UIDataSO> onSelected)
        {
            foreach (var data in list.uiDataList)
            {
                var btnTemplate = Instantiate(BtnTemplate, content);
                btnTemplate.transform.GetChild(1).GetComponent<Image>().sprite = data.uiSprite;
                btnTemplate.GetComponent<Button>().onClick.AddListener(() => onSelected?.Invoke(data));
            }
        }
    }
}