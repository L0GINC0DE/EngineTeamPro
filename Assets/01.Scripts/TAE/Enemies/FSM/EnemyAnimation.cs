using System;
using UnityEngine;

namespace _01.Scripts.TAE.Enemies.FSM
{
    public class EnemyAnimation : MonoBehaviour
    {
        public event Action OnAnimationEnd;
        public event Action OnAttack;

        public void AnimationEnd()
        {
            OnAnimationEnd?.Invoke();
        }

        public void Attack()
        {
            OnAttack?.Invoke();
        }
    }
}