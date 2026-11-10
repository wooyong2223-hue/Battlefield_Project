#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Battlefield.Features.PlayerCharacter;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerCharacterStanceTests
    {
        private readonly List<GameObject> _objects = new();
        private readonly Vector3 _origin = new(40000f, 10000f, 40000f);

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (GameObject item in _objects) if (item != null) Object.Destroy(item);
            _objects.Clear();
            yield return null;
        }

        [Test]
        public void CrouchingPreservesFeetAndStandingRestoresTheAuthoredCapsule()
        {
            PlayerCharacterStance stance = CreateActor();
            CharacterController body = stance.GetComponent<CharacterController>();
            Vector3 position = stance.transform.position;
            float foot = position.y + body.center.y - body.height * .5f;
            Assert.That(stance.TrySetCrouched(true), Is.True);
            Assert.That(body.height, Is.EqualTo(2.5f));
            Assert.That(position.y + body.center.y - body.height * .5f, Is.EqualTo(foot));
            Assert.That(stance.transform.position, Is.EqualTo(position));
            Assert.That(stance.TrySetCrouched(false), Is.True);
            Assert.That(body.height, Is.EqualTo(4f));
            Assert.That(body.center, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void LowCeilingPreventsStandingUntilTheEntireCapsuleIsClear()
        {
            PlayerCharacterStance stance = CreateActor();
            Assert.That(stance.TrySetCrouched(true), Is.True);
            GameObject ceiling = Box("Low ceiling", _origin + Vector3.up * 3f, new Vector3(4, .5f, 4));
            Assert.That(stance.TrySetCrouched(false), Is.False);
            Assert.That(stance.IsCrouched, Is.True);
            ceiling.GetComponent<Collider>().isTrigger = true;
            Assert.That(stance.TrySetCrouched(false), Is.True, "Trigger volumes must not block standing.");
        }

        [Test]
        public void CrouchMovementIgnoresSprintAndJumpRequests()
        {
            PlayerCharacterStance stance = CreateActor();
            PlayerCharacterMovement movement = stance.gameObject.AddComponent<PlayerCharacterMovement>();
            Set(movement, "_stance", stance);
            Assert.That(stance.TrySetCrouched(true), Is.True);
            for (int i = 0; i < 15; i++) movement.Tick(Vector2.up, true, true, .02f);
            Vector3 start = stance.transform.position;
            movement.Tick(Vector2.up, true, true, .1f);
            Assert.That(stance.transform.position.z - start.z, Is.EqualTo(.4f).Within(.01f));
            Assert.That(stance.transform.position.y, Is.LessThanOrEqualTo(start.y));
            Assert.That(movement.IsSprinting, Is.False);
        }

        [Test]
        public void DisablingStanceRestoresStandingGeometryAndCanBeEnabledAgain()
        {
            PlayerCharacterStance stance = CreateActor();
            Assert.That(stance.TrySetCrouched(true), Is.True);
            stance.enabled = false;
            Assert.That(stance.GetComponent<CharacterController>().height, Is.EqualTo(4f));
            Assert.That(stance.IsCrouched, Is.False);
            Assert.That(stance.TrySetCrouched(true), Is.False);
            stance.enabled = true;
            Assert.That(stance.TrySetCrouched(true), Is.True);
        }

        [UnityTest]
        public IEnumerator ExistingHumanoidAndBothCameraViewsFollowTheCrouchPose()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            PlayerCharacterMovement movement = context.Actor.GetComponent<PlayerCharacterMovement>();
            PlayerCharacterStance stance = context.Actor.GetComponent<PlayerCharacterStance>();
            Animator animator = context.Actor.GetComponentInChildren<Animator>();
            Box("Crouch test floor", context.Actor.position + Vector3.down * 2.25f, new Vector3(100, .5f, 100));
            movement.enabled = true;
            yield return new WaitForSeconds(.4f);
            float cameraY = context.Camera.transform.position.y;
            float headY = animator.GetBoneTransform(HumanBodyBones.Head).position.y;
            Vector3 modelPosition = animator.transform.localPosition;
            Vector3 modelScale = animator.transform.localScale;
            Quaternion modelRotation = animator.transform.localRotation;
            Assert.That(stance.TrySetCrouched(true), Is.True);
            yield return new WaitForSeconds(.5f);
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("CrouchLocomotion"), Is.True);
            Assert.That(animator.GetCurrentAnimatorClipInfo(0).Any(c => c.clip.name == "CrouchIdle" && c.weight > .9f), Is.True);
            Assert.That(headY - animator.GetBoneTransform(HumanBodyBones.Head).position.y, Is.GreaterThan(.75f));
            Vector3 right = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position - animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position;
            Vector3 up = animator.GetBoneTransform(HumanBodyBones.Spine).position - animator.GetBoneTransform(HumanBodyBones.Hips).position;
            Vector3 facing = Vector3.ProjectOnPlane(Vector3.Cross(right, up), context.Actor.up).normalized;
            Assert.That(Vector3.Dot(facing, context.Actor.forward), Is.GreaterThan(.8f), "Crouch must face the same way as the character's movement and weapon.");
            Assert.That(cameraY - context.Camera.transform.position.y, Is.EqualTo(1.5f).Within(.12f));
            context.TogglePerspective();
            yield return new WaitForSeconds(.4f);
            Assert.That(context.Camera.transform.position.y - context.Actor.position.y, Is.EqualTo(.1f).Within(.05f));
            Assert.That(Vector3.Distance(context.Follow.GetAimRay().origin, context.Camera.transform.position), Is.LessThan(.02f));
            Assert.That(animator.transform.localPosition, Is.EqualTo(modelPosition));
            Assert.That(animator.transform.localScale, Is.EqualTo(modelScale));
            Assert.That(Quaternion.Angle(animator.transform.localRotation, modelRotation), Is.LessThan(.01f));
            Assert.That(animator.applyRootMotion, Is.False);
            Assert.That(stance.TrySetCrouched(false), Is.True);
            yield return new WaitForSeconds(.4f);
            Assert.That(context.Camera.transform.position.y - context.Actor.position.y, Is.EqualTo(1.6f).Within(.05f));
        }

        [UnityTest]
        public IEnumerator RealMovementPlaysTheImportedCrouchWalkClip()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            PlayerCharacterMovement movement = context.Actor.GetComponent<PlayerCharacterMovement>();
            PlayerCharacterStance stance = context.Actor.GetComponent<PlayerCharacterStance>();
            Animator animator = context.Actor.GetComponentInChildren<Animator>();
            Box("Crouch walk floor", context.Actor.position + Vector3.down * 2.25f, new Vector3(100, .5f, 100));
            Set(movement, "_input", new CrouchWalkInput());movement.enabled = true;
            Assert.That(stance.TrySetCrouched(true), Is.True);
            Vector3 start = context.Actor.position;
            float started = Time.time;
            yield return new WaitForSeconds(.6f);
            Assert.That(context.Actor.position.z - start.z, Is.EqualTo(4f * (Time.time - started)).Within(.2f));
            Assert.That(animator.GetCurrentAnimatorClipInfo(0).Any(c => c.clip.name == "CrouchWalk" && c.weight > .8f), Is.True);
            Assert.That(movement.IsSprinting, Is.False);
        }

        [UnityTest]
        public IEnumerator BoardingWhileCrouchedRestoresStandingCapsuleAndExitState()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();context.PrepareEntry();
            PlayerCharacterStance stance = context.Actor.GetComponent<PlayerCharacterStance>();
            Assert.That(stance.TrySetCrouched(true), Is.True);
            context.Actor.GetComponent<PlayerCharacterMovement>().enabled = false;
            yield return new WaitForSeconds(.35f);
            context.Actor.GetComponent<PlayerCharacterMovement>().enabled = true;
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            yield return null;
            Assert.That(stance.enabled, Is.False);
            Assert.That(stance.IsCrouched, Is.False);
            Assert.That(context.Occupant.CollisionBody.height, Is.EqualTo(4f));
            Assert.That(Vector3.Distance(context.Animator.GetBoneTransform(HumanBodyBones.Hips).position,
                context.Seat.SeatPoint.position), Is.LessThan(.002f));
            Transform eye = context.Seat.GetComponentsInChildren<Transform>().Single(t => t.name == "FirstPersonCameraPoint");
            Assert.That(Vector3.Distance(context.Animator.GetBoneTransform(HumanBodyBones.Head).position, eye.position), Is.LessThan(.7f));
            Assert.That(context.Seat.TryExit(), Is.True);
            yield return null;
            yield return null;
            Assert.That(stance.enabled, Is.True);
            Assert.That(stance.IsCrouched, Is.False);
            Assert.That(context.Animator.GetBool("Crouched"), Is.False);
        }

        private PlayerCharacterStance CreateActor()
        {
            var actor = new GameObject("Stance test actor");_objects.Add(actor);
            actor.transform.position = _origin + Vector3.up * 2f;
            var body = actor.AddComponent<CharacterController>();body.height = 4f;body.radius = .5f;body.center = Vector3.zero;
            return actor.AddComponent<PlayerCharacterStance>();
        }
        private GameObject Box(string name, Vector3 position, Vector3 size)
        { var item = new GameObject(name);_objects.Add(item);item.transform.position = position;item.AddComponent<BoxCollider>().size = size;Physics.SyncTransforms();return item; }
        private static void Set(object target, string field, object value) =>
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        private sealed class CrouchWalkInput : IPlayerCharacterInput
        {
            public Vector2 Move => Vector2.up;
            public bool SprintHeld => true;
            public bool JumpPressed => false;
        }
    }
}
#endif
