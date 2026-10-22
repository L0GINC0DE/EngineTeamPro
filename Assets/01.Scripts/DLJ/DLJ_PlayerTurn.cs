using _01.Scripts.TAE;
using _01.Scripts.TAE.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

public class DLJ_PlayerTurn : Turn
{
    public override void Execute()
    {
        canExecute = true;
        Debug.Log("플레이어 턴");
        OnTurnEnd += TurnManager.Instance.NextTurn;
    }

    private void ExecuteEndTurn()
    {
        EndTurn();
    }
}
