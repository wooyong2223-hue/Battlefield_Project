using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class ForwardUiFollower : MonoBehaviour
    {
        [SerializeField] private Component _forwardSource;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private MonoBehaviour _viewStateSource;
        [SerializeField] private float _projectionDistance = 300f;
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private bool _centerOnFreeLook;

        private RectTransform _rectTransform;
        private RectTransform _parentRectTransform;
        private IThirdPersonViewState _viewState;
        private int _lastUpdatedFrame = -1;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentRectTransform = _rectTransform.parent as RectTransform;
            _viewState = _viewStateSource as IThirdPersonViewState;

            if (_forwardSource == null)
            {
                Debug.LogError("Forward source is missing", this);
            }

            if (_worldCamera == null)
            {
                Debug.LogError("World Camera is missing", this);
            }

            if (_viewState == null)
            {
                Debug.LogError("View state source must implement IThirdPersonViewState", this);
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
                _forwardSource == null ||
                _worldCamera == null ||
                _viewState == null)
            {
                return;
            }

            if (_centerOnFreeLook &&
                (!_viewState.IsThirdPersonView ||
                 _viewState.IsThirdPersonFreeLook))
            {
                _rectTransform.anchoredPosition = Vector2.zero;
                return;
            }

            Vector2 targetPosition = Vector2.zero;
            if (_viewState.IsThirdPersonView &&
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
            Transform forwardSource = _forwardSource.transform;
            Vector3 targetPosition = forwardSource.position +
                                     forwardSource.forward *
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
