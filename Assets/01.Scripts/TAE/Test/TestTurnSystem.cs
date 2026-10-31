using System.Collections.Generic;
using _01.Scripts.TAE.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.TAE.Test
{
    public class TestTurnSystem : MonoBehaviour
    {
        public List<BattleUnit> enemies = new();

        private void Start()
        {
            TurnManager manager = TurnManager.Instance;
            manager.AddTurn(enemies);
            manager.StartTurn();
            Debug.Log("테스트 전투: Space 키로 첫 번째 생존 적을 공격합니다.", this);
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.spaceKey.wasPressedThisFrame) return;

            TurnManager manager = TurnManager.Instance;
            if (manager.IsPlayerTurn)
                manager.PlayerAttack(manager.GetFirstLivingEnemy());
        }
    }
}
