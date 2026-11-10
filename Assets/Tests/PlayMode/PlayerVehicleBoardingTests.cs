#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using Battlefield.Features.Dummy;
using Battlefield.Features.Fighter;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Vehicles;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerVehicleBoardingTests
    {
        [UnityTest]
        public IEnumerator FighterCanBeBoardedFromItsLeftRightNoseAndTailSurfaces()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            MeshCollider surface = context.Seat.GetComponentInChildren<MeshCollider>();
            Assert.That(surface, Is.Not.Null);
            Assert.That(surface.convex, Is.True);
            Bounds bounds = surface.bounds;
            Vector3[] directions = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
            foreach (Vector3 direction in directions)
            {
                Ray ray = new Ray(bounds.center + direction * (bounds.extents.magnitude + 10f), -direction);
                Assert.That(surface.Raycast(ray, out RaycastHit hit, 100f), Is.True);
                context.PrepareEntry();
                CharacterController body = context.Actor.GetComponent<CharacterController>();
                body.enabled = false;
                context.Actor.position = hit.point + direction * 2.5f;
                body.enabled = true;
                Physics.SyncTransforms();
                Assert.That(context.Seat.CanEnter(context.Occupant), Is.True, direction.ToString());
            }
            context.PrepareEntry();
            CharacterController capsule = context.Actor.GetComponent<CharacterController>();
            capsule.enabled = false;
            context.Actor.position = bounds.center + Vector3.right * (bounds.extents.x + 4f);
            capsule.enabled = true;
            Physics.SyncTransforms();
            Assert.That(context.Seat.CanEnter(context.Occupant), Is.False);
        }

        [UnityTest]
        public IEnumerator WalkingBesideTheCockpitCanReachTheBoardingRange()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            Transform entry = (Transform)typeof(VehicleSeat).GetField("_entryPoint",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(context.Seat);
            CharacterController body = context.Actor.GetComponent<CharacterController>();
            Vector3 start = entry.position - context.Seat.transform.right * 6f;
            Assert.That(Physics.Raycast(start + Vector3.up * 10f, Vector3.down,
                out RaycastHit ground, 30f, Physics.AllLayers, QueryTriggerInteraction.Ignore), Is.True);
            body.enabled = false;
            context.Actor.position = new Vector3(start.x, ground.point.y + body.height * .5f + .1f, start.z);
            body.enabled = true;
            typeof(PlayerCharacterInput).GetField("_captured", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(context.Input, true);
            typeof(PlayerCharacterInput).GetField("_captureFrame", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(context.Input, -1);
            bool reached = false;
            for (int step = 0; step < 100; step++)
            {
                body.Move((context.Seat.transform.right * 4f + Vector3.down * 2f) * .02f);
                Physics.SyncTransforms();
                if (!context.Seat.CanEnter(context.Occupant)) continue;
                reached = true;
                break;
            }
            Assert.That(reached, Is.True, "The actual capsule must reach F interaction range without entering the fighter collider.");
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BoardingSeatsTheSameCharacterAndExitRestoresItsAuthoredSetup()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            int actorId = context.Actor.gameObject.GetInstanceID();
            Transform parent = context.Actor.parent;
            Transform model = context.Animator.transform;
            Vector3 modelPosition = model.localPosition;
            Vector3 modelScale = model.localScale;
            context.PrepareEntry();
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            yield return null;
            yield return null;

            Assert.That(context.Actor.gameObject.GetInstanceID(), Is.EqualTo(actorId));
            Assert.That(context.Actor.parent, Is.SameAs(parent));
            Assert.That(context.Pose.IsSeated, Is.True);
            Assert.That(context.Animator.enabled, Is.True);
            Assert.That(context.Pose.IsUsingAnimation, Is.True);
            Assert.That(context.Actor.GetComponent<CharacterController>().enabled, Is.False);
            Assert.That(context.Actor.GetComponent<PlayerCharacterMovement>().enabled, Is.False);
            Assert.That(context.Input.enabled && context.Input.HasPointerCapture, Is.True);
            Assert.That(context.Input.HasControl, Is.False);
            Assert.That(context.FootCamera.enabled, Is.False);
            Assert.That(context.JetCamera.enabled, Is.True);
            Assert.That(context.JetInput.enabled, Is.True);
            Assert.That(context.Renderer.enabled, Is.True);
            Assert.That(context.Renderer.shadowCastingMode, Is.Not.EqualTo(ShadowCastingMode.ShadowsOnly));
            Assert.That(Vector3.Distance(context.Animator.GetBoneTransform(HumanBodyBones.Hips).position,
                context.Seat.SeatPoint.position), Is.LessThan(0.002f));
            Vector3 thigh = context.Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position -
                            context.Animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position;
            Assert.That(Vector3.Dot(thigh.normalized, context.Seat.SeatPoint.forward), Is.GreaterThan(0.85f));
            Assert.That(model.localPosition, Is.EqualTo(modelPosition));
            Assert.That(model.localScale, Is.EqualTo(modelScale));

            Assert.That(context.Seat.TryExit(), Is.True);
            yield return null;
            yield return null;
            Assert.That(context.Occupant.CurrentSeat, Is.Null);
            Assert.That(context.Actor.parent, Is.SameAs(parent));
            Assert.That(context.Animator.enabled, Is.True);
            Assert.That(context.Actor.GetComponent<CharacterController>().enabled, Is.True);
            Assert.That(context.Actor.GetComponent<PlayerCharacterMovement>().enabled, Is.True);
            Assert.That(context.Input.HasControl, Is.True);
            Assert.That(context.FootCamera.enabled, Is.True);
            Assert.That(context.JetCamera.enabled, Is.False);
            Assert.That(context.JetInput.enabled, Is.False);
            Assert.That(context.Actor.localScale, Is.EqualTo(Vector3.one));
            Assert.That(model.localPosition, Is.EqualTo(modelPosition));
            Assert.That(model.localScale, Is.EqualTo(modelScale));
            Assert.That(context.Actor.GetComponent<CharacterController>().height, Is.EqualTo(4f));
            Assert.That(context.Actor.GetComponent<CharacterController>().radius, Is.EqualTo(0.5f));
            Assert.That(Object.FindObjectsByType<Camera>().Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<AudioListener>().Length, Is.EqualTo(1));
            context.PrepareEntry();
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True, "The same character can board again.");
        }

        [UnityTest]
        public IEnumerator SeatedHipsFollowTheMovingAndBankingFighter()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            context.PrepareEntry();
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            context.Seat.transform.SetPositionAndRotation(
                context.Seat.transform.position + Vector3.up * 8f,
                Quaternion.Euler(20f, 45f, 30f));
            Physics.SyncTransforms();
            yield return null;
            yield return null;
            Assert.That(Vector3.Distance(context.Animator.GetBoneTransform(HumanBodyBones.Hips).position,
                context.Seat.SeatPoint.position), Is.LessThan(0.002f));
            Assert.That(context.Actor.IsChildOf(context.Seat.transform), Is.False);
            Assert.That(Quaternion.Angle(context.Actor.rotation, context.Seat.SeatPoint.rotation), Is.LessThan(0.01f));
            Assert.That(context.Pose.IsSeated, Is.True);
        }

        [UnityTest]
        public IEnumerator DestroyingTheOccupiedFighterDoesNotDestroyTheActualCharacter()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            context.PrepareEntry();
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            GameObject actor = context.Actor.gameObject;
            Object.Destroy(context.Seat.gameObject);
            yield return null;
            yield return null;
            Assert.That(actor != null, Is.True);
            Assert.That(context.Occupant.CurrentSeat == null, Is.True);
            Assert.That(actor.GetComponent<CharacterController>().enabled, Is.True);
            Assert.That(context.FootCamera.enabled, Is.True);
        }
    }

    internal sealed class BoardingTestContext
    {
        internal readonly PlayerVehicleOccupant Occupant = Object.FindObjectsByType<PlayerVehicleOccupant>().Single();
        internal readonly VehicleSeat Seat = Object.FindObjectsByType<VehicleSeat>().Single();
        internal Transform Actor => Occupant.transform;
        internal PlayerCharacterInput Input => Actor.GetComponent<PlayerCharacterInput>();
        internal Animator Animator => Actor.GetComponentInChildren<Animator>();
        internal SeatedCharacterPose Pose => Actor.GetComponent<SeatedCharacterPose>();
        internal PlayerCharacterCamera FootCamera => Object.FindObjectsByType<PlayerCharacterCamera>().Single();
        internal JetCamera JetCamera => Object.FindObjectsByType<JetCamera>().Single();
        internal KeyboardJetInput JetInput => Seat.GetComponent<KeyboardJetInput>();
        internal SkinnedMeshRenderer Renderer => Actor.GetComponentInChildren<SkinnedMeshRenderer>();

        internal BoardingTestContext()
        {
            Rigidbody body = Seat.GetComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeAll;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        internal void PrepareEntry()
        {
            Transform entry = (Transform)typeof(VehicleSeat).GetField("_entryPoint", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Seat);
            CharacterController body = Actor.GetComponent<CharacterController>();
            body.enabled = false;
            Actor.position = entry.position;
            body.enabled = true;
            typeof(PlayerCharacterInput).GetField("_captured", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Input, true);
            typeof(PlayerCharacterInput).GetField("_captureFrame", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Input, -1);
            Physics.SyncTransforms();
        }

        internal static IEnumerator Load()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/SampleScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            foreach (DummyShooter shooter in Object.FindObjectsByType<DummyShooter>()) shooter.enabled = false;
            foreach (DummyMissileShooter shooter in Object.FindObjectsByType<DummyMissileShooter>()) shooter.enabled = false;
            yield return null;
        }
    }
}
#endif

