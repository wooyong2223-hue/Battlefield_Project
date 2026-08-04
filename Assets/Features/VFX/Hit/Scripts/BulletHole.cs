using UnityEngine;
using Battlefield.Framework.Pool;

namespace Battlefield.Features.VFX
{
    public class BulletHole : PoolableBehaviour
    {
        [SerializeField] private WFX_BulletHoleDecal _decal;
        private Quaternion _initialLocalRotation;

        private void Awake()
        {
            _initialLocalRotation = _decal.transform.localRotation;
        }

        public void Play(float duration)
        {
            CancelInvoke();
            Invoke(nameof(ReturnHole), duration);
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

            if (_decal != null)
            {
                _decal.StopAllCoroutines();
                _decal.transform.localRotation = _initialLocalRotation;
            }
        }

        private void ReturnHole()
        {
            ReturnToPool();
        }
    }
}
