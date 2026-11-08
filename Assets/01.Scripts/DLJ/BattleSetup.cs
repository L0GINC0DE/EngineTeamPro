using System.Collections.Generic;
using _01.Scripts.TAE;
using _01.Scripts.TAE.Managers;
using UnityEngine;

public class BattleSetup : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private List<BattleUnit> enemies = new();

    private void Start()
    {
        if (turnManager == null) return;

        turnManager.AddTurn(enemies);
        turnManager.StartTurn();
    }
}
