using System;
using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.Weapon
{
    public sealed class MeleeAttack : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _damage = 20f;
        [SerializeField, Min(.01f)] private float _range = 2.8f;
        [SerializeField, Min(.01f)] private float _radius = .45f;
        [SerializeField, Min(0f)] private float _windup = .2f;
        [SerializeField, Min(.01f)] private float _recovery = .6f;
        [SerializeField] private LayerMask _hitMask = ~0;
        private readonly RaycastHit[] _hits = new RaycastHit[64];
        private readonly Collider[] _overlaps = new Collider[64];
        private Transform _owner;
        private Vector3 _localOrigin, _localDirection;
        private float _impactAt, _readyAt;
        private bool _pending;
        public bool IsAttacking { get; private set; }
        public float Duration => _windup + _recovery;
        public float Windup => _windup;
        public event Action<Collider> Hit;

        public bool TryAttack(Transform owner, Vector3 origin, Vector3 direction)
        {
            if (!isActiveAndEnabled || owner == null || IsAttacking || Time.time < _readyAt ||
                direction.sqrMagnitude < .0001f) return false;
            Health health = owner.GetComponent<Health>();
            if (health != null && health.IsDead) return false;
            _owner = owner;
            _localOrigin = owner.InverseTransformPoint(origin);
            _localDirection = owner.InverseTransformDirection(direction.normalized);
            _impactAt = Time.time + _windup;
            _readyAt = _impactAt + _recovery;
            _pending = IsAttacking = true;
            return true;
        }
        private void Update()
        {
            if (!IsAttacking) return;
            if (_owner == null)
            {
                Cancel();
                return;
            }
            Health health = _owner.GetComponent<Health>();
            if (health != null && health.IsDead)
            {
                Cancel();
                return;
            }
            if (_pending && Time.time >= _impactAt)
            {
                _pending = false;
                Resolve();
            }
            if (Time.time >= _readyAt)
            {
                IsAttacking = false;
                _owner = null;
            }
        }
        private void Resolve()
        {
            Physics.SyncTransforms();
            Vector3 origin = _owner.TransformPoint(_localOrigin);
            Vector3 direction = _owner.TransformDirection(_localDirection).normalized;
            Collider nearest = null;
            float distance = float.PositiveInfinity;
            int overlaps = Physics.OverlapSphereNonAlloc(origin, _radius, _overlaps, _hitMask, QueryTriggerInteraction.Ignore);
            // A saturated query cannot establish the nearest obstruction reliably.
            if (overlaps == _overlaps.Length) return;
            for (int i = 0; i < overlaps; i++)
            {
                if (IsOwner(_overlaps[i])) continue;
                nearest = _overlaps[i];
                distance = 0f;
                break;
            }
            int count = Physics.SphereCastNonAlloc(origin, _radius, direction, _hits, _range, _hitMask, QueryTriggerInteraction.Ignore);
            if (count == _hits.Length) return;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _hits[i];
                if (!IsOwner(hit.collider) && hit.distance < distance)
                {
                    nearest = hit.collider;
                    distance = hit.distance;
                }
            }
            if (nearest == null) return;
            Team ownerTeam = _owner.GetComponentInParent<Team>();
            Team targetTeam = nearest.GetComponentInParent<Team>();
            if (ownerTeam != null && ownerTeam.CurrentTeam != TeamType.Neutral && targetTeam != null &&
                targetTeam.CurrentTeam == ownerTeam.CurrentTeam) return;
            nearest.GetComponentInParent<IDamageable>()?.TakeDamage(_damage);
            Hit?.Invoke(nearest);
        }
        private bool IsOwner(Collider collider) => collider.transform == _owner || collider.transform.IsChildOf(_owner);
        public void Cancel()
        {
            IsAttacking = false;
            _pending = false;
            _owner = null;
        }
        private void OnDisable() => Cancel();
    }
}
