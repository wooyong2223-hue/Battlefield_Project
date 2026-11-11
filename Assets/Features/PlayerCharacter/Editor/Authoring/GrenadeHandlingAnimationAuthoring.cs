using System;
using System.Collections.Generic;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Battlefield.Editor.PlayerCharacter
{
    // Authoring runs only in the Editor. The player evaluates ordinary Humanoid clips.
    public static class GrenadeHandlingAnimationAuthoring
    {
        private const string Folder = "Assets/Features/PlayerCharacter/Animation/Tactical";
        private const string BaseController = "Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller";

        [MenuItem("Tools/Battlefield/Animations/Create Missing Grenade Handling Clips")]
        public static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Author grenade clips outside Play Mode.");
            var ready = BuildReadyPose();
            var equip = CreateClip("GrenadeEquip", false, ready);
            var idle = CreateClip("GrenadeIdle", true, ready);
            var grenade = AssetDatabase.LoadAssetAtPath<GrenadeDefinition>(TacticalActionImporter.Folder + "/FragGrenade.asset");
            var profile = AssetDatabase.LoadAssetAtPath<TacticalAnimationSet>(TacticalActionImporter.ProfilePath);
            if (grenade == null || profile == null)
                throw new InvalidOperationException("Import the existing grenade and mantle assets first.");
            AssignMissing(grenade, "_equipAnimation", equip);
            AssignMissing(grenade, "_idleAnimation", idle);
            AssignMissing(profile, "_equipPlaceholder", equip);
            AssignMissing(profile, "_idlePlaceholder", idle);
            foreach (string path in new[] { BaseController, MixamoArmedLocomotionImporter.RifleController, MixamoArmedLocomotionImporter.PistolController })
            {
                var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                if (controller == null) continue;
                var layer = controller.layers.FirstOrDefault(l => l.name == "GrenadeUpperBody");
                if (layer == null) throw new InvalidOperationException("Missing grenade layer: " + path);
                if (!controller.parameters.Any(p => p.name == "GrenadeHandlingTime"))
                    controller.AddParameter("GrenadeHandlingTime", AnimatorControllerParameterType.Float);
                AddState(layer.stateMachine, "Equip", equip);
                AddState(layer.stateMachine, "Idle", idle);
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssetIfDirty(controller);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("GRENADE_HANDLING_AUTHORING_OK equip=" + equip.length + " idle=" + idle.length);
        }

        private static void AddState(AnimatorStateMachine machine, string name, AnimationClip clip)
        {
            if (machine.states.Any(s => s.state.name == name)) return;
            var state = machine.AddState(name);
            state.motion = clip;
            state.timeParameterActive = true;
            state.timeParameter = "GrenadeHandlingTime";
        }

        private static void AssignMissing(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var data = new SerializedObject(target);
            if (data.FindProperty(field).objectReferenceValue != null) return;
            data.FindProperty(field).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(target);
        }

        private static AnimationClip CreateClip(string name, bool looping, Dictionary<string, float> pose)
        {
            string path = Folder + "/" + name + ".anim";
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null) return existing; // Preserve hand-edited keys on subsequent imports.
            var clip = new AnimationClip { name = name, frameRate = 60f };
            float duration = looping ? 2f : .6f;
            foreach (string muscle in HumanTrait.MuscleName)
            {
                if (muscle.Contains("Leg") || muscle.Contains("Foot") || muscle.Contains("Toes")) continue;
                string property = MuscleProperty(muscle);
                float ready = pose[muscle];
                AnimationCurve curve;
                if (looping)
                {
                    // Closed, low-amplitude breathing. Feet and motor remain on the base layer.
                    float breath = muscle == "Chest Front-Back" ? .015f : muscle == "Right Arm Down-Up" ? .008f : 0f;
                    curve = new AnimationCurve(new Keyframe(0, ready), new Keyframe(1, ready + breath), new Keyframe(duration, ready));
                }
                else
                {
                    float low = muscle == "Right Arm Down-Up" ? -.95f : muscle == "Right Arm Front-Back" ? -.12f :
                        muscle == "Right Forearm Stretch" ? .55f : muscle == "Right Forearm Twist In-Out" ? 0f : ready;
                    curve = new AnimationCurve(new Keyframe(0, low), new Keyframe(.16f, low), new Keyframe(.43f, Mathf.Lerp(low, ready, .85f)), new Keyframe(duration, ready));
                }
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), property), curve);
            }
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = looping;
            settings.loopBlend = looping;
            settings.startTime = 0f;
            settings.stopTime = duration;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static string MuscleProperty(string name)
        {
            foreach (string side in new[] { "Left", "Right" })
            foreach (string finger in new[] { "Thumb", "Index", "Middle", "Ring", "Little" })
            {
                string prefix = side + " " + finger + " ";
                if (name.StartsWith(prefix)) return side + "Hand." + finger + "." + name.Substring(prefix.Length);
            }
            return name;
        }

        private static float Ready(string name)
        {
            switch (name)
            {
                case "Right Arm Down-Up": return -.55f;
                case "Right Arm Front-Back": return -.5f;
                case "Right Arm Twist In-Out": return -.1f;
                case "Right Forearm Stretch": return -.65f;
                case "Right Forearm Twist In-Out": return .1f;
                case "Right Hand Down-Up": return -.08f;
                case "Left Arm Down-Up": return -.95f;
                case "Left Forearm Stretch": return .55f;
            }
            if (name.StartsWith("Right ") && name.EndsWith("Stretched")) return name.Contains("Thumb") ? -.35f : -.65f;
            if (name.StartsWith("Left ") && name.EndsWith("Stretched")) return -.15f;
            return 0f;
        }

        private static Dictionary<string, float> BuildReadyPose()
        {
            const string modelPath = "Assets/ThirdParty/UnityStarterAssets/Models/Armature.fbx";
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (source == null) throw new InvalidOperationException("Missing authoring rig: " + modelPath);
            var rig = UnityEngine.Object.Instantiate(source);
            rig.hideFlags = HideFlags.HideAndDontSave;
            HumanPoseHandler handler = null;
            try
            {
                rig.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                var animator = rig.GetComponent<Animator>();
                if (animator == null || !animator.isHuman) throw new InvalidOperationException("Authoring rig must be Humanoid.");
                handler = new HumanPoseHandler(animator.avatar, rig.transform);
                var pose = new HumanPose(); handler.GetHumanPose(ref pose);
                var names = HumanTrait.MuscleName;
                for (int i = 0; i < names.Length; i++) pose.muscles[i] = Ready(names[i]);
                handler.SetHumanPose(ref pose);
                Transform upper = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                Transform lower = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                Transform hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                Transform chest = animator.GetBoneTransform(HumanBodyBones.Chest);
                float scale = animator.humanScale;
                Vector3 target = chest.position + new Vector3(.18f, .12f, .36f) * scale;
                Vector3 hint = upper.position + new Vector3(.18f, -.35f, .08f) * scale;
                Vector3 start = upper.position;
                float first = Vector3.Distance(start, lower.position), second = Vector3.Distance(lower.position, hand.position);
                float distance = Mathf.Clamp(Vector3.Distance(start, target), Mathf.Abs(first - second) + .001f, first + second - .001f);
                Vector3 direction = (target - start).normalized;
                Vector3 bend = Vector3.ProjectOnPlane(hint - start, direction).normalized;
                float along = (first * first - second * second + distance * distance) / (2f * distance);
                Vector3 elbow = start + direction * along + bend * Mathf.Sqrt(Mathf.Max(0f, first * first - along * along));
                upper.rotation = Quaternion.FromToRotation(lower.position - start, elbow - start) * upper.rotation;
                lower.rotation = Quaternion.FromToRotation(hand.position - lower.position, target - lower.position) * lower.rotation;
                handler.GetHumanPose(ref pose);
                var result = new Dictionary<string, float>();
                for (int i = 0; i < names.Length; i++) result.Add(names[i], pose.muscles[i]);
                return result;
            }
            finally { handler?.Dispose(); UnityEngine.Object.DestroyImmediate(rig); }
        }
    }
}
