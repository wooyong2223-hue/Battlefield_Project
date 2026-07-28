using UnityEngine;

namespace Battlefield.Fighter
{
    public class FighterMeshCollider : MonoBehaviour
    {
        [SerializeField] private string _collisionMeshName =
            "Main_LOD0_00_low.004";

        private void Awake()
        {
            MeshFilter targetMeshFilter = FindCollisionMesh();

            if (targetMeshFilter == null)
            {
                Debug.LogError(
                    $"Fighter collision mesh '{_collisionMeshName}' was not found.",
                    this);
                return;
            }

            MeshCollider meshCollider =
                targetMeshFilter.GetComponent<MeshCollider>();

            if (meshCollider == null)
            {
                meshCollider = targetMeshFilter.gameObject
                    .AddComponent<MeshCollider>();
            }

            meshCollider.convex = true;
            meshCollider.sharedMesh = targetMeshFilter.sharedMesh;
            meshCollider.isTrigger = false;
        }

        private MeshFilter FindCollisionMesh()
        {
            foreach (MeshFilter meshFilter in
                     GetComponentsInChildren<MeshFilter>(true))
            {
                if (meshFilter.sharedMesh != null &&
                    meshFilter.sharedMesh.name == _collisionMeshName)
                {
                    return meshFilter;
                }
            }

            return null;
        }
    }
}
