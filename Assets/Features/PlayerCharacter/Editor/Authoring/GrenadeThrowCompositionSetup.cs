using System;
using System.IO;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Battlefield.Editor.PlayerCharacter
{
    // Explicit opt-in authoring. Original FBX files and the old grenade clip are preserved.
    public static class GrenadeThrowCompositionSetup
    {
        private const string Output = "Assets/Features/PlayerCharacter/Animation/Tactical";
        private const string BaseController = "Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller";

        [MenuItem("Tools/Battlefield/Animations/Mixamo/Apply Throw With Throw Object Left Arm")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Apply the throw composition outside Play Mode.");
            foreach (string name in new[] { "Throw", "Throw Object" })
                if (!File.Exists("LocalAssets/Mixamo/" + name + ".fbx"))
                    throw new FileNotFoundException("Missing user animation: " + name);

            var grenade = AssetDatabase.LoadAssetAtPath<GrenadeDefinition>(TacticalActionImporter.Folder + "/FragGrenade.asset");
            var profile = AssetDatabase.LoadAssetAtPath<TacticalAnimationSet>(TacticalActionImporter.ProfilePath);
            if (grenade == null || profile == null) throw new InvalidOperationException("Import grenade assets first.");
            var throwing = Import("Throw");
            var left = Import("Throw Object");
            string placeholderPath = Output + "/ThrowLeftArmPlaceholder.anim";
            if (!File.Exists(placeholderPath)) AssetDatabase.CopyAsset(Output + "/ThrowPlaceholder.anim", placeholderPath);
            var placeholder = AssetDatabase.LoadAssetAtPath<AnimationClip>(placeholderPath);
            string maskPath = Output + "/GrenadeLeftArm.mask";
            var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);
            if (mask == null)
            {
                mask = new AvatarMask { name = "GrenadeLeftArm" };
                for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                    mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
                mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
                mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, true);
                AssetDatabase.CreateAsset(mask, maskPath);
            }
            foreach (string path in new[] { BaseController, MixamoArmedLocomotionImporter.RifleController, MixamoArmedLocomotionImporter.PistolController })
            {
                var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                if (controller == null) throw new InvalidOperationException("Missing controller: " + path);
                if (controller.layers.Any(l => l.name == "GrenadeLeftArm")) continue;
                var machine = new AnimatorStateMachine { name = "GrenadeLeftArm", hideFlags = HideFlags.HideInHierarchy };
                AssetDatabase.AddObjectToAsset(machine, controller);
                var state = machine.AddState("Throw Object");
                state.motion = placeholder;
                state.timeParameterActive = true;
                state.timeParameter = "ThrowTime";
                state.writeDefaultValues = false;
                machine.defaultState = state;
                controller.AddLayer(new AnimatorControllerLayer
                {
                    name = "GrenadeLeftArm", defaultWeight = 0f, avatarMask = mask,
                    blendingMode = AnimatorLayerBlendingMode.Override, stateMachine = machine
                });
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssetIfDirty(controller);
            }
            var data = new SerializedObject(grenade);
            data.FindProperty("_throwAnimation").objectReferenceValue = throwing;
            data.FindProperty("_leftThrowAnimation").objectReferenceValue = left;
            data.ApplyModifiedPropertiesWithoutUndo();
            data = new SerializedObject(profile);
            data.FindProperty("_leftThrowPlaceholder").objectReferenceValue = placeholder;
            data.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log($"GRENADE_COMPOSITION_APPLIED base={throwing.name} length={throwing.length} left={left.name} length={left.length}");
        }

        private static AnimationClip Import(string name)
        {
            string path = TacticalActionImporter.Folder + "/" + name + ".fbx";
            bool first = !File.Exists(path);
            if (first) File.Copy("LocalAssets/Mixamo/" + name + ".fbx", path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            if (first)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importCameras = importer.importLights = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.animationCompression = ModelImporterAnimationCompression.Off;
                var take = importer.defaultClipAnimations.First();
                importer.clipAnimations = new[] { new ModelImporterClipAnimation
                {
                    name = name, takeName = take.takeName, firstFrame = take.firstFrame, lastFrame = take.lastFrame,
                    loopTime = false, loopPose = false, lockRootRotation = true, keepOriginalOrientation = true,
                    lockRootHeightY = false, keepOriginalPositionY = false, heightFromFeet = true,
                    lockRootPositionXZ = false, keepOriginalPositionXZ = true
                }};
                importer.SaveAndReimport();
            }
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var avatar = assets.OfType<Avatar>().FirstOrDefault();
            var clip = assets.OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__"));
            if (avatar == null || !avatar.isValid || !avatar.isHuman || clip == null || !clip.humanMotion)
                throw new InvalidOperationException("Invalid Humanoid animation: " + name);
            return clip;
        }
    }
}
