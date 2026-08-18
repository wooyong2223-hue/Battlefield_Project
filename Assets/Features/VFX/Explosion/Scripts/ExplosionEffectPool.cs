using Battlefield.Framework.Pool;
using UnityEngine;

namespace Battlefield.Features.VFX
{
    public interface IExplosionEffectPlayer
    {
        void Play(Vector3 position);
    }

    public sealed class ExplosionEffectPool : ObjectPool<ExplosionEffect>,
        IExplosionEffectPlayer
    {
        [SerializeField] private ExplosionEffect _effectPrefab;
        [SerializeField] private float _effectScale = 10f;

        protected override ExplosionEffect Create()
        {
            ExplosionEffect effect = Instantiate(_effectPrefab, transform);
            return Register(effect);
        }

        public void Play(Vector3 position)
        {
            ExplosionEffect effect = Get();
            effect.transform.SetPositionAndRotation(
                position,
                Quaternion.identity);
            effect.transform.localScale =
                Vector3.one * Mathf.Max(0.01f, _effectScale);
            effect.Play();
        }
    }
}
