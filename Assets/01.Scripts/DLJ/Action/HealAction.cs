using _01.Scripts.TAE.Managers;
using UnityEngine;

public class HealAction : PlayerAction
{
    public override bool PlayAction(Player owner, PlayableUI playerUI, PlayableUI weaponUI)
    {
        if (owner == null || !owner.CanAct) return false;

        var health = owner.BattleUnit.HealthCompo;
        health.Heal(Mathf.RoundToInt(health.MaxHealth * 0.5f));
        owner.AddDestructionStacks(1);
        TurnManager.Instance.EndTurn();
        return !TurnManager.Instance.IsPlayerTurn;
    }
}
