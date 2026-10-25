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

    private void SelectAction(PlayerAction action)
    {
        if (selectionStep == SelectionStep.Action)
        {
            selectedAction = action;
            selectionStep = SelectionStep.Execute;
        }
    }
}
