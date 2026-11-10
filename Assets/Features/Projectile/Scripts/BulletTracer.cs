using UnityEngine;

namespace Battlefield.Features.Projectile
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class BulletTracer : MonoBehaviour
    {
        [SerializeField, Min(.01f)] private float _length = 4f;
        [SerializeField, Min(.001f)] private float _fadeDuration = .04f;
        [SerializeField, Min(.01f)] private float _widthMultiplier = 6f;
        private LineRenderer _line;
        private Vector3 _previous;
        private Vector3 _origin;
        private bool _running;
        private bool _fading;
        private float _fadeStarted;
        private float _width;
        private float _activeLength;

        private void Awake() => _line = GetComponent<LineRenderer>();

        public void Begin(Vector3 origin, float diameter, float lengthMultiplier = 1f)
        {
            transform.SetParent(null, true);
            gameObject.SetActive(true);
            _origin = _previous = origin;
            _running = true;
            _fading = false;
            _width = Mathf.Max(.001f, diameter * _widthMultiplier);
            _activeLength = _length * Mathf.Max(.01f, lengthMultiplier);
            _line.useWorldSpace = true;
            _line.widthMultiplier = _width;
            _line.positionCount = 2;
            _line.SetPosition(0, origin);
            _line.SetPosition(1, origin);
        }

        public void Advance(Vector3 position)
        {
            if (!_running || _fading) return;
            Vector3 delta = position - _previous;
            float distance = delta.magnitude;
            if (distance > .00001f)
            {
                float length = Mathf.Min(_activeLength, Vector3.Distance(_origin, position));
                _line.SetPosition(0, position - delta / distance * length);
                _line.SetPosition(1, position);
            }
            _previous = position;
        }

        public void Stop()
        {
            if (!_running || _fading) return;
            _fading = true;
            _fadeStarted = Time.time;
        }

        private void Update()
        {
            if (!_running || !_fading) return;
            float remaining = 1f - (Time.time - _fadeStarted) / _fadeDuration;
            _line.widthMultiplier = _width * Mathf.Clamp01(remaining);
            if (remaining > 0f) return;
            _line.positionCount = 0;
            _running = false;
            gameObject.SetActive(false);
        }
    }
}
