using UnityEngine;

public class AttackUI : PlayableUI
{
    private PlayerAction action = new AttackAction();

    public override void Select()
    {
        base.Select();
    }

    private void ExecuteAction(PlayableUI player, PlayableUI weapon)
    {
        action.PlayAction(player, weapon);
    }
}
