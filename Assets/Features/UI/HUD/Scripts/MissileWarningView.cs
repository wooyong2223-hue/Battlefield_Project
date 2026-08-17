using Battlefield.Features.Fighter;
using Battlefield.Features.Targeting;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Battlefield.Features.UI
{
    public sealed class MissileWarningView : MonoBehaviour
    {
        [SerializeField] private MissileThreatDetector _threatDetector;
        [SerializeField] private GameObject _warningGroup;
        [SerializeField] private TMP_Text _warningText;
        [SerializeField] private Image _warningBackground;
        [SerializeField] private Graphic _warningBorder;
        [SerializeField] private Image[] _leftWarningTriangles;
        [SerializeField] private Image[] _rightWarningTriangles;
        [SerializeField] private Sprite _triangleOutlineSprite;
        [SerializeField] private Sprite _triangleFillSprite;

        [Header("Text")]
        [SerializeField] private string _lockingText = "록온 중";
        [SerializeField] private string _lockedText = "록온 됨";
        [SerializeField] private string _incomingText = "접근 중";

        [Header("Colors")]
        [SerializeField] private Color _lockingColor =
            new(1f, 0.92f, 0.45f, 1f);
        [SerializeField] private Color _dangerColor =
            new(1f, 0.3f, 0.25f, 1f);
        [SerializeField] private Color _normalBackgroundColor =
            new(0f, 0f, 0f, 0.35f);
        [SerializeField] private Color _lockedBackgroundColor =
            new(1f, 0f, 0f, 0f);
        [SerializeField] private Color _incomingBackgroundColor =
            new(1f, 0f, 0f, 0.55f);
        [SerializeField] private Color _transparentBackgroundColor =
            new(1f, 0f, 0f, 0f);
        [SerializeField] private Color _incomingTriangleRedColor =
            new(0.55f, 0f, 0f, 1f);
        [SerializeField] private Color _incomingTriangleDarkColor =
            Color.black;

        [Header("Incoming Transition")]
        [FormerlySerializedAs("_blinkInterval")]
        [SerializeField] private float _backgroundTransitionDuration = 0.25f;

        [Header("Incoming Triangle")]
        [SerializeField] private float _incomingTriangleScale = 1.5f;

        private MissileWarningState _currentState;
        private int _filledTriangleCount;
        private float _transitionElapsed;
        private Vector2[] _leftWarningTriangleSizes;
        private Vector2[] _rightWarningTriangleSizes;

        private void Awake()
        {
            _leftWarningTriangleSizes = CaptureTriangleSizes(
                _leftWarningTriangles);
            _rightWarningTriangleSizes = CaptureTriangleSizes(
                _rightWarningTriangles);
        }

        private void OnEnable()
        {
            if (_threatDetector == null)
            {
                SetWarningVisible(false);
                return;
            }

            _threatDetector.WarningStateChanged +=
                HandleWarningStateChanged;
            _threatDetector.LockProgressChanged +=
                HandleLockProgressChanged;
            HandleWarningStateChanged(_threatDetector.WarningState);
            HandleLockProgressChanged(_threatDetector.LockProgress);
        }

        private void OnDisable()
        {
            if (_threatDetector != null)
            {
                _threatDetector.WarningStateChanged -=
                    HandleWarningStateChanged;
                _threatDetector.LockProgressChanged -=
                    HandleLockProgressChanged;
            }

            _currentState = MissileWarningState.None;
            SetWarningVisible(false);
        }

        private void Update()
        {
            if (_currentState != MissileWarningState.Incoming ||
                _warningBackground == null)
            {
                return;
            }

            _transitionElapsed += Time.unscaledDeltaTime;
            float duration = Mathf.Max(
                0.01f,
                _backgroundTransitionDuration);
            float transition = Mathf.PingPong(
                _transitionElapsed / duration,
                1f);
            _warningBackground.color = Color.Lerp(
                _transparentBackgroundColor,
                _incomingBackgroundColor,
                transition);

            SetTriangleColor(Color.Lerp(
                _incomingTriangleDarkColor,
                _incomingTriangleRedColor,
                transition));
        }

        private void HandleWarningStateChanged(MissileWarningState state)
        {
            _currentState = state;

            if (state == MissileWarningState.None)
            {
                SetWarningVisible(false);
                return;
            }

            if (_warningText != null)
            {
                _warningText.text = GetWarningText(state);
                _warningText.color = state == MissileWarningState.Locking
                    ? _lockingColor
                    : _dangerColor;
            }

            SetTrianglesForState(state);
            SetBorderColor(state);
            SetBackgroundForState(state);

            SetWarningVisible(true);
        }

        private void HandleLockProgressChanged(float progress)
        {
            int filledTriangleCount = Mathf.Clamp(
                Mathf.FloorToInt(Mathf.Clamp01(progress) * 4f),
                0,
                3);

            if (_filledTriangleCount == filledTriangleCount)
            {
                return;
            }

            _filledTriangleCount = filledTriangleCount;
            if (_currentState == MissileWarningState.Locking)
            {
                SetLockProgressTriangles(_filledTriangleCount);
            }
        }

        private string GetWarningText(MissileWarningState state)
        {
            return state switch
            {
                MissileWarningState.Locking => _lockingText,
                MissileWarningState.Locked => _lockedText,
                MissileWarningState.Incoming => _incomingText,
                _ => string.Empty
            };
        }

        private void SetWarningVisible(bool visible)
        {
            if (_warningGroup != null &&
                _warningGroup.activeSelf != visible)
            {
                _warningGroup.SetActive(visible);
            }
        }

        private void SetBackgroundForState(MissileWarningState state)
        {
            if (_warningBackground == null)
            {
                return;
            }

            _transitionElapsed = 0f;
            _warningBackground.color = state switch
            {
                MissileWarningState.Locked => _lockedBackgroundColor,
                MissileWarningState.Incoming =>
                    _transparentBackgroundColor,
                _ => _normalBackgroundColor
            };
        }

        private void SetTrianglesForState(MissileWarningState state)
        {
            switch (state)
            {
                case MissileWarningState.Locking:
                    SetLockProgressTriangles(_filledTriangleCount);
                    SetTriangleColor(_lockingColor);
                    break;
                case MissileWarningState.Locked:
                    SetLockProgressTriangles(3);
                    SetTriangleColor(_dangerColor);
                    break;
                case MissileWarningState.Incoming:
                    SetIncomingTriangles();
                    SetTriangleColor(_incomingTriangleDarkColor);
                    break;
            }
        }

        private void SetLockProgressTriangles(int filledCount)
        {
            SetLockProgressTriangles(
                _leftWarningTriangles,
                _leftWarningTriangleSizes,
                filledCount);
            SetLockProgressTriangles(
                _rightWarningTriangles,
                _rightWarningTriangleSizes,
                filledCount);
        }

        private void SetLockProgressTriangles(
            Image[] triangles,
            Vector2[] originalSizes,
            int filledCount)
        {
            if (triangles == null)
            {
                return;
            }

            for (int i = 0; i < triangles.Length; i++)
            {
                Image triangle = triangles[i];
                if (triangle == null)
                {
                    continue;
                }

                RestoreTriangleSize(triangle, originalSizes, i);
                triangle.gameObject.SetActive(true);
                triangle.sprite = i < filledCount
                    ? _triangleFillSprite
                    : _triangleOutlineSprite;
            }
        }

        private void SetIncomingTriangles()
        {
            SetIncomingTriangles(
                _leftWarningTriangles,
                _leftWarningTriangleSizes);
            SetIncomingTriangles(
                _rightWarningTriangles,
                _rightWarningTriangleSizes);
        }

        private void SetIncomingTriangles(
            Image[] triangles,
            Vector2[] originalSizes)
        {
            if (triangles == null)
            {
                return;
            }

            int middleTriangleIndex = triangles.Length / 2;
            for (int i = 0; i < triangles.Length; i++)
            {
                Image triangle = triangles[i];
                if (triangle == null)
                {
                    continue;
                }

                RestoreTriangleSize(triangle, originalSizes, i);
                bool isMiddleTriangle = i == middleTriangleIndex;
                triangle.gameObject.SetActive(isMiddleTriangle);
                triangle.sprite = _triangleFillSprite;

                if (isMiddleTriangle)
                {
                    triangle.rectTransform.sizeDelta *= Mathf.Max(
                        0.01f,
                        _incomingTriangleScale);
                }
            }
        }

        private static Vector2[] CaptureTriangleSizes(Image[] triangles)
        {
            if (triangles == null)
            {
                return null;
            }

            Vector2[] sizes = new Vector2[triangles.Length];
            for (int i = 0; i < triangles.Length; i++)
            {
                if (triangles[i] != null)
                {
                    sizes[i] = triangles[i].rectTransform.sizeDelta;
                }
            }

            return sizes;
        }

        private static void RestoreTriangleSize(
            Image triangle,
            Vector2[] originalSizes,
            int index)
        {
            if (originalSizes == null || index >= originalSizes.Length)
            {
                return;
            }

            triangle.rectTransform.sizeDelta = originalSizes[index];
        }

        private void SetTriangleColor(Color color)
        {
            SetTriangleColor(_leftWarningTriangles, color);
            SetTriangleColor(_rightWarningTriangles, color);
        }

        private static void SetTriangleColor(
            Image[] triangles,
            Color color)
        {
            if (triangles == null)
            {
                return;
            }

            for (int i = 0; i < triangles.Length; i++)
            {
                if (triangles[i] != null)
                {
                    triangles[i].color = color;
                }
            }
        }

        private void SetBorderColor(MissileWarningState state)
        {
            if (_warningBorder == null)
            {
                return;
            }

            _warningBorder.color = state == MissileWarningState.Locking
                ? _lockingColor
                : _dangerColor;
        }
    }
}
