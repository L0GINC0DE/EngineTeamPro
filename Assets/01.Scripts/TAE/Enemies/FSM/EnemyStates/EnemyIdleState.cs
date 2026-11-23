namespace _01.Scripts.TAE.Enemies.FSM.EnemyStates
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