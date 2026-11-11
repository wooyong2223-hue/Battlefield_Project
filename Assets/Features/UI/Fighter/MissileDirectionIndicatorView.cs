using System.Collections.Generic;
using Battlefield.Features.Fighter;
using UnityEngine;

namespace Battlefield.Features.UI
{
    public sealed class MissileDirectionIndicatorView : MonoBehaviour
    {
        [SerializeField] private MissileThreatDetector _threatDetector;
        [SerializeField] private Camera _worldCamera;
        [SerializeField]
        private MissileDirectionIndicatorGraphic[] _indicators;
        [SerializeField] private Vector2 _indicatorRadius =
            new(360f, 220f);
        [SerializeField] private Vector2 _indicatorSize =
            new(150f, 180f);

        private void OnEnable()
        {
            HideAllIndicators();
        }

        private void OnDisable()
        {
            HideAllIndicators();
        }

        private void LateUpdate()
        {
            if (_threatDetector == null ||
                _worldCamera == null ||
                _indicators == null)
            {
                HideAllIndicators();
                return;
            }

            IReadOnlyList<MissileDirectionThreat> threats =
                _threatDetector.DirectionThreats;
            int visibleCount = Mathf.Min(
                threats.Count,
                _indicators.Length);

            for (int i = 0; i < visibleCount; i++)
            {
                UpdateIndicator(_indicators[i], threats[i]);
            }

            for (int i = visibleCount; i < _indicators.Length; i++)
            {
                SetIndicatorVisible(_indicators[i], false);
            }
        }

        private void UpdateIndicator(
            MissileDirectionIndicatorGraphic indicator,
            MissileDirectionThreat threat)
        {
            if (indicator == null)
            {
                return;
            }

            Vector3 worldDirection =
                threat.Position - _threatDetector.transform.position;
            if (worldDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                SetIndicatorVisible(indicator, false);
                return;
            }

            Vector2 screenDirection = GetScreenDirection(worldDirection);
            RectTransform indicatorTransform = indicator.rectTransform;
            indicatorTransform.anchoredPosition = new Vector2(
                screenDirection.x * Mathf.Max(0f, _indicatorRadius.x),
                screenDirection.y * Mathf.Max(0f, _indicatorRadius.y));
            indicatorTransform.sizeDelta = _indicatorSize;

            float angle = Mathf.Atan2(
                screenDirection.y,
                screenDirection.x) * Mathf.Rad2Deg;
            indicatorTransform.localRotation = Quaternion.Euler(
                0f,
                0f,
                angle + 180f);
            indicator.SetThreat(threat.State, threat.LockProgress);
            SetIndicatorVisible(indicator, true);
        }

        private Vector2 GetScreenDirection(Vector3 worldDirection)
        {
            Transform cameraTransform = _worldCamera.transform;
            Vector2 screenDirection = new(
                Vector3.Dot(worldDirection, cameraTransform.right),
                Vector3.Dot(worldDirection, cameraTransform.up));

            if (screenDirection.sqrMagnitude > Mathf.Epsilon)
            {
                return screenDirection.normalized;
            }

            float forwardDirection = Vector3.Dot(
                worldDirection,
                cameraTransform.forward);
            return forwardDirection >= 0f
                ? Vector2.up
                : Vector2.down;
        }

        private void HideAllIndicators()
        {
            if (_indicators == null)
            {
                return;
            }

            for (int i = 0; i < _indicators.Length; i++)
            {
                SetIndicatorVisible(_indicators[i], false);
            }
        }

        private static void SetIndicatorVisible(
            MissileDirectionIndicatorGraphic indicator,
            bool visible)
        {
            if (indicator != null &&
                indicator.gameObject.activeSelf != visible)
            {
                indicator.gameObject.SetActive(visible);
            }
        }
    }
}
