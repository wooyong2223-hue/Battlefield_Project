using System.Collections;
using UnityEngine;

namespace Battlefield.Features.VFX
{
    public class MuzzleFlash : MonoBehaviour
    {
        [Header("Particle")]
        [SerializeField] private ParticleSystem[] _particleSystems;

        [Header("Light")]
        [SerializeField] private Light _muzzleLight;
        [SerializeField] private float _lightDuration = 0.03f;

        [Header("Settings")]
        [SerializeField] private bool _warmUpOnStart = true;
        [SerializeField] private float _effectDuration = 0.05f;

        private Coroutine _particleCoroutine;
        private Coroutine _lightCoroutine;

        private void Awake()
        {
            if (_particleSystems == null || _particleSystems.Length == 0)
            {
                _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            }

            if (_muzzleLight != null)
            {
                _muzzleLight.enabled = false;
            }
        }

        private void Start()
        {
            if (_warmUpOnStart)
            {
                StartCoroutine(WarmUp());
            }
        }

        public void Play()
        {
            foreach (ParticleSystem particle in _particleSystems)
            {
                particle.Clear();
                particle.Play();
            }

            if (_particleCoroutine != null)
            {
                StopCoroutine(_particleCoroutine);
            }

            _particleCoroutine = StartCoroutine(StopParticles());

            if (_muzzleLight != null)
            {
                if (_lightCoroutine != null)
                {
                    StopCoroutine(_lightCoroutine);
                }

                _lightCoroutine = StartCoroutine(FlashLight());
            }
        }

        private IEnumerator StopParticles()
        {
            yield return new WaitForSeconds(_effectDuration);

            foreach (ParticleSystem particle in _particleSystems)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private IEnumerator FlashLight()
        {
            _muzzleLight.enabled = true;

            yield return new WaitForSeconds(_lightDuration);

            _muzzleLight.enabled = false;
        }

        private IEnumerator WarmUp()
        {
            foreach (ParticleSystem particle in _particleSystems)
            {
                particle.Play();
            }

            yield return null;
            yield return null;

            foreach (ParticleSystem particle in _particleSystems)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particle.Clear();
            }
        }
    }
}