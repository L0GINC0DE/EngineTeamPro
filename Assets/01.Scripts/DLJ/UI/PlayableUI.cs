using UnityEngine;

public abstract class PlayableUI : MonoBehaviour
{
    public PlayableUISO UISo;
    public int Attack => UISo != null ? UISo.damage : 0;
    public int Defend => UISo != null ? UISo.defend : 0;
    public int Utility => UISo != null ? UISo.utility : 0;
    public bool IsOnCooldown { get; set; }
    public bool IsDestroyed { get; private set; }
    public bool CanEquip => !IsDestroyed && !IsOnCooldown;

    public virtual void SetDestroyed(bool destroyed)
    {
        IsDestroyed = destroyed;
    }

    public virtual void Select()
    {
        if (CanEquip) PlayerUIManager.Instance?.SelectPlayableUI(this);
    }
}
