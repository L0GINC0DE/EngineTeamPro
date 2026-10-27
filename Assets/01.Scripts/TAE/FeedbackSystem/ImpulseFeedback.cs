using Unity.Cinemachine;
using UnityEngine;

namespace _01.Scripts.KTH.FeedbackSystem
{
    public class ImpulseFeedback : AbstractFeedback
    {
        [SerializeField] private Vector3 velocity;
        [SerializeField] private CinemachineImpulseSource impulser;
        
        public override void CreateFeedback()
        {
            impulser.GenerateImpulseWithVelocity(velocity);
        }

        public override void FinishFeedback()
        { }
    }
}
