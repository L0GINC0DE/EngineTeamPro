public class AttackAction : PlayerAction
{
    public override bool PlayAction(Player owner, PlayableUI playerUI, PlayableUI weaponUI)
    {
        if (owner == null || !owner.CanAct || owner.AttackCompo == null) return false;
        if (!owner.AttackCompo.TryAttack()) return false;

        if (weaponUI != null && weaponUI.UISo != null)
        {
            PlayerAppearance appearance = owner.GetComponentInChildren<PlayerAppearance>();
            if (appearance != null) appearance.PlayWeaponAttack(weaponUI.UISo.uiType);
        }
        return true;
    }
}
