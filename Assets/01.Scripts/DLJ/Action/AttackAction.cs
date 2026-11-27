public class AttackAction : PlayerAction
{
    public override bool PlayAction(Player owner, PlayableUI playerUI, PlayableUI weaponUI)
    {
        if (owner == null || !owner.CanAct || owner.AttackCompo == null) return false;
        if (!owner.AttackCompo.TryAttack()) return false;

        PlayerAppearance appearance = owner.GetComponentInChildren<PlayerAppearance>();
        if (appearance != null)
            appearance.PlayAttack(weaponUI != null && weaponUI.UISo != null ? weaponUI.UISo.uiType : null);
        return true;
    }
}
