using UnityEngine;

public class SettingAction : PlayerAction
{
    private readonly GameObject panel;
    public SettingAction(GameObject panel) => this.panel = panel;

    public override bool PlayAction(Player owner, PlayableUI playerUI, PlayableUI weaponUI)
    {
        if (panel != null) panel.SetActive(true);
        return false;
    }
}
