using Unity.Cinemachine;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    // Starter Assets PlayerFollowCamera uses root tracking with Y damping .25
    // in third person, and zero damping in first person. Use its Cinemachine
    // damper directly; no animation bone or custom jump impulse is sampled.
    public sealed class PlayerCharacterJumpPoseState
    {
        private Vector3 _previousRoot;
        private float _correction;
        private bool _initialized, _firstPerson;
        public Vector3 Offset => Vector3.up * _correction;

        public void Tick(Vector3 rootPosition, bool firstPerson, bool active, float deltaTime)
        {
            if (!active || !_initialized || firstPerson != _firstPerson)
            {
                Reset();
                _previousRoot = rootPosition;
                _firstPerson = firstPerson;
                _initialized = active;
                return;
            }
            if (deltaTime <= 0f) return;
            _correction += _previousRoot.y - rootPosition.y;
            _previousRoot = rootPosition;
            _correction -= Damper.Damp(_correction, firstPerson ? 0f : .25f, deltaTime);
        }

        public void Reset()
        {
            _initialized = false;
            _correction = 0f;
        }
    }
}
