using System;
using UnityEngine;

namespace Battlefield.Features.Targeting
{
    [Serializable]
    public sealed class AirTargetScreenArea
    {
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private RectTransform _reticle;

        public bool TryGetCenterDistance(Vector3 worldPosition, out float centerDistance)
        {
            centerDistance = float.PositiveInfinity;
            if (_worldCamera == null || _reticle == null) return false;

            Vector3 viewportPoint = _worldCamera.WorldToViewportPoint(worldPosition);
            if (viewportPoint.z <= 0f) return false;

            Vector2 screenPoint = _worldCamera.WorldToScreenPoint(worldPosition);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _reticle,
                    screenPoint,
                    null,
                    out Vector2 localPoint))
            {
                return false;
            }

            Rect rect = _reticle.rect;
            float radiusX = rect.width * 0.5f;
            float radiusY = rect.height * 0.5f;
            if (radiusX <= Mathf.Epsilon || radiusY <= Mathf.Epsilon) return false;

            Vector2 offset = localPoint - rect.center;
            float normalizedX = offset.x / radiusX;
            float normalizedY = offset.y / radiusY;
            centerDistance = normalizedX * normalizedX + normalizedY * normalizedY;
            return centerDistance <= 1f;
        }
    }
}
