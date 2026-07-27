using UnityEngine;

namespace Battlefield.Particle
{
    public class ParticlePlayer : MonoBehaviour
    {
        protected ParticleSystem[] _particles;

        protected virtual void Awake()
        {
            _particles = GetComponentsInChildren<ParticleSystem>(true);
        }

        protected void PlayParticles()
        {
            foreach (ParticleSystem particle in _particles)
            {
                particle.Clear(true);
                particle.Play(true);
            }
        }

        protected void StopParticles()
        {
            foreach (ParticleSystem particle in _particles)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        protected float GetMaxDuration()
        {
            float duration = 0f;

            foreach (ParticleSystem particle in _particles)
            {
                float time =
                    particle.main.duration +
                    particle.main.startLifetime.constantMax;

                duration = Mathf.Max(duration, time);
            }

            return duration;
        }
    }
}