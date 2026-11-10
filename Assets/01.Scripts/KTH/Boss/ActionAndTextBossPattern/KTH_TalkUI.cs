using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 선택지는 이벤트 시스템으로 고른다. 화살표로 이동, 엔터로 선택
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
        // 빈 곳을 클릭해서 선택이 풀려도 화살표가 계속 먹히게 첫 버튼을 다시 잡는다
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
        // 이미 선택된 버튼을 다시 잡으면 하이라이트가 안 바뀌므로 한 번 비운다
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}
