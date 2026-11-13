using _01.Scripts.TAE.Enemies;
using _01.Scripts.TAE.Enemies.FSM;
using UnityEngine;

namespace _01_Scripts.Enemies.FSM
{
    public class EnemyHitState : EnemyState
    {
        public EnemyHitState(AbstractEnemy owner, EnemyStateMachine _stateMachine, string _animBoolName) : base(owner, _stateMachine, _animBoolName)
        {
        }

        public override void HandleAnimEnd()
        {
            base.HandleAnimEnd();
            enemy.targetTrm = null;
            enemy.MoveCompo.SetMoveDir(Vector2.zero);
            stateMachine.ChangeState(EnemyType.Idle);
        }
    }
}