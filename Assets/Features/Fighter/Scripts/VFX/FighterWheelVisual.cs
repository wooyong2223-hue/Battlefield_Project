using UnityEngine;

namespace Battlefield.Features.Fighter
{
    public sealed class FighterWheelVisual : MonoBehaviour
    {
        private static readonly Quaternion CylinderAxleRotation = Quaternion.Euler(0f, 0f, 90f);

        [SerializeField] private WheelCollider _wheel;

        private void LateUpdate()
        {
            if (_wheel == null) return;

            _wheel.GetWorldPose(out Vector3 position, out Quaternion rotation);
            transform.SetPositionAndRotation(position, rotation * CylinderAxleRotation);
        }
    }
}
