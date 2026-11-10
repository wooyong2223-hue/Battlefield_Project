using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [DefaultExecutionOrder(-55)]
    public sealed class PlayerCharacterRecoil : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterController _combat;
        [SerializeField] private PlayerCharacterView _view;
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private PlayerCharacterWeaponMount _mount;
        [SerializeField, Min(0f)] private float _pitchPerShot = 1.2f;
        [SerializeField, Min(0f)] private float _yawPerShot = .2f;
        [SerializeField, Min(0f)] private float _maximumPitch = 6f;
        [SerializeField, Min(0f)] private float _recoverySpeed = 8f;
        private Vector2 _kick;
        private int _sequence;
        private float _lastShot = float.NegativeInfinity;
        public Vector2 Offset => _kick;
        private void OnEnable() { if (_combat != null) _combat.Fired += OnFired; }
        private void Update() => Tick(Time.deltaTime);
        public void Tick(float deltaTime)
        {
            if (!isActiveAndEnabled || deltaTime <= 0f) return;
            if (_combat == null || !_combat.CanUseCombat) { _kick = Vector2.zero; return; }
            _kick = Vector2.MoveTowards(_kick, Vector2.zero, _recoverySpeed * deltaTime);
        }
        private void OnFired()
        {
            if (Time.time - _lastShot > .5f) _sequence = 0;
            _lastShot = Time.time;
            float multiplier = _view != null && _view.IsAds ? .65f : 1f;
            if (_stance != null) multiplier *= _stance.IsProne ? .55f : _stance.IsCrouched ? .8f : 1f;
            if (_mount != null) multiplier *= _mount.RecoilMultiplier;
            _kick.x = Mathf.Min(_maximumPitch, _kick.x + _pitchPerShot * multiplier);
            _kick.y = Mathf.Clamp(_kick.y + (++_sequence % 2 == 0 ? -1f : 1f) * _yawPerShot * multiplier, -_maximumPitch, _maximumPitch);
        }
        private void OnDisable()
        {
            if (_combat != null) _combat.Fired -= OnFired;
            _kick = Vector2.zero; _sequence = 0; _lastShot = float.NegativeInfinity;
        }
    }
}
