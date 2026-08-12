using Battlefield.Features.Weapon;
using Battlefield.Features.Fighter;
using UnityEngine;
using UnityEngine.UI;

namespace Battlefield.Features.UI
{
    public sealed class FighterGunAimView : MonoBehaviour
    {
        [SerializeField] private MachineGun _machineGun;
        [SerializeField] private Rigidbody _ownerRigidbody;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private JetCamera _jetCamera;
        [SerializeField] private Image _boresightImage;
        [SerializeField] private Graphic _lockOnAreaGraphic;
        [SerializeField] private Image _gunAimReticleImage;
        [SerializeField] private Color _reticleColor = Color.white;
        [SerializeField] private float _predictionDistance = 300f;
        [SerializeField] private float _reticleMoveSpeed = 1000f;

        private RectTransform _rectTransform;
        private MachineGunAimPredictor _aimPredictor;
        private FighterGunAimReticleMover _reticleMover;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _reticleMover = new FighterGunAimReticleMover(
                _gunAimReticleImage != null
                    ? _gunAimReticleImage.rectTransform
                    : null,
                _reticleMoveSpeed);
            ApplyReticleColor();
            SetBoresightVisible(false);
            SetGunAimReticleVisible(false);
        }

        private void Start()
        {
            _aimPredictor = new MachineGunAimPredictor(
                _machineGun != null ? _machineGun.FirePoint : null,
                _ownerRigidbody,
                _machineGun != null
                    ? _machineGun.ProjectilePrefab
                    : null,
                _predictionDistance,
                Time.fixedDeltaTime);
            _aimPredictor.ResetTrajectoryHistory(Time.fixedTime);
        }

        private void OnEnable()
        {
            _aimPredictor?.ResetTrajectoryHistory(Time.fixedTime);
        }

        private void FixedUpdate()
        {
            _aimPredictor?.RecordTrajectorySample(Time.fixedTime);
        }

        private void LateUpdate()
        {
            bool isFirstPersonView =
                _jetCamera != null &&
                _jetCamera.IsFirstPersonView;
            SetBoresightVisible(isFirstPersonView);

            if (!isFirstPersonView)
            {
                SetGunAimReticleVisible(false);
                return;
            }

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
                    Time.time,
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

            _reticleMover.MoveTo(localPoint, Time.deltaTime);
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

        private void SetBoresightVisible(bool isVisible)
        {
            if (_boresightImage == null ||
                _boresightImage.enabled == isVisible)
            {
                SetLockOnAreaVisible(isVisible);
                return;
            }

            _boresightImage.enabled = isVisible;
            SetLockOnAreaVisible(isVisible);
        }

        private void SetLockOnAreaVisible(bool isVisible)
        {
            if (_lockOnAreaGraphic != null)
            {
                _lockOnAreaGraphic.enabled = isVisible;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ApplyReticleColor();
        }
#endif
    }
}
