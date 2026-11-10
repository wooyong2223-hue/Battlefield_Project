using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.Vehicles
{
    // Input components initialize first; an empty seat then removes their authority.
    [DefaultExecutionOrder(25)]
    [DisallowMultipleComponent]
    public sealed class VehicleSeat : MonoBehaviour
    {
        [SerializeField] private Transform _seatPoint;
        [SerializeField] private Transform _entryPoint;
        [SerializeField] private Transform[] _exitPoints;
        [SerializeField] private Rigidbody _vehicleBody;
        [SerializeField] private Health _health;
        [SerializeField] private Behaviour[] _driverInputBehaviours;
        [SerializeField, Min(0f)] private float _interactionDistance = 3f;
        [SerializeField] private bool _useVehicleSurfaceForEntry;

        private const float GroundProbeReach = 2f;
        private readonly Collider[] _overlaps = new Collider[64];
        private readonly RaycastHit[] _groundHits = new RaycastHit[64];
        private bool[] _authoredDriverEnabled;
        private IVehicleSeatInput[] _weaponInputGates;
        private bool _vehicleDestroyed;
        private bool _isTransitioning;
        private Collider[] _entrySurfaces;

        public IVehicleSeatOccupant Occupant { get; private set; }
        public Transform SeatPoint => _seatPoint;
        public bool IsOccupied => HasRoot(Occupant);
        public bool IsAvailable => isActiveAndEnabled && !_vehicleDestroyed &&
                                   (_health == null || !_health.IsDead);

        private void Awake()
        {
            int count = _driverInputBehaviours != null ? _driverInputBehaviours.Length : 0;
            _authoredDriverEnabled = new bool[count];
            _weaponInputGates = new IVehicleSeatInput[count];
            for (int i = 0; i < count; i++)
            {
                Behaviour driver = _driverInputBehaviours[i];
                _authoredDriverEnabled[i] = driver != null && driver.enabled;
                _weaponInputGates[i] = driver as IVehicleSeatInput;
            }
            _vehicleDestroyed = _health != null && _health.IsDead;
            // Vehicle collision components initialize before this seat (execution order 25).
            _entrySurfaces = _useVehicleSurfaceForEntry && _vehicleBody != null
                ? _vehicleBody.GetComponentsInChildren<Collider>(true) : null;
        }

        private void OnEnable()
        {
            _health?.OnDeath?.AddListener(HandleVehicleDeath);
            RefreshDriverControl();
        }

        private void Update()
        {
            if (Occupant != null && !HasRoot(Occupant)) Occupant = null;
            RefreshDriverControl();
        }

        public bool CanEnter(IVehicleSeatOccupant occupant)
        {
            if (_isTransitioning || !IsAvailable || Occupant != null ||
                _seatPoint == null || _entryPoint == null || _vehicleBody == null ||
                !HasExitPoint() || !HasRoot(occupant) || occupant.CollisionBody == null ||
                !occupant.CanEnterSeat || !occupant.HasInputControl)
                return false;

            float distance = Mathf.Max(0f, _interactionDistance);
            if (_useVehicleSurfaceForEntry)
            {
                // Settings can arrive after Awake (Inspector changes or a script reload).
                // Surface mode must never silently revert to the legacy entry point.
                if (_entrySurfaces == null || _entrySurfaces.Length == 0)
                    _entrySurfaces = _vehicleBody.GetComponentsInChildren<Collider>(true);
                Vector3 center = occupant.CollisionBody.transform.TransformPoint(occupant.CollisionBody.center);
                foreach (Collider surface in _entrySurfaces)
                {
                    if (surface == null || !surface.enabled || !surface.gameObject.activeInHierarchy ||
                        surface.isTrigger || surface is MeshCollider mesh && !mesh.convex)
                        continue;
                    if ((center - surface.ClosestPoint(center)).sqrMagnitude <= distance * distance)
                        return true;
                }
                return false;
            }
            return (occupant.Root.position - _entryPoint.position).sqrMagnitude <= distance * distance;
        }

        public bool TryEnter(IVehicleSeatOccupant occupant)
        {
            if (!CanEnter(occupant)) return false;

            _isTransitioning = true;
            Occupant = occupant;
            try
            {
                if (!occupant.TryAttachToSeat(this))
                {
                    Occupant = null;
                    return false;
                }
                RefreshDriverControl();
                return true;
            }
            catch
            {
                Occupant = null;
                RefreshDriverControl();
                throw;
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public bool TryExit()
        {
            if (_isTransitioning || !HasRoot(Occupant) ||
                !TryFindExitPose(Occupant, out Vector3 position, out Quaternion rotation))
                return false;

            _isTransitioning = true;
            IVehicleSeatOccupant occupant = Occupant;
            Occupant = null;
            RefreshDriverControl();
            try
            {
                occupant.DetachFromSeat(this, position, rotation);
                return true;
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public void RefreshDriverControl()
        {
            bool hasControl = IsAvailable && HasRoot(Occupant) && Occupant.HasInputControl;
            if (_driverInputBehaviours == null || _authoredDriverEnabled == null) return;

            for (int i = 0; i < _driverInputBehaviours.Length; i++)
            {
                Behaviour driver = _driverInputBehaviours[i];
                if (driver == null || driver == this) continue;
                _weaponInputGates[i]?.SetWeaponFireAllowed(hasControl && Occupant.CanFireWeapon);
                bool enabledForDriver = hasControl && i < _authoredDriverEnabled.Length &&
                                        _authoredDriverEnabled[i];
                if (driver.enabled != enabledForDriver) driver.enabled = enabledForDriver;
            }
        }

        public bool TryFindExitPose(
            IVehicleSeatOccupant occupant, out Vector3 position, out Quaternion rotation)
        {
            position = default;
            rotation = Quaternion.identity;
            if (!HasRoot(occupant) || occupant.CollisionBody == null || _exitPoints == null)
                return false;

            Physics.SyncTransforms();
            foreach (Transform exit in _exitPoints)
            {
                if (exit == null) continue;
                Quaternion candidateRotation = UprightRotation(exit.forward);
                if (!TryPlaceCapsule(occupant, exit.position, candidateRotation, out position))
                    continue;
                rotation = candidateRotation;
                return true;
            }
            return false;
        }

        private bool TryPlaceCapsule(
            IVehicleSeatOccupant occupant, Vector3 candidate, Quaternion rotation, out Vector3 position)
        {
            position = candidate;
            GetCapsule(occupant, position, rotation, out Vector3 bottom, out Vector3 top,
                out float radius, out float padding);

            Vector3 foot = bottom - Vector3.up * radius;
            float standingHeight = Vector3.Distance(bottom, top) + radius * 2f;
            int hitCount = Physics.RaycastNonAlloc(
                foot + Vector3.up * (standingHeight + GroundProbeReach), Vector3.down, _groundHits,
                standingHeight + GroundProbeReach * 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            if (hitCount == _groundHits.Length) return false;

            float closestGround = float.PositiveInfinity;
            float groundHeight = 0f;
            float minimumUpDot = Mathf.Cos(occupant.CollisionBody.slopeLimit * Mathf.Deg2Rad);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _groundHits[i];
                if (IsOccupantCollider(hit.collider, occupant) || IsVehicleCollider(hit.collider) ||
                    hit.normal.y < minimumUpDot || hit.distance >= closestGround)
                    continue;
                // Exit anchors can sit inside a thick runway. Search above the capsule,
                // but reject overhead surfaces rather than teleporting onto their roofs.
                float maximumGroundHeight = (bottom.y + top.y) * 0.5f +
                                            occupant.CollisionBody.stepOffset;
                if (hit.point.y > maximumGroundHeight) continue;
                closestGround = hit.distance;
                groundHeight = hit.point.y;
            }

            if (!float.IsPositiveInfinity(closestGround))
            {
                Vector3 adjustment = Vector3.up * (groundHeight + padding - foot.y);
                position += adjustment;
                bottom += adjustment;
                top += adjustment;
            }

            int count = Physics.OverlapCapsuleNonAlloc(
                bottom, top, radius, _overlaps, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            if (count == _overlaps.Length) return false;
            for (int i = 0; i < count; i++)
            {
                if (!IsOccupantCollider(_overlaps[i], occupant)) return false;
            }
            return true;
        }

        private static void GetCapsule(
            IVehicleSeatOccupant occupant, Vector3 position, Quaternion rotation,
            out Vector3 bottom, out Vector3 top, out float radius, out float padding)
        {
            CharacterController body = occupant.CollisionBody;
            Vector3 scale = body.transform.lossyScale;
            radius = body.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float height = Mathf.Max(radius * 2f, body.height * Mathf.Abs(scale.y));
            Quaternion turn = rotation * Quaternion.Inverse(occupant.Root.rotation);
            Vector3 center = position + turn *
                (body.transform.TransformPoint(body.center) - occupant.Root.position);
            Vector3 axis = turn * body.transform.up;
            Vector3 halfSegment = axis * (height * 0.5f - radius);
            bottom = center - halfSegment;
            top = center + halfSegment;
            padding = Mathf.Max(0.01f, body.skinWidth * Mathf.Max(
                Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
        }

        private void HandleVehicleDeath()
        {
            _vehicleDestroyed = true;
            RefreshDriverControl();
        }

        private void OnDisable()
        {
            _health?.OnDeath?.RemoveListener(HandleVehicleDeath);
            RefreshDriverControl();
            ReleaseOccupant(Occupant);
        }

        public bool ReleaseOccupant(IVehicleSeatOccupant occupant)
        {
            if (occupant == null || !ReferenceEquals(Occupant, occupant)) return false;

            _isTransitioning = true;
            Occupant = null;
            try
            {
                RefreshDriverControl();
                if (!HasRoot(occupant)) return true;
                if (!TryFindExitPose(occupant, out Vector3 position, out Quaternion rotation))
                    FindEmergencyExitPose(occupant, out position, out rotation);
                // Unparent before either participant is removed, even when every exit is obstructed.
                occupant.DetachFromSeat(this, position, rotation);
                return true;
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        private void FindEmergencyExitPose(
            IVehicleSeatOccupant occupant, out Vector3 position, out Quaternion rotation)
        {
            position = occupant.Root.position;
            rotation = UprightRotation(occupant.Root.forward);
            if (occupant.CollisionBody == null) return;

            Transform vehicle = _vehicleBody != null ? _vehicleBody.transform : transform;
            Bounds bounds = new(vehicle.position, Vector3.zero);
            foreach (Collider collider in vehicle.GetComponentsInChildren<Collider>(true))
            {
                if (collider.enabled && !collider.isTrigger && !IsOccupantCollider(collider, occupant))
                    bounds.Encapsulate(collider.bounds);
            }
            GetCapsule(occupant, position, rotation, out _, out _, out float radius, out _);
            float step = Mathf.Max(1f, radius * 2f);
            Vector3 candidate = new(position.x, bounds.max.y + step * 2f, position.z);
            for (int i = 0; i < 16; i++)
            {
                if (TryPlaceCapsule(occupant, candidate, rotation, out Vector3 clearPosition))
                {
                    position = clearPosition;
                    return;
                }
                candidate += Vector3.up * step;
            }
        }

        private bool HasExitPoint()
        {
            if (_exitPoints == null) return false;
            foreach (Transform exit in _exitPoints) if (exit != null) return true;
            return false;
        }

        private bool IsVehicleCollider(Collider collider)
        {
            Transform vehicle = _vehicleBody != null ? _vehicleBody.transform : transform;
            return collider.transform == vehicle || collider.transform.IsChildOf(vehicle);
        }

        private static bool IsOccupantCollider(Collider collider, IVehicleSeatOccupant occupant) =>
            collider != null && (collider.transform == occupant.Root ||
                                 collider.transform.IsChildOf(occupant.Root));

        private static bool HasRoot(IVehicleSeatOccupant occupant) =>
            occupant != null && (!(occupant is Object unityObject) || unityObject != null) &&
            occupant.Root != null;

        private static Quaternion UprightRotation(Vector3 forward)
        {
            forward = Vector3.ProjectOnPlane(forward, Vector3.up);
            return Quaternion.LookRotation(
                forward.sqrMagnitude > 0.0001f ? forward : Vector3.forward, Vector3.up);
        }
    }
}
