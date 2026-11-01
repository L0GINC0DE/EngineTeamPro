using System;
using System.Collections.Generic;
using UnityEngine;

public enum SelectionStep
{
    Player,
    Weapon,
    Action,
    Execute
}

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private List<PlayableUI> uiList;
    [SerializeField] private Player player;

    public static PlayerUIManager Instance { get; private set; }

    private SelectionStep selectionStep = SelectionStep.Player;
    private PlayableUI selectedPlayer;
    private PlayableUI selectedWeapon;
    private PlayerAction selectedAction;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (selectionStep != SelectionStep.Execute) return;
        
        selectionStep = SelectionStep.Player;
        
        selectedAction.PlayAction(player, selectedPlayer, selectedWeapon);

        selectedPlayer = null;
        selectedWeapon = null;
        selectedAction = null;
    }

    public void SelectPlayableUI(PlayableUI ui)
    {
        switch (selectionStep)
        {
            case SelectionStep.Player:
                selectedPlayer = ui;
                selectionStep = SelectionStep.Weapon;
                break;
            case SelectionStep.Weapon:
                selectedWeapon = ui;
                selectionStep = SelectionStep.Action;
                break;
        }
    }

    public void SelectAction(PlayerAction action)
    {
        if (selectionStep == SelectionStep.Action)
        {
            selectedAction = action;
            selectionStep = SelectionStep.Execute;
        }
    }
}
