using UnityEngine;

namespace Battlefield.Fighter
{
    public class FighterMeshCollider : MonoBehaviour
    {
        [SerializeField] private MeshFilter _collisionMesh;

        private void Awake()
        {
            if (_collisionMesh == null || _collisionMesh.sharedMesh == null)
            {
                Debug.LogError(
                    "Fighter collision mesh is not assigned.",
                    this);
                return;
            }

            MeshCollider meshCollider =
                _collisionMesh.GetComponent<MeshCollider>();

            if (meshCollider == null)
            {
                meshCollider = _collisionMesh.gameObject
                    .AddComponent<MeshCollider>();
            }

            meshCollider.convex = true;
            meshCollider.sharedMesh = _collisionMesh.sharedMesh;
            meshCollider.isTrigger = false;
        }
    }
}
