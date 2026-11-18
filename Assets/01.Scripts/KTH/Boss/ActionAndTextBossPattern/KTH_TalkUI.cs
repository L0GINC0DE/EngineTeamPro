using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KTH_TalkUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text bossText;
    [SerializeField] private Button[] buttons;

    private void Awake()
    {
        panel.SetActive(false);
    }

    private void Update()
    {
        if (panel.activeSelf && EventSystem.current.currentSelectedGameObject == null)
            Select(buttons[0]);
    }

    public void Show(Color selectColor,KTH_TextSo so, Action<bool> onSelect)
    {
        panel.SetActive(true);
        bossText.text = so.text;

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].GetComponentInChildren<TMP_Text>().text = so.choices[i];
            buttons[i].onClick.RemoveAllListeners();
            ColorBlock cb = buttons[i].colors;
            cb.selectedColor = selectColor;
            buttons[i].colors = cb;
            buttons[i].onClick.AddListener(() =>
            {
                EventSystem.current.SetSelectedGameObject(null);
                panel.SetActive(false);
                onSelect(index == so.answer);
            });
        }

        Select(buttons[0]);
    }

    private void Select(Button button)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}
