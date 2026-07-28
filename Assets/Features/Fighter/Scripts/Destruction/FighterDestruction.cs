using UnityEngine;
using Battlefield.Core;
using Battlefield.Fighter.Camera;
using Battlefield.Fighter.Controller;
using Battlefield.Fighter.Movement;
using Battlefield.Weapon;

namespace Battlefield.Fighter
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody))]
    public class FighterDestruction : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [Header("Explosion")]
        [SerializeField] private ParticleSystem _explosionPrefab;
        [SerializeField, Min(0.01f)] private float _explosionScale = 3f;

        [Header("Damaged Appearance")]
        [SerializeField] private Color _damagedColor = new(0.15f, 0.15f, 0.15f, 1f);

        private Health _health;
        private Rigidbody _rigidbody;
        private Renderer[] _renderers;
        private float _previousHealth;
        private bool _isDestroyed;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _rigidbody = GetComponent<Rigidbody>();
            _renderers = GetComponentsInChildren<Renderer>(true);
            _previousHealth = _health.MaxHealth;
        }

        private void OnEnable()
        {
            _health.OnDamaged.AddListener(HandleDamaged);
            _health.OnDeath.AddListener(HandleDeath);
        }

        private void OnDisable()
        {
            _health.OnDamaged.RemoveListener(HandleDamaged);
            _health.OnDeath.RemoveListener(HandleDeath);
        }

        private void HandleDamaged()
        {
            float damage = _previousHealth - _health.CurrentHealth;
            _previousHealth = _health.CurrentHealth;

            Debug.Log(
                $"[Fighter Hit] damage={damage:0.##}, " +
                $"health={_health.CurrentHealth:0.##}/{_health.MaxHealth:0.##}",
                this);
        }

        private void HandleDeath()
        {
            if (_isDestroyed) return;

            _isDestroyed = true;

            Debug.Log("[Fighter Destroyed]", this);

            PlayExplosion();
            ApplyDamagedAppearance();
            DisableControls();
            BeginDestructionCamera();
            BeginFalling();
        }

        private void BeginDestructionCamera()
        {
            JetCamera jetCamera = FindFirstObjectByType<JetCamera>();
            jetCamera?.BeginDestructionView(transform);
        }

        private void PlayExplosion()
        {
            if (_explosionPrefab == null) return;

            ParticleSystem explosion = Instantiate(
                _explosionPrefab,
                transform.position,
                Quaternion.identity);
            explosion.transform.localScale = Vector3.one * _explosionScale;
            explosion.Play();
        }

        private void ApplyDamagedAppearance()
        {
            MaterialPropertyBlock propertyBlock = new();

            foreach (Renderer targetRenderer in _renderers)
            {
                if (targetRenderer is not MeshRenderer &&
                    targetRenderer is not SkinnedMeshRenderer)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, _damagedColor);
                propertyBlock.SetColor(ColorId, _damagedColor);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void DisableControls()
        {
            DisableComponent(GetComponent<KeyboardJetInput>());
            DisableComponent(GetComponent<JetMovement>());
            DisableComponent(GetComponent<JetRotation>());
            DisableComponent(GetComponent<JetPhysics>());

            foreach (WeaponController controller in
                     GetComponentsInChildren<WeaponController>(true))
            {
                controller.enabled = false;
            }

            foreach (WeaponBase weapon in GetComponentsInChildren<WeaponBase>(true))
            {
                weapon.enabled = false;
            }
        }

        private void BeginFalling()
        {
            float forwardSpeed = _rigidbody.linearVelocity.magnitude;

            _rigidbody.linearVelocity = transform.forward * forwardSpeed;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.useGravity = true;
        }

        private static void DisableComponent(Behaviour behaviour)
        {
            if (behaviour != null)
            {
                behaviour.enabled = false;
            }
        }
    }
}
