#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using Battlefield.Features.Training;
using Battlefield.Features.PlayerCharacter;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerCharacterAnimationTests
    {
        [Test]
        public void JumpStatesUseStarterAssetsMotionsAndKeepLandingDuration()
        {
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller");
            string[] states={"Jump","InAir","Land"},names={"JumpStart","InAir","JumpLand"};
            for(int i=0;i<3;i++)
            {
                string file=i==1?"Jump--InAir.anim.fbx":"Jump--Jump.anim.fbx";
                var original=AssetDatabase.LoadAllAssetsAtPath("Assets/ThirdParty/UnityStarterAssets/Animations/"+file).OfType<AnimationClip>().Single(c=>c.name==names[i]);
                var state=controller.layers[0].stateMachine.states.Single(s=>s.state.name==states[i]).state;
                var clip=(AnimationClip)state.motion;
                Assert.That(clip.name,Is.EqualTo("Starter"+names[i]));Assert.That(clip.humanMotion,Is.True);
                Assert.That(clip.length,Is.EqualTo(original.length).Within(.00001f));Assert.That(clip.isLooping,Is.EqualTo(original.isLooping));
                Assert.That(AnimationUtility.GetAnimationEvents(clip).Any(e=>e.functionName=="OnLand"),Is.False);
                Assert.That(state.speed,Is.EqualTo(1f).Within(.00001f));
                if(i==2){Assert.That(clip.length/state.speed,Is.EqualTo(.6333333f).Within(.00001f));Assert.That(AnimationUtility.GetAnimationEvents(original).Any(e=>e.functionName=="OnLand"),Is.True);}
            }
        }

        [UnityTest]
        public IEnumerator ArmatureUsesAHumanoidRigAndPreservesTheExistingCharacterSetup()
        {
            yield return LoadCharacter();
            PlayerCharacterMovement movement = Object.FindObjectsByType<PlayerCharacterMovement>().Single();
            Animator animator = movement.GetComponentInChildren<Animator>();
            Assert.That(animator, Is.Not.Null);
            Assert.That(animator.avatar.isHuman && animator.avatar.isValid, Is.True);
            Assert.That(animator.applyRootMotion, Is.False);
            Assert.That(animator.runtimeAnimatorController, Is.Not.Null);
            Assert.That(animator.GetComponent<PlayerCharacterAnimationView>(), Is.Not.Null);
            Transform visual = movement.transform.Find("Visual");
            Assert.That(visual.localScale, Is.EqualTo(new Vector3(2f, 4f, 1f)));
            Assert.That(visual.GetComponent<MeshRenderer>().enabled, Is.False);
            Assert.That(visual.GetComponent<MeshFilter>(), Is.Not.Null);
            Assert.That(movement.GetComponent<CharacterController>().height, Is.EqualTo(4f));
            Assert.That(movement.MoveSpeed, Is.EqualTo(8f));
            Assert.That(movement.SprintSpeed, Is.EqualTo(12f));
            Assert.That(animator.transform.localScale.x, Is.EqualTo(2.2703f).Within(0.001f));
            SkinnedMeshRenderer renderer = animator.GetComponentInChildren<SkinnedMeshRenderer>();
            Assert.That(renderer.sharedMaterials.Length, Is.EqualTo(3));
            Assert.That(renderer.sharedMaterials.All(material => material != null && material.shader != null), Is.True);
        }

        [UnityTest]
        public IEnumerator MovementDrivesIdleWalkRunJumpFallAndLandingWithoutRootMotion()
        {
            yield return LoadCharacter();
            PlayerCharacterMovement movement = Object.FindObjectsByType<PlayerCharacterMovement>().Single();
            Animator animator = movement.GetComponentInChildren<Animator>();
            PlayerCharacterAnimationView view = animator.GetComponent<PlayerCharacterAnimationView>();
            // Drive the real movement with deterministic steps; keep the presentation active.
            typeof(PlayerCharacterMovement).GetField("_input", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(movement, null);
            typeof(PlayerCharacterAnimationView).GetField("_speedDamping", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(view, 0f);
            CharacterController body = movement.GetComponent<CharacterController>();
            body.enabled = false;
            movement.transform.position = new Vector3(10000f, 10002.02f, 10000f);
            body.enabled = true;
            GameObject floor = new("Animation Test Floor");
            floor.transform.position = new Vector3(10000f, 9999.75f, 10000f);
            floor.AddComponent<BoxCollider>().size = new Vector3(100f, 0.5f, 100f);
            Physics.SyncTransforms();
            try
            {
                yield return Advance(movement, animator, Vector2.zero, false, 10);
                Assert.That(animator.GetBool("Grounded"), Is.True);
                Assert.That(animator.GetFloat("Speed"), Is.EqualTo(0f).Within(0.01f));
                Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"), Is.True);
                Vector3 localPosition = animator.transform.localPosition;
                Quaternion localRotation = animator.transform.localRotation;

                bool mixamo = UnityEditor.AssetDatabase.LoadAssetAtPath<ArmedLocomotionAnimationSet>("Assets/ThirdParty/MixamoLocal/ArmedLocomotion/ArmedLocomotion.asset") != null;
                yield return Advance(movement, animator, Vector2.up, false, 15);
                Assert.That(animator.GetFloat("Speed"), Is.EqualTo(1f).Within(0.03f));
                Assert.That(animator.GetCurrentAnimatorClipInfo(0).Any(info => info.clip.name == (mixamo ? "rifle run" : "Jog_Fwd_Loop") && info.weight > 0.9f), Is.True);
                yield return Advance(movement, animator, Vector2.up, true, 15);
                Assert.That(animator.GetFloat("Speed"), Is.EqualTo(2f).Within(0.03f));
                Assert.That(animator.GetCurrentAnimatorClipInfo(0).Any(info => info.clip.name == (mixamo ? "rifle run" : "LocomotionSprint") && info.weight > 0.9f), Is.True);

                bool sawJump = false, sawAir = false, sawLand = false;
                for (int frame = 0; frame < 90; frame++)
                {
                    movement.Tick(Vector2.zero, false, frame == 0, 0.02f);
                    yield return null;
                    animator.Update(0.02f);
                    AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                    sawJump |= state.IsName("Jump");
                    sawAir |= state.IsName("InAir");
                    sawLand |= state.IsName("Land");
                }
                Assert.That(sawJump && sawAir && sawLand, Is.True, "Expected Jump → InAir → Land transitions.");
                Assert.That(movement.IsGrounded, Is.True);
                Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"), Is.True);
                Assert.That(Vector3.Distance(animator.transform.localPosition, localPosition), Is.LessThan(0.001f));
                Assert.That(Quaternion.Angle(animator.transform.localRotation, localRotation), Is.LessThan(0.01f));
            }
            finally { Object.Destroy(floor); }
        }

        private static IEnumerator LoadCharacter()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/SampleScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            foreach (DummyShooter shooter in Object.FindObjectsByType<DummyShooter>()) shooter.enabled = false;
            foreach (DummyMissileShooter shooter in Object.FindObjectsByType<DummyMissileShooter>()) shooter.enabled = false;
            foreach (PlayerCharacterCombatController shooter in Object.FindObjectsByType<PlayerCharacterCombatController>()) shooter.enabled = false;
        }

        private static IEnumerator Advance(PlayerCharacterMovement movement, Animator animator, Vector2 move, bool sprint, int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                movement.Tick(move, sprint, false, 0.02f);
                yield return null;
                animator.Update(0.02f);
            }
        }
    }
}
#endif
