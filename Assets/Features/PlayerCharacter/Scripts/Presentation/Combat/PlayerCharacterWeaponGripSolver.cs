using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    internal static class PlayerCharacterWeaponGripSolver
    {
        internal static Pose CalculateAimGrip(Quaternion aim, Vector3 eyePosition, Pose body, Vector3 bodyScale,
            Vector3 handOffset, Quaternion gripRotation, float recoil, float minimumDownwardReach,
            Vector3? mountedContact, out float reachScale)
        {
            float pitch = Mathf.DeltaAngle(0f, aim.eulerAngles.x);
            float steepness = Mathf.InverseLerp(45f, 80f, Mathf.Abs(pitch));
            Vector3 offset = new Vector3(handOffset.x, 0f,
                handOffset.z * Mathf.Lerp(1f, .75f, steepness) - recoil * .03f);

            // Use shoulder height as the pivot so looking down keeps the hands ahead of the body.
            Vector3 position = eyePosition + body.up * (handOffset.y * bodyScale.y) +
                               aim * Vector3.Scale(offset, bodyScale);
            if (pitch > 0f)
            {
                Vector3 forward = body.forward;
                float reach = Vector3.Dot(position - body.position, forward);
                position += forward * Mathf.Max(0f, minimumDownwardReach * bodyScale.z - reach);
            }

            Quaternion rotation = aim * Quaternion.Inverse(gripRotation);
            if (mountedContact.HasValue)
                position = mountedContact.Value + Vector3.up * .15f - aim * Vector3.forward * .35f;

            reachScale = Mathf.Lerp(1f, .92f, Mathf.InverseLerp(0f, 80f, Mathf.Abs(pitch)));
            return new Pose(position, rotation);
        }

        internal static Vector3 FitTwoHandGrip(Vector3 target, Vector3 rightShoulder, float rightReach,
            Vector3 leftShoulder, float leftReach, Vector3 supportOffset, int iterations)
        {
            // Both hands share a target constrained by the two arms' reach.
            Vector3 leftAnchor = leftShoulder - supportOffset;
            for (int i = 0; i < iterations; i++)
            {
                target = WithinReach(target, rightShoulder, rightReach);
                target = WithinReach(target, leftAnchor, leftReach);
            }
            return target;
        }

        private static Vector3 WithinReach(Vector3 target, Vector3 shoulder, float reach) =>
            shoulder + Vector3.ClampMagnitude(target - shoulder, reach);
    }
}
