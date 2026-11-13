using System;
using System.Collections.Generic;
using System.Linq;
using _01.Scripts.TAE.Data;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts.TAE.UI
{
    public class Slot : MonoBehaviour
    {
        public Transform content;
        public GameObject BtnTemplate;

        private Button firstBtn;

        private List<Button> btns = new();

        public void Initialize(UIDataListSO list, Action<UIDataSO> onSelected)
        {
            foreach (var data in list.uiDataList)
            {
                var btnTemplate = Instantiate(BtnTemplate, content);
                btnTemplate.transform.GetChild(1).GetComponent<Image>().sprite = data.uiSprite;
                Button button = btnTemplate.GetComponent<Button>();
                button.onClick.AddListener(() => onSelected?.Invoke(data));
                btns.Add(button);
            }

            firstBtn = btns.First();

            ConnectNavigation(btns, 3);
        }

        private void ConnectNavigation(List<Button> buttons, int columns)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                int above = i - columns;
                int below = i + columns;

                buttons[i].navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnLeft = i > 0 ? buttons[i - 1] : null,
                    selectOnRight = i + 1 < buttons.Count ? buttons[i + 1] : null,
                    selectOnUp = above >= 0 ? buttons[above] : null,
                    selectOnDown = below < buttons.Count ? buttons[below] : null
                };
            }
        }

        public void FocusFirstBtn() => firstBtn.Select();
    }
}