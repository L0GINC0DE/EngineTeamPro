using UnityEngine;

public readonly struct ActionSelection
{
    private PlayableUI Player { get; }
    private PlayableUI Weapon { get; }
    private PlayableUI Action { get; }

    public ActionSelection(
        PlayableUI player,
        PlayableUI weapon,
        PlayableUI action)
    {
        Player =  player;
        Weapon = weapon;
        Action = action;
    }
}
