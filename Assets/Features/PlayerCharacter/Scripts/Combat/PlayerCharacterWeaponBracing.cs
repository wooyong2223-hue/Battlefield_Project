using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [DefaultExecutionOrder(-30)]
    public sealed class PlayerCharacterWeaponBracing : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterCamera _camera;
        [SerializeField] private PlayerCharacterLookController _view;
        [SerializeField] private PlayerCharacterMovement _movement;
        [SerializeField] private PlayerCharacterCombatController _combat;
        [SerializeField] private PlayerCharacterActionPolicy _actions;
        [SerializeField, Min(0f)] private float _reach = 1.3f;
        [SerializeField] private float _recoilMultiplier = .4f;
        [SerializeField] private LayerMask _coverMask = ~0;
        private readonly RaycastHit[] _hits = new RaycastHit[32];
        private IPlayerCharacterTacticalInput _input;
        private Collider _cover;
        private Vector3 _localContact, _anchor;
        private Quaternion _aim;
        public bool IsMounted { get; private set; }
        public Vector3 Contact => _cover != null ? _cover.transform.TransformPoint(_localContact) : Vector3.zero;
        public float RecoilMultiplier => IsMounted ? Mathf.Clamp01(_recoilMultiplier) : 1f;
        private void Awake() => _input = GetComponent<IPlayerCharacterTacticalInput>();
        private void Update() => Tick(_input != null && _input.MountHeld);
        public void Tick(bool held)
        {
            if (!isActiveAndEnabled || !held || _camera == null || _view == null || _movement == null || _combat == null ||
                !_combat.CanUseCombat || !_combat.IsArmed || (!_view.IsAiming && !_view.IsAds) || _movement.IsSprinting ||
                Vector3.ProjectOnPlane(_movement.Velocity, Vector3.up).sqrMagnitude > .04f ||
                _combat.IsAttacking || (_combat.Magazine != null && _combat.Magazine.IsReloading) ||
                (_actions != null && !_actions.CanAttack)) { Release(); return; }
            if (IsMounted)
            {
                if (_cover == null || !_cover.enabled || !_cover.gameObject.activeInHierarchy ||
                    Vector3.Distance(_anchor, transform.position) > .25f || Quaternion.Angle(_aim, _view.AimRotation) > 45f ||
                    Vector3.Distance(Contact, _camera.EyePosition) > _reach + 1f) Release();
                return;
            }
            Vector3 forward = Vector3.ProjectOnPlane(_view.AimRotation * Vector3.forward, Vector3.up).normalized;
            Vector3 probe = _camera.EyePosition + forward * _reach + Vector3.up * .2f;
            int count = Physics.RaycastNonAlloc(probe, Vector3.down, _hits, 1.1f, _coverMask, QueryTriggerInteraction.Ignore);
            if (count == _hits.Length) return;
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _hits[i];
                if (hit.transform.IsChildOf(transform) || hit.normal.y < .7f || hit.distance >= nearest) continue;
                nearest = hit.distance; _cover = hit.collider;
                _localContact = hit.transform.InverseTransformPoint(hit.point);
            }
            if (_cover == null) return;
            _anchor = transform.position; _aim = _view.AimRotation; IsMounted = true;
        }
        public void Release() { IsMounted = false; _cover = null; }
        private void OnDisable() => Release();
    }
}
