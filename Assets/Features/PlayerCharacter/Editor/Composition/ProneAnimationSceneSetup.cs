using System;
using System.IO;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Battlefield.Editor.PlayerCharacter
{
    public static class ProneAnimationSceneSetup
    {
        public const string Folder = "Assets/ThirdParty/MixamoLocal/Prone";
        public const string ProfilePath = Folder + "/ProneAnimations.asset";
        [MenuItem("Tools/Battlefield/Animations/Mixamo/Import Prone Animations")]
        public static void Import()
        {
            string source = Path.GetFullPath(Path.Combine(Application.dataPath, "../LocalAssets/Mixamo"));
            if (!Directory.Exists(source)) throw new DirectoryNotFoundException(source);
            if (!AssetDatabase.IsValidFolder(MixamoSlideImporter.Folder)) AssetDatabase.CreateFolder("Assets/ThirdParty", "MixamoLocal");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(MixamoSlideImporter.Folder, "Prone");
            foreach (string file in Directory.GetFiles(source, "*.fbx"))
            {
                if (Path.GetFileName(file) == "Running Slide.fbx") continue;
                string path = Folder + "/" + Path.GetFileName(file);
                string target = Path.GetFullPath(Path.Combine(Application.dataPath, "../" + path));
                bool firstImport = !File.Exists(target);
                bool changed = firstImport || !File.ReadAllBytes(file).SequenceEqual(File.ReadAllBytes(target));
                if (changed) File.Copy(file, target, true);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = (ModelImporter)AssetImporter.GetAtPath(path);
                // Refresh source bytes without resetting a user-authored import configuration.
                if (firstImport || importer.clipAnimations.Length == 0)
                {
                    importer.animationType = ModelImporterAnimationType.Human;
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    importer.importAnimation = true; importer.importCameras = importer.importLights = false;
                    importer.animationCompression = ModelImporterAnimationCompression.Off;
                    var take = importer.defaultClipAnimations.First();
                    importer.clipAnimations = new[] { new ModelImporterClipAnimation
                    {
                        name = Path.GetFileNameWithoutExtension(file), takeName = take.takeName,
                        firstFrame = take.firstFrame, lastFrame = take.lastFrame,
                        lockRootRotation = true, keepOriginalOrientation = true,
                        lockRootHeightY = true, keepOriginalPositionY = true,
                        lockRootPositionXZ = false, keepOriginalPositionXZ = false
                    }};
                    importer.SaveAndReimport();
                }
                var avatar = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();
                if (avatar == null || !avatar.isValid || !avatar.isHuman) throw new InvalidOperationException("Invalid Humanoid: " + path);
            }
            var profile = AssetDatabase.LoadAssetAtPath<ProneAnimationSet>(ProfilePath);
            if (profile == null) { profile = ScriptableObject.CreateInstance<ProneAnimationSet>(); AssetDatabase.CreateAsset(profile, ProfilePath); }
            var data = new SerializedObject(profile);
            string[] fields = { "idle", "forward", "backward", "left", "right", "enter", "exit", "fire", "reload", "death", "armedEnter", "armedExit" };
            string[][] names = {
                new[]{"Prone Idle"},new[]{"Crawl Forward","Crawling","Crawling Forward","Prone Forward"},
                new[]{"Crawl Backwards In Prone","Moving Backward In Prone Position"},new[]{"Crawl Left","Prone Left","Prone Left Turn"},new[]{"Crawl Right","Prone Right","Prone Right Turn"},
                new[]{"Crouch To Prone","Rifle Kneel To Prone"},new[]{"Prone To Crouch Transition","Rifle Prone To Kneel"},
                new[]{"Prone Firing Rifle"},new[]{"Prone Reloading"},new[]{"Prone Death"},new[]{"Rifle Kneel To Prone"},new[]{"Rifle Prone To Kneel"}
            };
            for (int i = 0; i < fields.Length; i++)
            {
                var property = data.FindProperty("_" + fields[i]); if (property.objectReferenceValue != null) continue;
                foreach (string name in names[i])
                {
                    var clip = AssetDatabase.LoadAllAssetsAtPath(Folder + "/" + name + ".fbx").OfType<AnimationClip>().FirstOrDefault(c => c.name == name);
                    if (clip != null) { property.objectReferenceValue = clip; break; }
                }
            }
            if (data.FindProperty("_meleeMask").objectReferenceValue == null)
                data.FindProperty("_meleeMask").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AvatarMask>("Assets/Features/PlayerCharacter/Animation/ProneMeleeArms.mask");
            string[] meleeFields = { "_meleeWeapon", "_meleeJab", "_meleeCross" };
            string[] meleeNames = { "PunchCross", "PunchJab", "PunchCross" };
            for (int i = 0; i < meleeFields.Length; i++) if (data.FindProperty(meleeFields[i]).objectReferenceValue == null)
                data.FindProperty(meleeFields[i]).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/ThirdParty/Quaternius/Animations/" + meleeNames[i] + ".anim");
            data.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(profile);
            Debug.Log("Prone originals imported; existing profile choices preserved: " + ProfilePath);
        }
        public static void Apply(Scene scene)
        {
            var profile = AssetDatabase.LoadAssetAtPath<ProneAnimationSet>(ProfilePath);
            if (profile == null || !profile.IsValid || !scene.IsValid() || !scene.isLoaded) return;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var combat in root.GetComponentsInChildren<PlayerCharacterCombatController>(true))
            {
                var animator = combat.GetComponentInChildren<Animator>(true);
                if (animator == null || animator.GetComponent<PlayerCharacterProneAnimationView>() != null) continue;
                var view = animator.gameObject.AddComponent<PlayerCharacterProneAnimationView>();
                view.Initialize(animator, combat.GetComponent<PlayerCharacterMovement>(), combat.GetComponent<PlayerCharacterStance>(), combat, combat.GetComponent<InfantryLifeState>(), profile);
                animator.GetComponent<PlayerCharacterCombatView>()?.SetPronePresentation(view);
                animator.GetComponent<PlayerCharacterPostureView>()?.SetPronePresentation(view);
                animator.GetComponent<PlayerCharacterWeaponAimView>()?.SetPronePresentation(view);
                animator.GetComponent<PlayerCharacterRifleReloadView>()?.SetPronePresentation(view);
                combat.GetComponentInChildren<FirstPersonArmsView>(true)?.SetPronePresentation(view);
                var occupant = combat.GetComponent<PlayerVehicleOccupant>(); if (occupant == null) continue;
                var data = new SerializedObject(occupant); var components = data.FindProperty("_onFootComponents");
                int index = components.arraySize++; components.GetArrayElementAtIndex(index).objectReferenceValue = view;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
