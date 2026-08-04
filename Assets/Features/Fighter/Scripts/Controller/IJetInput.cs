namespace Battlefield.Features.Fighter
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