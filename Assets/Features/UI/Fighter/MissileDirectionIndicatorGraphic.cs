using Battlefield.Features.Targeting;
using UnityEngine;
using UnityEngine.UI;

namespace Battlefield.Features.UI
{
    public sealed class MissileDirectionIndicatorGraphic : MaskableGraphic
    {
        [Header("Layout")]
        [SerializeField] private float _triangleWidth = 24f;
        [SerializeField] private float _triangleHeight = 28f;
        [SerializeField] private float _triangleSpacing = 5f;
        [SerializeField] private float _arcRadius = 115f;
        [SerializeField] private float _arcLength = 62f;
        [SerializeField] private int _arcSegments = 8;
        [SerializeField] private float _lineThickness = 3f;
        [SerializeField] private float _outlineThickness = 2f;
        [SerializeField] private float _incomingTriangleScale = 1.6f;

        [Header("Colors")]
        [SerializeField] private Color _lockingColor =
            new(1f, 0.92f, 0.45f, 1f);
        [SerializeField] private Color _dangerColor =
            new(1f, 0.3f, 0.25f, 1f);

        [Header("Blink")]
        [SerializeField] private float _blinkDuration = 0.25f;
        [SerializeField] private float _minimumBlinkAlpha = 0.25f;

        private MissileWarningState _state;
        private float _lockProgress;

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        private void Update()
        {
            if (_state == MissileWarningState.Locked ||
                _state == MissileWarningState.Incoming)
            {
                SetVerticesDirty();
            }
        }

        public void SetThreat(
            MissileWarningState state,
            float lockProgress)
        {
            lockProgress = Mathf.Clamp01(lockProgress);
            if (_state == state &&
                Mathf.Approximately(_lockProgress, lockProgress))
            {
                return;
            }

            _state = state;
            _lockProgress = lockProgress;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            if (_state == MissileWarningState.None)
            {
                return;
            }

            Color drawColor = GetDrawColor();
            if (_state == MissileWarningState.Incoming)
            {
                DrawIncoming(vertexHelper, drawColor);
                return;
            }

            DrawLockState(vertexHelper, drawColor);
        }

        private void DrawLockState(
            VertexHelper vertexHelper,
            Color drawColor)
        {
            int filledCount = _state == MissileWarningState.Locked
                ? 3
                : Mathf.Clamp(
                    Mathf.FloorToInt(_lockProgress * 4f),
                    0,
                    3);
            float groupWidth = _triangleWidth * 3f +
                               _triangleSpacing * 2f;
            float startX = -groupWidth * 0.5f;

            DrawSideArcs(
                vertexHelper,
                _triangleHeight * 0.65f,
                drawColor);
            for (int i = 0; i < 3; i++)
            {
                float left = startX +
                             i * (_triangleWidth + _triangleSpacing);
                DrawTriangle(
                    vertexHelper,
                    left,
                    _triangleWidth,
                    _triangleHeight,
                    drawColor,
                    i < filledCount);
            }
        }

        private void DrawIncoming(
            VertexHelper vertexHelper,
            Color drawColor)
        {
            float width = _triangleWidth *
                          Mathf.Max(1f, _incomingTriangleScale);
            float height = _triangleHeight *
                           Mathf.Max(1f, _incomingTriangleScale);
            DrawSideArcs(
                vertexHelper,
                height * 0.65f,
                drawColor);
            DrawTriangle(
                vertexHelper,
                -width * 0.5f,
                width,
                height,
                drawColor,
                true);
        }

        private void DrawSideArcs(
            VertexHelper vertexHelper,
            float centerGap,
            Color drawColor)
        {
            DrawArc(
                vertexHelper,
                Mathf.Max(0f, centerGap),
                1f,
                drawColor);
            DrawArc(
                vertexHelper,
                Mathf.Max(0f, centerGap),
                -1f,
                drawColor);
        }

        private void DrawArc(
            VertexHelper vertexHelper,
            float centerGap,
            float side,
            Color drawColor)
        {
            float radius = Mathf.Max(1f, _arcRadius);
            float arcLength = Mathf.Clamp(_arcLength, 0f, radius * 0.95f);
            int segmentCount = Mathf.Max(2, _arcSegments);
            Vector2 previous = GetArcPoint(
                centerGap,
                side,
                0f,
                radius);

            for (int i = 1; i <= segmentCount; i++)
            {
                float distance = arcLength * i / segmentCount;
                Vector2 current = GetArcPoint(
                    centerGap,
                    side,
                    distance,
                    radius);
                AddLine(
                    vertexHelper,
                    previous,
                    current,
                    Mathf.Max(0.5f, _lineThickness),
                    drawColor);
                previous = current;
            }
        }

        private static Vector2 GetArcPoint(
            float centerGap,
            float side,
            float distance,
            float radius)
        {
            float inwardOffset = radius - Mathf.Sqrt(
                Mathf.Max(0f, radius * radius - distance * distance));
            return new Vector2(
                inwardOffset,
                side * (centerGap + distance));
        }

        private void DrawTriangle(
            VertexHelper vertexHelper,
            float left,
            float width,
            float height,
            Color drawColor,
            bool filled)
        {
            Vector2 topLeft = new(left, height * 0.5f);
            Vector2 bottomLeft = new(left, -height * 0.5f);
            Vector2 right = new(left + width, 0f);

            if (filled)
            {
                AddTriangle(
                    vertexHelper,
                    topLeft,
                    bottomLeft,
                    right,
                    drawColor);
                return;
            }

            float thickness = Mathf.Max(0.5f, _outlineThickness);
            AddLine(vertexHelper, topLeft, bottomLeft, thickness, drawColor);
            AddLine(vertexHelper, bottomLeft, right, thickness, drawColor);
            AddLine(vertexHelper, right, topLeft, thickness, drawColor);
        }

        private Color GetDrawColor()
        {
            Color drawColor = _state == MissileWarningState.Locking
                ? _lockingColor
                : _dangerColor;

            if (_state != MissileWarningState.Locked &&
                _state != MissileWarningState.Incoming)
            {
                return drawColor;
            }

            float duration = Mathf.Max(0.01f, _blinkDuration);
            float blink = Mathf.PingPong(
                Time.unscaledTime / duration,
                1f);
            drawColor.a *= Mathf.Lerp(
                Mathf.Clamp01(_minimumBlinkAlpha),
                1f,
                blink);
            return drawColor;
        }

        private static void AddLine(
            VertexHelper vertexHelper,
            Vector2 start,
            Vector2 end,
            float thickness,
            Color color)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 normal = new(-direction.y, direction.x);
            Vector2 offset = normal * (thickness * 0.5f);
            int index = vertexHelper.currentVertCount;
            vertexHelper.AddVert(start - offset, color, Vector2.zero);
            vertexHelper.AddVert(start + offset, color, Vector2.zero);
            vertexHelper.AddVert(end + offset, color, Vector2.zero);
            vertexHelper.AddVert(end - offset, color, Vector2.zero);
            vertexHelper.AddTriangle(index, index + 1, index + 2);
            vertexHelper.AddTriangle(index, index + 2, index + 3);
        }

        private static void AddTriangle(
            VertexHelper vertexHelper,
            Vector2 first,
            Vector2 second,
            Vector2 third,
            Color color)
        {
            int index = vertexHelper.currentVertCount;
            vertexHelper.AddVert(first, color, Vector2.zero);
            vertexHelper.AddVert(second, color, Vector2.zero);
            vertexHelper.AddVert(third, color, Vector2.zero);
            vertexHelper.AddTriangle(index, index + 1, index + 2);
        }
    }
}
