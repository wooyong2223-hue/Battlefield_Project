using UnityEngine;
using Battlefield.Framework.Pool;

namespace Battlefield.Features.VFX
{
    public class HitEffectPool : ObjectPool<HitEffect>
    {
        [SerializeField] private HitEffect _effectPrefab;

        protected override HitEffect Create()
        {
            HitEffect effect = Instantiate(_effectPrefab, transform);
            return Register(effect);
        }
    }
}
