using _01.Scripts.TAE.Enemies;
using _01.Scripts.TAE.Enemies.FSM;
using UnityEngine;

namespace _01_Scripts.Enemies.FSM
{
    public class EnemyChaseState : EnemyState
    {
        public EnemyChaseState(AbstractEnemy owner, EnemyStateMachine _stateMachine, string _animBoolName) : base(owner, _stateMachine, _animBoolName)
        {
        }

        public override void UpdateState()
        {
            base.UpdateState();
            
            var dir = enemy.targetTrm.position - enemy.transform.position;
            enemy.MoveCompo.SetMoveDir(new Vector2(Mathf.Sign(dir.x), 0));
            enemy.Renderer.FlipController(dir.x);

            float distance = dir.magnitude;

            if (enemy.detectRange < distance)
            {
                enemy.targetTrm = null;
                enemy.MoveCompo.SetMoveDir(Vector2.zero);
                stateMachine.ChangeState(EnemyType.Idle);
                return;
            }

            //if (enemy.lastAttackTime + enemy.attackCoolTime > Time.time) return;
            //if (enemy.attackRange > distance)
            //{
            //    stateMachine.ChangeState(EnemyType.Attack);
            //}
        }
    }
}