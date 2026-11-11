using Battlefield.Features.Weapon;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    internal sealed class PlayerCharacterReloadGripReference
    {
        private Vector3 _eyeToTorso;
        private Quaternion _torsoRotation;
        private Vector3 _gripCorrection;
        private InfantryWeaponDefinition _calibratedWeapon;

        internal void CaptureTorso(Transform body, Vector3 eye, Transform torso)
        {
            _eyeToTorso = body.InverseTransformVector(eye - torso.position);
            _torsoRotation = Quaternion.Inverse(body.rotation) * torso.rotation;
        }

        internal void CaptureGrip(Transform body, InfantryWeaponDefinition weapon, Vector3 actual, Vector3 target)
        {
            _gripCorrection = body.InverseTransformVector(actual - target);
            _calibratedWeapon = weapon;
        }

        internal void Resolve(Transform body, Vector3 eye, Vector3 torsoScale, InfantryWeaponDefinition weapon,
            Vector3 position, Quaternion rotation, out Vector3 positionInTorso, out Quaternion rotationInTorso)
        {
            if (_calibratedWeapon == weapon) position += body.TransformVector(_gripCorrection);
            Vector3 torsoPosition = eye - body.TransformVector(_eyeToTorso);
            Quaternion torsoRotation = body.rotation * _torsoRotation;
            var reference = Matrix4x4.TRS(torsoPosition, torsoRotation, torsoScale);
            positionInTorso = reference.inverse.MultiplyPoint3x4(position);
            rotationInTorso = Quaternion.Inverse(torsoRotation) * rotation;
        }
    }
}
