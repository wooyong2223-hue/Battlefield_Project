using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
#endif

namespace Battlefield.Features.PlayerCharacter
{
    // Geometry is prepared in the Editor; runtime sampling does not require a readable mesh.
    public sealed class SkinnedPoseContactData : ScriptableObject
    {
        [Serializable]
        private struct Point
        {
            public int Bone0, Bone1, Bone2, Bone3;
            public Vector3 Local0, Local1, Local2, Local3;
            public Vector4 Weights;
        }
        [SerializeField] private Mesh _sourceMesh;
        [SerializeField] private int _boneCount;
        [SerializeField] private Point[] _points;

        internal Sampler CreateSampler(SkinnedMeshRenderer renderer) =>
            renderer != null && renderer.sharedMesh == _sourceMesh && renderer.bones.Length == _boneCount &&
            _points != null && _points.Length > 0 ? new Sampler(this, renderer.bones) : null;

        internal sealed class Sampler
        {
            private readonly SkinnedPoseContactData _data;
            private readonly Transform[] _bones;
            private readonly Matrix4x4[] _matrices;
            internal Sampler(SkinnedPoseContactData data, Transform[] bones)
            { _data = data; _bones = bones; _matrices = new Matrix4x4[bones.Length]; }
            internal bool TryGetLowestHeight(out float height)
            {
                height = float.PositiveInfinity;
                for (int i = 0; i < _bones.Length; i++)
                {
                    if (_bones[i] == null) return false;
                    _matrices[i] = _bones[i].localToWorldMatrix;
                }
                foreach (Point p in _data._points)
                {
                    float y = WorldY(p.Bone0, p.Local0) * p.Weights.x + WorldY(p.Bone1, p.Local1) * p.Weights.y +
                              WorldY(p.Bone2, p.Local2) * p.Weights.z + WorldY(p.Bone3, p.Local3) * p.Weights.w;
                    height = Mathf.Min(height, y);
                }
                return !float.IsInfinity(height) && !float.IsNaN(height);
            }
            private float WorldY(int bone, Vector3 p)
            {
                Matrix4x4 m = _matrices[bone];
                return m.m10 * p.x + m.m11 * p.y + m.m12 * p.z + m.m13;
            }
        }

#if UNITY_EDITOR
        public void BakeFrom(SkinnedMeshRenderer renderer)
        {
            Mesh mesh = renderer.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            BoneWeight[] weights = mesh.boneWeights;
            Matrix4x4[] bindPoses = mesh.bindposes;
            if (vertices.Length != weights.Length || bindPoses.Length != renderer.bones.Length)
                throw new InvalidOperationException("Contact data requires matching mesh weights and bones.");
            var directions = new List<Vector3>();
            for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++) for (int z = -1; z <= 1; z++)
                if (x != 0 || y != 0 || z != 0) directions.Add(new Vector3(x, y, z));
            int width = directions.Count;
            var minima = new float[bindPoses.Length * width];
            var indices = new int[minima.Length];
            for (int i = 0; i < minima.Length; i++) { minima[i] = float.PositiveInfinity; indices[i] = -1; }
            for (int i = 0; i < vertices.Length; i++)
            {
                BoneWeight w = weights[i];
                Include(w.boneIndex0, w.weight0, i); Include(w.boneIndex1, w.weight1, i);
                Include(w.boneIndex2, w.weight2, i); Include(w.boneIndex3, w.weight3, i);
            }
            void Include(int bone, float weight, int vertex)
            {
                if (weight <= .0001f) return;
                Vector3 local = bindPoses[bone].MultiplyPoint3x4(vertices[vertex]);
                for (int d = 0; d < width; d++)
                {
                    int slot = bone * width + d; float value = Vector3.Dot(local, directions[d]);
                    if (value < minima[slot]) { minima[slot] = value; indices[slot] = vertex; }
                }
            }
            var selected = new SortedSet<int>();
            foreach (int index in indices) if (index >= 0) selected.Add(index);
            var points = new List<Point>(selected.Count);
            foreach (int i in selected)
            {
                BoneWeight w = weights[i];
                points.Add(new Point { Bone0 = w.boneIndex0, Bone1 = w.boneIndex1, Bone2 = w.boneIndex2, Bone3 = w.boneIndex3,
                    Local0 = bindPoses[w.boneIndex0].MultiplyPoint3x4(vertices[i]), Local1 = bindPoses[w.boneIndex1].MultiplyPoint3x4(vertices[i]),
                    Local2 = bindPoses[w.boneIndex2].MultiplyPoint3x4(vertices[i]), Local3 = bindPoses[w.boneIndex3].MultiplyPoint3x4(vertices[i]),
                    Weights = new Vector4(w.weight0, w.weight1, w.weight2, w.weight3) });
            }
            _sourceMesh = mesh; _boneCount = bindPoses.Length; _points = points.ToArray();
            Debug.Log("Skinned pose contact prepared: " + _points.Length + " sample vertices / " + vertices.Length);
        }
#endif
    }
}
