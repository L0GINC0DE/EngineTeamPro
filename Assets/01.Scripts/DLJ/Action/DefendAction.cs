using _01.Scripts.TAE.Managers;

public class DefendAction : PlayerAction
{
    public override bool PlayAction(Player owner, PlayableUI playerUI, PlayableUI weaponUI)
    {
        if (owner == null || !owner.CanAct) return false;

        owner.DefenseStats.defendModifier = -0.5f;
        owner.DefenseStats.isDefending = true;
        TurnManager.Instance.EndTurn();
        return !TurnManager.Instance.IsPlayerTurn;
    }
}
