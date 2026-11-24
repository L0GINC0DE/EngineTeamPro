using System;
using System.Collections.Generic;
using _01.Scripts.TAE.CoreLib;
using _01.Scripts.TAE.Data;
using _01.Scripts.TAE.Managers;
using _01.Scripts.TAE.Utils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts.TAE.UI
{
    public class GimmickSelectUI : MonoBehaviour
    {
        private const float CardStep = 160f;

        [SerializeField] private UIDataListSO optionData;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private Card[] visibleCards;
        [SerializeField] private Card waitingCard;

        private readonly NotifyValue<int> selectedIndex = new(0);
        private readonly Queue<int> pendingMoves = new();
        private Tween slide;
        private int OptionCount => optionData?.uiDataList?.Count ?? 0;

        private void OnEnable() => selectedIndex.OnValueChanged += ArrangeCards;

        private void OnDisable()
        {
            selectedIndex.OnValueChanged -= ArrangeCards;
            slide?.Kill();
            slide = null;
            pendingMoves.Clear();
        }

        public void Show(Vector3 worldPosition)
        {
            gameObject.SetActive(true);
            viewport.position = Utility.GetPos(worldPosition) + Vector3.up * 175f;

            if (selectedIndex.Value == 0)
                ArrangeCards(0, 0);
            else
                selectedIndex.Value = 0;
        }

        public void Hide() => gameObject.SetActive(false);

        public void MoveSelection(int direction)
        {
            if (OptionCount < 2)
                return;

            pendingMoves.Enqueue(direction);
            PlayNextMove();
        }

        public bool TryConfirm()
        {
            if (slide != null || pendingMoves.Count > 0)
                return false;

            UIDataSO data = GetOption(selectedIndex.Value);
            if (data == null)
                return false;

            UIManager.Instance.ApplyGimmick(data);
            return true;
        }

        private void PlayNextMove()
        {
            if (slide != null || pendingMoves.Count == 0)
                return;

            int direction = pendingMoves.Dequeue();
            int next = WrapIndex(selectedIndex.Value + direction);
            waitingCard.Place(direction < 0 ? -2 : 2, GetOption(next + direction), false);

            slide = content.DOAnchorPosX(-direction * CardStep, 0.15f)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .OnComplete(() => FinishMove(direction, next));
        }

        private void FinishMove(int direction, int next)
        {
            int leavingSlot = direction > 0 ? 0 : 2;
            int enteringSlot = direction > 0 ? 2 : 0;
            Card leavingCard = visibleCards[leavingSlot];

            for (int slot = leavingSlot; slot != enteringSlot; slot += direction)
                visibleCards[slot] = visibleCards[slot + direction];

            visibleCards[enteringSlot] = waitingCard;
            waitingCard = leavingCard;

            selectedIndex.Value = next;
            slide = null;
            PlayNextMove();
        }

        private void ArrangeCards(int _, int index)
        {
            for (int slot = -1; slot <= 1; slot++)
            {
                UIDataSO data = slot == 0 || OptionCount > 1 ? GetOption(index + slot) : null;
                visibleCards[slot + 1].Place(slot, data, slot == 0);
            }

            waitingCard.Place(2, null, false);
            content.anchoredPosition = Vector2.zero;
        }

        private UIDataSO GetOption(int index)
        {
            return OptionCount > 0 ? optionData.uiDataList[WrapIndex(index)] : null;
        }

        private int WrapIndex(int index) => (index + OptionCount) % OptionCount;

        [Serializable]
        private sealed class Card
        {
            [SerializeField] private Button button;
            [SerializeField] private Image icon;
            [SerializeField] private TMP_Text label;

            public void Place(int slot, UIDataSO data, bool selected)
            {
                ((RectTransform)button.transform).anchoredPosition = new Vector2(slot * CardStep, 0f);
                button.gameObject.SetActive(data != null);
                if (data == null)
                    return;

                SetUp(data);
                button.image.color = selected ? button.colors.selectedColor : button.colors.normalColor;
            }

            private void SetUp(UIDataSO data)
            {
                icon.sprite = data.uiSprite;
                label.text = data.uiName;
            }
        }
    }
}
