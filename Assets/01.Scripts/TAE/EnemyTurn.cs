using _01.Scripts.TAE.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.TAE
{
    public class EnemyTurn : Turn
    {
        public override void Execute()
        {
            canExecute = true;
            Debug.Log("적 턴");
            OnTurnEnd += TurnManager.Instance.NextTurn;
        }
        
        private void Update()
        {
            if (canExecute) EndTurn();
        }
    }
}