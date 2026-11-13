using TMPro;
using UnityEngine;

public class TextBarUI : ActionUI
{
    [SerializeField] private GameObject content;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject strongAttackWarning;
    protected override PlayerAction CreateAction() => new TextBarAction(this);

    public void SetText(string message, bool warnsStrongAttack = false)
    {
        if (IsDestroyed) return;
        if (dialogueText != null) dialogueText.text = message;
        if (strongAttackWarning != null) strongAttackWarning.SetActive(warnsStrongAttack);
        Show();
    }

    public void Show()
    {
        if (content != null) content.SetActive(!IsDestroyed);
    }

    public override void SetDestroyed(bool destroyed)
    {
        base.SetDestroyed(destroyed);
        if (!destroyed) return;
        if (content != null) content.SetActive(false);
        if (dialogueText != null) dialogueText.text = string.Empty;
        if (strongAttackWarning != null) strongAttackWarning.SetActive(false);
    }
}
