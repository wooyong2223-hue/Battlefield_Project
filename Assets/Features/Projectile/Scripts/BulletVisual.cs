using UnityEngine;

namespace Battlefield.Features.Projectile
{
    [RequireComponent(typeof(MeshFilter))]
    public sealed class BulletVisual : MonoBehaviour
    {
        private MeshFilter _filter;
        private Mesh _defaultMesh;

        public float Diameter => _filter.sharedMesh.bounds.size.x * Mathf.Abs(transform.lossyScale.x);

        private void Awake()
        {
            _filter = GetComponent<MeshFilter>();
            _defaultMesh = _filter.sharedMesh;
        }

        private void OnEnable() => SetMesh(null);

        public void SetMesh(Mesh mesh)
        {
            _filter.sharedMesh = mesh != null ? mesh : _defaultMesh;
        }
    }
}
