#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Battlefield.Features.Dummy;
using Battlefield.Features.PlayerCharacter;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerCharacterCameraTests
    {
        private readonly List<GameObject> _objects = new();

        [TearDown]
        public void CleanUp()
        {
            foreach (GameObject item in _objects) if (item != null) Object.DestroyImmediate(item);
            _objects.Clear();
        }

        [UnityTest]
        public IEnumerator AuthoredSpawnStartsOutsideMaterialPanelsAndShowsTheThirdPersonModel()
        {
            yield return CharacterViewTestContext.Load();
            var movement = Object.FindObjectsByType<PlayerCharacterMovement>().Single();
            var camera = Object.FindObjectsByType<PlayerCharacterCamera>().Single();
            movement.GetComponent<PlayerCharacterController>().enabled = false;
            var body = movement.GetComponent<CharacterController>();
            Transform root = movement.transform;
            Vector3 center = root.TransformPoint(body.center);
            float radius = body.radius * Mathf.Max(Mathf.Abs(root.lossyScale.x), Mathf.Abs(root.lossyScale.z));
            float halfSegment = Mathf.Max(0f, body.height * Mathf.Abs(root.lossyScale.y) * .5f - radius);
            var panels = GameObject.Find("MaterialImpactRange");
            Assert.That(panels, Is.Not.Null);
            bool overlapsPanels = Physics.OverlapCapsule(center + root.up * halfSegment,
                center - root.up * halfSegment, radius).Any(c => c.transform.IsChildOf(panels.transform));
            Assert.That(overlapsPanels, Is.False, "The authored spawn must stay outside the material panels.");
            yield return new WaitForSeconds(0.8f);
            Assert.That(camera.DistanceFromCharacter, Is.GreaterThan(4.8f));
            Assert.That(movement.GetComponentInChildren<SkinnedMeshRenderer>().shadowCastingMode,
                Is.Not.EqualTo(ShadowCastingMode.ShadowsOnly));
        }

        [UnityTest]
        public IEnumerator PerspectiveAndAdsUseEyePositionAndRestoreModelVisibility()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            yield return null;
            Assert.That(context.Renderer.shadowCastingMode, Is.Not.EqualTo(ShadowCastingMode.ShadowsOnly));
            context.TogglePerspective();
            yield return null;
            yield return null;
            Vector3 eye = context.Actor.TransformPoint(new Vector3(0f, 1.6f, 0f));
            Assert.That(Vector3.Distance(context.Camera.transform.position, eye), Is.LessThan(0.01f));
            Assert.That(context.Renderer.shadowCastingMode, Is.EqualTo(ShadowCastingMode.ShadowsOnly));
            context.TogglePerspective();
            context.TapAim();
            yield return new WaitForSeconds(0.6f);
            Assert.That(context.View.IsAds, Is.True);
            Assert.That(context.Camera.fieldOfView, Is.EqualTo(40f).Within(0.15f));
            context.TapAim();
            yield return new WaitForSeconds(0.6f);
            Assert.That(context.View.IsFirstPersonView, Is.False);
            Assert.That(context.Camera.fieldOfView, Is.EqualTo(60f).Within(0.15f));
            Assert.That(context.Renderer.shadowCastingMode, Is.Not.EqualTo(ShadowCastingMode.ShadowsOnly));
            Assert.That(context.Actor.Find("Visual").GetComponent<MeshRenderer>().enabled, Is.False);
        }

        [UnityTest]
        public IEnumerator HoldingAimUsesTheShoulderCameraAndDoesNotLatchAdsOnRelease()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            context.Input.AimHeld = context.Input.AimPressed = true;
            context.View.Tick(context.Input, 0.02f);
            context.Input.AimPressed = false;
            yield return new WaitForSeconds(0.6f);
            Assert.That(context.View.IsAiming, Is.True);
            Assert.That(context.Camera.fieldOfView, Is.EqualTo(50f).Within(0.15f));
            Assert.That(context.Camera.transform.position.z - context.Actor.position.z, Is.EqualTo(-3f).Within(0.05f));
            context.Input.AimHeld = false;
            context.Input.AimReleased = true;
            context.View.Tick(context.Input, 0.02f);
            context.Input.AimReleased = false;
            Assert.That(context.View.IsAds, Is.False);
        }

        [UnityTest]
        public IEnumerator ThirdPersonCameraContractsAtAWallAndReturnsAfterItIsRemoved()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            yield return null;
            yield return null;
            GameObject wall = new("Camera Test Wall");
            _objects.Add(wall);
            wall.transform.position = context.Actor.position + new Vector3(0f, 1.6f, -2f);
            wall.AddComponent<BoxCollider>().size = new Vector3(10f, 10f, 0.5f);
            Physics.SyncTransforms();
            yield return null;
            yield return null;
            Assert.That(context.Camera.transform.position.z - context.Actor.position.z, Is.GreaterThan(-1.6f));
            Object.DestroyImmediate(wall);
            yield return new WaitForSeconds(0.6f);
            Assert.That(context.Camera.transform.position.z - context.Actor.position.z, Is.EqualTo(-5f).Within(0.05f));
        }

        [UnityTest]
        public IEnumerator ACameraPushedInsideTheCharacterHidesTheMeshAndRestoresIt()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            GameObject wall = new("Close Camera Wall");
            _objects.Add(wall);
            wall.transform.position = context.Actor.position + new Vector3(0f, 1.6f, -0.7f);
            wall.AddComponent<BoxCollider>().size = new Vector3(10f, 10f, 0.1f);
            Physics.SyncTransforms();
            yield return null;
            yield return null;
            Assert.That(context.View.IsFirstPersonView, Is.False);
            Assert.That(context.Renderer.shadowCastingMode, Is.EqualTo(ShadowCastingMode.ShadowsOnly));
            Object.DestroyImmediate(wall);
            yield return new WaitForSeconds(0.6f);
            Assert.That(context.Renderer.shadowCastingMode, Is.Not.EqualTo(ShadowCastingMode.ShadowsOnly));
        }

        private sealed class LeanInput : IPlayerCharacterTacticalInput
        {
            public float Lean { get; set; }
            public bool MountHeld => false;
        }

        [UnityTest]
        public IEnumerator LeanKeepsTheCameraAndAimLevelInBothPerspectivesAndFreeLook()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            var lean = context.Actor.GetComponent<PlayerCharacterLean>();
            var input = new LeanInput();
            CharacterViewTestContext.SetField(lean, "_input", input);
            context.Input.Look = new Vector2(10f, 5f);
            context.View.Tick(context.Input, .02f);
            context.Input.Look = Vector2.zero;
            for (int perspective = 0; perspective < 2; perspective++)
            {
                foreach (float direction in new[] {-1f, 1f})
                {
                    input.Lean = direction;
                    lean.Tick(direction, .3f);
                    yield return null;
                    yield return null;
                    Assert.That(Mathf.Abs(lean.Amount), Is.GreaterThan(.9f), "Exercise the actual lean, not zero input.");
                    Assert.That(Mathf.Abs(Mathf.DeltaAngle(0f, context.View.AimRotation.eulerAngles.z)), Is.LessThan(.01f));
                    Assert.That(Mathf.Abs(Mathf.DeltaAngle(0f, context.Camera.transform.eulerAngles.z)), Is.LessThan(.01f));
                    Assert.That(lean.WorldOffset.magnitude, Is.GreaterThan(.4f), "Retain the requested sideways peek.");
                    context.Input.FreeLookHeld = true;
                    context.Input.Look = new Vector2(5f, 0f);
                    context.View.Tick(context.Input, .02f);
                    context.Input.Look = Vector2.zero;
                    yield return null;
                    yield return null;
                    Assert.That(Mathf.Abs(Mathf.DeltaAngle(0f, context.View.LookRotation.eulerAngles.z)), Is.LessThan(.01f));
                    Assert.That(Mathf.Abs(Mathf.DeltaAngle(0f, context.Camera.transform.eulerAngles.z)), Is.LessThan(.01f));
                    context.Input.FreeLookHeld = false;
                    context.View.Tick(context.Input, .02f);
                }
                context.TogglePerspective();
            }
        }

        [UnityTest]
        public IEnumerator FreeLookMovesTheCameraWithoutChangingTheWeaponAimRay()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            Ray before = context.Follow.GetAimRay();
            context.Input.FreeLookHeld = true;
            context.Input.Look = new Vector2(30f, 0f);
            context.View.Tick(context.Input, 0.02f);
            context.Input.Look = Vector2.zero;
            yield return null;
            yield return null;
            Assert.That(Vector3.Angle(context.Camera.transform.forward, context.Actor.forward), Is.GreaterThan(80f));
            Assert.That(Vector3.Angle(context.Follow.GetAimRay().direction, before.direction), Is.LessThan(0.01f));
            Assert.That(context.Actor.eulerAngles.y, Is.EqualTo(0f).Within(0.01f));
        }
    }

    internal sealed class CharacterViewTestContext
    {
        internal readonly PlayerCharacterController Shooter;
        internal readonly PlayerCharacterView View;
        internal readonly PlayerCharacterCamera Follow;
        internal readonly Camera Camera;
        internal readonly SkinnedMeshRenderer Renderer;
        internal readonly ViewInput Input = new();
        internal Transform Actor => Shooter.transform;

        internal CharacterViewTestContext(bool useRealInput = false)
        {
            Shooter = Object.FindObjectsByType<PlayerCharacterController>().Single();
            View = Shooter.GetComponent<PlayerCharacterView>();
            Follow = Object.FindObjectsByType<PlayerCharacterCamera>().Single();
            Camera = Follow.GetComponent<Camera>();
            Renderer = Shooter.GetComponentInChildren<SkinnedMeshRenderer>();
            // These legacy view fixtures explicitly exercise the pistol; the authored scene starts with AK-47.
            Shooter.GetComponent<PlayerCharacterLoadout>()?.TryEquip(PlayerCharacterLoadout.PistolSlot);
            Shooter.enabled = false;
            Shooter.GetComponent<PlayerCharacterMovement>().enabled = false;
            if (!useRealInput)
            {
                Shooter.GetComponent<PlayerCharacterInput>().enabled = false;
                SetField(View, "_input", Input);
            }
            CharacterController body = Shooter.GetComponent<CharacterController>();
            body.enabled = false;
            Actor.SetParent(null, true);
            Actor.SetPositionAndRotation(new Vector3(10000f, 10002f, 10000f), Quaternion.identity);
            body.enabled = true;
            Physics.SyncTransforms();
        }

        internal void TogglePerspective()
        {
            Input.TogglePerspectivePressed = true;
            View.Tick(Input, 0.02f);
            Input.TogglePerspectivePressed = false;
        }

        internal void TapAim()
        {
            Input.AimHeld = Input.AimPressed = true;
            View.Tick(Input, 0.02f);
            Input.AimHeld = Input.AimPressed = false;
            Input.AimReleased = true;
            View.Tick(Input, 0.02f);
            Input.AimReleased = false;
        }

        internal static IEnumerator Load()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/SampleScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            foreach (DummyShooter shooter in Object.FindObjectsByType<DummyShooter>()) shooter.enabled = false;
            foreach (DummyMissileShooter shooter in Object.FindObjectsByType<DummyMissileShooter>()) shooter.enabled = false;
        }

        internal static void SetField(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        internal sealed class ViewInput : IPlayerCharacterViewInput
        {
            public bool HasControl => true;
            public Vector2 Look { get; set; }
            public bool TogglePerspectivePressed { get; set; }
            public bool FreeLookHeld { get; set; }
            public bool AimHeld { get; set; }
            public bool AimPressed { get; set; }
            public bool AimReleased { get; set; }
        }
    }
}
#endif
