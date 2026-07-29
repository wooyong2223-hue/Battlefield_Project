using UnityEngine;
using UnityEngine.UI;

namespace Battlefield.Editor
{
    public class HeatDotRingBuilder : MonoBehaviour
    {
        [Header("Dot Sprite")]
        [SerializeField] private Sprite _dotSprite;

        [Header("Dot Layout")]
        [SerializeField, Range(4, 64)] private int _dotCount = 16;
        [SerializeField, Min(1f)] private float _dotSize = 20f;
        [SerializeField, Min(1f)] private float _radius = 50f;
        [SerializeField] private bool _startFromTop = true;

        [Header("Default Dot Color")]
        [SerializeField] private Color _defaultDotColor = new Color(1f, 1f, 1f, 0.15f);

        [ContextMenu("Generate Dots")]
        public void GenerateDots()
        {
            ClearChildren();

            float startAngle = _startFromTop ? 90f : 0f;

            for (int i = 0; i < _dotCount; i++)
            {
                GameObject dotObj = new GameObject($"Dot_{i:00}", typeof(RectTransform), typeof(Image));
                dotObj.transform.SetParent(transform, false);

                RectTransform rectTransform = dotObj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = Vector2.one * _dotSize;

                float angleDeg = startAngle - (360f / _dotCount) * i; // 시계 방향
                float rad = angleDeg * Mathf.Deg2Rad;
                rectTransform.anchoredPosition = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * _radius;

                Image image = dotObj.GetComponent<Image>();
                image.sprite = _dotSprite;
                image.color = _defaultDotColor;
            }
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child.gameObject);
                else
                    Destroy(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }
    }
}
