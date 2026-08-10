using UnityEngine;

namespace Battlefield.Features.Fighter
{
    public interface IJetInput
    {
        // Controller
        float Throttle { get; }
        float Pitch { get; }
        float Roll { get; }
        float Yaw { get; }
        bool Afterburner { get; }

        // Camera
        bool ChangeCamera { get; }
        bool RearView { get; }
        Vector2 CameraLook { get; }
        float CameraDistanceDelta { get; }
        bool FreeLook { get; }
        bool Zoom { get; }

        // Weapon
        bool FireWeapon { get; }
    }
}
