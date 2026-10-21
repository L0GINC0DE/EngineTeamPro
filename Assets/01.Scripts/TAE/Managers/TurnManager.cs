    using System;
    using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts.TAE.Managers
{
        public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance;
        
        private List<Turn> _turnCycle = new();
        public Turn CurrentTurn { get; private set; }
        
        [SerializeField] private PlayerTurn player;

        private int index;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            
            _turnCycle.Add(player);
        }

        public void StartTurn()
        {
            index = 0;
            CurrentTurn = _turnCycle[index];
            CurrentTurn.Execute();
        }

        public void AddTurn(List<EnemyTurn> enemies)
        {
            foreach (var e in enemies) _turnCycle.Add(e);
            index = _turnCycle.Count - 1;
        }

        public void NextTurn()
        {
            CurrentTurn = _turnCycle[NextTurnIndex()];
            CurrentTurn?.Execute();
        }

        private int NextTurnIndex()
        {
            if (index == _turnCycle.Count - 1)
            {
                index = 0;
                return 0;
            }

            index++;
            return index;
        }
    }
}