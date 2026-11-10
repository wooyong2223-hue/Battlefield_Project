using Battlefield.Features.PlayerCharacter;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlefield.Features.UI
{
    // Presentation only: reads the same launch conditions used by the throw ability.
    [RequireComponent(typeof(LineRenderer))]
    public sealed class GrenadeTrajectoryView : MonoBehaviour
    {
        private const int MaxPoints = 256;
        private const float Radius = .12f;
        [SerializeField] private PlayerCharacterGrenadeThrow _grenade;
        [SerializeField] private Material _material;
        [SerializeField] private Camera _viewCamera;
        private readonly RaycastHit[] _hits = new RaycastHit[32];
        private readonly Vector3[] _points = new Vector3[MaxPoints];
        private readonly float[] _lengths = new float[MaxPoints];
        private readonly Gradient _gradient = new Gradient();
        private readonly AnimationCurve _width = new AnimationCurve();
        private readonly Keyframe[] _widthKeys = new Keyframe[5];
        private readonly GradientColorKey[] _colors = new GradientColorKey[3];
        private readonly GradientAlphaKey[] _alpha = new GradientAlphaKey[4];
        private LineRenderer _line;

        public void Initialize(PlayerCharacterGrenadeThrow grenade, Material material, Camera viewCamera = null)
        {
            _grenade = grenade;
            _material = material;
            _viewCamera = viewCamera;
            Configure();
        }

        private void Awake() => Configure();

        private void Configure()
        {
            _line = GetComponent<LineRenderer>();
            _line.enabled = false;
            _line.positionCount = 0;
            _line.useWorldSpace = true;
            _line.widthMultiplier = 1f;
            _line.textureMode = LineTextureMode.Stretch;
            _line.numCapVertices = 4;
            _line.shadowCastingMode = ShadowCastingMode.Off;
            _line.receiveShadows = false;
            _line.sharedMaterial = _material;
        }

        private void LateUpdate()
        {
            _line.enabled = false;
            _line.positionCount = 0;
            if (_grenade == null || _material == null ||
                !_grenade.TryGetAimTrajectory(out Vector3 position, out Vector3 velocity)) return;

            float step = Mathf.Max(.001f, Time.fixedDeltaTime);
            int steps = Mathf.Clamp(Mathf.CeilToInt(_grenade.TrajectoryDuration / step), 1, MaxPoints - 1);
            _line.positionCount = steps + 1;
            _line.SetPosition(0, position);
            _points[0] = position;
            _lengths[0] = 0f;
            for (int i = 1; i <= steps; i++)
            {
                // Match the Rigidbody's gravity-before-position integration.
                velocity += Physics.gravity * step;
                Vector3 next = position + velocity * step;
                Vector3 delta = next - position;
                float distance = delta.magnitude;
                int count = distance > .00001f ? Physics.SphereCastNonAlloc(position, Radius,
                    delta / distance, _hits, distance, ~0, QueryTriggerInteraction.Ignore) : 0;
                float nearest = float.PositiveInfinity;
                for (int n = 0; n < count; n++)
                {
                    Transform hit = _hits[n].transform;
                    if (hit.IsChildOf(_grenade.transform) ||
                        (_grenade.Held != null && hit.IsChildOf(_grenade.Held.transform))) continue;
                    nearest = Mathf.Min(nearest, _hits[n].distance);
                }
                if (count == _hits.Length) nearest = 0f; // Never preview through an overflowing hit buffer.
                bool collided = !float.IsPositiveInfinity(nearest);
                _points[i] = collided ? position + delta.normalized * nearest : next;
                _lengths[i] = _lengths[i - 1] + Vector3.Distance(_points[i - 1], _points[i]);
                _line.SetPosition(i, _points[i]);
                if (collided) { _line.positionCount = i + 1; break; }
                position = next;
            }
            StyleGuide(step);
            _line.enabled = true;
        }

        private void StyleGuide(float step)
        {
            int last = _line.positionCount - 1;
            float total = Mathf.Max(.001f, _lengths[last]);
            // Keep full collision/ballistic samples; reveal only a short aiming guide.
            int end = Mathf.Clamp(Mathf.CeilToInt(.8f / step), 1, last);
            int start = end - 1;
            if (_viewCamera != null)
            {
                float pixels = 0f;
                Vector3 previous = _viewCamera.WorldToScreenPoint(_points[end]);
                for (int i = end - 1; i >= 0; i--)
                {
                    Vector3 current = _viewCamera.WorldToScreenPoint(_points[i]);
                    if (current.z <= _viewCamera.nearClipPlane) break;
                    pixels += Vector2.Distance(new Vector2(previous.x, previous.y), new Vector2(current.x, current.y));
                    if (pixels > _viewCamera.pixelHeight * .16f) break;
                    start = i; previous = current;
                }
            }
            else start = Mathf.Max(0, end - 12);

            int tip = Mathf.Clamp(start + Mathf.RoundToInt((end - start) * .7f), start, end);
            float from = _lengths[start] / total;
            float to = _lengths[end] / total;
            float orange = Mathf.Lerp(from, to, .7f);
            _colors[0] = new GradientColorKey(new Color(.92f, .92f, .88f), 0f);
            _colors[1] = new GradientColorKey(new Color(.92f, .92f, .88f), orange);
            _colors[2] = new GradientColorKey(new Color(1f, .36f, .06f), Mathf.Min(to, orange + .005f));
            _alpha[0] = new GradientAlphaKey(from <= .003f ? 1f : 0f, Mathf.Max(0f, from - .003f));
            _alpha[1] = new GradientAlphaKey(1f, Mathf.Max(.00001f, from));
            _alpha[2] = new GradientAlphaKey(1f, Mathf.Min(.99999f, to));
            _alpha[3] = new GradientAlphaKey(to >= .997f ? 1f : 0f, Mathf.Min(1f, to + .003f));
            _gradient.SetKeys(_colors, _alpha);
            _line.colorGradient = _gradient;
            _widthKeys[0] = new Keyframe(0f, PixelWidth(_points[0], 3f));
            _widthKeys[1] = new Keyframe(from, PixelWidth(_points[start], 3f));
            _widthKeys[2] = new Keyframe(orange, PixelWidth(_points[tip], 3f));
            _widthKeys[3] = new Keyframe(to, PixelWidth(_points[end], 5f));
            _widthKeys[4] = new Keyframe(1f, PixelWidth(_points[last], 5f));
            _width.keys = _widthKeys;
            _line.widthCurve = _width;
        }

        private float PixelWidth(Vector3 point, float pixels)
        {
            if (_viewCamera == null) return .025f;
            float height = _viewCamera.orthographic ? _viewCamera.orthographicSize * 2f :
                2f * Mathf.Max(_viewCamera.nearClipPlane, Vector3.Dot(point - _viewCamera.transform.position, _viewCamera.transform.forward)) *
                Mathf.Tan(_viewCamera.fieldOfView * Mathf.Deg2Rad * .5f);
            return height * pixels / Mathf.Max(1, _viewCamera.pixelHeight);
        }

        private void OnDisable()
        {
            if (_line == null) return;
            _line.enabled = false;
            _line.positionCount = 0;
        }

    }
}
