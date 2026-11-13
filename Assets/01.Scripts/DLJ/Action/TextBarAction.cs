public class TextBarAction : PlayerAction
{
    private readonly TextBarUI ui;
    public TextBarAction(TextBarUI ui) => this.ui = ui;

    public override bool PlayAction(Player owner, PlayableUI playerUI, PlayableUI weaponUI)
    {
        if (ui != null) ui.Show();
        return false;
    }
}
