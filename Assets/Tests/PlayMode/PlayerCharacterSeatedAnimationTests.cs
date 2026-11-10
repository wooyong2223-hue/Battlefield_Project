#if UNITY_EDITOR
using System.Collections;
using Battlefield.Features.PlayerCharacter;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public sealed class PlayerCharacterSeatedAnimationTests
    {
        [UnityTest]
        public IEnumerator SeatedSourceKeepsItsAuthoredLimbRotationsAfterCockpitFitting()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext(); context.PrepareEntry();
            var source = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Features/PlayerCharacter/Prefabs/PlayerCharacterArmature.prefab"));
            var animator = source.GetComponent<Animator>(); animator.applyRootMotion = false; animator.Rebind(); animator.Update(0f);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/ThirdParty/Quaternius/Animations/SeatedIdle.anim");
            var graph = PlayableGraph.Create("Seated source comparison"); graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            try
            {
                var motion = AnimationClipPlayable.Create(graph, clip); motion.SetApplyFootIK(false); motion.SetApplyPlayableIK(false);
                var output = AnimationPlayableOutput.Create(graph, "Source", animator); output.SetSourcePlayable(motion); graph.Play(); graph.Evaluate(0f);
                Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
                foreach (var bone in new[] {HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg, HumanBodyBones.LeftLowerLeg,
                    HumanBodyBones.RightLowerLeg, HumanBodyBones.LeftUpperArm, HumanBodyBones.RightUpperArm,
                    HumanBodyBones.LeftLowerArm, HumanBodyBones.RightLowerArm, HumanBodyBones.LeftHand, HumanBodyBones.RightHand})
                    Assert.That(Quaternion.Angle(animator.GetBoneTransform(bone).localRotation, context.Animator.GetBoneTransform(bone).localRotation),
                        Is.LessThan(.1f), bone.ToString());
                Assert.That(Vector3.Distance(context.Animator.GetBoneTransform(HumanBodyBones.Hips).position, context.Seat.SeatPoint.position), Is.LessThan(.002f));
                context.Seat.TryExit();
            }
            finally { graph.Destroy(); Object.Destroy(source); }
        }

        [UnityTest]
        public IEnumerator AuthoredIdleMovesPausesAndKeepsTheHipsAtTheSeat()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            context.PrepareEntry();
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            yield return null;
            Assert.That(context.Pose.IsUsingAnimation, Is.True);
            Assert.That(context.Animator.enabled, Is.True);
            var chest = context.Animator.GetBoneTransform(HumanBodyBones.UpperChest);
            var head = context.Animator.GetBoneTransform(HumanBodyBones.Head);
            Quaternion chestStart = chest.localRotation, headStart = head.localRotation;
            float motion = 0f;
            for (int sample = 0; sample < 12; sample++)
            {
                yield return new WaitForSeconds(.08f);
                motion = Mathf.Max(motion, Quaternion.Angle(chestStart, chest.localRotation), Quaternion.Angle(headStart, head.localRotation));
                Assert.That(Vector3.Distance(context.Animator.GetBoneTransform(HumanBodyBones.Hips).position, context.Seat.SeatPoint.position), Is.LessThan(.002f));
            }
            Debug.Log("SEATED_AUTHORED_MOTION degrees=" + motion);
            Assert.That(motion, Is.GreaterThan(.05f), "Authored breathing/head motion must survive cockpit fitting.");
            float timeScale = Time.timeScale;
            try
            {
                Time.timeScale = 0f;
                Quaternion paused = head.localRotation;
                for (int frame = 0; frame < 6; frame++) yield return null;
                Assert.That(Quaternion.Angle(paused, head.localRotation), Is.LessThan(.01f));
            }
            finally { Time.timeScale = timeScale; }
            Assert.That(context.Seat.TryExit(), Is.True);
            Assert.That(context.Pose.IsUsingAnimation, Is.False);
        }

        [UnityTest]
        public IEnumerator ExitRestoresAnimatorSettingsAndTheCharacterCanBoardAgain()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            context.PrepareEntry();
            var controller = context.Animator.runtimeAnimatorController;
            context.Animator.applyRootMotion = true;
            context.Animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            yield return null;
            Assert.That(context.Animator.applyRootMotion, Is.False);
            Assert.That(context.Animator.cullingMode, Is.EqualTo(AnimatorCullingMode.AlwaysAnimate));
            Assert.That(context.Seat.TryExit(), Is.True);
            Assert.That(context.Pose.IsUsingAnimation, Is.False);
            Assert.That(context.Animator.runtimeAnimatorController, Is.SameAs(controller));
            Assert.That(context.Animator.enabled, Is.True);
            Assert.That(context.Animator.applyRootMotion, Is.True);
            Assert.That(context.Animator.cullingMode, Is.EqualTo(AnimatorCullingMode.CullUpdateTransforms));
            context.Animator.applyRootMotion = false;
            yield return null;
            context.PrepareEntry();
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            yield return null;
            Assert.That(context.Pose.IsUsingAnimation, Is.True);
        }

        [UnityTest]
        public IEnumerator MissingClipRetainsTheExistingManualSeatPose()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            CharacterViewTestContext.SetField(context.Pose, "_seatedAnimation", null);
            var frame = context.Actor.gameObject.AddComponent<SeatedFrameProbe>();
            frame.Hips = context.Animator.GetBoneTransform(HumanBodyBones.Hips);
            frame.Seat = context.Seat.SeatPoint;
            context.PrepareEntry();
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            yield return null;
            Assert.That(context.Pose.IsUsingAnimation, Is.False);
            Assert.That(context.Animator.enabled, Is.False);
            // Compare the seat and fitted hips from the same rendered frame, after pose LateUpdate.
            Assert.That(frame.Captured, Is.True);
            Assert.That(frame.Distance, Is.LessThan(.002f));
            Assert.That(context.Seat.TryExit(), Is.True);
            Assert.That(context.Animator.enabled, Is.True);
        }

        [UnityTest]
        public IEnumerator NonHumanoidClipRejectsEntryBeforeDisablingWalking()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            var invalid = new AnimationClip();
            try
            {
                CharacterViewTestContext.SetField(context.Pose, "_seatedAnimation", invalid);
                context.PrepareEntry();
                Assert.That(context.Seat.TryEnter(context.Occupant), Is.False);
                Assert.That(context.Pose.IsSeated, Is.False);
                Assert.That(context.Animator.enabled, Is.True);
                Assert.That(context.Actor.GetComponent<CharacterController>().enabled, Is.True);
                Assert.That(context.Actor.GetComponent<PlayerCharacterMovement>().enabled, Is.True);
            }
            finally { Object.Destroy(invalid); }
        }
    }

    [DefaultExecutionOrder(10000)]
    public sealed class SeatedFrameProbe : MonoBehaviour
    {
        public Transform Hips, Seat;
        public bool Captured { get; private set; }
        public float Distance { get; private set; }
        private void LateUpdate()
        {
            if (Hips == null || Seat == null) return;
            Distance = Vector3.Distance(Hips.position, Seat.position);
            Captured = true;
        }
    }
}
#endif
