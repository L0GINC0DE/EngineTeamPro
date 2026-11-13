using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts.TAE.Enemies.FSM
{
    public enum EnemyType { Idle, Chase, Attack, Hit }
    public class EnemyStateMachine
    {
        private Dictionary<EnemyType, EnemyState> stateDict = new();

        public EnemyState CurrentState { get; private set; }

        public void AddState(EnemyType type, EnemyState state)
        {
            stateDict.Add(type, state);
        }

        public void ChangeState(EnemyType type)
        {
            CurrentState?.Exit();
            CurrentState = stateDict[type];
            CurrentState?.Enter();
        }

        public void Initialize(EnemyType type)
        {
            CurrentState = stateDict[type];
            CurrentState?.Enter();
        }
    }
}