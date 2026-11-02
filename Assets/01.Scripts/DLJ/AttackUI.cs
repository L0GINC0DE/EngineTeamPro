using UnityEngine;

public class AttackUI : PlayableUI
{
    private PlayerAction action = new AttackAction();

    public override void Select()
    {
        if (PlayerUIManager.Instance.CurrentStep == SelectionStep.Action)
        {
            PlayerUIManager.Instance.SelectAction(action);
        }
        else
        {
            base.Select();
        }
    }

    private void ExecuteAction(Player player, PlayableUI playerUI, PlayableUI weaponUI)
    {
        action.PlayAction(player, playerUI, weaponUI);
    }
}
