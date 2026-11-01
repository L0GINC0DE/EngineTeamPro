using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace _01.Scripts.KTH.Effects
{
    public class ParticleLighting : MonoBehaviour
    {
        [SerializeField] private Light2D particleLight;
        [SerializeField] private ParticleSystem particle;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float maxIntensity = 5f;

        private float _particleDuration;
        private float _currentTimer;

        private void Awake()
        {
            _particleDuration = particle.main.duration;
        }

        private void OnEnable()
        {
            _currentTimer = 0;
        }

        private void Update()
        {
            float normalizedTimer = Mathf.Clamp01( _currentTimer / _particleDuration);
            particleLight.intensity = curve.Evaluate(normalizedTimer) * maxIntensity;
            _currentTimer += Time.deltaTime;
        }
    }
}