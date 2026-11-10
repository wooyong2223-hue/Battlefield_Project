using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [DefaultExecutionOrder(-40)]
    public sealed class PlayerCharacterLean : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterCamera _camera;
        [SerializeField] private PlayerCharacterActionPolicy _actions;
        [SerializeField, Min(0f)] private float _distance = .45f;
        [SerializeField, Min(0f)] private float _angle = 12f;
        [SerializeField, Min(.01f)] private float _speed = 10f;
        [SerializeField, Min(.01f)] private float _radius = .2f;
        [SerializeField] private LayerMask _obstacleMask = ~0;
        private readonly RaycastHit[] _hits = new RaycastHit[32];
        private IPlayerCharacterTacticalInput _input;
        public float Amount { get; private set; }
        public Vector3 WorldOffset => transform.right * (Amount * _distance);
        public float Roll => -Amount * _angle;
        private void Awake() => _input = GetComponent<IPlayerCharacterTacticalInput>();
        private void Update() => Tick(_input != null ? _input.Lean : 0f, Time.deltaTime);
        public void Tick(float request, float deltaTime)
        {
            if (!isActiveAndEnabled || _camera == null || deltaTime <= 0f) return;
            float target = _actions == null || _actions.CanAttack ? Mathf.Clamp(request, -1f, 1f) : 0f;
            float next = Mathf.MoveTowards(Amount, target, _speed * deltaTime);
            if (Mathf.Abs(next) > .0001f && _distance > .0001f)
            {
                Vector3 direction = transform.right * Mathf.Sign(next);
                float distance = Mathf.Abs(next) * _distance;
                int count = Physics.SphereCastNonAlloc(_camera.UnleanedEyePosition, _radius, direction,
                    _hits, distance + .02f, _obstacleMask, QueryTriggerInteraction.Ignore);
                if (count == _hits.Length) distance = 0f;
                for (int i = 0; i < count; i++)
                    if (!_hits[i].transform.IsChildOf(transform)) distance = Mathf.Min(distance, Mathf.Max(0f, _hits[i].distance - .02f));
                next = Mathf.Sign(next) * distance / _distance;
            }
            Amount = next;
        }
        private void OnDisable() => Amount = 0f;
    }
}
