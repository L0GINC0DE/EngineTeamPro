using _01.Scripts.TAE.Enemies;
using _01.Scripts.TAE.Enemies.FSM;

namespace _01_Scripts.Enemies.FSM
{
    public class EnemyAttackState : EnemyState
    {
        public float jumpPower = 3f;
        public EnemyAttackState(AbstractEnemy owner, EnemyStateMachine _stateMachine, string _animBoolName) : base(owner, _stateMachine, _animBoolName)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            var dir = enemy.targetTrm.position - enemy.transform.position;
            dir.x *= 0.5f;
            dir.y = jumpPower;
            enemy.MoveCompo.SetMoveDir(dir);
        }

        public override void HandleAttack()
        {
            base.HandleAttack();
        }

        public override void HandleAnimEnd()
        {
            base.HandleAnimEnd();
            stateMachine.ChangeState(EnemyType.Chase);
        }
    }
}