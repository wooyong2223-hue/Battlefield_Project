using System;
using System.IO;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using UnityEditor;
using UnityEngine;

namespace Battlefield.Editor.PlayerCharacter
{
    public static class MixamoSlideImporter
    {
        public const string Folder = "Assets/ThirdParty/MixamoLocal";
        public const string ProfilePath = Folder + "/RunningSlide.asset";
        public const string ModelPath = Folder + "/RunningSlide.fbx";
        [MenuItem("Tools/Battlefield/Animations/Mixamo/Import Running Slide")]
        public static void ImportRunningSlide()
        { Import(false); }
        [MenuItem("Tools/Battlefield/Animations/Mixamo/Restore Original Running Slide")]
        public static void RestoreOriginalRunningSlide() { Import(true); }
        [MenuItem("Tools/Battlefield/Animations/Mixamo/Use Running Slide Lower Body")]
        public static void UseRunningSlideLowerBody()
        {
            Import(false);
            var profile = AssetDatabase.LoadAssetAtPath<SlideAnimationSet>(ProfilePath);
            if (!profile.IsValid) throw new InvalidOperationException("Import a valid slide animation set before enabling lower-body playback.");
            var data = new SerializedObject(profile);
            data.FindProperty("_lowerBodyOnly").boolValue = true;
            data.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(profile);
        }
        [MenuItem("Tools/Battlefield/Animations/Mixamo/Use Forward Slide Entry")]
        public static void UseForwardSlideEntry()
        {
            Import(false);
            var profile = AssetDatabase.LoadAssetAtPath<SlideAnimationSet>(ProfilePath);
            var entry = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/ThirdParty/Quaternius/Animations/Slide_Start.anim");
            var original = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().Single(c => c.name == "RunningSlide");
            if (entry == null) throw new InvalidOperationException("Import Quaternius Slide_Start before selecting the forward entry.");
            Undo.RecordObject(profile, "Use native forward slide entry");
            var data = new SerializedObject(profile);
            data.FindProperty("_start").objectReferenceValue = entry;
            data.FindProperty("_loop").objectReferenceValue = original;
            data.FindProperty("_exit").objectReferenceValue = original;
            data.FindProperty("_lowerBodyOnly").boolValue = true;
            data.ApplyModifiedProperties(); AssetDatabase.SaveAssetIfDirty(profile);
        }
        private static void Import(bool restoreOriginal)
        {
            string source = Path.GetFullPath(Path.Combine(Application.dataPath, "../LocalAssets/Mixamo/Running Slide.fbx"));
            if (!File.Exists(source)) throw new FileNotFoundException("Place Running Slide.fbx in LocalAssets/Mixamo first.", source);
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/ThirdParty", "MixamoLocal");
            string target = Path.GetFullPath(Path.Combine(Application.dataPath, "../" + ModelPath));
            if (!File.Exists(target) || !File.ReadAllBytes(source).SequenceEqual(File.ReadAllBytes(target))) File.Copy(source, target, true);
            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
            var existing = AssetDatabase.LoadAssetAtPath<SlideAnimationSet>(ProfilePath);
            if (restoreOriginal || (existing == null && importer.clipAnimations.Length == 0))
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = true; importer.animationCompression = ModelImporterAnimationCompression.Off;
                importer.importCameras = false; importer.importLights = false;
                var take = importer.defaultClipAnimations.First();
                // Keep one complete source take and its authored orientation; no segmented cuts or body-heading correction.
                importer.clipAnimations = new[] { new ModelImporterClipAnimation
                {
                    name = "RunningSlide", takeName = take.takeName,
                    firstFrame = take.firstFrame, lastFrame = take.lastFrame,
                    loopTime = false, loopPose = false, lockRootRotation = true, keepOriginalOrientation = true,
                    lockRootHeightY = true, keepOriginalPositionY = true, heightFromFeet = false,
                    lockRootPositionXZ = false, keepOriginalPositionXZ = false
                }};
                importer.SaveAndReimport();
            }
            var assets = AssetDatabase.LoadAllAssetsAtPath(ModelPath);
            var avatar = assets.OfType<Avatar>().FirstOrDefault();
            if (avatar == null || !avatar.isValid || !avatar.isHuman) throw new InvalidOperationException("Running Slide must import with a valid Humanoid avatar.");
            var complete = assets.OfType<AnimationClip>().SingleOrDefault(c => c.name == "RunningSlide");
            var replacements = complete != null ? new[] { complete, complete, complete } :
                new[] { "RunningSlide_Start", "RunningSlide_Loop", "RunningSlide_Exit" }
                    .Select(n => assets.OfType<AnimationClip>().Single(c => c.name == n)).ToArray();
            if (replacements.Any(c => !c.humanMotion)) throw new InvalidOperationException("Running Slide clips must be Humanoid.");
            if (existing == null || restoreOriginal)
            {
                if (existing == null) { existing = ScriptableObject.CreateInstance<SlideAnimationSet>(); AssetDatabase.CreateAsset(existing, ProfilePath); }
                var data = new SerializedObject(existing);
                var originals = new[] { "Slide_Start", "Slide_Loop", "Slide_Exit" };
                var fields = new[] { "Start", "Loop", "Exit" };
                for (int i = 0; i < 3; i++)
                {
                    data.FindProperty("_original" + fields[i]).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/ThirdParty/Quaternius/Animations/" + originals[i] + ".anim");
                    data.FindProperty("_" + fields[i].ToLowerInvariant()).objectReferenceValue = replacements[i];
                }
                data.FindProperty("_lowerBodyOnly").boolValue = !restoreOriginal;
                data.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(existing);
            }
            Debug.Log("Mixamo Running Slide ready: " + ProfilePath + ". Existing profile/import settings are preserved on re-import.");
        }
    }
}
