using System;
using _01.Scripts.TAE.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.TAE
{
    public abstract class Turn : MonoBehaviour
    {
        public event Action OnTurnEnd;
        
        public bool canExecute;
        
        public abstract void Execute();

        public virtual void EndTurn()
        {
            canExecute = false;
            OnTurnEnd?.Invoke();
            OnTurnEnd -= TurnManager.Instance.NextTurn;
        }
    }
}