using Battlefield.Features.Projectile;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    internal sealed class PlayerCharacterShotPath
    {
        private readonly RaycastHit[] _aimHits = new RaycastHit[64];

        internal (Vector3 Origin, Vector3 Direction) Calculate(CharacterController body,
            IPlayerCharacterAimSource aimSource, Transform muzzle, float muzzleOffset, float aimDistance, LayerMask aimMask)
        {
            Transform owner = body.transform;
            Vector3 direction = owner.forward;
            Vector3 origin = body.ClosestPoint(owner.position + direction) +
                             direction * muzzleOffset;
            if (aimSource != null)
            {
                Ray aim = aimSource.GetAimRay();
                Vector3 aimPoint = aim.GetPoint(FindNearestSurface(aim, aimDistance, aimMask, owner));
                Vector3 anchor = body.bounds.center;
                Vector3 toTarget = aimPoint - anchor;
                direction = toTarget.sqrMagnitude > 0.0001f && Vector3.Dot(toTarget, aim.direction) > 0f
                    ? toTarget.normalized : aim.direction;
                Vector3 desiredMuzzle = muzzle != null ? muzzle.position :
                    body.ClosestPoint(anchor + direction * body.height) + direction * muzzleOffset;
                Vector3 muzzleDelta = desiredMuzzle - anchor;
                float muzzleDistance = muzzleDelta.magnitude;
                Vector3 muzzleDirection = muzzleDistance > .0001f ? muzzleDelta / muzzleDistance : direction;
                float surfaceDistance = FindNearestSurface(new Ray(anchor, muzzleDirection), muzzleDistance, aimMask, owner);
                // Do not spawn past cover that lies between the owner and the muzzle.
                origin = anchor + muzzleDirection * Mathf.Max(0f, surfaceDistance - 0.01f);
                Vector3 muzzleToTarget = aimPoint - origin;
                direction = muzzleToTarget.sqrMagnitude > .0001f && Vector3.Dot(muzzleToTarget, aim.direction) > 0f
                    ? muzzleToTarget.normalized : aim.direction;
            }

            return (origin, direction);
        }

        private float FindNearestSurface(Ray ray, float distance, LayerMask aimMask, Transform owner)
        {
            int count = UnityEngine.Physics.RaycastNonAlloc(
                ray, _aimHits, distance, aimMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _aimHits[i];
                if (hit.collider.transform.IsChildOf(owner) ||
                    hit.collider.GetComponentInParent<ProjectileBase>() != null) continue;
                distance = Mathf.Min(distance, hit.distance);
            }
            return distance;
        }
    }
}
