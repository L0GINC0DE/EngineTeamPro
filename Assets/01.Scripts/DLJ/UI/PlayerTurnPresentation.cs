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

    private void Awake()
    {
        if (textBar != null) originalTextBarPosition = textBar.anchoredPosition;

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
        if (isPlayerTurn == showingPlayerTurn) return;

        showingPlayerTurn = isPlayerTurn;
        MoveToTurnPosition(isPlayerTurn);
    }

    private void MoveToTurnPosition(bool isPlayerTurn)
    {
        movementSequence?.Kill();
        movementSequence = DOTween.Sequence()
            .SetUpdate(UpdateType.Late)
            .OnKill(() => movementSequence = null);

        if (textBar != null)
        {
            Vector2 target = isPlayerTurn ? textBarTargetPosition : originalTextBarPosition;
            movementSequence.Join(textBar.DOAnchorPos(target, transitionDuration).SetEase(Ease.OutQuart));
        }

        float rise = isPlayerTurn ? unitRise : 0f;
        for (int i = 0; i < battleUnits.Length; i++)
        {
            Transform unit = battleUnits[i];
            if (unit != null)
                movementSequence.Join(unit.DOMoveY(originalUnitY[i] + rise, transitionDuration)
                    .SetEase(Ease.OutQuart));
        }

        if (transitionDuration <= 0f) movementSequence.Complete();
    }

    private void OnDisable()
    {
        movementSequence?.Kill();
        showingPlayerTurn = false;

        if (textBar != null) textBar.anchoredPosition = originalTextBarPosition;

        for (int i = 0; i < battleUnits.Length; i++)
        {
            Transform unit = battleUnits[i];
            if (unit == null) continue;

            Vector3 position = unit.position;
            position.y = originalUnitY[i];
            unit.position = position;
        }
    }
}
