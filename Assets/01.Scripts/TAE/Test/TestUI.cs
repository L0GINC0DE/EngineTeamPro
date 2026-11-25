using System.Collections.Generic;
using _01.Scripts.TAE.CoreLib;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts.TAE.Test
{
    public class TestUI : MonoBehaviour
    {
        [SerializeField] private float cardStep = 160f;

        public Button[] buttons;
        public Button CurrentBtn { get; private set; }

        private readonly NotifyValue<int> selectedIndex = new(0);
        private readonly Queue<int> pendingMoves = new();

        private Tween slide;
        private RectTransform Content => (RectTransform)transform;

        private void OnEnable()
        {
            selectedIndex.OnValueChanged += ArrangeCards;
            ArrangeCards(selectedIndex.Value, selectedIndex.Value);
        }

        private void OnDisable()
        {
            selectedIndex.OnValueChanged -= ArrangeCards;
            slide?.Kill();
            slide = null;
            pendingMoves.Clear();
        }

        public void MoveSelection(int direction)
        {
            if (!isActiveAndEnabled || (direction != -1 && direction != 1))
                return;

            pendingMoves.Enqueue(direction);
            PlayNextMove();
        }

        private void PlayNextMove()
        {
            if (slide != null || !pendingMoves.TryDequeue(out int direction))
                return;

            Button waitingCard = buttons[Wrap(selectedIndex.Value + 2)];
            SetCardSlot(waitingCard, direction == -1 ? -1 : 3);
            waitingCard.gameObject.SetActive(true);

            slide = Content.DOAnchorPosX(-direction * cardStep, 0.15f)
                .SetEase(Ease.Linear)
                .OnComplete(() => FinishMove(direction));
        }

        private void FinishMove(int direction)
        {
            selectedIndex.Value = Wrap(selectedIndex.Value + direction);
            slide = null;
            PlayNextMove();
        }

        private void ArrangeCards(int _, int next)
        {
            CurrentBtn = buttons[next];

            for (int slot = 0; slot < 4; slot++)
            {
                Button button = buttons[Wrap(next + slot - 1)];
                SetCardSlot(button, slot);
                button.gameObject.SetActive(slot < 3);
            }

            SetContentX(0f);
        }

        private void SetCardSlot(Button button, int slot)
        {
            var card = (RectTransform)button.transform;
            float x = card.rect.width * 0.5f + slot * cardStep;
            card.anchoredPosition = new Vector2(x, card.anchoredPosition.y);
        }

        private void SetContentX(float x)
        {
            Content.anchoredPosition = new Vector2(x, Content.anchoredPosition.y);
        }

        private static int Wrap(int index) => (index + 4) % 4;
    }
}