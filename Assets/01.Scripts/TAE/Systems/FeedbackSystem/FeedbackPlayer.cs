using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _01.Scripts.KTH.FeedbackSystem
{
    public class FeedbackPlayer : MonoBehaviour
    {
        private List<AbstractFeedback> _feedbackToPlay;

        private void Awake()
        {
            _feedbackToPlay = GetComponents<AbstractFeedback>().ToList();
        }
 
        public void PlayAllFeedback()
        {
            _feedbackToPlay.ForEach(feedback => feedback.CreateFeedback());
        }

        public void StopAllFeedback()
        {
            _feedbackToPlay.ForEach(feedback => feedback.FinishFeedback());
        }
    }
}