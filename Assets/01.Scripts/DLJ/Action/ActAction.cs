public class ActAction : PlayerAction
{
    private readonly DialogueActionMenu menu;
    public ActAction(DialogueActionMenu menu) => this.menu = menu;

    public override bool PlayAction(Player owner, PlayableUI playerUI, PlayableUI weaponUI)
    {
        if (menu != null) menu.Open();
        return false;
    }
}
