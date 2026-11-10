using System.Collections.Generic;
using Battlefield.Features.VFX;
using Battlefield.Features.Weapon;
using Battlefield.Framework.Core;
using UnityEngine;
namespace Battlefield.Features.Projectile
{
    // A bouncing, timed projectile has a different lifecycle from impact-despawn bullets.
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class GrenadeProjectile : MonoBehaviour
    {
        private Rigidbody _body;
        private Collider _collider;
        private Transform _owner;
        private TeamType _ownerTeam;
        private GrenadeDefinition _definition;
        private IExplosionEffectPlayer _effect;
        private float _remaining;
        private bool _armed, _detonated;
        public bool IsArmed => _armed && !_detonated;
        public Transform Owner => _owner;
        private void Awake() { _body = GetComponent<Rigidbody>(); _collider = GetComponent<Collider>(); }
        public bool Arm(Transform owner, Vector3 velocity, GrenadeDefinition definition, IExplosionEffectPlayer effect = null)
        {
            if (_armed || _detonated || definition == null || owner == null) return false;
            _owner = owner; _definition = definition; _effect = effect;
            var team = owner.GetComponentInParent<Team>(); _ownerTeam = team != null ? team.CurrentTeam : TeamType.Neutral;
            _remaining = definition.Fuse; _armed = true;
            foreach (var collider in owner.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(_collider, collider);
            _body.isKinematic = false; _body.useGravity = true;
            _body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _body.interpolation = RigidbodyInterpolation.Interpolate; _body.linearVelocity = velocity;
            _body.angularVelocity = new Vector3(3f, 5f, 2f);
            return true;
        }
        private void FixedUpdate()
        {
            if (!IsArmed) return;
            _remaining -= Time.fixedDeltaTime;
            if (_remaining <= 0f) Detonate();
        }
        public bool Detonate()
        {
            if (!IsArmed) return false;
            _detonated = true; // Guard re-entry through damage callbacks.
            var damage = new Dictionary<IDamageable, float>();
            Vector3 center = _body.position;
            foreach (var hit in Physics.OverlapSphere(center, _definition.Radius, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit == _collider || (_owner != null && hit.transform.IsChildOf(_owner))) continue;
                var team = hit.GetComponentInParent<Team>();
                if (_ownerTeam != TeamType.Neutral && team != null && team.CurrentTeam == _ownerTeam) continue;
                var target = hit.GetComponentInParent<IDamageable>();
                if (target == null) continue;
                Vector3 point = hit.ClosestPoint(center); Vector3 delta = point - center;
                bool blocked = false;
                if (delta.sqrMagnitude > .000001f)
                foreach (var barrier in Physics.RaycastAll(center, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                {
                    var collider = barrier.collider;
                    if (collider == _collider || (_owner != null && collider.transform.IsChildOf(_owner)) ||
                        ReferenceEquals(collider.GetComponentInParent<IDamageable>(), target)) continue;
                    blocked = true; break;
                }
                if (blocked) continue;
                float amount = _definition.Damage * Mathf.Clamp01(1f - delta.magnitude / _definition.Radius);
                if (!damage.TryGetValue(target, out float previous) || amount > previous) damage[target] = amount;
            }
            foreach (var pair in damage) pair.Key.TakeDamage(pair.Value);
            _effect?.Play(center);
            Destroy(gameObject); return true;
        }
        private void OnDisable() { _armed = false; }
    }
}
