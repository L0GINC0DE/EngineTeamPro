using UnityEngine;

public abstract class PlayableUI : MonoBehaviour
{
    public PlayableUISO UISo;
    public int Damage { get;  private set; }
    public int Defend { get;  private set; }
    public int Utility { get;  private set; }

    protected PlayableUI()
    {
        if (UISo != null)
        {
            Damage = UISo.damage;
            Defend = UISo.defend;
            Utility = UISo.utility;
        }

    }
    
    public abstract void Use();
}
