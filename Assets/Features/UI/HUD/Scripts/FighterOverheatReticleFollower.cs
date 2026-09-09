using Battlefield.Features.Fighter;
using UnityEngine;

namespace Battlefield.Features.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class FighterOverheatReticleFollower : MonoBehaviour
    {
        [SerializeField] private Rigidbody _ownerRigidbody;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private JetCamera _jetCamera;
        [SerializeField] private float _projectionDistance = 300f;
        [SerializeField] private float _moveSpeed = 10f;

        private RectTransform _rectTransform;
        private RectTransform _parentRectTransform;
        private int _lastUpdatedFrame = -1;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentRectTransform = _rectTransform.parent as RectTransform;

            if (_ownerRigidbody == null)
            {
                Debug.LogError("Owner Rigidbody is missing", this);
            }

            if (_worldCamera == null)
            {
                Debug.LogError("World Camera is missing", this);
            }

            if (_jetCamera == null)
            {
                Debug.LogError("Jet Camera is missing", this);
            }

            if (_parentRectTransform == null)
            {
                Debug.LogError("Parent RectTransform is missing", this);
            }
        }

        private void OnEnable()
        {
            Canvas.willRenderCanvases += UpdateReticlePosition;
        }

        private void OnDisable()
        {
            Canvas.willRenderCanvases -= UpdateReticlePosition;
            _lastUpdatedFrame = -1;
        }

        private void UpdateReticlePosition()
        {
            if (_lastUpdatedFrame == Time.frameCount)
            {
                return;
            }

            _lastUpdatedFrame = Time.frameCount;

            if (_rectTransform == null ||
                _parentRectTransform == null ||
                _ownerRigidbody == null ||
                _worldCamera == null ||
                _jetCamera == null)
            {
                return;
            }

            Vector2 targetPosition = Vector2.zero;
            if (_jetCamera.IsThirdPersonView &&
                TryGetForwardScreenPosition(out Vector2 forwardPosition))
            {
                targetPosition = forwardPosition;
            }

            float interpolation = 1f - Mathf.Exp(
                -Mathf.Max(0f, _moveSpeed) * Mathf.Max(0f, Time.deltaTime));
            _rectTransform.anchoredPosition = Vector2.Lerp(
                _rectTransform.anchoredPosition,
                targetPosition,
                interpolation);
        }

        private bool TryGetForwardScreenPosition(out Vector2 localPosition)
        {
            Transform fighter = _ownerRigidbody.transform;
            Vector3 targetPosition = fighter.position +
                                     fighter.forward *
                                     Mathf.Max(1f, _projectionDistance);
            Vector3 viewportPosition =
                _worldCamera.WorldToViewportPoint(targetPosition);

            if (viewportPosition.z <= 0f)
            {
                localPosition = default;
                return false;
            }

            Vector2 screenPosition =
                _worldCamera.WorldToScreenPoint(targetPosition);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRectTransform,
                screenPosition,
                null,
                out localPosition);
        }
    }
}
