using UnityEngine;
using UnityEngine.UI;

namespace Battlefield.Features.UI
{
    public sealed class RefillProgressView : MonoBehaviour
    {
        [SerializeField] private RefillProgressType _progressType;
        [SerializeField] private RectTransform _progressLine;
        [SerializeField] private Graphic _progressGraphic;
        [SerializeField] private RectTransform _filledArea;
        [SerializeField] private Graphic _filledAreaGraphic;

        private IRefillProgressSource _source;

        private void Awake()
        {
            SetProgressVisible(false, 0f);
        }

        public void Bind(IRefillProgressSource source)
        {
            _source = source;
            SetProgressVisible(false, 0f);
        }

        private void LateUpdate()
        {
            float progress = 0f;
            bool hasProgress =
                _source != null &&
                _source.TryGetRefillProgress(
                    _progressType,
                    out progress);

            SetProgressVisible(hasProgress, progress);
        }

        private void OnDisable()
        {
            SetProgressVisible(false, 0f);
        }

        private void SetProgressVisible(bool visible, float progress)
        {
            RectTransform progressArea = _progressLine != null
                ? _progressLine.parent as RectTransform
                : null;

            if (_progressLine == null || progressArea == null)
            {
                return;
            }

            float normalizedProgress = Mathf.Clamp01(progress);
            float lineY = Mathf.Lerp(
                progressArea.rect.yMin,
                progressArea.rect.yMax,
                normalizedProgress);
            _progressLine.anchoredPosition = new Vector2(
                _progressLine.anchoredPosition.x,
                lineY);

            if (_filledArea != null)
            {
                _filledArea.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    Mathf.Max(0f, lineY - progressArea.rect.yMin));
            }

            if (_progressGraphic != null &&
                _progressGraphic.enabled != visible)
            {
                _progressGraphic.enabled = visible;
            }

            if (_filledAreaGraphic != null &&
                _filledAreaGraphic.enabled != visible)
            {
                _filledAreaGraphic.enabled = visible;
            }
        }
    }
}
