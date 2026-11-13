using _01.Scripts.TAE.Enemies;
using _01.Scripts.TAE.Enemies.FSM;
using UnityEngine;

namespace _01_Scripts.Enemies.FSM
{
    public class EnemyIdleState : EnemyState
    {
        public EnemyIdleState(AbstractEnemy owner, EnemyStateMachine _stateMachine, string _animBoolName) : base(owner, _stateMachine, _animBoolName)
        {
        }

        public override void UpdateState()
        {
            var player = enemy.DetectPlayer();
            if (player != null)
            {
                enemy.targetTrm = player.transform;
                stateMachine.ChangeState(EnemyType.Chase);
            }
        }
    }
}