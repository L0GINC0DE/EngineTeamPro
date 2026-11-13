using _01.Scripts.TAE.Managers;

public class AttackAction : PlayerAction
{
    public override bool PlayAction(Player owner, PlayableUI playerUI, PlayableUI weaponUI)
    {
        TurnManager manager = TurnManager.Instance;
        if (owner == null || !owner.CanAct || owner.AttackCompo == null) return false;

        owner.AttackCompo.Attack();
        return !manager.IsPlayerTurn;
    }
}
