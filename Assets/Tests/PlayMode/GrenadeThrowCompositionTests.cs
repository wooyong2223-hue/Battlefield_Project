#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public sealed class GrenadeThrowCompositionTests
    {
        private sealed class IdleInput : IWeaponInput
        {
            public bool FireWeapon => false;
            public int WeaponSlotSelection => 0;
        }

        [Test]
        public void LeftArmMaskAndTimeParameterAreRestrictedToTheRequestedBodyParts()
        {
            var profile = AssetDatabase.LoadAssetAtPath<TacticalAnimationSet>("Assets/ThirdParty/MixamoLocal/TacticalActions/TacticalActions.asset");
            Assert.That(profile.HasLeftThrow, Is.True);
            Assert.That(profile.Grenade.ThrowAnimation.name, Is.EqualTo("Throw"));
            Assert.That(profile.Grenade.LeftThrowAnimation.name, Is.EqualTo("Throw Object"));
            foreach (string path in new[] {
                "Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller",
                "Assets/ThirdParty/MixamoLocal/ArmedLocomotion/RifleLocomotion.controller",
                "Assets/ThirdParty/MixamoLocal/ArmedLocomotion/PistolLocomotion.controller" })
            {
                var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                var layer = controller.layers.Single(l => l.name == "GrenadeLeftArm");
                Assert.That(layer.defaultWeight, Is.Zero);
                for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                    Assert.That(layer.avatarMask.GetHumanoidBodyPartActive((AvatarMaskBodyPart)i),
                        Is.EqualTo(i == (int)AvatarMaskBodyPart.LeftArm || i == (int)AvatarMaskBodyPart.LeftFingers));
                var state = layer.stateMachine.defaultState;
                Assert.That(state.timeParameterActive, Is.True);
                Assert.That(state.timeParameter, Is.EqualTo("ThrowTime"));
            }
        }

        [UnityTest]
        public IEnumerator CompositionChangesOnlyTheLeftArmAcrossAllLoadoutsAndViews()
        {
            float previous = Time.captureDeltaTime;
            Time.captureDeltaTime = .02f;
            try
            {
                foreach (int slot in new[] { 1, 2, 3 })
                foreach (bool firstPerson in new[] { false, true })
                {
                    yield return CharacterViewTestContext.Load();
                    var c = new CharacterViewTestContext();
                    using var animationScope = new GrenadeAnimationTestScope(c);
                    CharacterViewTestContext.SetField(c.Shooter, "_input", new IdleInput());
                    c.Shooter.enabled = true;
                    var motor = c.Actor.GetComponent<PlayerCharacterMovement>();
                    CharacterViewTestContext.SetField(motor, "_input", null);
                    motor.enabled = true;
                    c.Actor.GetComponent<PlayerCharacterLoadout>().TryEquip(slot);
                    if (c.View.IsFirstPersonView != firstPerson) c.TogglePerspective();
                    var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    floor.transform.position = c.Actor.position + Vector3.down * 2.25f;
                    floor.transform.localScale = new Vector3(100, .5f, 100);
                    Physics.SyncTransforms();
                    try
                    {
                        for (int i = 0; i < 20; i++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                        var grenade = c.Actor.GetComponent<PlayerCharacterGrenadeThrow>();
                        var animator = c.Actor.GetComponentInChildren<Animator>();
                        int leftLayer = animator.GetLayerIndex("GrenadeLeftArm");
                        Assert.That(leftLayer, Is.GreaterThanOrEqualTo(0));
                        Assert.That(grenade.TryEquip(), Is.True);
                        for (int i = 0; i < 40; i++) yield return null;
                        Assert.That(animator.GetLayerWeight(leftLayer), Is.Zero, "Equip/idle must retain their own left arm.");
                        Assert.That(grenade.TryThrow(), Is.True);
                        for (int i = 0; i < 42; i++) yield return null;
                        Assert.That(animator.GetLayerWeight(leftLayer), Is.EqualTo(1f).Within(.001f));
                        Assert.That(animator.GetCurrentAnimatorClipInfo(leftLayer)[0].clip.name, Is.EqualTo("Throw Object"));
                        Assert.That(animator.GetCurrentAnimatorClipInfo(animator.GetLayerIndex("GrenadeUpperBody"))[0].clip.name, Is.EqualTo("Throw"));
                        Assert.That(animator.GetFloat("ThrowTime"), Is.EqualTo(grenade.Progress).Within(.001f));

                        animator.Update(0f);
                        Quaternion left = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).localRotation;
                        Quaternion right = animator.GetBoneTransform(HumanBodyBones.RightUpperArm).localRotation;
                        Quaternion torso = animator.GetBoneTransform(HumanBodyBones.Chest).localRotation;
                        Quaternion leg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).localRotation;
                        animator.SetLayerWeight(leftLayer, 0f);
                        animator.Update(0f);
                        Assert.That(Quaternion.Angle(left, animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).localRotation), Is.GreaterThan(1f));
                        Assert.That(Quaternion.Angle(right, animator.GetBoneTransform(HumanBodyBones.RightUpperArm).localRotation), Is.LessThan(.1f));
                        Assert.That(Quaternion.Angle(torso, animator.GetBoneTransform(HumanBodyBones.Chest).localRotation), Is.LessThan(.1f));
                        Assert.That(Quaternion.Angle(leg, animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).localRotation), Is.LessThan(.1f));
                        Assert.That(animator.applyRootMotion, Is.False);
                        grenade.Cancel();
                        yield return null;
                        Assert.That(animator.GetLayerWeight(leftLayer), Is.Zero);
                    }
                    finally { Object.Destroy(floor); }
                }
            }
            finally { Time.captureDeltaTime = previous; }
        }
    }
}
#endif
