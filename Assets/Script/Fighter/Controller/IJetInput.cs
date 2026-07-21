using UnityEngine;

namespace Battlefield.Fighter.Controller
{
    public interface IJetInput
    {
        // Controller
        float Throttle { get; }
        float Pitch { get; }
        float Roll { get; }
        float Yaw { get; }

        // Camera
        bool ChangeCamera { get; }
        bool RearView { get; }

        // Weapon
        bool FireWeapon { get; }
    }
}