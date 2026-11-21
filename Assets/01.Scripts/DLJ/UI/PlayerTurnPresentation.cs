using _01.Scripts.TAE.Managers;
using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerTurnPresentation : MonoBehaviour
{
    [SerializeField] private RectTransform textBar;
    [SerializeField] private Vector2 textBarTargetPosition;
    [SerializeField] private Transform[] battleUnits;

    [Header("Player turn movement")]
    [SerializeField, Min(0f)] private float unitRise = 0.6f;
    [SerializeField, Min(0f)] private float transitionDuration = 0.3f;

    private Vector2 originalTextBarPosition;
    private float[] originalUnitY;
    private bool showingPlayerTurn;
    private Sequence movementSequence;
    private Tween textBarTween;
    private Transform originalTextBarParent;
    private bool textBarWasEquipped;

    private void Awake()
    {
        if (textBar != null)
        {
            originalTextBarPosition = textBar.anchoredPosition;
            originalTextBarParent = textBar.parent;
        }

        if (battleUnits == null) battleUnits = new Transform[0];
        originalUnitY = new float[battleUnits.Length];
        for (int i = 0; i < battleUnits.Length; i++)
        {
            if (battleUnits[i] != null) originalUnitY[i] = battleUnits[i].position.y;
        }
    }

    private void LateUpdate()
    {
        TurnManager turnManager = TurnManager.Instance;
        bool isPlayerTurn = turnManager.isActiveAndEnabled && !turnManager.BattleEnded && turnManager.IsPlayerTurn;
        bool textBarIsEquipped = textBar != null && textBar.parent != originalTextBarParent;
        if (textBarIsEquipped) textBarTween?.Kill();
        else if (textBarWasEquipped) MoveTextBar(isPlayerTurn);
        textBarWasEquipped = textBarIsEquipped;
        if (isPlayerTurn == showingPlayerTurn) return;

        showingPlayerTurn = isPlayerTurn;
        MoveToTurnPosition(isPlayerTurn);
    }

    private void MoveToTurnPosition(bool isPlayerTurn)
    {
        movementSequence?.Kill();
        MoveTextBar(isPlayerTurn);
        if (battleUnits.Length == 0) return;
        movementSequence = DOTween.Sequence()
            .SetUpdate(UpdateType.Late)
            .OnKill(() => movementSequence = null);

        for (int i = 0; i < battleUnits.Length; i++)
        {
            if (battleUnits[i] == null) continue;
            float targetUnitY = originalUnitY[i] + (isPlayerTurn ? unitRise : 0f);
            movementSequence.Join(battleUnits[i].DOMoveY(targetUnitY, transitionDuration)
                .SetEase(Ease.OutQuart));
        }

        if (transitionDuration <= 0f) movementSequence.Complete();
    }

    private void MoveTextBar(bool isPlayerTurn)
    {
        textBarTween?.Kill();
        if (textBar == null || textBar.parent != originalTextBarParent) return;
        Vector2 target = isPlayerTurn ? textBarTargetPosition : originalTextBarPosition;
        textBarTween = textBar.DOAnchorPos(target, transitionDuration)
            .SetEase(Ease.OutQuart)
            .SetUpdate(UpdateType.Late)
            .OnKill(() => textBarTween = null);
        if (transitionDuration <= 0f) textBarTween.Complete();
    }

    private void OnDisable()
    {
        movementSequence?.Kill();
        textBarTween?.Kill();
        showingPlayerTurn = false;

        if (textBar != null && textBar.parent == originalTextBarParent)
            textBar.anchoredPosition = originalTextBarPosition;

        for (int i = 0; i < battleUnits.Length; i++)
        {
            if (battleUnits[i] == null) continue;
            Vector3 position = battleUnits[i].position;
            position.y = originalUnitY[i];
            battleUnits[i].position = position;
        }
    }
}
