using UnityEngine;
using Battlefield.Particle;

namespace Battlefield.VFX.Hit
{
    public class HitEffect : ParticlePlayer
    {
        public void Play()
        {
            PlayParticles();

            Destroy(gameObject, GetMaxDuration());
        }
    }
}