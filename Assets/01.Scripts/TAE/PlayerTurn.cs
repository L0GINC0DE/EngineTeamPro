using System;
using _01.Scripts.TAE.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.TAE
{
    public class PlayerTurn : Turn
    {
        public override void Execute()
        {
            canExecute = true;
            Debug.Log("플레이어 턴");
            OnTurnEnd += TurnManager.Instance.NextTurn;
        }
        
        protected virtual void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame && canExecute)
                EndTurn();
        }
    }
}