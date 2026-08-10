using Battlefield.Features.Weapon;
using UnityEngine;
using UnityEngine.UI;

namespace Battlefield.Features.UI
{
    public sealed class FighterGunAimView : MonoBehaviour
    {
        [SerializeField] private MachineGunAimPredictor _aimPredictor;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Image _boresightImage;
        [SerializeField] private Image _gunAimReticleImage;
        [SerializeField] private Color _reticleColor = Color.white;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            ApplyReticleColor();
            SetGunAimReticleVisible(false);
        }

        private void LateUpdate()
        {
            UpdateGunAimReticle();
        }

        public void SetReticleColor(Color color)
        {
            _reticleColor = color;
            ApplyReticleColor();
        }

        private void UpdateGunAimReticle()
        {
            if (_aimPredictor == null ||
                _worldCamera == null ||
                _gunAimReticleImage == null ||
                !_aimPredictor.TryGetPredictedPoint(
                    out Vector3 predictedPoint))
            {
                SetGunAimReticleVisible(false);
                return;
            }

            Vector3 viewportPoint =
                _worldCamera.WorldToViewportPoint(predictedPoint);
            bool isOnScreen = viewportPoint.z > 0f &&
                              viewportPoint.x >= 0f &&
                              viewportPoint.x <= 1f &&
                              viewportPoint.y >= 0f &&
                              viewportPoint.y <= 1f;

            if (!isOnScreen)
            {
                SetGunAimReticleVisible(false);
                return;
            }

            Vector2 screenPoint =
                _worldCamera.WorldToScreenPoint(predictedPoint);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform,
                    screenPoint,
                    null,
                    out Vector2 localPoint))
            {
                SetGunAimReticleVisible(false);
                return;
            }

            _gunAimReticleImage.rectTransform.anchoredPosition = localPoint;
            SetGunAimReticleVisible(true);
        }

        private void ApplyReticleColor()
        {
            if (_boresightImage != null)
            {
                _boresightImage.color = _reticleColor;
            }

            if (_gunAimReticleImage != null)
            {
                _gunAimReticleImage.color = _reticleColor;
            }
        }

        private void SetGunAimReticleVisible(bool isVisible)
        {
            if (_gunAimReticleImage == null ||
                _gunAimReticleImage.enabled == isVisible)
            {
                return;
            }

            _gunAimReticleImage.enabled = isVisible;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ApplyReticleColor();
        }
#endif
    }
}
