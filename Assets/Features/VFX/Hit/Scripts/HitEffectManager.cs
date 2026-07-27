using UnityEngine;

namespace Battlefield.VFX.Hit
{
    public class HitEffectManager : MonoBehaviour
    {
        public static HitEffectManager Instance { get; private set; }

        [SerializeField] private HitEffectDatabase _database;
        [SerializeField] private float _destroyDelay = 10f;
        [SerializeField, Min(0.01f)] private float _effectScale = 10f;

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

            ParticleSystem prefab = _database.GetEffect(material);

            if (prefab == null) return;

            Vector3 effectDirection = incomingDirection.sqrMagnitude > 0f
                ? -incomingDirection.normalized
                : normal;
            Quaternion rotation = Quaternion.LookRotation(effectDirection);

            ParticleSystem effect =
                Instantiate(prefab, position, rotation);

            effect.transform.localScale = Vector3.one * _effectScale;

            foreach (ParticleSystem particle in effect.GetComponentsInChildren<ParticleSystem>(true))
            {
                particle.Clear();
                particle.Play();
            }

            float maxDuration = 0f;

            foreach (ParticleSystem particle in effect.GetComponentsInChildren<ParticleSystem>(true))
            {
                float duration =
                    particle.main.duration +
                    particle.main.startLifetime.constantMax;

                maxDuration = Mathf.Max(maxDuration, duration);
            }
            Destroy(effect.gameObject, _destroyDelay);
        }

    }
}
