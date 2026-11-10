using UnityEngine;
namespace Battlefield.Features.PlayerCharacter
{
    internal static class HumanoidArmSolver
    {
        internal static void Solve(Transform upper, Transform lower, Transform hand,
            Vector3 target, Quaternion rotation)
        {
            Vector3 shoulder = upper.position;
            float upperLength = Vector3.Distance(shoulder, lower.position);
            float lowerLength = Vector3.Distance(lower.position, hand.position);
            Vector3 direction = target - shoulder;
            float distance = direction.magnitude;
            if (distance < .0001f || upperLength < .0001f || lowerLength < .0001f) return;
            direction /= distance;
            distance = Mathf.Clamp(distance, Mathf.Abs(upperLength - lowerLength) + .0001f,
                upperLength + lowerLength - .0001f);
            Vector3 bend = Vector3.ProjectOnPlane(lower.position - shoulder, direction);
            if (bend.sqrMagnitude < .000001f) bend = Vector3.ProjectOnPlane(upper.up, direction);
            if (bend.sqrMagnitude < .000001f) bend = Vector3.ProjectOnPlane(upper.right, direction);
            float along = (upperLength * upperLength - lowerLength * lowerLength + distance * distance) / (2f * distance);
            Vector3 elbow = shoulder + direction * along + bend.normalized *
                Mathf.Sqrt(Mathf.Max(0f, upperLength * upperLength - along * along));
            upper.rotation = Quaternion.FromToRotation(lower.position - shoulder, elbow - shoulder) * upper.rotation;
            lower.rotation = Quaternion.FromToRotation(hand.position - lower.position,
                shoulder + direction * distance - lower.position) * lower.rotation;
            hand.rotation = rotation;
        }

    }
}
