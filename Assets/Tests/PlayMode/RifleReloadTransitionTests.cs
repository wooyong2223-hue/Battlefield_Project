#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace Battlefield.Tests.PlayMode
{
    public class RifleReloadTransitionTests
    {
        private sealed class Idle : IWeaponInput
        { public bool FireWeapon => false; public int WeaponSlotSelection => 0; }
        private static void Call(object target, string method) => target.GetType()
            .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
        private static CharacterViewTestContext Prepare()
        {
            var c = new CharacterViewTestContext();
            CharacterViewTestContext.SetField(c.Shooter, "_input", new Idle()); c.Shooter.enabled = true;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = c.Actor.position + Vector3.down * 2.25f;
            floor.transform.localScale = new Vector3(100, .5f, 100); Physics.SyncTransforms();
            var motor = c.Actor.GetComponent<PlayerCharacterMovement>();
            CharacterViewTestContext.SetField(motor, "_input", null); motor.enabled = true;
            return c;
        }

        [UnityTest]
        public IEnumerator RifleReloadStartsFromTheSameWorldGripInEitherPerspective()
        {
            yield return CharacterViewTestContext.Load(); var c = Prepare();
            var animator = c.Actor.GetComponentInChildren<Animator>();
            var presentation = animator.GetComponent<PlayerCharacterCombatView>();
            var reload = animator.GetComponent<PlayerCharacterRifleReloadView>();
            var torso = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            var loadout = c.Actor.GetComponent<PlayerCharacterLoadout>();
            float worstAngle = 0f, worstPosition = 0f;
            foreach (string name in new[] { "AK47", "M4A1" })
            {
                Vector3 reference = Vector3.zero;
                foreach (bool fp in new[] { false, true })
                {
                    RealRifleModelTests.Select(loadout, AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>(
                        "Assets/Features/Weapon/Definitions/" + name + ".asset"));
                    if (c.View.IsFirstPersonView != fp) c.TogglePerspective();
                    c.Input.AimHeld = true; c.View.Tick(c.Input, .02f);
                    yield return new WaitForSeconds(.7f);
                    c.Shooter.Magazine.TryConsumeRound(); Assert.That(c.Shooter.Magazine.TryReload(), Is.True);
                    Call(reload, "LateUpdate");
                    float angle = Quaternion.Angle(hand.rotation * presentation.GripRotation, c.View.AimRotation);
                    Vector3 position = torso.InverseTransformPoint(hand.position);
                    if (!fp) reference = position;
                    else worstPosition = Mathf.Max(worstPosition, Vector3.Distance(reference, position));
                    worstAngle = Mathf.Max(worstAngle, angle);
                    Debug.Log($"RELOAD_ENTRY {name} FP={fp} angle={angle:F3} localPosition={position}");
                    yield return new WaitForSeconds(1.9f);
                }
            }
            Assert.That(worstAngle, Is.LessThan(2f), "Reload start must not inherit an uncalibrated wrist roll.");
            Assert.That(worstPosition, Is.LessThan(.05f), "Changing perspective must not choose a different source grip.");
        }

        [UnityTest]
        public IEnumerator CancelAndRestartBeforeAnotherCopyDoesNotReuseThePreviousReloadFrame()
        {
            yield return CharacterViewTestContext.Load(); var c = Prepare();
            c.Actor.GetComponent<PlayerCharacterLoadout>().TryEquip(1);
            if (!c.View.IsFirstPersonView) c.TogglePerspective();
            yield return new WaitForSeconds(.6f);
            c.Shooter.Magazine.TryConsumeRound(); Assert.That(c.Shooter.Magazine.TryReload(), Is.True);
            yield return new WaitForSeconds(.3f);
            var animator = c.Actor.GetComponentInChildren<Animator>(); animator.speed = 0f;
            var torso = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            var rig = c.Actor.GetComponentInChildren<FirstPersonArmsView>();
            torso.position += c.Actor.right * .12f;
            c.Shooter.Magazine.CancelReload(); Assert.That(c.Shooter.Magazine.TryReload(), Is.True);
            Call(rig, "CopyPose"); Vector3 restarted = rig.RightHand.position;
            // A lifecycle reset must not change the first displayed pose of this same new reload.
            rig.enabled = false; rig.enabled = true; Call(rig, "CopyPose");
            float error = Vector3.Distance(restarted, rig.RightHand.position); animator.speed = 1f;
            Debug.Log($"RELOAD_RESTART differenceFromFreshRig={error:F4}");
            Assert.That(error, Is.LessThan(.003f));
        }

        [UnityTest]
        public IEnumerator AdsToggleDoesNotInstantlyJumpTheReloadOffset()
        {
            yield return CharacterViewTestContext.Load(); var c = Prepare();
            c.Actor.GetComponent<PlayerCharacterLoadout>().TryEquip(1);
            if (!c.View.IsFirstPersonView) c.TogglePerspective();
            yield return new WaitForSeconds(.5f);
            c.Shooter.Magazine.TryConsumeRound(); Assert.That(c.Shooter.Magazine.TryReload(), Is.True);
            yield return new WaitForSeconds(.4f);
            var reload = c.Actor.GetComponentInChildren<PlayerCharacterRifleReloadView>();
            Vector3 before = reload.ViewOffset; c.TapAim();
            float entry = Vector3.Distance(before, reload.ViewOffset);
            yield return new WaitForSeconds(.3f);
            before = reload.ViewOffset; c.TapAim();
            float exit = Vector3.Distance(before, reload.ViewOffset);
            Debug.Log($"RELOAD_ADS instantaneousEntry={entry:F4} exit={exit:F4}");
            Assert.That(Mathf.Max(entry, exit), Is.LessThan(.003f));
        }
    }
}
#endif
