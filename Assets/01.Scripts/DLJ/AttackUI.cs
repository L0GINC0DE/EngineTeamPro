using UnityEngine;
using _01.Scripts.TAE;
using _01.Scripts.TAE.Managers;

public class AttackUI : PlayableUI
{
    [SerializeField] private BattleUnit target;

    public override void Use()
    {
        TurnManager manager = TurnManager.Instance;
        manager.PlayerAttack(target != null ? target : manager.GetFirstLivingEnemy());
    }
}
