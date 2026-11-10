#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace Battlefield.Tests.PlayMode
{
    public class SlideAnimationOverrideTests
    {
        private sealed class Idle : IWeaponInput { public bool FireWeapon => false; public int WeaponSlotSelection => 0; }
        private sealed class Crouch : IPlayerCharacterTraversalInput { public bool SlidePressed { get; set; } public bool PronePressed => false; }
        private static AnimationClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/ThirdParty/Quaternius/Animations/" + name + ".anim");
        private static SlideAnimationSet TestSet()
        {
            var set = ScriptableObject.CreateInstance<SlideAnimationSet>(); var data = new SerializedObject(set);
            var fields = new[] { "Start", "Loop", "Exit" }; var names = new[] { "Slide_Start", "Slide_Loop", "Slide_Exit" };
            for (int i = 0; i < 3; i++)
            {
                data.FindProperty("_original" + fields[i]).objectReferenceValue = Clip(names[i]);
                data.FindProperty("_" + fields[i].ToLowerInvariant()).objectReferenceValue = Clip(names[(i + 1) % 3]);
            }
            data.ApplyModifiedPropertiesWithoutUndo(); return set;
        }
        [UnityTest]
        public IEnumerator SlideOverridesPreserveEquipmentClipsAndDoNotRebindDuringTemporarySuspension()
        {
            yield return CharacterViewTestContext.Load(); var c = new CharacterViewTestContext();
            CharacterViewTestContext.SetField(c.Shooter, "_input", new Idle()); c.Shooter.enabled = true;
            var animator = c.Actor.GetComponentInChildren<Animator>();
            var view = animator.GetComponent<PlayerCharacterSlideAnimationView>() ?? animator.gameObject.AddComponent<PlayerCharacterSlideAnimationView>();
            var loadout = c.Actor.GetComponent<PlayerCharacterLoadout>();
            // Record the actual equipment controller before the optional slide wrapper;
            // a local movement profile may already compose that equipment controller.
            view.Initialize(animator, c.Shooter, null, c.Actor.GetComponent<PlayerCharacterTraversal>());
            loadout.TryEquip(PlayerCharacterLoadout.RifleSlot); yield return null;
            var equipmentController = animator.runtimeAnimatorController;
            loadout.TryEquip(PlayerCharacterLoadout.PistolSlot); yield return null;
            var set = TestSet(); view.Initialize(animator, c.Shooter, set, c.Actor.GetComponent<PlayerCharacterTraversal>());
            var hold = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/ThirdParty/KayKit/Rifle/RifleHold.anim");
            loadout.TryEquip(PlayerCharacterLoadout.RifleSlot); yield return null;
            Assert.That(animator.runtimeAnimatorController.animationClips, Does.Contain(hold));
            Assert.That(((AnimatorOverrideController)animator.runtimeAnimatorController)[Clip("Slide_Start").name], Is.SameAs(set.Start));
            loadout.TryEquip(PlayerCharacterLoadout.PistolSlot); yield return null;
            Assert.That(animator.runtimeAnimatorController.animationClips, Has.No.Member(hold));
            loadout.TryEquip(PlayerCharacterLoadout.RifleSlot); yield return null;
            var composed = animator.runtimeAnimatorController;
            view.enabled = false;
            Assert.That(animator.runtimeAnimatorController, Is.SameAs(composed));
            view.enabled = true; yield return null;
            Assert.That(((AnimatorOverrideController)animator.runtimeAnimatorController)[Clip("Slide_Exit").name], Is.SameAs(set.Exit));
            view.Initialize(animator, c.Shooter, null, c.Actor.GetComponent<PlayerCharacterTraversal>());
            Assert.That(animator.runtimeAnimatorController, Is.SameAs(equipmentController));
            view.enabled = false; Object.Destroy(set);
        }

        [UnityTest]
        public IEnumerator OptionalLocalProfileIsAppliedWhenPresentAndNotRequiredWhenMissing()
        {
            yield return CharacterViewTestContext.Load(); var c = new CharacterViewTestContext();
            var animator = c.Actor.GetComponentInChildren<Animator>();
            var profile = AssetDatabase.LoadAssetAtPath<SlideAnimationSet>("Assets/ThirdParty/MixamoLocal/RunningSlide.asset");
            var view = animator.GetComponent<PlayerCharacterSlideAnimationView>();
            if (profile == null) { Assert.That(view, Is.Null); yield break; }
            Assert.That(profile.IsValid, Is.True, "Re-import the incomplete local animation set.");
            Assert.That(view, Is.Not.Null); Assert.That(view.Animations, Is.SameAs(profile));
            if (profile.LowerBodyOnly)
            {
                Assert.That(animator.GetLayerIndex("LowerBodySlide"), Is.GreaterThanOrEqualTo(0));
                var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>("Assets/Features/PlayerCharacter/Animation/SlideLowerBody.mask");
                foreach (AvatarMaskBodyPart part in System.Enum.GetValues(typeof(AvatarMaskBodyPart)))
                {
                    if (part == AvatarMaskBodyPart.LastBodyPart) continue;
                    Assert.That(mask.GetHumanoidBodyPartActive(part), Is.EqualTo(part == AvatarMaskBodyPart.Root || part == AvatarMaskBodyPart.Body || part == AvatarMaskBodyPart.LeftLeg || part == AvatarMaskBodyPart.RightLeg), part.ToString());
                }
            }
            if (profile.IsContinuous)
            {
                Assert.That(profile.Start, Is.SameAs(profile.Loop)); Assert.That(profile.Exit, Is.SameAs(profile.Loop));
                // The active clip may be a derived composition; validate the retained FBX source separately.
                var importer = (ModelImporter)AssetImporter.GetAtPath("Assets/ThirdParty/MixamoLocal/RunningSlide.fbx");
                Assert.That(importer.clipAnimations.Length, Is.EqualTo(1));
                var clip = importer.clipAnimations[0]; var take = importer.defaultClipAnimations[0];
                Assert.That(clip.firstFrame, Is.EqualTo(take.firstFrame)); Assert.That(clip.lastFrame, Is.EqualTo(take.lastFrame));
                Assert.That(clip.lockRootRotation && clip.keepOriginalOrientation, Is.True);
            }
            foreach (var clip in new[] { profile.Start, profile.Loop, profile.Exit })
            { Assert.That(clip.humanMotion, Is.True); Assert.That(clip.length, Is.GreaterThan(.01f)); }
            Assert.That(((AnimatorOverrideController)animator.runtimeAnimatorController)[Clip("Slide_Start").name], Is.SameAs(profile.Start));
        }

        [Test]
        public void ForwardEntryReferencesUnmodifiedExternalSourcesInsteadOfBakedCurves()
        {
            var profile = AssetDatabase.LoadAssetAtPath<SlideAnimationSet>("Assets/ThirdParty/MixamoLocal/RunningSlide.asset");
            if (profile == null) Assert.Ignore("Optional Mixamo source is not installed.");
            var original = AssetDatabase.LoadAllAssetsAtPath("Assets/ThirdParty/MixamoLocal/RunningSlide.fbx")
                .OfType<AnimationClip>().Single(clip => clip.name == "RunningSlide");
            Assert.That(profile.LowerBodyOnly, Is.True);
            Assert.That(profile.UsesForwardEntry, Is.True);
            Assert.That(profile.Start, Is.SameAs(Clip("Slide_Start")));
            Assert.That(profile.Loop, Is.SameAs(original));
            Assert.That(profile.Exit, Is.SameAs(original));
            var controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(
                "Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller");
            var entry = controller.layers.Single(layer => layer.name == "LowerBodySlideEntry");
            var body = controller.layers.Single(layer => layer.name == "LowerBodySlide");
            Assert.That(entry.avatarMask, Is.SameAs(body.avatarMask));
            Assert.That(entry.stateMachine.defaultState.motion, Is.SameAs(Clip("Slide_Start")));
            Assert.That(entry.stateMachine.defaultState.timeParameter, Is.EqualTo("SlideEntryTime"));
            Assert.That(body.stateMachine.defaultState.timeParameter, Is.EqualTo("SlideTime"));
        }

        [UnityTest]
        public IEnumerator SlidingKeepsTheModelOnItsMotorInsteadOfApplyingTheFbxTravelAgain()
        {
            yield return CharacterViewTestContext.Load(); var c = new CharacterViewTestContext();
            CharacterViewTestContext.SetField(c.Shooter, "_input", new Idle()); c.Shooter.enabled = true;
            c.Actor.GetComponent<PlayerCharacterLoadout>().TryEquip(PlayerCharacterLoadout.RifleSlot);
            var floor = new GameObject("Slide test floor"); floor.transform.position = c.Actor.position + Vector3.down * 2.25f;
            floor.AddComponent<BoxCollider>().size = new Vector3(100, .5f, 100); Physics.SyncTransforms();
            var motor = c.Actor.GetComponent<PlayerCharacterMovement>(); CharacterViewTestContext.SetField(motor, "_input", null); motor.enabled = true;
            var traversal = c.Actor.GetComponent<PlayerCharacterTraversal>(); var stance = c.Actor.GetComponent<PlayerCharacterStance>();
            var animator = c.Actor.GetComponentInChildren<Animator>();
            float old = Time.captureDeltaTime; Time.captureDeltaTime = .02f;
            try
            {
                for (int i = 0; i < 20; i++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                for (int i = 0; i < 20; i++) { motor.Tick(Vector2.up, true, false, .02f); yield return null; }
                var input = new Crouch { SlidePressed = true }; CharacterViewTestContext.SetField(traversal, "_input", input);
                Assert.That(stance.TrySetCrouched(true), Is.True); Vector3 local = animator.transform.localPosition;
                Quaternion rotation = animator.transform.localRotation; bool sawSlide = false; bool sawLowerBody = false; bool sawFadeOut = false; float lastLowerTime = 0f;
                for (int i = 0; i < 42; i++)
                {
                    motor.Tick(Vector2.zero, false, false, .02f); input.SlidePressed = false; yield return null;
                    var playback = animator.GetComponent<PlayerCharacterSlideAnimationView>()?.Animations;
                    if (playback != null && playback.LowerBodyOnly && animator.GetLayerWeight(animator.GetLayerIndex("LowerBodySlide")) > 0f)
                    {
                        float clipTime = animator.GetFloat("SlideTime");
                        Assert.That(clipTime, Is.GreaterThanOrEqualTo(lastLowerTime - .0001f), "The fading leg pose must not rewind at slide end.");
                        lastLowerTime = clipTime; if (!traversal.IsSliding) sawFadeOut = true;
                    }
                    if (!traversal.IsSliding) continue; sawSlide = true;
                    Assert.That(Vector3.ProjectOnPlane(animator.bodyPosition - c.Actor.position, Vector3.up).magnitude, Is.LessThan(1.5f));
                    Assert.That(animator.transform.localPosition, Is.EqualTo(local));
                    Assert.That(Quaternion.Angle(rotation, animator.transform.localRotation), Is.LessThan(.01f));
                    Assert.That(animator.applyRootMotion, Is.False);
                    var motion = animator.GetComponent<PlayerCharacterSlideAnimationView>()?.Animations;
                    if (motion != null && motion.LowerBodyOnly && traversal.Progress > .3f && traversal.Progress < .8f)
                    {
                        sawLowerBody = true;
                        Assert.That(animator.GetInteger("TraversalMode"), Is.Zero, "Full-body slide must not replace the base pose.");
                        float lower = animator.GetLayerWeight(animator.GetLayerIndex("LowerBodySlide"));
                        int entryLayer = animator.GetLayerIndex("LowerBodySlideEntry");
                        float entry = entryLayer >= 0 ? animator.GetLayerWeight(entryLayer) : 0f;
                        Assert.That(entry + lower * (1f - entry), Is.GreaterThan(.95f), "The two native layers must fully replace the locomotion legs.");
                        if (motion.UsesForwardEntry)
                        {
                            float p = animator.GetFloat("SlideTime");
                            float mix = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.5f, .8f, p));
                            Assert.That(lower * (1f - entry), Is.EqualTo(mix).Within(.001f));
                            Assert.That(animator.GetFloat("SlideEntryTime"), Is.EqualTo(Mathf.Clamp01(p / .5f)).Within(.001f));
                        }
                        Assert.That(animator.GetLayerWeight(animator.GetLayerIndex("UpperCombat")), Is.GreaterThan(0f), "Weapon upper-body presentation must remain active.");
                    }
                    if (motion != null && motion.IsContinuous)
                        Assert.That(animator.GetFloat("TraversalTime"), Is.EqualTo(traversal.Progress).Within(.0001f), "A complete clip must use global progress, not restart at phase boundaries.");
                }
                Assert.That(sawSlide, Is.True); Assert.That(traversal.IsSliding, Is.False); Assert.That(stance.IsCrouched, Is.False);
                if (animator.GetComponent<PlayerCharacterSlideAnimationView>()?.Animations?.LowerBodyOnly == true)
                {
                    Assert.That(sawLowerBody, Is.True); Assert.That(sawFadeOut, Is.True);
                    Assert.That(animator.GetLayerWeight(animator.GetLayerIndex("LowerBodySlide")), Is.LessThan(.01f));
                }
            }
            finally { Time.captureDeltaTime = old; Object.Destroy(floor); }
        }

        [UnityTest]
        public IEnumerator CancellingDuringTheForwardEntryFadesBothLayersWithoutRewinding()
        {
            yield return CharacterViewTestContext.Load(); var c = new CharacterViewTestContext();
            var animator = c.Actor.GetComponentInChildren<Animator>();
            var playback = animator.GetComponent<PlayerCharacterSlideAnimationView>();
            if (playback == null || !playback.Animations.UsesForwardEntry) Assert.Ignore("Optional native forward entry is not selected.");
            var motor = c.Actor.GetComponent<PlayerCharacterMovement>(); CharacterViewTestContext.SetField(motor, "_input", null); motor.enabled = true;
            var traversal = c.Actor.GetComponent<PlayerCharacterTraversal>();
            var floor = new GameObject("Slide cancellation floor"); floor.transform.position = c.Actor.position + Vector3.down * 2.25f;
            floor.AddComponent<BoxCollider>().size = new Vector3(100, .5f, 100); Physics.SyncTransforms();
            float old = Time.captureDeltaTime; Time.captureDeltaTime = .02f;
            try
            {
                for (int i = 0; i < 15; i++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                for (int i = 0; i < 20; i++) { motor.Tick(Vector2.up, true, false, .02f); yield return null; }
                var input = new Crouch { SlidePressed = true }; CharacterViewTestContext.SetField(traversal, "_input", input);
                motor.Tick(Vector2.zero, false, false, .02f); input.SlidePressed = false; yield return null;
                for (int i = 0; i < 5; i++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                Assert.That(traversal.IsSliding, Is.True);
                int entry = animator.GetLayerIndex("LowerBodySlideEntry"), lower = animator.GetLayerIndex("LowerBodySlide");
                Assert.That(animator.GetLayerWeight(entry), Is.GreaterThan(.95f));
                float progress = animator.GetFloat("SlideTime"), weight = animator.GetLayerWeight(entry);
                traversal.Cancel();
                for (int i = 0; i < 6; i++)
                {
                    motor.Tick(Vector2.zero, false, false, .02f); yield return null;
                    Assert.That(animator.GetFloat("SlideTime"), Is.EqualTo(progress).Within(.0001f));
                    Assert.That(animator.GetLayerWeight(entry), Is.LessThanOrEqualTo(weight)); weight = animator.GetLayerWeight(entry);
                }
                Assert.That(animator.GetLayerWeight(entry), Is.Zero); Assert.That(animator.GetLayerWeight(lower), Is.Zero);
                playback.enabled = false;
                Assert.That(animator.GetLayerWeight(entry), Is.Zero);
            }
            finally { Time.captureDeltaTime = old; Object.Destroy(floor); }
        }

        [UnityTest]
        public IEnumerator SlideRecoveryDoesNotRevealASecondCrouchAcrossEquipmentAndMovement()
        {
            foreach (int slot in new[] { 1, 2, 3 })
            foreach (bool keepMoving in new[] { false, true })
            {
                yield return CharacterViewTestContext.Load(); var c = new CharacterViewTestContext();
                CharacterViewTestContext.SetField(c.Shooter, "_input", new Idle()); c.Shooter.enabled = true;
                c.Actor.GetComponent<PlayerCharacterLoadout>().TryEquip(slot);
                var animator = c.Actor.GetComponentInChildren<Animator>();
                if (animator.GetComponent<PlayerCharacterSlideAnimationView>()?.Animations?.LowerBodyOnly != true)
                    Assert.Ignore("Optional lower-body slide is not installed.");
                var motor = c.Actor.GetComponent<PlayerCharacterMovement>();
                CharacterViewTestContext.SetField(motor, "_input", null); motor.enabled = true;
                var traversal = c.Actor.GetComponent<PlayerCharacterTraversal>();
                var stance = c.Actor.GetComponent<PlayerCharacterStance>();
                var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                var floor = new GameObject("Slide recovery floor");
                floor.transform.position = c.Actor.position + Vector3.down * 2.25f;
                floor.AddComponent<BoxCollider>().size = new Vector3(100, .5f, 100); Physics.SyncTransforms();
                float old = Time.captureDeltaTime; Time.captureDeltaTime = .02f;
                try
                {
                    for (int n = 0; n < 20; n++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                    for (int n = 0; n < 25; n++) { motor.Tick(Vector2.up, true, false, .02f); yield return null; }
                    var input = new Crouch { SlidePressed = true };
                    CharacterViewTestContext.SetField(traversal, "_input", input);
                    float peak = float.NegativeInfinity, dip = 0f;
                    bool prepared = false;
                    for (int n = 0; n < 55; n++)
                    {
                        motor.Tick(keepMoving ? Vector2.up : Vector2.zero, keepMoving, false, .02f);
                        input.SlidePressed = false; yield return null;
                        if (traversal.IsSliding && traversal.Progress > .75f && traversal.Progress < .95f)
                        {
                            prepared = true;
                            Assert.That(stance.IsCrouched, Is.True, "Presentation must not stand the collision capsule early.");
                            Assert.That(animator.GetBool("Crouched"), Is.False, "Prepare standing behind the masked slide legs.");
                        }
                        // Read the preceding fully evaluated frame, including posture/weapon LateUpdate.
                        if (n >= 37 && n <= 48)
                        {
                            float height = hips.position.y - c.Actor.position.y;
                            peak = Mathf.Max(peak, height); dip = Mathf.Max(dip, peak - height);
                        }
                    }
                    Assert.That(prepared, Is.True);
                    Assert.That(dip, Is.LessThan(.32f), $"slot={slot}, moving={keepMoving}: slide recovery revealed crouch again");
                    Assert.That(stance.IsCrouched, Is.False);
                }
                finally { Time.captureDeltaTime = old; Object.Destroy(floor); }
            }
        }

        [UnityTest]
        public IEnumerator SlideRecoveryKeepsTheCrouchPoseWhenStandingIsBlocked()
        {
            yield return CharacterViewTestContext.Load(); var c = new CharacterViewTestContext();
            var animator = c.Actor.GetComponentInChildren<Animator>();
            if (animator.GetComponent<PlayerCharacterSlideAnimationView>()?.Animations?.LowerBodyOnly != true)
                Assert.Ignore("Optional lower-body slide is not installed.");
            var motor = c.Actor.GetComponent<PlayerCharacterMovement>();
            CharacterViewTestContext.SetField(motor, "_input", null); motor.enabled = true;
            var traversal = c.Actor.GetComponent<PlayerCharacterTraversal>();
            var stance = c.Actor.GetComponent<PlayerCharacterStance>();
            var floor = new GameObject("Blocked slide recovery floor");
            floor.transform.position = c.Actor.position + Vector3.down * 2.25f;
            floor.AddComponent<BoxCollider>().size = new Vector3(100, .5f, 100);
            var ceiling = new GameObject("Blocked slide recovery ceiling");
            ceiling.transform.position = c.Actor.position + Vector3.up * 1.2f;
            ceiling.AddComponent<BoxCollider>().size = new Vector3(100, .3f, 100);
            ceiling.SetActive(false); Physics.SyncTransforms();
            float old = Time.captureDeltaTime; Time.captureDeltaTime = .02f;
            try
            {
                for (int n = 0; n < 20; n++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                for (int n = 0; n < 25; n++) { motor.Tick(Vector2.up, true, false, .02f); yield return null; }
                var input = new Crouch { SlidePressed = true }; CharacterViewTestContext.SetField(traversal, "_input", input);
                motor.Tick(Vector2.zero, false, false, .02f); input.SlidePressed = false; yield return null;
                Assert.That(traversal.IsSliding, Is.True);
                ceiling.SetActive(true); Physics.SyncTransforms();
                bool sawRecovery = false;
                for (int n = 0; n < 50; n++)
                {
                    motor.Tick(Vector2.zero, false, false, .02f); yield return null;
                    if (traversal.Progress > .75f || !traversal.IsSliding)
                    {
                        sawRecovery = true;
                        Assert.That(animator.GetBool("Crouched"), Is.True);
                        Assert.That(stance.IsCrouched, Is.True);
                    }
                }
                Assert.That(sawRecovery, Is.True); Assert.That(traversal.IsSliding, Is.False);
            }
            finally { Time.captureDeltaTime = old; Object.Destroy(floor); Object.Destroy(ceiling); }
        }

        private sealed class PoseSample { public float CrouchHeight; public float MinimumHip = float.PositiveInfinity; }
        [UnityTest]
        public IEnumerator MaskedSlideKeepsTheOriginalLowPelvisInsteadOfTheCrouchPose()
        {
            var profile = AssetDatabase.LoadAssetAtPath<SlideAnimationSet>("Assets/ThirdParty/MixamoLocal/RunningSlide.asset");
            if (profile == null || !profile.LowerBodyOnly) yield break;
            foreach (int slot in new[] { PlayerCharacterLoadout.RifleSlot, PlayerCharacterLoadout.PistolSlot, PlayerCharacterLoadout.UnarmedSlot })
            {
                var masked = new PoseSample(); yield return SamplePelvis(slot, masked);
                Debug.Log($"SLIDE_POSE slot={slot} crouch={masked.CrouchHeight} masked={masked.MinimumHip}");
                Assert.That(masked.MinimumHip, Is.LessThan(masked.CrouchHeight - .3f), "The slide must visibly lower the pelvis beyond ordinary crouch.");
            }
        }
        private static IEnumerator SamplePelvis(int slot, PoseSample result)
        {
            yield return CharacterViewTestContext.Load(); var c = new CharacterViewTestContext();
            CharacterViewTestContext.SetField(c.Shooter, "_input", new Idle()); c.Shooter.enabled = true;
            c.Actor.GetComponent<PlayerCharacterLoadout>().TryEquip(slot);
            var animator = c.Actor.GetComponentInChildren<Animator>(); var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            var traversal = c.Actor.GetComponent<PlayerCharacterTraversal>(); var stance = c.Actor.GetComponent<PlayerCharacterStance>();
            var motor = c.Actor.GetComponent<PlayerCharacterMovement>(); CharacterViewTestContext.SetField(motor, "_input", null); motor.enabled = true;
            var floor = new GameObject("Slide pose comparison floor"); floor.transform.position = c.Actor.position + Vector3.down * 2.25f;
            floor.AddComponent<BoxCollider>().size = new Vector3(100, .5f, 100); Physics.SyncTransforms();
            float old = Time.captureDeltaTime; Time.captureDeltaTime = .02f;
            try
            {
                Assert.That(stance.TrySetCrouched(true), Is.True);
                for (int n = 0; n < 20; n++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                float feet = floor.transform.position.y + .25f; result.CrouchHeight = hips.position.y - feet;
                Assert.That(stance.TrySetCrouched(false), Is.True);
                for (int n = 0; n < 20; n++) { motor.Tick(Vector2.up, true, false, .02f); yield return null; }
                var input = new Crouch { SlidePressed = true }; CharacterViewTestContext.SetField(traversal, "_input", input);
                for (int n = 0; n < 44; n++)
                {
                    motor.Tick(Vector2.zero, false, false, .02f); input.SlidePressed = false; yield return null;
                    if (traversal.IsSliding && traversal.Progress > .3f && traversal.Progress < .55f)
                        result.MinimumHip = Mathf.Min(result.MinimumHip, hips.position.y - feet);
                }
            }
            finally
            {
                Time.captureDeltaTime = old; Object.Destroy(floor);
            }
        }

        [UnityTest]
        public IEnumerator MissingOptionalSetLeavesTheCurrentControllerUntouched()
        {
            yield return CharacterViewTestContext.Load(); var c = new CharacterViewTestContext();
            var animator = c.Actor.GetComponentInChildren<Animator>();
            var view = animator.GetComponent<PlayerCharacterSlideAnimationView>() ?? animator.gameObject.AddComponent<PlayerCharacterSlideAnimationView>();
            view.enabled = false;
            view.Initialize(animator, c.Shooter, null, c.Actor.GetComponent<PlayerCharacterTraversal>()); var original = animator.runtimeAnimatorController;
            view.enabled = true; yield return null;
            Assert.That(animator.runtimeAnimatorController, Is.SameAs(original));
        }
    }
}
#endif
