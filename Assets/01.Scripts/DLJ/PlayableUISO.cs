using UnityEngine;

public enum UIType
{
    Attack,
    Defend,
    Act,
    Heal,
    HealthBar,
    Setting,
    TextBar
}

[CreateAssetMenu(fileName = "PlayableUISO", menuName = "Scriptable Objects/PlayableUISO")]
public class PlayableUISO : ScriptableObject
{
    [Header("UI")]
    public UIType uiType;
    
    [Header("Stats")]
    public int damage;
    public int defend;
    public int utility;
}
