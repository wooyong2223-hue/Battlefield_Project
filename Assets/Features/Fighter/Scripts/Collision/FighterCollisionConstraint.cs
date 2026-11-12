using System.Collections.Generic;
using UnityEngine;

namespace Battlefield.Features.Fighter
{
    public class FighterCollisionConstraint : MonoBehaviour
    {
        private readonly Dictionary<Collider, Vector3> _contacts = new();

        public bool HasContact
        {
            get
            {
                foreach (Collider collider in _contacts.Keys)
                {
                    if (IsActive(collider))
                        return true;
                }

                return false;
            }
        }

        public void RegisterContact(Collider target, Vector3 contactNormal)
        {
            if (target == null) return;

            _contacts[target] = contactNormal.normalized;
        }

        public void RemoveContact(Collider target)
        {
            if (target != null)
            {
                _contacts.Remove(target);
            }
        }

        public void ClearContacts()
        {
            _contacts.Clear();
        }

        public Vector3 ConstrainAcceleration(Vector3 acceleration)
        {
            foreach (var contact in _contacts)
            {
                if (!IsActive(contact.Key)) continue;

                Vector3 contactNormal = contact.Value;
                float inwardAcceleration = Vector3.Dot(acceleration, contactNormal);
                if (inwardAcceleration < 0f)
                {
                    acceleration -= contactNormal * inwardAcceleration;
                }
            }

            return acceleration;
        }

        private static bool IsActive(Collider collider)
        {
            return collider != null && collider.enabled && collider.gameObject.activeInHierarchy;
        }

        private void OnDisable()
        {
            ClearContacts();
        }
    }
}
