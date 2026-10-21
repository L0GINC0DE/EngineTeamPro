using System;
using System.Collections.Generic;
using _01.Scripts.TAE.Managers;
using UnityEngine;

namespace _01.Scripts.TAE.Test
{
    public class TestTurnSystem : MonoBehaviour
    {
        public List<EnemyTurn> enemies = new();

        private void Start()
        {
            TurnManager.Instance.AddTurn(enemies);
            TurnManager.Instance.StartTurn();
        }
    }
}