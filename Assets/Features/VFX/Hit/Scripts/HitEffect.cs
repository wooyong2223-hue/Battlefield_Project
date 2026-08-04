using UnityEngine;
using Battlefield.Framework.Particle;

namespace Battlefield.Features.VFX
{
    public class HitEffect : ParticlePlayer
    {
        public void Play(float duration)
        {
            CancelInvoke();
            PlayParticles();

            Invoke(nameof(ReturnEffect), duration);
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
