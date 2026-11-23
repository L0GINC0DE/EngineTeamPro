using _01.Scripts.TAE;
using _01.Scripts.TAE.Agents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextBarUI : ActionUI
{
    private enum Message
    {
        Status,
        Attack,
        Stats
    }

    [SerializeField] private GameObject content;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private CanvasGroup frameVisibility;
    [SerializeField] private GameObject equippedFrame;
    [SerializeField] private TMP_Text equippedText;

    [Header("적의 다음 공격")]
    [SerializeField, TextArea] private string nextAttack;
    [SerializeField, Min(0.1f)] private float attackPreviewSeconds = 2f;
    [SerializeField, Min(0.1f)] private float attackReminderSeconds = 3f;

    [Header("현재 상태")]
    [SerializeField, TextArea] private string status;
    [SerializeField] private string bodyStep;
    [SerializeField] private string weaponStep;
    [SerializeField] private string actionStep;

    [Header("포커스된 버튼의 SO 스탯")]
    [SerializeField, TextArea] private string focusStats;

    private (string template, string step, int hp, int maxHp, int stacks)? lastStatus;
    private (BattleUnit enemy, string name, string template) lastAttack;
    private PlayableUI lastButton;
    private (string format, int attack, int defend, int utility) lastStats;
    private (BattleUnit enemy, string text) attackOverride;
    private Message message;
    private float nextMessageAt;
    private Image icon;

    internal string Text
    {
        get => dialogueText != null ? dialogueText.text : string.Empty;
        set
        {
            if (dialogueText != null) dialogueText.text = value;
            if (equippedText != null) equippedText.text = value;
        }
    }

    protected override PlayerAction CreateAction() => new TextBarAction(this);

    private void Awake()
    {
        icon = GetComponent<Image>();
        UpdateFrameVisibility();
    }

    public void Refresh(Player player, BattleUnit nextEnemy, SelectionStep step, PlayableUI focused)
    {
        HealthModule health = player.BattleUnit.HealthCompo;
        if (IsDestroyed || dialogueText == null || health == null) return;

        Show();
        string stepText = step switch
        {
            SelectionStep.Player => bodyStep,
            SelectionStep.Weapon => weaponStep,
            _ => actionStep
        };
        var playerStatus = (template: status, step: stepText, hp: health.CurrentHealth,
            maxHp: health.MaxHealth, stacks: player.DestructionStacks);
        var attack = (enemy: nextEnemy, name: nextEnemy != null ? nextEnemy.name : string.Empty,
            template: AttackText(nextEnemy));

        (string format, int attack, int defend, int utility) buttonStats = default;
        if (focused != null && focused.UISo != null)
            buttonStats = (focusStats, focused.Attack, focused.Defend, focused.Utility);

        bool attackChanged = lastStatus == null || lastAttack != attack;
        bool statusChanged = lastStatus != playerStatus;
        bool focusChanged = lastButton != focused;
        bool statsChanged = lastStats != buttonStats;
        lastStatus = playerStatus;
        lastAttack = attack;
        lastButton = focused;
        lastStats = buttonStats;

        bool hasAttack = !string.IsNullOrWhiteSpace(attack.template);
        float now = Time.unscaledTime;
        float seconds = attackReminderSeconds;

        if (attackChanged && hasAttack)
        {
            message = Message.Attack;
            seconds = attackPreviewSeconds;
        }
        else if (statusChanged)
            message = Message.Status;
        else if (focusChanged || (message == Message.Stats && statsChanged))
            message = string.IsNullOrWhiteSpace(buttonStats.format) ? Message.Status : Message.Stats;
        else if (message == Message.Attack && (!hasAttack || now >= nextMessageAt))
            message = Message.Status;
        else if (message != Message.Attack && hasAttack && now >= nextMessageAt)
        {
            message = Message.Attack;
            seconds = float.PositiveInfinity;
        }
        else return;

        nextMessageAt = now + seconds;
        Text = message switch
        {
            Message.Attack => attack.template.Replace("{enemy}", attack.name),
            Message.Stats => buttonStats.format.Replace("{attack}", buttonStats.attack.ToString())
                .Replace("{defend}", buttonStats.defend.ToString())
                .Replace("{utility}", buttonStats.utility.ToString()),
            _ => (playerStatus.template ?? string.Empty).Replace("{hp}", playerStatus.hp.ToString())
                .Replace("{maxHp}", playerStatus.maxHp.ToString())
                .Replace("{step}", playerStatus.step ?? string.Empty)
                .Replace("{stacks}", playerStatus.stacks.ToString())
        };
    }

    private string AttackText(BattleUnit enemy)
    {
        if (enemy == null) return null;
        if (enemy == attackOverride.enemy && attackOverride.text != null) return attackOverride.text;
        return nextAttack;
    }

    public void EndTurn() => lastStatus = null;

    public void SetEnemyIntent(BattleUnit enemy, string text) => attackOverride = (enemy, text);

    private void OnTransformParentChanged() => UpdateFrameVisibility();

    internal void ResumeMessages()
    {
        if (message != Message.Attack)
            nextMessageAt = Time.unscaledTime + attackReminderSeconds;
    }

    private void UpdateFrameVisibility()
    {
        bool equipped = frameVisibility != null && transform.parent != frameVisibility.transform.parent;
        bool showEquipped = equipped && equippedFrame != null && !IsDestroyed;
        if (frameVisibility != null) frameVisibility.alpha = showEquipped ? 0f : 1f;
        if (equippedFrame != null && equippedFrame.activeSelf != showEquipped)
            equippedFrame.SetActive(showEquipped);
        if (icon != null) icon.enabled = !IsDestroyed && !showEquipped;
    }

    public void Show()
    {
        bool visible = !IsDestroyed;
        if (content != null && content.activeSelf != visible) content.SetActive(visible);
    }

    public override void SetDestroyed(bool destroyed)
    {
        base.SetDestroyed(destroyed);
        UpdateFrameVisibility();
        Show();
        if (!destroyed) return;

        EndTurn();
        Text = string.Empty;
    }
}
