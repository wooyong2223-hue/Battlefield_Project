using System;
using System.Collections.Generic;
using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.Targeting
{
    [Serializable]
    public sealed class AirTargetScanner
    {
        [SerializeField] private float _maximumDistance = 1000f;

        public AirTarget FindBestTarget(
            Transform origin,
            Transform owner,
            TeamType ownerTeam,
            AirTargetScreenArea screenArea)
        {
            AirTarget best = null;
            float bestCenterDistance = float.PositiveInfinity;
            float bestDistance = float.PositiveInfinity;
            IReadOnlyList<AirTarget> targets = AirTarget.ActiveTargets;

            for (int i = 0; i < targets.Count; i++)
            {
                AirTarget target = targets[i];
                if (!IsHostile(target, owner, ownerTeam)) continue;

                Vector3 offset = target.AimPosition - origin.position;
                float distance = offset.magnitude;
                if (distance <= Mathf.Epsilon || distance > Mathf.Max(0f, _maximumDistance)) continue;

                if (screenArea == null ||
                    !screenArea.TryGetCenterDistance(target.AimPosition, out float centerDistance) ||
                    centerDistance > bestCenterDistance ||
                    (Mathf.Approximately(centerDistance, bestCenterDistance) && distance >= bestDistance)) continue;

                best = target;
                bestCenterDistance = centerDistance;
                bestDistance = distance;
            }

            return best;
        }

        private static bool IsHostile(AirTarget target, Transform owner, TeamType ownerTeam)
        {
            if (target == null || !target.IsAvailable || target.transform == owner || target.transform.IsChildOf(owner)) return false;
            return ownerTeam != TeamType.Neutral && target.Team != TeamType.Neutral && target.Team != ownerTeam;
        }
    }
}
