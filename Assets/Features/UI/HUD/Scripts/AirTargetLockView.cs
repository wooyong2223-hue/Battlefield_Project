using Battlefield.Features.Targeting;
using Battlefield.Features.Weapon;
using Battlefield.Features.Fighter;
using UnityEngine;
using UnityEngine.UI;

namespace Battlefield.Features.UI
{
    public sealed class AirTargetLockView : MonoBehaviour
    {
        [SerializeField] private AirTargetLock _targetLock;
        [SerializeField] private WeaponController _weaponController;
        [SerializeField] private JetCamera _jetCamera;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Graphic _lockAreaGraphic;
        [SerializeField] private RectTransform _targetMarker;
        [SerializeField] private Graphic _targetMarkerGraphic;
        [SerializeField] private Color _acquiringColor = new(1f, 0.65f, 0f, 1f);
        [SerializeField] private Color _lockedColor = new(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private Vector2 _acquiringSize = new(64f, 64f);
        [SerializeField] private Vector2 _lockedSize = new(44f, 44f);

        private void OnEnable()
        {
            HideTargetMarker();
        }

        private void LateUpdate()
        {
            bool isMissileSelected =
                _weaponController != null &&
                _weaponController.SelectedWeapon is AirToAirMissileLauncher;

            if (_lockAreaGraphic != null)
            {
                bool showLockArea =
                    isMissileSelected &&
                    _jetCamera != null &&
                    _jetCamera.IsFirstPersonView;
                _lockAreaGraphic.gameObject.SetActive(showLockArea);
            }

            if (!isMissileSelected ||
                _targetLock == null ||
                _targetLock.CurrentTarget == null ||
                _worldCamera == null ||
                _targetMarker == null)
            {
                HideTargetMarker();
                return;
            }

            Vector3 viewportPoint = _worldCamera.WorldToViewportPoint(
                _targetLock.CurrentTarget.AimPosition);
            if (viewportPoint.z <= 0f ||
                viewportPoint.x < 0f || viewportPoint.x > 1f ||
                viewportPoint.y < 0f || viewportPoint.y > 1f)
            {
                HideTargetMarker();
                return;
            }

            RectTransform parent = _targetMarker.parent as RectTransform;
            if (parent == null ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent,
                    _worldCamera.WorldToScreenPoint(
                        _targetLock.CurrentTarget.AimPosition),
                    null,
                    out Vector2 localPoint))
            {
                HideTargetMarker();
                return;
            }

            _targetMarker.gameObject.SetActive(true);
            _targetMarker.anchoredPosition = localPoint;

            bool isLocked = _targetLock.State == AirTargetLockState.Locked;
            float progress = isLocked ? 1f : _targetLock.LockProgress;
            _targetMarker.sizeDelta = Vector2.Lerp(
                _acquiringSize,
                _lockedSize,
                progress);

            if (_targetMarkerGraphic != null)
            {
                _targetMarkerGraphic.color = isLocked
                    ? _lockedColor
                    : _acquiringColor;
            }
        }

        private void OnDisable()
        {
            HideTargetMarker();
        }

        private void HideTargetMarker()
        {
            if (_targetMarker != null)
            {
                _targetMarker.gameObject.SetActive(false);
            }
        }
    }
}
