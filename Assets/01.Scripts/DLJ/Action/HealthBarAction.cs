public class HealthBarAction : PlayerAction
{
    private readonly HealthBarUI ui;
    public HealthBarAction(HealthBarUI ui) => this.ui = ui;

    public override bool PlayAction(Player owner, PlayableUI playerUI, PlayableUI weaponUI)
    {
        if (ui != null) ui.Refresh();
        return false;
    }
}
