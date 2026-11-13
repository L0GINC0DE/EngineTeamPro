using _01_Scripts.Enemies.FSM;
using _01.Scripts.TAE.Enemies.FSM;
using UnityEngine;

namespace _01.Scripts.TAE.Enemies
{
    public class TestEnemy : AbstractEnemy
    {
        private EnemyStateMachine stateMachine;

        protected override void Awake()
        {
            base.Awake();

            stateMachine = new();

            stateMachine.AddState(EnemyType.Idle, new EnemyIdleState(this, stateMachine, "Idle"));
            stateMachine.AddState(EnemyType.Chase, new EnemyChaseState(this, stateMachine, "Chase"));
            
            stateMachine.Initialize(EnemyType.Idle);
        }

        private void Update()
        {
            stateMachine.CurrentState.UpdateState();
        }
    }
}