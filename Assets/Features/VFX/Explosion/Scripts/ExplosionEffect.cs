using Battlefield.Framework.Particle;
using UnityEngine;

namespace Battlefield.Features.VFX
{
    public sealed class ExplosionEffect : ParticlePlayer
    {
        public void Play()
        {
            CancelInvoke();
            PlayParticles();
            Invoke(nameof(ReturnEffect), GetMaxDuration());
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            CancelInvoke();
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            CancelInvoke();
            StopParticles();
        }

        private void ReturnEffect()
        {
            ReturnToPool();
        }
    }
}
