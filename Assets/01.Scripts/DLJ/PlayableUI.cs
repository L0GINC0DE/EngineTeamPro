using UnityEngine;

public abstract class PlayableUI : MonoBehaviour
{
    public PlayableUISO UISo;
    public int Damage { get;  private set; }
    public int Defend { get;  private set; }
    public int Utility { get;  private set; }

    protected virtual void Awake()
    {
        if (UISo != null)
        {
            Damage = UISo.damage;
            Defend = UISo.defend;
            Utility = UISo.utility;
        }

    }

    public virtual void Select()
    {
        PlayerUIManager.Instance.SelectPlayableUI(this);
    }
}
