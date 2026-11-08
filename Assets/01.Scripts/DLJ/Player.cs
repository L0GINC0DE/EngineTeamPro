using _01.Scripts.TAE;
using UnityEngine;

[RequireComponent(typeof(BattleUnit))]
public class Player : MonoBehaviour
{
    public BattleUnit BattleUnit { get; private set; }
    public PlayerAttack AttackCompo { get; private set; }

    private void Awake()
    {
        BattleUnit = GetComponent<BattleUnit>();
        AttackCompo = GetComponentInChildren<PlayerAttack>(true);
    }
}
