using UnityEngine;
using Battlefield.Framework.Pool;

namespace Battlefield.Features.VFX
{
    public class HitEffectPool : ObjectPool<HitEffect>
    {
        [SerializeField] private ParticleSystem _effectPrefab;

        protected override HitEffect Create()
        {
            ParticleSystem particle = Instantiate(_effectPrefab, transform);

            foreach (CFX_AutoDestructShuriken autoDestruct in
                     particle.GetComponentsInChildren<CFX_AutoDestructShuriken>(true))
            {
                autoDestruct.StopAllCoroutines();
                autoDestruct.enabled = false;
            }

            HitEffect effect = particle.GetComponent<HitEffect>();
            if (effect == null)
            {
                effect = particle.gameObject.AddComponent<HitEffect>();
            }

            return Register(effect);
        }
    }
}
