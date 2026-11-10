#if UNITY_EDITOR
using System.Collections;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public sealed class GrenadeHandlingAnimationTests
    {
        private sealed class IdleInput : IWeaponInput
        {
            public bool FireWeapon => false;
            public int WeaponSlotSelection => 0;
        }
        [UnityTest]
        public IEnumerator GameplayNeedsNoGrenadeClipsAndReleasesAtThePreviewedVelocity()
        {
            foreach (int slot in new[] { 1, 2, 3 })
            foreach (bool firstPerson in new[] { false, true })
            {
                yield return CharacterViewTestContext.Load();
                var c = new CharacterViewTestContext();
                CharacterViewTestContext.SetField(c.Shooter, "_input", new IdleInput());
                c.Shooter.enabled = true;
                var motor = c.Actor.GetComponent<PlayerCharacterMovement>();
                CharacterViewTestContext.SetField(motor, "_input", null); motor.enabled = true;
                c.Actor.GetComponent<PlayerCharacterLoadout>().TryEquip(slot);
                if (c.View.IsFirstPersonView != firstPerson) c.TogglePerspective();
                var grenade = c.Actor.GetComponent<PlayerCharacterGrenadeThrow>();
                CharacterViewTestContext.SetField(grenade, "_input", null);
                var view = c.Actor.GetComponentInChildren<PlayerCharacterTacticalAnimationView>();
                var original = AssetDatabase.LoadAssetAtPath<TacticalAnimationSet>("Assets/ThirdParty/MixamoLocal/TacticalActions/TacticalActions.asset");
                Assert.That(original.Grenade.UseAnimations, Is.False);
                var definition = Object.Instantiate(original.Grenade);
                var profile = Object.Instantiate(original);
                foreach (string field in new[] { "_throwAnimation", "_leftThrowAnimation", "_equipAnimation", "_idleAnimation" })
                    CharacterViewTestContext.SetField(definition, field, null);
                foreach (string field in new[] { "_throwPlaceholder", "_leftThrowPlaceholder", "_equipPlaceholder", "_idlePlaceholder" })
                    CharacterViewTestContext.SetField(profile, field, null);
                CharacterViewTestContext.SetField(profile, "_grenade", definition);
                CharacterViewTestContext.SetField(grenade, "_definition", definition);
                CharacterViewTestContext.SetField(view, "_profile", profile);
                CharacterViewTestContext.SetField(view, "_validated", null);
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.position = c.Actor.position + Vector3.down * 2.25f;
                floor.transform.localScale = new Vector3(100, .5f, 100); Physics.SyncTransforms();
                try
                {
                    Assert.That(profile.IsValid, Is.True, "Missing grenade animation assets must not prevent setup.");
                    for (int i = 0; i < 20; i++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                    Assert.That(grenade.TryEquip(), Is.True);
                    Assert.That(grenade.State, Is.EqualTo(GrenadeHandlingState.Ready));
                    Assert.That(grenade.Progress + grenade.HandlingProgress + grenade.PoseWeight, Is.Zero);
                    Assert.That(grenade.TryBeginAim(), Is.True);
                    yield return null; yield return null;
                    var animator = c.Actor.GetComponentInChildren<Animator>();
                    Assert.That(animator.GetLayerWeight(animator.GetLayerIndex("GrenadeUpperBody")), Is.Zero);
                    Assert.That(animator.GetLayerWeight(animator.GetLayerIndex("GrenadeLeftArm")), Is.Zero);
                    Assert.That(profile.Read((AnimatorOverrideController)animator.runtimeAnimatorController).Length, Is.EqualTo(1), "Only mantle should be mapped.");
                    Assert.That(c.Actor.GetComponent<LineRenderer>().enabled, Is.True);
                    Assert.That(grenade.TryGetAimTrajectory(out Vector3 origin, out Vector3 velocity), Is.True);
                    var projectile = grenade.Held;
                    int before = grenade.Remaining;
                    Assert.That(grenade.TryThrow(), Is.True);
                    Assert.That(projectile.IsArmed, Is.True, "Release must be synchronous, without an animation timer.");
                    Assert.That(projectile.Owner, Is.SameAs(c.Actor));
                    Assert.That(Vector3.Distance(projectile.GetComponent<Rigidbody>().linearVelocity, velocity), Is.LessThan(.001f));
                    Assert.That(Vector3.Distance(projectile.transform.position, origin), Is.LessThan(.001f));
                    Assert.That(grenade.IsEquipped, Is.False); Assert.That(grenade.Remaining, Is.EqualTo(before - 1));
                    Assert.That(grenade.TryThrow(), Is.False); Assert.That(grenade.Remaining, Is.EqualTo(before - 1));
                    yield return null; yield return null;
                    Assert.That(c.Actor.GetComponent<LineRenderer>().enabled, Is.False);
                }
                finally
                {
                    grenade.Cancel(); CharacterViewTestContext.SetField(grenade, "_definition", original.Grenade);
                    CharacterViewTestContext.SetField(view, "_profile", original);
                    Object.Destroy(profile); Object.Destroy(definition); Object.Destroy(floor);
                }
            }
        }
        [UnityTest]
        public IEnumerator SeparateEquipAndLoopingIdleLeadIntoTheFullThrowClip()
        {
            yield return CharacterViewTestContext.Load();
            var c = new CharacterViewTestContext();
            using var animationScope = new GrenadeAnimationTestScope(c);
            var motor = c.Actor.GetComponent<PlayerCharacterMovement>();
            CharacterViewTestContext.SetField(c.Shooter, "_input", new IdleInput());
            c.Shooter.enabled = true;
            CharacterViewTestContext.SetField(motor, "_input", null);
            motor.enabled = true;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = c.Actor.position + Vector3.down * 2.25f;
            floor.transform.localScale = new Vector3(100, .5f, 100);
            Physics.SyncTransforms();
            float previous = Time.captureDeltaTime;
            Time.captureDeltaTime = .02f;
            try
            {
                for (int i = 0; i < 20; i++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                var grenade = c.Actor.GetComponent<PlayerCharacterGrenadeThrow>();
                var animator = c.Actor.GetComponentInChildren<Animator>();
                int layer = animator.GetLayerIndex("GrenadeUpperBody");
                Assert.That(grenade.TryEquip(), Is.True);
                for (int i = 0; i < 12; i++) yield return null;
                Assert.That(animator.GetCurrentAnimatorStateInfo(layer).IsName("Equip"), Is.True);
                Assert.That(grenade.Progress, Is.Zero, "Equip must not consume the throwing clip's timeline.");
                for (int i = 0; i < 38; i++) yield return null;
                Assert.That(grenade.State, Is.EqualTo(GrenadeHandlingState.Ready));
                Assert.That(animator.GetCurrentAnimatorStateInfo(layer).IsName("Idle"), Is.True);
                var idle = animator.GetCurrentAnimatorClipInfo(layer)[0].clip;
                Assert.That(idle.name, Is.EqualTo("GrenadeIdle"));
                Assert.That(AnimationUtility.GetAnimationClipSettings(idle).loopTime, Is.True);
                var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                Vector3 start = c.Actor.InverseTransformPoint(hand.position);
                for (int i = 0; i < 100; i++) yield return null;
                Assert.That(Vector3.Distance(start, c.Actor.InverseTransformPoint(hand.position)), Is.LessThan(.08f), "Ready must wrap without a hand snap.");
                Assert.That(grenade.Held, Is.Not.Null);
                Assert.That(grenade.Remaining, Is.EqualTo(3));
                Assert.That(grenade.Progress, Is.Zero);
                Assert.That(grenade.TryThrow(), Is.True);
                Assert.That(grenade.Progress, Is.Zero, "Throw must start at the first frame, not the old 50% hold frame.");
                for (int i = 0; i < 12; i++) yield return null;
                Assert.That(animator.GetCurrentAnimatorStateInfo(layer).IsName("Throw"), Is.True);
                Assert.That(grenade.Held, Is.Not.Null, "Equipping must never launch a live grenade.");
                grenade.Cancel();
            }
            finally { Time.captureDeltaTime = previous; Object.Destroy(floor); }
        }
    }
    // Opt-in tests keep the deferred animation implementation covered without
    // changing the user's gameplay asset or enabling it in ordinary tests.
    internal sealed class GrenadeAnimationTestScope : System.IDisposable
    {
        private readonly PlayerCharacterGrenadeThrow _grenade;
        private readonly PlayerCharacterTacticalAnimationView _view;
        private readonly TacticalAnimationSet _original, _profile;
        private readonly GrenadeDefinition _definition;
        internal GrenadeAnimationTestScope(CharacterViewTestContext context)
        {
            _grenade = context.Actor.GetComponent<PlayerCharacterGrenadeThrow>();
            _view = context.Actor.GetComponentInChildren<PlayerCharacterTacticalAnimationView>();
            _original = AssetDatabase.LoadAssetAtPath<TacticalAnimationSet>("Assets/ThirdParty/MixamoLocal/TacticalActions/TacticalActions.asset");
            _definition = Object.Instantiate(_original.Grenade);
            _profile = Object.Instantiate(_original);
            CharacterViewTestContext.SetField(_definition, "_useAnimations", true);
            CharacterViewTestContext.SetField(_profile, "_grenade", _definition);
            CharacterViewTestContext.SetField(_grenade, "_definition", _definition);
            CharacterViewTestContext.SetField(_view, "_profile", _profile);
            CharacterViewTestContext.SetField(_view, "_validated", null);
        }
        public void Dispose()
        {
            if (_grenade != null) { _grenade.Cancel(); CharacterViewTestContext.SetField(_grenade, "_definition", _original.Grenade); }
            if (_view != null) { CharacterViewTestContext.SetField(_view, "_profile", _original); CharacterViewTestContext.SetField(_view, "_validated", null); }
            Object.Destroy(_profile); Object.Destroy(_definition);
        }
    }
}
#endif
