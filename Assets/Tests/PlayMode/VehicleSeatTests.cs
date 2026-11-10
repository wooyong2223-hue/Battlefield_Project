using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Battlefield.Features.Vehicles;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class VehicleSeatTests
    {
        private readonly List<GameObject> _objects = new();
        private readonly Vector3 _origin = new(30000f, 10000f, 30000f);

        [UnityTearDown]
        public IEnumerator CleanUp()
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.Destroy(_objects[i]);
            _objects.Clear();
            yield return null;
        }

        [Test]
        public void EntryRequiresProximityAndOneOccupantOwnsTheSeat()
        {
            SeatFixture fixture = CreateSeat();
            Occupant first = CreateOccupant(fixture.Entry.position + Vector3.right * 20f);
            Assert.That(fixture.Seat.CanEnter(first), Is.False);
            Assert.That(fixture.Seat.TryEnter(first), Is.False);
            Assert.That(first.CollisionBody.enabled, Is.True);
            Assert.That(first.Root.parent, Is.Null);

            first.MoveTo(fixture.Entry.position);
            Assert.That(fixture.Seat.CanEnter(first), Is.True);
            Assert.That(fixture.Seat.TryEnter(first), Is.True);
            Assert.That(fixture.Seat.Occupant, Is.SameAs(first));
            Assert.That(first.Root.parent, Is.SameAs(fixture.Seat.SeatPoint));
            Assert.That(first.CollisionBody.enabled, Is.False);

            Occupant second = CreateOccupant(fixture.Entry.position + Vector3.forward);
            Assert.That(fixture.Seat.TryEnter(second), Is.False);
            Assert.That(fixture.Seat.TryEnter(first), Is.False);
            Assert.That(first.Attachments, Is.EqualTo(1));
            Assert.That(fixture.Seat.Occupant, Is.SameAs(first));
            Assert.That(second.CurrentSeat, Is.Null);
            Assert.That(second.CollisionBody.enabled, Is.True);
        }

        [Test]
        public void SurfaceEntryAllowsEverySideButRejectsDistantAndElevatedPositions()
        {
            SeatFixture fixture = CreateSeat();
            BoxCollider surface = fixture.Vehicle.GetComponent<BoxCollider>();
            SetField(fixture.Seat, "_useVehicleSurfaceForEntry", true);
            SetField(fixture.Seat, "_entrySurfaces", new Collider[] { surface });
            Occupant occupant = CreateOccupant(_origin);
            Vector3[] allowed = { Vector3.right * 4.5f, Vector3.left * 4.5f,
                Vector3.forward * 5.5f, Vector3.back * 5.5f };
            foreach (Vector3 offset in allowed)
            {
                occupant.MoveTo(fixture.Vehicle.transform.position + offset);
                Assert.That(fixture.Seat.CanEnter(occupant), Is.True, offset.ToString());
            }
            occupant.MoveTo(fixture.Vehicle.transform.position + Vector3.left * 5.5f);
            Assert.That(fixture.Seat.CanEnter(occupant), Is.False);
            occupant.MoveTo(fixture.Vehicle.transform.position + Vector3.up * 5f);
            Assert.That(fixture.Seat.CanEnter(occupant), Is.False);
            surface.enabled = false;
            occupant.MoveTo(fixture.Entry.position);
            Assert.That(fixture.Seat.CanEnter(occupant), Is.False);
        }

        [Test]
        public void SurfaceEntryInitializesItsCacheWhenEnabledAfterAwake()
        {
            SeatFixture fixture = CreateSeat();
            Occupant occupant = CreateOccupant(fixture.Entry.position);
            Assert.That(fixture.Seat.CanEnter(occupant), Is.True);
            // A newly imported setting or runtime Inspector change can arrive after Awake.
            SetField(fixture.Seat, "_useVehicleSurfaceForEntry", true);
            occupant.MoveTo(fixture.Vehicle.transform.position + Vector3.left * 4.5f);
            Assert.That(fixture.Seat.CanEnter(occupant), Is.True,
                "Surface mode must not silently fall back to the old entry point when its cache is missing.");
        }

        [UnityTest]
        public IEnumerator DriverCaptureGatesInputAndPreservesAuthoredSimulation()
        {
            SeatFixture fixture = CreateSeat();
            Occupant occupant = CreateOccupant(fixture.Entry.position);
            Assert.That(fixture.Driver.enabled, Is.False);
            Assert.That(fixture.DisabledDriver.enabled, Is.False);
            AssertSimulationUnchanged(fixture);

            Assert.That(fixture.Seat.TryEnter(occupant), Is.True);
            Assert.That(fixture.Driver.enabled, Is.True);
            Assert.That(fixture.DisabledDriver.enabled, Is.False,
                "An authored disabled input component must remain disabled.");

            occupant.HasInputControl = false;
            yield return null;
            Assert.That(fixture.Driver.enabled, Is.False);
            Assert.That(fixture.Seat.Occupant, Is.SameAs(occupant));
            AssertSimulationUnchanged(fixture);

            occupant.HasInputControl = true;
            yield return null;
            Assert.That(fixture.Driver.enabled, Is.True);
            Assert.That(fixture.DisabledDriver.enabled, Is.False);
            Assert.That(fixture.Seat.TryExit(), Is.True);
            Assert.That(fixture.Driver.enabled, Is.False);
            AssertSimulationUnchanged(fixture);
        }

        [Test]
        public void ObstructedUpperBodyKeepsOccupancyUntilAnEntireExitCapsuleFits()
        {
            SeatFixture fixture = CreateSeat();
            Occupant occupant = CreateOccupant(fixture.Entry.position);
            Assert.That(fixture.Seat.TryEnter(occupant), Is.True);
            GameObject[] blockers = BlockExits(fixture);

            Assert.That(fixture.Seat.TryFindExitPose(occupant, out _, out _), Is.False);
            Assert.That(fixture.Seat.TryExit(), Is.False);
            Assert.That(fixture.Seat.Occupant, Is.SameAs(occupant));
            Assert.That(occupant.Root.parent, Is.SameAs(fixture.Seat.SeatPoint));
            Assert.That(occupant.CollisionBody.enabled, Is.False);
            Assert.That(fixture.Driver.enabled, Is.True);
            Assert.That(occupant.Detachments, Is.Zero);

            Object.DestroyImmediate(blockers[0]);
            Assert.That(fixture.Seat.TryExit(), Is.True);
            Assert.That(fixture.Seat.Occupant, Is.Null);
            Assert.That(occupant.Root.parent, Is.Null);
            Assert.That(occupant.CollisionBody.enabled, Is.True);
            Assert.That(occupant.Root.position.x, Is.EqualTo(fixture.Exits[0].position.x).Within(0.01f));
            Assert.That(occupant.CollisionBody.bounds.min.y, Is.GreaterThanOrEqualTo(_origin.y));
            Assert.That(fixture.Driver.enabled, Is.False);
            Assert.That(occupant.Detachments, Is.EqualTo(1));
        }

        [Test]
        public void VehicleDeathRetainsThePilotAndCannotRestoreInputOrAcceptANewPilot()
        {
            SeatFixture fixture = CreateSeat();
            Occupant occupant = CreateOccupant(fixture.Entry.position);
            Assert.That(fixture.Seat.TryEnter(occupant), Is.True);

            fixture.Health.TakeDamage(fixture.Health.MaxHealth);
            Assert.That(fixture.Seat.IsAvailable, Is.False);
            Assert.That(fixture.Driver.enabled, Is.False);
            Assert.That(fixture.Seat.Occupant, Is.SameAs(occupant));
            Assert.That(occupant.Root.parent, Is.SameAs(fixture.Seat.SeatPoint));

            occupant.HasInputControl = false;
            fixture.Seat.RefreshDriverControl();
            occupant.HasInputControl = true;
            fixture.Seat.RefreshDriverControl();
            Assert.That(fixture.Driver.enabled, Is.False);
            Assert.That(fixture.Seat.TryExit(), Is.True,
                "A dead vehicle must still allow its pilot to leave.");

            Occupant nextPilot = CreateOccupant(fixture.Entry.position);
            fixture.Seat.enabled = false;
            fixture.Seat.enabled = true;
            Assert.That(fixture.Seat.CanEnter(nextPilot), Is.False);
            Assert.That(fixture.Seat.TryEnter(nextPilot), Is.False);
            Assert.That(fixture.Driver.enabled, Is.False);
            Assert.That(nextPilot.CollisionBody.enabled, Is.True);
            AssertSimulationUnchanged(fixture);
        }

        [UnityTest]
        public IEnumerator DisablingAnOccupiedSeatReleasesItsPilotBeforeVehicleRemoval()
        {
            SeatFixture fixture = CreateSeat();
            Occupant occupant = CreateOccupant(fixture.Entry.position);
            Assert.That(fixture.Seat.TryEnter(occupant), Is.True);
            BlockExits(fixture);

            fixture.Seat.enabled = false;
            Assert.That(fixture.Seat.Occupant, Is.Null);
            Assert.That(occupant.CurrentSeat, Is.Null);
            Assert.That(occupant.Root.parent, Is.Null);
            Assert.That(occupant.Root.gameObject.activeInHierarchy, Is.True);
            Assert.That(occupant.CollisionBody.enabled, Is.True);
            Assert.That(occupant.Detachments, Is.EqualTo(1));
            Assert.That(fixture.Driver.enabled, Is.False);

            Object.Destroy(fixture.Vehicle);
            yield return null;
            Assert.That(occupant.Root != null, Is.True);
            Assert.That(occupant.Root.gameObject.activeInHierarchy, Is.True);
            Assert.That(occupant.CollisionBody.enabled, Is.True);
            Assert.That(occupant.Detachments, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator RemovingAnOccupiedVehicleDoesNotRemoveItsPilot()
        {
            SeatFixture fixture = CreateSeat();
            Occupant occupant = CreateOccupant(fixture.Entry.position);
            Assert.That(fixture.Seat.TryEnter(occupant), Is.True);

            Object.Destroy(fixture.Vehicle);
            yield return null;
            Assert.That(occupant.Root != null, Is.True);
            Assert.That(occupant.Root.parent, Is.Null);
            Assert.That(occupant.CurrentSeat, Is.Null);
            Assert.That(occupant.Root.gameObject.activeInHierarchy, Is.True);
            Assert.That(occupant.CollisionBody.enabled, Is.True);
            Assert.That(occupant.Detachments, Is.EqualTo(1));
        }

        private SeatFixture CreateSeat()
        {
            CreateBox("Seat Test Floor", _origin + Vector3.down * 0.5f, new Vector3(80f, 1f, 80f));
            GameObject vehicle = CreateObject("Seat Test Vehicle", _origin + Vector3.up * 2f);
            vehicle.SetActive(false);
            Rigidbody body = vehicle.AddComponent<Rigidbody>();
            body.mass = 1234f;
            body.isKinematic = true;
            body.useGravity = true;
            body.linearDamping = 0.4f;
            body.angularDamping = 0.7f;
            body.constraints = RigidbodyConstraints.FreezeRotationX;
            vehicle.AddComponent<BoxCollider>().size = new Vector3(4f, 2f, 6f);
            Health health = vehicle.AddComponent<Health>();
            health.OnDamaged = new UnityEvent();
            health.OnHealed = new UnityEvent();
            health.OnDeath = new UnityEvent();
            Light driver = vehicle.AddComponent<Light>();
            Light disabledDriver = CreateAnchor("Authored Disabled Driver", vehicle.transform, Vector3.zero)
                .gameObject.AddComponent<Light>();
            disabledDriver.enabled = false;
            Light simulation = CreateAnchor("Unmanaged Simulation", vehicle.transform, Vector3.zero)
                .gameObject.AddComponent<Light>();
            Transform seatPoint = CreateAnchor("Seat Point", vehicle.transform, Vector3.up);
            Transform entry = CreateAnchor("Entry Point", vehicle.transform, Vector3.right * 4f);
            Transform[] exits =
            {
                CreateAnchor("Right Exit", vehicle.transform, Vector3.right * 6f),
                CreateAnchor("Left Exit", vehicle.transform, Vector3.left * 6f)
            };

            VehicleSeat seat = vehicle.AddComponent<VehicleSeat>();
            SetField(seat, "_seatPoint", seatPoint);
            SetField(seat, "_entryPoint", entry);
            SetField(seat, "_exitPoints", exits);
            SetField(seat, "_vehicleBody", body);
            SetField(seat, "_health", health);
            SetField(seat, "_driverInputBehaviours", new Behaviour[] { driver, disabledDriver });
            vehicle.SetActive(true);
            Physics.SyncTransforms();
            return new SeatFixture(vehicle, seat, body, health, driver, disabledDriver, simulation, entry, exits);
        }

        private Occupant CreateOccupant(Vector3 position)
        {
            GameObject actor = CreateObject("Seat Test Occupant", position);
            CharacterController body = actor.AddComponent<CharacterController>();
            body.height = 4f;
            body.radius = 0.5f;
            body.center = Vector3.zero;
            body.skinWidth = 0.05f;
            return new Occupant(actor.transform, body);
        }

        private GameObject[] BlockExits(SeatFixture fixture)
        {
            GameObject[] blockers = new GameObject[fixture.Exits.Length];
            for (int i = 0; i < blockers.Length; i++)
            {
                // The root point remains clear; the capsule's upper body must still be checked.
                blockers[i] = CreateBox("Blocked Exit " + i,
                    fixture.Exits[i].position + Vector3.up * 1.4f, new Vector3(2f, 1f, 2f));
            }
            Physics.SyncTransforms();
            return blockers;
        }

        private GameObject CreateBox(string name, Vector3 position, Vector3 size)
        {
            GameObject item = CreateObject(name, position);
            item.AddComponent<BoxCollider>().size = size;
            return item;
        }

        private GameObject CreateObject(string name, Vector3 position)
        {
            GameObject item = new(name);
            item.transform.position = position;
            _objects.Add(item);
            return item;
        }

        private Transform CreateAnchor(string name, Transform parent, Vector3 localPosition)
        {
            Transform anchor = CreateObject(name, parent.position).transform;
            anchor.SetParent(parent, false);
            anchor.localPosition = localPosition;
            return anchor;
        }

        private static void AssertSimulationUnchanged(SeatFixture fixture)
        {
            Assert.That(fixture.Simulation.enabled, Is.True);
            Assert.That(fixture.Body.isKinematic, Is.True);
            Assert.That(fixture.Body.useGravity, Is.True);
            Assert.That(fixture.Body.mass, Is.EqualTo(1234f));
            Assert.That(fixture.Body.linearDamping, Is.EqualTo(0.4f));
            Assert.That(fixture.Body.angularDamping, Is.EqualTo(0.7f));
            Assert.That(fixture.Body.constraints, Is.EqualTo(RigidbodyConstraints.FreezeRotationX));
        }

        private static void SetField(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        private sealed class SeatFixture
        {
            internal readonly GameObject Vehicle;
            internal readonly VehicleSeat Seat;
            internal readonly Rigidbody Body;
            internal readonly Health Health;
            internal readonly Light Driver;
            internal readonly Light DisabledDriver;
            internal readonly Light Simulation;
            internal readonly Transform Entry;
            internal readonly Transform[] Exits;

            internal SeatFixture(GameObject vehicle, VehicleSeat seat, Rigidbody body, Health health,
                Light driver, Light disabledDriver, Light simulation, Transform entry, Transform[] exits)
            {
                Vehicle = vehicle;
                Seat = seat;
                Body = body;
                Health = health;
                Driver = driver;
                DisabledDriver = disabledDriver;
                Simulation = simulation;
                Entry = entry;
                Exits = exits;
            }
        }

        private sealed class Occupant : IVehicleSeatOccupant
        {
            public Transform Root { get; }
            public CharacterController CollisionBody { get; }
            public bool HasInputControl { get; set; } = true;
            public bool CanFireWeapon => HasInputControl;
            public bool CanEnterSeat => CurrentSeat == null;
            internal VehicleSeat CurrentSeat { get; private set; }
            internal int Attachments { get; private set; }
            internal int Detachments { get; private set; }

            internal Occupant(Transform root, CharacterController body)
            {
                Root = root;
                CollisionBody = body;
            }

            public bool TryAttachToSeat(VehicleSeat seat)
            {
                if (CurrentSeat != null) return false;
                CurrentSeat = seat;
                CollisionBody.enabled = false;
                Root.SetParent(seat.SeatPoint, false);
                Root.localPosition = Vector3.zero;
                Root.localRotation = Quaternion.identity;
                Attachments++;
                return true;
            }

            public void DetachFromSeat(VehicleSeat seat, Vector3 position, Quaternion rotation)
            {
                if (CurrentSeat != seat) return;
                Root.SetParent(null, true);
                Root.SetPositionAndRotation(position, rotation);
                CollisionBody.enabled = true;
                CurrentSeat = null;
                Detachments++;
            }

            internal void MoveTo(Vector3 position)
            {
                CollisionBody.enabled = false;
                Root.position = position;
                CollisionBody.enabled = true;
            }
        }
    }
}
