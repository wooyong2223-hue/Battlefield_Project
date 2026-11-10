#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.TestTools;
namespace Battlefield.Tests.PlayMode
{
    public sealed class NativeSlidePoseTests
    {
        [UnityTest]
        public IEnumerator NativeLegLayersMatchSourceEntryAndExitAndDoNotSpinDuringReturn()
        {
            yield return CharacterViewTestContext.Load();
            var profile = AssetDatabase.LoadAssetAtPath<SlideAnimationSet>("Assets/ThirdParty/MixamoLocal/RunningSlide.asset");
            if (profile == null) Assert.Ignore("Optional local Mixamo source is not installed.");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/UnityStarterAssets/Models/Armature.fbx");
            var targetObject = Object.Instantiate(prefab); var sourceObject = Object.Instantiate(prefab);
            var target = targetObject.GetComponent<Animator>(); var source = sourceObject.GetComponent<Animator>();
            var controller = new AnimatorOverrideController(AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller"));
            controller["Slide_Start"] = profile.Start; controller["Slide_Loop"] = profile.Loop; controller["Slide_Exit"] = profile.Exit;
            target.runtimeAnimatorController = controller; target.applyRootMotion = source.applyRootMotion = false;
            target.cullingMode = source.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            target.Update(0f);
            int lower = target.GetLayerIndex("LowerBodySlide"), entry = target.GetLayerIndex("LowerBodySlideEntry");
            for (int i = 1; i < target.layerCount; i++) target.SetLayerWeight(i, 0f);
            target.SetBool("Grounded", true);
            var bones = new[] {HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg, HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg, HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot};
            var graph = PlayableGraph.Create("External source slide comparison");
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            var output = AnimationPlayableOutput.Create(graph, "Raw source", source);
            Quaternion[] previous = null; float maximumStep = 0f, maximumBlendStep = 0f; float peakProgress = 0f; HumanBodyBones peakBone = HumanBodyBones.Hips;
            try
            {
                for (int frame = 1; frame <= 99; frame++)
                {
                    float p = frame / 100f;
                    float mix = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.5f, .8f, p));
                    target.SetLayerWeight(lower, mix > 0f ? 1f : 0f); target.SetLayerWeight(entry, 1f - mix);
                    target.SetFloat("SlideTime", p); target.SetFloat("SlideEntryTime", Mathf.Clamp01(p / .5f));
                    target.Update(0f);
                    var pose = bones.Select(b => target.GetBoneTransform(b).localRotation).ToArray();
                    if (previous != null) for (int i = 0; i < pose.Length; i++) { float step = Quaternion.Angle(previous[i], pose[i]); if(p >= .51f && p <= .8f) maximumBlendStep = Mathf.Max(maximumBlendStep, step); if(step > maximumStep) {maximumStep=step; peakProgress=p; peakBone=bones[i];} }
                    previous = pose;
                    if (p > .49f && p < .81f) continue;
                    var clip = p <= .49f ? profile.Start : profile.Loop;
                    var playable = AnimationClipPlayable.Create(graph, clip);
                    playable.SetApplyFootIK(false); playable.SetApplyPlayableIK(false);
                    playable.SetTime((p <= .49f ? Mathf.Clamp01(p / .5f) : p) * clip.length);
                    output.SetSourcePlayable(playable); graph.Play(); graph.Evaluate(0f);
                    for (int i = 0; i < bones.Length; i++)
                        Assert.That(Quaternion.Angle(pose[i], source.GetBoneTransform(bones[i]).localRotation), Is.LessThan(.25f), bones[i] + " p=" + p);
                    playable.Destroy();
                }
                Debug.Log("NATIVE_SLIDE maximum leg rotation per 1%=" + maximumStep + " p=" + peakProgress + " bone=" + peakBone + " return blend maximum=" + maximumBlendStep);
                Assert.That(maximumBlendStep, Is.LessThan(20f), "Native return blending must not flip a leg between adjacent samples; source-only entry/exit poses are compared directly above.");
            }
            finally { graph.Destroy(); Object.Destroy(targetObject); Object.Destroy(sourceObject); Object.Destroy(controller); }
        }
    }
}
#endif
