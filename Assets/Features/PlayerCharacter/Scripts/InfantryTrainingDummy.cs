using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    // Passive training actor created by the Editor test menu; no input or weapon authority.
    public sealed class InfantryTrainingDummy : MonoBehaviour
    {
        private PlayerCharacterMovement _motor;
        private PlayerCharacterPostureView _pose;
        private Animator _animator;
        public void Initialize(PlayerCharacterMovement motor, PlayerCharacterPostureView pose, Animator animator)
        { _motor = motor; _pose = pose; _animator = animator; }
        private void Update() { if (_motor != null) _motor.Tick(Vector2.zero, false, false, Time.deltaTime); }
        private void OnAnimatorIK(int layer) { if (layer == 0 && _pose != null) _pose.Apply(_animator); }
    }
}
