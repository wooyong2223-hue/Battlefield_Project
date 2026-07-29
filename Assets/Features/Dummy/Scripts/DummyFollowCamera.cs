using UnityEngine;

namespace Battlefield.Dummy
{
    public class DummyFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 18f, -14f);
        [SerializeField] private Vector3 _lookOffset = new Vector3(0f, 0f, 5f);
        [SerializeField, Min(0.01f)] private float _followSpeed = 12f;

        private void LateUpdate()
        {
            if (_target == null) return;

            float ratio = 1f - Mathf.Exp(-_followSpeed * Time.deltaTime);
            Vector3 targetPosition =
                _target.position + _target.rotation * _offset;
            Vector3 lookPosition =
                _target.position + _target.rotation * _lookOffset;

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                ratio);
            transform.rotation = Quaternion.LookRotation(
                lookPosition - transform.position);
        }
    }
}
