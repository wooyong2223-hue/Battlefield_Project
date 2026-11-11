using UnityEngine;
using UnityEngine.UI;

namespace Battlefield.Features.UI
{
    public sealed class LockOnAreaGraphic : MaskableGraphic
    {
        [SerializeField] private float _thickness = 2f;
        [SerializeField] private float _cornerLength = 14f;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            Rect rect = rectTransform.rect;
            float thickness = Mathf.Max(0f, _thickness);
            float cornerLength = Mathf.Clamp(
                _cornerLength,
                thickness,
                Mathf.Min(rect.width, rect.height) * 0.5f);

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            AddQuad(vertexHelper, vertex, rect.xMin, rect.yMax - thickness, rect.xMin + cornerLength, rect.yMax);
            AddQuad(vertexHelper, vertex, rect.xMin, rect.yMax - cornerLength, rect.xMin + thickness, rect.yMax);

            AddQuad(vertexHelper, vertex, rect.xMax - cornerLength, rect.yMax - thickness, rect.xMax, rect.yMax);
            AddQuad(vertexHelper, vertex, rect.xMax - thickness, rect.yMax - cornerLength, rect.xMax, rect.yMax);

            AddQuad(vertexHelper, vertex, rect.xMin, rect.yMin, rect.xMin + cornerLength, rect.yMin + thickness);
            AddQuad(vertexHelper, vertex, rect.xMin, rect.yMin, rect.xMin + thickness, rect.yMin + cornerLength);

            AddQuad(vertexHelper, vertex, rect.xMax - cornerLength, rect.yMin, rect.xMax, rect.yMin + thickness);
            AddQuad(vertexHelper, vertex, rect.xMax - thickness, rect.yMin, rect.xMax, rect.yMin + cornerLength);
        }

        private static void AddQuad(
            VertexHelper vertexHelper,
            UIVertex vertex,
            float xMin,
            float yMin,
            float xMax,
            float yMax)
        {
            int startIndex = vertexHelper.currentVertCount;

            vertex.position = new Vector2(xMin, yMin);
            vertexHelper.AddVert(vertex);
            vertex.position = new Vector2(xMin, yMax);
            vertexHelper.AddVert(vertex);
            vertex.position = new Vector2(xMax, yMax);
            vertexHelper.AddVert(vertex);
            vertex.position = new Vector2(xMax, yMin);
            vertexHelper.AddVert(vertex);

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _thickness = Mathf.Max(0f, _thickness);
            _cornerLength = Mathf.Max(_thickness, _cornerLength);
            SetVerticesDirty();
        }
#endif
    }
}
