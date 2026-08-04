using System.Collections.Generic;
using UnityEngine;

namespace Battlefield.Features.Fighter
{
    public class FighterCollisionConstraint : MonoBehaviour
    {
        private readonly Dictionary<Collider, Vector3> _contactNormals = new();

        public void RegisterContact(Collider target, Vector3 contactNormal)
        {
            if (target == null || contactNormal.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            _contactNormals[target] = contactNormal.normalized;
        }

        public void RemoveContact(Collider target)
        {
            if (target != null)
            {
                _contactNormals.Remove(target);
            }
        }

        public void ClearContacts()
        {
            _contactNormals.Clear();
        }

        public Vector3 ConstrainVelocity(Vector3 velocity)
        {
            foreach (Vector3 contactNormal in _contactNormals.Values)
            {
                float inwardSpeed = Vector3.Dot(velocity, contactNormal);
                if (inwardSpeed < 0f)
                {
                    velocity -= contactNormal * inwardSpeed;
                }
            }

            return velocity;
        }

        private void OnDisable()
        {
            ClearContacts();
        }
    }
}
