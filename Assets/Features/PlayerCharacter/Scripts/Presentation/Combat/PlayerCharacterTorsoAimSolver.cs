using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    internal static class PlayerCharacterTorsoAimSolver
    {
        private const float ArmedTorsoPitchLimit = 3f;
        private const float LocomotionTorsoYawLimit = 3f;
        private const float LocomotionTorsoRollLimit = 3f;

        internal static bool TryCalculateArmedCorrection(Quaternion root, Quaternion chestRotation,
            Quaternion chestReference, Vector3 shoulderLine, float intendedPitch, float intendedRoll,
            out Quaternion correction)
        {
            Quaternion local = Quaternion.Inverse(root) * chestRotation * Quaternion.Inverse(chestReference);
            Vector3 angles = local.eulerAngles;
            float pitch = Mathf.DeltaAngle(0f, angles.x);
            float targetPitch = intendedPitch + CompressGaitAngle(pitch - intendedPitch, ArmedTorsoPitchLimit);
            Vector3 up = root * Vector3.up;
            Vector3 planar = Vector3.ProjectOnPlane(shoulderLine, up);
            if (planar.sqrMagnitude < .0001f)
            {
                correction = Quaternion.identity;
                return false;
            }
            float shoulderYaw = Vector3.SignedAngle(root * Vector3.right, planar, up);
            float shoulderRoll = Mathf.Atan2(Vector3.Dot(shoulderLine, up), planar.magnitude) * Mathf.Rad2Deg;
            float targetYaw = CompressGaitAngle(shoulderYaw, LocomotionTorsoYawLimit);
            float targetRoll = intendedRoll + CompressGaitAngle(shoulderRoll - intendedRoll, LocomotionTorsoRollLimit);
            Quaternion yawFrame = Quaternion.AngleAxis(targetYaw, up);
            Vector3 targetRight = Quaternion.AngleAxis(targetRoll, yawFrame * (root * Vector3.forward)) *
                (yawFrame * (root * Vector3.right));
            Quaternion align = Quaternion.FromToRotation(shoulderLine, targetRight);
            Quaternion anatomical = chestRotation * Quaternion.Inverse(chestReference);
            Vector3 alignedForward = align * (anatomical * Vector3.forward);
            Vector3 alignedUp = align * (anatomical * Vector3.up);
            // After aligning the shoulder line, twist around that line to reach the
            // desired aim pitch without undoing shoulder yaw/roll. Rodrigues' formula
            // gives A*cos(t)+B*sin(t)+C for the forward vector's vertical component.
            float projection = Vector3.Dot(targetRight, alignedForward);
            float c = Vector3.Dot(targetRight, up) * projection;
            float a = Vector3.Dot(alignedForward, up) - c;
            float b = Vector3.Dot(Vector3.Cross(targetRight, alignedForward), up);
            float amplitude = Mathf.Sqrt(a * a + b * b);
            Quaternion twist = Quaternion.identity;
            if (amplitude > .0001f)
            {
                float phase = Mathf.Atan2(b, a);
                float arc = Mathf.Acos(Mathf.Clamp((-Mathf.Sin(targetPitch * Mathf.Deg2Rad) - c) / amplitude, -1f, 1f));
                Quaternion first = Quaternion.AngleAxis((phase + arc) * Mathf.Rad2Deg, targetRight);
                Quaternion second = Quaternion.AngleAxis((phase - arc) * Mathf.Rad2Deg, targetRight);
                twist = Vector3.Dot(first * alignedUp, up) >= Vector3.Dot(second * alignedUp, up) ? first : second;
            }
            correction = twist * align;
            return true;
        }

        internal static Vector2 CalculateGaitCorrection(Quaternion root, Vector3 shoulderLine,
            float intendedPitch, float weight)
        {
            Vector3 up = root * Vector3.up;
            Vector3 shoulderRight = Quaternion.AngleAxis(intendedPitch, root * Vector3.right) *
                                    shoulderLine;
            Vector3 planar = Vector3.ProjectOnPlane(shoulderRight, up);
            if (planar.sqrMagnitude < .0001f) return Vector2.zero;
            float yaw = Vector3.SignedAngle(root * Vector3.right, planar, up);
            float roll = Mathf.Atan2(Vector3.Dot(shoulderRight, up), planar.magnitude) * Mathf.Rad2Deg;
            // Keep a small continuous gait motion on both axes. These are project
            // tuning targets, not joint angles measured from reference footage.
            float targetYaw = CompressGaitAngle(yaw, LocomotionTorsoYawLimit);
            float targetRoll = CompressGaitAngle(roll, LocomotionTorsoRollLimit);
            return new Vector2(targetYaw - yaw, targetRoll - roll) * weight;
        }

        private static float CompressGaitAngle(float angle, float limit) =>
            limit * (2f / Mathf.PI) * Mathf.Atan(angle / limit);

        internal static void CalculateGaitRotations(Quaternion root, float pitch, Vector2 correction,
            out Quaternion torso, out Quaternion gaze)
        {
            Quaternion pitchRotation = Quaternion.AngleAxis(pitch, root * Vector3.right);
            Quaternion stabilization = Quaternion.AngleAxis(correction.y, root * Vector3.forward) *
                                       Quaternion.AngleAxis(correction.x, root * Vector3.up);
            // Each of the spine and chest joints applies half of the correction.
            torso = Quaternion.Slerp(Quaternion.identity, stabilization * pitchRotation, .5f);
            // Cancel stabilization at the head while retaining the intended pitch.
            gaze = Quaternion.Inverse(pitchRotation) * Quaternion.Inverse(stabilization) * pitchRotation;
        }
    }
}
