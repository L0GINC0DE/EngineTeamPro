using _01.Scripts.TAE.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum SelectionStep
{
    Player,
    Weapon,
    Action
}

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private CanvasGroup buttonGroup;
    [SerializeField] private Selectable firstSelectedButton;

    [Header("Equipped UI (DLJ)")]
    [SerializeField] private EquippedUIView bodyUIView;
    [SerializeField] private EquippedUIView weaponUIView;

    [Header("전투 대사")]
    [SerializeField] private TextBarUI dialogue;
    [SerializeField] private DialogueActionMenu actionMenu;

    public static PlayerUIManager Instance { get; private set; }

    private SelectionStep selectionStep = SelectionStep.Player;
    public SelectionStep CurrentStep => selectionStep;

    private PlayableUI selectedPlayer;
    private PlayableUI selectedWeapon;
    private bool inputEnabled;

    private EventSystem EventSystem => EventSystem.current;

    private PlayerAppearance playerAppearance;

    // 현재 턴을 가진 플레이어만 UI를 선택할 수 있음
    private bool CanSelect => isActiveAndEnabled && player != null && player.CanAct;
    private bool IsActionMenuOpen => actionMenu != null && actionMenu.IsOpen;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (player != null)
            playerAppearance = player.GetComponentInChildren<PlayerAppearance>(true);
    }

    private void OnEnable() => SetInputEnabled(false);

    private void OnDisable()
    {
        RestoreViews();
        ResetSelection();
        SetInputEnabled(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (CanSelect && !inputEnabled)
        {
            RestoreViews();
            ResetSelection();
            if (player.BodyUI != null) player.BodyUI.gameObject.SetActive(true);
            if (player.WeaponUI != null) player.WeaponUI.gameObject.SetActive(true);
            if (playerAppearance != null) playerAppearance.HideSelection();
        }
        if (!CanSelect) ResetSelection();
        SetInputEnabled(CanSelect);
    }

    private void LateUpdate()
    {
        if (!CanSelect || IsActionMenuOpen || dialogue == null) return;

        GameObject selected = EventSystem != null ? EventSystem.currentSelectedGameObject : null;
        PlayableUI focused = selected != null ? selected.GetComponent<PlayableUI>() : null;
        dialogue.Refresh(player, TurnManager.Instance.GetFirstLivingEnemy(), selectionStep, focused);
    }

    private void RestoreViews()
    {
        // 선택한 순서의 역순으로 돌려놔야 원래 버튼 순서가 유지됨
        if (weaponUIView != null) weaponUIView.Restore();
        if (bodyUIView != null) bodyUIView.Restore();
    }

    private void ResetSelection()
    {
        selectionStep = SelectionStep.Player;
        selectedPlayer = null;
        selectedWeapon = null;
    }

    public void SelectPlayableUI(PlayableUI ui)
    {
        if (!CanSelect || IsActionMenuOpen || ui == null || !ui.CanEquip || selectionStep == SelectionStep.Action
            || ui == selectedPlayer || ui == selectedWeapon) return;

        Selectable selectable = ui.GetComponent<Selectable>();
        Selectable next = selectable != null
            ? selectable.FindSelectableOnRight() ?? selectable.FindSelectableOnLeft()
            : null;
        bool attached = false;

        switch (selectionStep)
        {
            case SelectionStep.Player:
                selectedPlayer = ui;
                if (ui.UISo != null && playerAppearance != null)
                    playerAppearance.ShowBody(ui.UISo.uiType);
                attached = bodyUIView != null && bodyUIView.Attach(ui);
                selectionStep = SelectionStep.Weapon;
                break;
            case SelectionStep.Weapon:
                selectedWeapon = ui;
                if (ui.UISo != null && playerAppearance != null)
                    playerAppearance.ShowWeapon(ui.UISo.uiType);
                attached = weaponUIView != null && weaponUIView.Attach(ui);
                selectionStep = SelectionStep.Action;
                break;
        }

        if (!attached) ui.gameObject.SetActive(false);
        SetInputEnabled(CanSelect);
        if (EventSystem != null)
            EventSystem.SetSelectedGameObject(next != null ? next.gameObject : attached ? ui.gameObject : null);
    }

    public void SelectAction(PlayerAction action)
    {
        if (!CanSelect || IsActionMenuOpen || selectionStep != SelectionStep.Action || action == null) return;

        if (selectedPlayer == null || !selectedPlayer.CanEquip || selectedWeapon == null || !selectedWeapon.CanEquip)
        {
            ResetSelection();
            return;
        }
        player.Equip(selectedPlayer, selectedWeapon);
        UseAction(action);
    }

    public void UseAction(PlayerAction action)
    {
        if (action == null || player == null) return;
        if (player.CanAct) player.DefenseStats.isDefending = false;
        if (action.PlayAction(player, player.BodyUI, player.WeaponUI)) ResetSelection();
        SetInputEnabled(CanSelect);
    }

    private void SetInputEnabled(bool enabled)
    {
        if (!enabled && inputEnabled)
        {
            if (actionMenu != null) actionMenu.Close(restoreFocus: false);
            if (dialogue != null) dialogue.EndTurn();
        }

        bool mainButtonsEnabled = enabled && !IsActionMenuOpen;
        if (buttonGroup != null) buttonGroup.interactable = buttonGroup.blocksRaycasts = mainButtonsEnabled;

        if (!enabled && EventSystem != null && buttonGroup != null)
        {
            GameObject selected = EventSystem.currentSelectedGameObject;
            if (selected != null)
                EventSystem.SetSelectedGameObject(null);
        }
        else if (mainButtonsEnabled && !inputEnabled && EventSystem != null && firstSelectedButton != null
            && firstSelectedButton.IsActive() && firstSelectedButton.IsInteractable())
        {
            EventSystem.SetSelectedGameObject(firstSelectedButton.gameObject);
        }

        inputEnabled = enabled;
    }
}
