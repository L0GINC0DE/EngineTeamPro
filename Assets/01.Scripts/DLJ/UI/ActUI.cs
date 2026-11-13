using UnityEngine;

public class ActUI : ActionUI
{
    [SerializeField] private GameObject actionMenu;
    protected override PlayerAction CreateAction() => new ActAction(actionMenu);

    public override void SetDestroyed(bool destroyed)
    {
        base.SetDestroyed(destroyed);
        if (destroyed && actionMenu != null) actionMenu.SetActive(false);
    }
}
