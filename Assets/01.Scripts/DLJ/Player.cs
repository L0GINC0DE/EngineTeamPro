using _01.Scripts.TAE;
using _01.Scripts.TAE.Managers;
using UnityEngine;

[RequireComponent(typeof(BattleUnit), typeof(KTH_DefenseStats))]
public class Player : MonoBehaviour
{
    public BattleUnit BattleUnit { get; private set; }
    public PlayerAttack AttackCompo { get; private set; }
    public KTH_DefenseStats DefenseStats { get; private set; }
    public PlayableUI BodyUI { get; private set; }
    public PlayableUI WeaponUI { get; private set; }
    public int BodyUtility => BodyUI != null ? BodyUI.Utility : 0;
    public int DestructionStacks { get; private set; }
    private TurnManager turnManager;
    public bool CanAct => isActiveAndEnabled && BattleUnit != null && BattleUnit.IsAlive
        && turnManager != null && turnManager.isActiveAndEnabled && !turnManager.BattleEnded
        && turnManager.IsPlayerTurn && turnManager.CurrentUnit == BattleUnit;

    private void Awake()
    {
        BattleUnit = GetComponent<BattleUnit>();
        AttackCompo = GetComponentInChildren<PlayerAttack>(true);
        DefenseStats = GetComponent<KTH_DefenseStats>();
        if (DefenseStats == null) DefenseStats = gameObject.AddComponent<KTH_DefenseStats>();
        turnManager = TurnManager.Instance;
    }

    private void Update()
    {
        if (turnManager != null && (CanAct || turnManager.BattleEnded)) DefenseStats.isDefending = false;
    }

    private void OnDisable()
    {
        if (DefenseStats != null) DefenseStats.isDefending = false;
    }

    public void Equip(PlayableUI body, PlayableUI weapon)
    {
        BodyUI = body;
        WeaponUI = weapon;
        DefenseStats.bodyDefense = body != null ? body.Defend : 0;
        DefenseStats.weaponDefense = weapon != null ? weapon.Defend : 0;
    }

    public void AddDestructionStacks(int count) => DestructionStacks += Mathf.Max(0, count);
}
