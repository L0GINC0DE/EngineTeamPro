using UnityEngine;

namespace _01.Scripts.TAE.Enemies.FSM
{
    public abstract class EnemyState
    {
        protected AbstractEnemy enemy;
        protected EnemyStateMachine stateMachine;
        protected int animBoolName;
        public EnemyState(AbstractEnemy owner, EnemyStateMachine _stateMachine, string _animBoolName)
        {
            enemy = owner;
            stateMachine = _stateMachine;
            animBoolName = Animator.StringToHash(_animBoolName);
        }

        public virtual void Enter()
        {
            enemy.AnimCompo.SetBool(animBoolName, true);
            enemy.EnemyAnimCompo.OnAnimationEnd += HandleAnimEnd;
            enemy.EnemyAnimCompo.OnAttack += HandleAttack;
        }

        public virtual void Exit()
        {
            enemy.AnimCompo.SetBool(animBoolName, false);
            enemy.EnemyAnimCompo.OnAnimationEnd -= HandleAnimEnd;
            enemy.EnemyAnimCompo.OnAttack -= HandleAttack;
        }

        public virtual void UpdateState()
        {

        }

        public virtual void HandleAnimEnd()
        {
        }

        public virtual void HandleAttack()
        {

        }
    }
}