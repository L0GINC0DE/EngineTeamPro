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

    public static PlayerUIManager Instance { get; private set; }

    private SelectionStep selectionStep = SelectionStep.Player;
    public SelectionStep CurrentStep => selectionStep;

    private PlayableUI selectedPlayer;
    private PlayableUI selectedWeapon;
    private bool inputEnabled;

    //간단히하면 그냥 현재턴이 플레이어 턴이면 참
    private bool CanSelect => isActiveAndEnabled && player != null && player.CanAct;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable() => SetInputEnabled(false);

    private void OnDisable()
    {
        ResetSelection();
        SetInputEnabled(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!CanSelect) ResetSelection();
        SetInputEnabled(CanSelect);
    }

    private void ResetSelection()
    {
        selectionStep = SelectionStep.Player;
        selectedPlayer = null;
        selectedWeapon = null;
    }

    public void SelectPlayableUI(PlayableUI ui)
    {
        if (!CanSelect || ui == null || !ui.CanEquip) return;

        switch (selectionStep)
        {
            case SelectionStep.Player:
                selectedPlayer = ui;
                Debug.Log(selectedPlayer);
                selectionStep = SelectionStep.Weapon;
                break;
            case SelectionStep.Weapon:
                selectedWeapon = ui;
                Debug.Log(selectedWeapon);
                selectionStep = SelectionStep.Action;
                break;
        }
    }

    public void SelectAction(PlayerAction action)
    {
        if (!CanSelect || selectionStep != SelectionStep.Action || action == null) return;

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

    //인풋시스템으로 버튼 고르는거
    //매개변수로 받은 불 값으로 UI 선택 / 해제
    private void SetInputEnabled(bool enabled)
    {
        if (buttonGroup != null) buttonGroup.interactable = enabled;

        EventSystem eventSystem = EventSystem.current;
        
        if (!enabled && eventSystem != null && buttonGroup != null)
        {
            GameObject selected = eventSystem.currentSelectedGameObject;
            if (selected != null)
                eventSystem.SetSelectedGameObject(null);
        }
        else if (enabled && !inputEnabled && eventSystem != null && firstSelectedButton != null
            && firstSelectedButton.IsActive() && firstSelectedButton.IsInteractable())
        {
            eventSystem.SetSelectedGameObject(firstSelectedButton.gameObject);
        }

        inputEnabled = enabled;
    }
}
