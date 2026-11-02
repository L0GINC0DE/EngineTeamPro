using System.Collections;
using UnityEngine;

namespace _01.Scripts.KTH.FeedbackSystem
{
    public class BlinkFeedback : AbstractFeedback
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private float blinkDuration = 0.15f;

        private MaterialPropertyBlock _mpb;
        
        private readonly int BLINK_HASH = Shader.PropertyToID("_BlinkValue");
        private WaitForSeconds _waitForSeconds;
        private Coroutine _blinkCoroutine = null;
        
        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(_mpb);
            //해당 렌더러에 붙어있는 메티리얼을 기반으로 mbp를 채워준다.
            _waitForSeconds = new WaitForSeconds(blinkDuration);
        }

        public override void CreateFeedback()
        {
            _blinkCoroutine = StartCoroutine(BlinkCoroutine());
        }

        private IEnumerator BlinkCoroutine()
        {
            _mpb.Clear();
            _mpb.SetFloat(BLINK_HASH, 1f);
            targetRenderer.SetPropertyBlock(_mpb);
            yield return _waitForSeconds;
            _mpb.SetFloat(BLINK_HASH, 0f);
            targetRenderer.SetPropertyBlock(_mpb);
        }

        public override void FinishFeedback()
        {
            if(_blinkCoroutine != null)
                StopCoroutine(_blinkCoroutine);
        }
    }
}
