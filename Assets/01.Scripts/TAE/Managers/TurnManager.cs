using System.Collections.Generic;
using CoreLib;
using UnityEngine;

namespace _01.Scripts.TAE.Managers
{
    public class TurnManager : MonoSingleton<TurnManager>
    {
        [SerializeField] private BattleUnit player;

        private readonly List<BattleUnit> enemies = new();
        private bool isProcessing;

        public BattleUnit CurrentUnit { get; private set; }
        public bool BattleEnded { get; private set; }
        public bool IsPlayerTurn => CurrentUnit != null && CurrentUnit == player;

        private void LateUpdate()
        {
            if (CurrentUnit != null && !isProcessing) CheckBattleEnd();
        }

        public void AddTurn(IEnumerable<BattleUnit> units)
        {
            if (isProcessing) return;

            CurrentUnit = null;
            enemies.Clear();
            BattleEnded = false;

            if (units != null) enemies.AddRange(units);
        }

        public void StartTurn()
        {
            if (!isActiveAndEnabled || isProcessing) return;
            if (CurrentUnit != null || BattleEnded) return;
            if (player == null) return;
            BeginPlayerTurn();
        }

        public bool PlayerAttack(BattleUnit target)
        {
            if (!isActiveAndEnabled || isProcessing) return false;
            if (!IsPlayerTurn) return false;
            if (target == null || !enemies.Contains(target)) return false;

            isProcessing = true;
            bool executed = new AttackCommand(player, target).Execute();
            isProcessing = false;

            if (!executed) return false;
            Debug.Log($"플레이어가 적 [{target.name}]을 공격", target);
            EndTurn();
            return true;
        }


        public void EndTurn()
        {
            if (!isActiveAndEnabled || isProcessing) return;
            if (!IsPlayerTurn) return;

            isProcessing = true;
            enemies.ForEach(EnemyTurn);
            isProcessing = false;
            BeginPlayerTurn();
        }

        private void BeginPlayerTurn()
        {
            if (CheckBattleEnd()) return;
            CurrentUnit = player;
            Debug.Log("플레이어 턴", player);
        }

        private bool CheckBattleEnd()
        {
            if (BattleEnded) return true;
            bool playerAlive = IsAlive(player);
            if (playerAlive && GetFirstLivingEnemy() != null) return false;

            BattleEnded = true;
            CurrentUnit = null;
            Debug.Log(playerAlive ? "승리" : "패배", this);
            return true;
        }

        public BattleUnit GetFirstLivingEnemy() => enemies.Find(IsAlive);

        public void EnemyTurn(BattleUnit enemy)
        {
            if (CheckBattleEnd() || !IsAlive(enemy)) return;
            CurrentUnit = enemy;
            Debug.Log($"적 턴: {enemy.name}", enemy);
            new AttackCommand(enemy, player).Execute();
        }
        private static bool IsAlive(BattleUnit unit) => unit != null && unit.IsAlive;
    }
}
