using UnityEngine;
using Battlefield.Core;

namespace Battlefield.Projectile
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class ProjectileBase : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _speed = 300f;
        [SerializeField] private float _lifeTime = 5f;

        [Header("Layer")]
        [SerializeField] private LayerMask _hitMask = ~0;

        protected Rigidbody Rigidbody { get; private set; }
        protected Transform Owner { get; private set; }
        protected TeamType OwnerTeam { get; private set; } = TeamType.Neutral;
        protected float CurrentSpeed { get; private set; }


        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.isKinematic = true;
            CurrentSpeed = _speed;
        }

        protected virtual void OnEnable()
        {
            Destroy(gameObject, _lifeTime);
        }

        protected virtual void FixedUpdate()
        {
            float moveDistance = CurrentSpeed * Time.fixedDeltaTime;
            Vector3 previousPosition = transform.position;
            Vector3 direction = transform.forward;

            if (Physics.Raycast(previousPosition, direction, out RaycastHit hit, moveDistance, _hitMask, QueryTriggerInteraction.Ignore))
            {
                HandleHit(hit.collider);
                return;
            }

            Rigidbody.MovePosition(previousPosition + direction * moveDistance);
        }

        private void HandleHit(Collider other)
        {
            if (ShouldIgnore(other)) return;

            if (IsAlly(other))
            {
                DestroyProjectile();
                return;
            }

            OnHit(other);
            DestroyProjectile();
        }

        public virtual void Initialize(Transform owner)
        {
            Owner = owner;

            if (Owner != null && Owner.TryGetComponent<Team>(out var team))
            {
                OwnerTeam = team.CurrentTeam;
            }

            if (Owner != null && Owner.TryGetComponent<Rigidbody>(out var ownerRigidbody))
            {
                float forwardVelocity = Vector3.Dot(ownerRigidbody.linearVelocity, transform.forward);
                CurrentSpeed = _speed + Mathf.Max(forwardVelocity, 0f);
            }
        }

        protected virtual bool ShouldIgnore(Collider other)
        {
            return Owner != null && other.transform.root == Owner.root;
        }

        protected virtual bool IsAlly(Collider other)
        {
            return OwnerTeam != TeamType.Neutral &&
                   other.TryGetComponent<Team>(out var team) &&
                   team.CurrentTeam == OwnerTeam;
        }

        protected virtual void OnHit(Collider other)
        {

        }

        protected virtual void DestroyProjectile()
        {
            Destroy(gameObject);
        }
    }
}