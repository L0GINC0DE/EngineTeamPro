using _01.Scripts.KTH.Effects;
using Systems;
using UnityEngine;

namespace _01.Scripts.KTH.FeedbackSystem
{
    public class PlayEffectFeedback : AbstractFeedback
    {
        [SerializeField] protected PoolItemSO effect;
        
        public override void CreateFeedback()
        {
            EffectPlayer effectPlayer = PoolManager.Instance.Pop(effect.ItemName) as EffectPlayer;
            effectPlayer?.SetPositionAndPlay(transform.position);
        }

        public override void FinishFeedback()
        {
            
        }
    }
}