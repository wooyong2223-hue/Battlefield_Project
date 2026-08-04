using UnityEngine;
using Battlefield.Framework.Core;
using Battlefield.Framework.Pool;
using Battlefield.Features.VFX;

namespace Battlefield.Features.Projectile
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class ProjectileBase : PoolableBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _speed = 300f;
        [SerializeField] private float _lifeTime = 5f;
        [SerializeField] private float _gravityScale = 0f;

        [Header("Layer")]
        [SerializeField] private LayerMask _hitMask = ~0;

        protected Rigidbody Rigidbody { get; private set; }
        protected Transform Owner { get; private set; }
        protected TeamType OwnerTeam { get; private set; } = TeamType.Neutral;
        protected Vector3 Velocity { get; private set; }

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.isKinematic = true;
        }

        protected virtual void FixedUpdate()
        {
            // 낙차
            Velocity += UnityEngine.Physics.gravity * _gravityScale * Time.fixedDeltaTime;
            float moveDistance = Velocity.magnitude * Time.fixedDeltaTime;
            Vector3 previousPosition = Rigidbody.position;
            Vector3 direction = Velocity.normalized;

            if (UnityEngine.Physics.Raycast(previousPosition, direction, out RaycastHit hit, moveDistance, _hitMask, QueryTriggerInteraction.Ignore))
            {
                if (HandleHit(hit)) return;
            }

            Rigidbody.MovePosition(previousPosition + direction * moveDistance);
        }

        public virtual void Initialize(ProjectileData data)
        {
            Rigidbody.position = transform.position;
            Rigidbody.rotation = transform.rotation;

            Owner = data.Owner;

            if (Owner != null && Owner.TryGetComponent<Team>(out Team team))
            {
                OwnerTeam = team.CurrentTeam;
            }

            Vector3 ownerVelocity = Vector3.zero;
            if (Owner != null && Owner.TryGetComponent(out Rigidbody ownerRigidBody))
            {
                ownerVelocity = ownerRigidBody.linearVelocity;
            }
            Velocity = transform.forward * _speed + ownerVelocity;

            CancelInvoke();
            Invoke(nameof(LifeExpired), _lifeTime);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            CancelInvoke();

            Velocity = Vector3.zero;
            Owner = null;
            OwnerTeam = TeamType.Neutral;
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            CancelInvoke();

            Velocity = Vector3.zero;
            Owner = null;
            OwnerTeam = TeamType.Neutral;
        }

        private bool HandleHit(RaycastHit hit)
        {
            Collider other = hit.collider;

            if (ShouldIgnore(other)) return false;
            if (IsAlly(other))
            {
                DestroyProjectile();
                return true;
            }

            HitEffectManager.Instance?.Play(
                other.sharedMaterial,
                hit.point,
                hit.normal,
                Velocity.normalized);
            OnHit(other);
            DestroyProjectile();
            return true;
        }

        protected virtual bool ShouldIgnore(Collider other)
        {
            return Owner != null &&
                   (other.transform == Owner || other.transform.IsChildOf(Owner));
        }

        protected virtual bool IsAlly(Collider other)
        {
            Team team = other.GetComponentInParent<Team>();

            return OwnerTeam != TeamType.Neutral &&
                   team != null &&
                   team.CurrentTeam == OwnerTeam;
        }

        protected abstract void OnHit(Collider other);
        protected virtual void LifeExpired()
        {
            DestroyProjectile();
        }

        protected virtual void DestroyProjectile()
        {
            ReturnToPool();
        }
    }
}
