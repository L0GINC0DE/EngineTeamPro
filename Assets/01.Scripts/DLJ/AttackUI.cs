public class AttackUI : PlayableUI
{
    public override void Select()
    {
        if (PlayerUIManager.Instance == null) return;

        if (PlayerUIManager.Instance.CurrentStep == SelectionStep.Action)
        {
            PlayerUIManager.Instance.SelectAction(new AttackAction());
        }
        else
        {
            base.Select();
        }
    }

    public void Use() => Select();
}
