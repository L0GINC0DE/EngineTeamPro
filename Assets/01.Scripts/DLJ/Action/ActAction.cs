using UnityEngine;

public class ActAction : PlayerAction
{
    private readonly GameObject menu;
    public ActAction(GameObject menu) => this.menu = menu;

    public override bool PlayAction(Player owner, PlayableUI playerUI, PlayableUI weaponUI)
    {
        if (menu != null) menu.SetActive(true);
        return false;
    }
}
