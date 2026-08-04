using UnityEngine;

namespace Battlefield.Features.VFX
{
    public class HitEffectManager : MonoBehaviour
    {
        public static HitEffectManager Instance { get; private set; }

        [SerializeField] private HitEffectDatabase _database;
        [SerializeField] private float _destroyDelay = 10f;
        [SerializeField, Min(0.01f)] private float _effectScale = 10f;
        [SerializeField] private float _bulletHoleDuration = 10f;
        [SerializeField, Min(0.01f)] private float _bulletHoleScale = 10f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Play(
            PhysicsMaterial material,
            Vector3 position,
            Vector3 normal,
            Vector3 incomingDirection)
        {
            if (_database == null || material == null) return;

            HitEffectDatabase.Entry entry = _database.GetEntry(material);

            if (entry == null) return;

            Vector3 effectDirection = incomingDirection.sqrMagnitude > 0f
                ? -incomingDirection.normalized
                : normal;
            Quaternion effectRotation = Quaternion.LookRotation(effectDirection);

            Vector3 surfaceDirection = normal.sqrMagnitude > 0f
                ? normal.normalized
                : effectDirection;
            Quaternion bulletHoleRotation = Quaternion.LookRotation(surfaceDirection);

            if (entry.EffectPool != null)
            {
                HitEffect effect = entry.EffectPool.Get();
                effect.transform.SetPositionAndRotation(position, effectRotation);
                effect.transform.localScale = Vector3.one * _effectScale;
                effect.Play(_destroyDelay);
            }

            if (entry.BulletHolePool != null)
            {
                BulletHole bulletHole = entry.BulletHolePool.Get();
                bulletHole.transform.SetPositionAndRotation(position, bulletHoleRotation);
                bulletHole.transform.localScale = Vector3.one * _bulletHoleScale;
                bulletHole.Play(_bulletHoleDuration);
            }
        }
    }
}
