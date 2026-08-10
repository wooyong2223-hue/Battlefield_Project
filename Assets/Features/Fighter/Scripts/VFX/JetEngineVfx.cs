using System.Collections.Generic;
using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [RequireComponent(typeof(Afterburner))]
    public class JetEngineVfx : MonoBehaviour
    {
        [SerializeField] private GameObject _effectPrefab;
        [SerializeField] private Transform[] _engineAnchors;
        [SerializeField, Min(0f)] private float _normalSizeMultiplier = 0.6f;
        [SerializeField, Min(0f)] private float _afterburnerSizeMultiplier = 1.35f;
        [SerializeField, Min(0f)] private float _afterburnerEmissionMultiplier = 1.75f;
        [SerializeField, Min(0f)] private float _afterburnerLifetimeMultiplier = 1.2f;

        private readonly List<ParticleState> _particles = new();

        private Afterburner _afterburner;
        private IJetInput _input;
        private bool _isPlaying;

        private void Awake()
        {
            _afterburner = GetComponent<Afterburner>();
            _input = GetComponent<IJetInput>();

            if (_effectPrefab == null ||
                _engineAnchors == null ||
                _engineAnchors.Length == 0 ||
                _input == null)
            {
                Debug.LogError("Jet engine VFX configuration is missing.", this);
                enabled = false;
                return;
            }

            CreateEffects();
        }

        private void Update()
        {
            bool shouldPlay = _input.Throttle > 0f || _afterburner.IsActive;
            if (!shouldPlay)
            {
                SetPlaying(false);
                return;
            }

            float sizeMultiplier = _normalSizeMultiplier;
            float emissionMultiplier = 1f;
            float lifetimeMultiplier = 1f;

            if (_afterburner.IsActive)
            {
                sizeMultiplier *= _afterburnerSizeMultiplier;
                emissionMultiplier *= _afterburnerEmissionMultiplier;
                lifetimeMultiplier *= _afterburnerLifetimeMultiplier;
            }

            ApplyMultipliers(
                sizeMultiplier,
                emissionMultiplier,
                lifetimeMultiplier);
            SetPlaying(true);
        }

        private void OnDisable()
        {
            SetPlaying(false);
        }

        private void CreateEffects()
        {
            Transform prefabTransform = _effectPrefab.transform;

            foreach (Transform engineAnchor in _engineAnchors)
            {
                if (engineAnchor == null) continue;

                GameObject effect = Instantiate(_effectPrefab, engineAnchor);
                Transform effectTransform = effect.transform;
                effectTransform.localPosition = prefabTransform.localPosition;
                effectTransform.localRotation = prefabTransform.localRotation;
                effectTransform.localScale = prefabTransform.localScale;

                foreach (ParticleSystem particleSystem in
                         effect.GetComponentsInChildren<ParticleSystem>(true))
                {
                    particleSystem.Stop(
                        false,
                        ParticleSystemStopBehavior.StopEmittingAndClear);
                    _particles.Add(new ParticleState(particleSystem));
                }
            }
        }

        private void SetPlaying(bool playing)
        {
            if (_isPlaying == playing) return;

            _isPlaying = playing;

            foreach (ParticleState particle in _particles)
            {
                if (playing)
                {
                    particle.System.Play(false);
                }
                else
                {
                    particle.System.Stop(
                        false,
                        ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }

        private void ApplyMultipliers(
            float sizeMultiplier,
            float emissionMultiplier,
            float lifetimeMultiplier)
        {
            foreach (ParticleState particle in _particles)
            {
                ParticleSystem.MainModule main = particle.System.main;
                main.startSizeXMultiplier =
                    particle.StartSizeXMultiplier * sizeMultiplier;
                main.startSizeYMultiplier =
                    particle.StartSizeYMultiplier * sizeMultiplier;
                main.startSizeZMultiplier =
                    particle.StartSizeZMultiplier * sizeMultiplier;
                main.startLifetimeMultiplier =
                    particle.StartLifetimeMultiplier * lifetimeMultiplier;

                ParticleSystem.EmissionModule emission = particle.System.emission;
                emission.rateOverTimeMultiplier =
                    particle.RateOverTimeMultiplier * emissionMultiplier;
                emission.rateOverDistanceMultiplier =
                    particle.RateOverDistanceMultiplier * emissionMultiplier;
            }
        }

        private sealed class ParticleState
        {
            public ParticleState(ParticleSystem particleSystem)
            {
                System = particleSystem;

                ParticleSystem.MainModule main = particleSystem.main;
                StartSizeXMultiplier = main.startSizeXMultiplier;
                StartSizeYMultiplier = main.startSizeYMultiplier;
                StartSizeZMultiplier = main.startSizeZMultiplier;
                StartLifetimeMultiplier = main.startLifetimeMultiplier;

                ParticleSystem.EmissionModule emission = particleSystem.emission;
                RateOverTimeMultiplier = emission.rateOverTimeMultiplier;
                RateOverDistanceMultiplier =
                    emission.rateOverDistanceMultiplier;
            }

            public ParticleSystem System { get; }
            public float StartSizeXMultiplier { get; }
            public float StartSizeYMultiplier { get; }
            public float StartSizeZMultiplier { get; }
            public float StartLifetimeMultiplier { get; }
            public float RateOverTimeMultiplier { get; }
            public float RateOverDistanceMultiplier { get; }
        }
    }
}
