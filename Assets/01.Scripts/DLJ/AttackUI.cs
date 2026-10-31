using UnityEngine;
using _01.Scripts.TAE;
using _01.Scripts.TAE.Managers;

public class AttackUI : PlayableUI
{
    [SerializeField] private BattleUnit target;

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

    public void Use()
    {
        TurnManager manager = TurnManager.Instance;
        manager.PlayerAttack(target != null ? target : manager.GetFirstLivingEnemy());
    }
}
