using UnityEngine;

public class SettingUI : ActionUI
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject destroyedPanel;
    protected override bool CanUse => true;
    protected override PlayerAction CreateAction() => new SettingAction(IsDestroyed ? destroyedPanel : settingsPanel);

    public override void SetDestroyed(bool destroyed)
    {
        base.SetDestroyed(destroyed);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (destroyedPanel != null) destroyedPanel.SetActive(false);
    }
}
