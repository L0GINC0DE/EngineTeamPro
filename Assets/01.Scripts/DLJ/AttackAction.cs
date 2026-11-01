using Agents;
using UnityEngine;

public class AttackAction : PlayerAction
{
    public override void PlayAction(Player owner, PlayableUI playerUI, PlayableUI weaponUI)
    {
        if (owner == null || owner.Enemy == null || playerUI == null || weaponUI == null) return;
        
        HealthModule enemyHealth = owner.Enemy.GetComponent<HealthModule>();

        if (enemyHealth == null || enemyHealth.IsDead) return;
        
        enemyHealth.TakeDamage(weaponUI.Damage);
    }
}
