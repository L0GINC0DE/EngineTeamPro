using UnityEngine;

public class AttackUI : PlayableUI
{
    private PlayerAction action = new AttackAction();

    public override void Select()
    {
        base.Select();
        PlayerUIManager.Instance.SelectAction(action);
    }

    private void ExecuteAction(Player player, PlayableUI playerUI, PlayableUI weaponUI)
    {
        action.PlayAction(player, playerUI, weaponUI);
    }
}
