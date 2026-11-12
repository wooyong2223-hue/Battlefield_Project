using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [RequireComponent(typeof(Rigidbody))]
    public class JetPhysics : MonoBehaviour
    {
        [Header("Body")]
        [SerializeField] private float _mass = 1000f;
        [SerializeField] private float _linearDamping = 0f;

        private void Awake()
        {
            Rigidbody body = GetComponent<Rigidbody>();
            body.mass = Mathf.Max(0.0001f, _mass);
            body.linearDamping = Mathf.Max(0f, _linearDamping);
            body.useGravity = true;
        }
    }
}
