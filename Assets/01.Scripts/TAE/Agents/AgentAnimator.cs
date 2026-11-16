using UnityEngine;

namespace _01.Scripts.TAE.Agents
{
    public class AgentAnimator : MonoBehaviour
    {
        public Animator Animator { get; private set; }
        protected virtual void Awake()
        {
            Animator = GetComponent<Animator>();
        }
        
        public void SetBoolean(int paramHash, bool value) => Animator.SetBool(paramHash, value);
        public void SetFloat(int paramHash, float value) => Animator.SetFloat(paramHash, value);
    }
}