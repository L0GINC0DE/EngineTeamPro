public abstract class ActionUI : PlayableUI
{
    protected virtual bool CanUse => !IsDestroyed;
    protected abstract PlayerAction CreateAction();

    public override void Select()
    {
        if (PlayerUIManager.Instance?.CurrentStep == SelectionStep.Action)
        {
            if (CanUse) PlayerUIManager.Instance.SelectAction(CreateAction());
        }
        else
            base.Select();
    }

    public void Use()
    {
        //if (CanUseOriginal) PlayerUIManager.Instance?.UseAction(CreateAction());
    }
}
