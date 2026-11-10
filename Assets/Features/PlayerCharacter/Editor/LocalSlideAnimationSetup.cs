using Battlefield.Features.PlayerCharacter;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlefield.Editor.PlayerCharacter
{
    [InitializeOnLoad]
    public static class LocalSlideAnimationSetup
    {
        static LocalSlideAnimationSetup()
        {
            SceneManager.sceneLoaded += (scene, mode) => { if (EditorApplication.isPlaying) Apply(scene); };
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode) return;
                for (int i = 0; i < SceneManager.sceneCount; i++) Apply(SceneManager.GetSceneAt(i));
            };
        }
        public static void Apply(Scene scene)
        {
            var profile = AssetDatabase.LoadAssetAtPath<SlideAnimationSet>(MixamoSlideImporter.ProfilePath);
            if (profile == null || !profile.IsValid || !scene.IsValid() || !scene.isLoaded) return;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var traversal in root.GetComponentsInChildren<PlayerCharacterTraversal>(true))
            {
                var combat = traversal.GetComponent<PlayerCharacterController>();
                var presentation = traversal.GetComponentInChildren<PlayerCharacterAnimationView>(true);
                if (combat == null || presentation == null || presentation.GetComponent<PlayerCharacterSlideAnimationView>() != null) continue;
                var animator = presentation.GetComponent<Animator>();
                var view = presentation.gameObject.AddComponent<PlayerCharacterSlideAnimationView>();
                view.Initialize(animator, combat, profile, traversal);
                presentation.SetSlideAnimations(profile);
                var occupant = traversal.GetComponent<PlayerVehicleOccupant>();
                if (occupant == null) continue;
                // Register in the existing suspension lifecycle, after the authored presentation components.
                var data = new SerializedObject(occupant);
                var components = data.FindProperty("_onFootComponents");
                int index = components.arraySize;
                components.arraySize++;
                components.GetArrayElementAtIndex(index).objectReferenceValue = view;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    // Inject only into the scene being built; never save raw Mixamo references into source scenes.
    public sealed class LocalSlideAnimationBuildProcessor : IProcessSceneWithReport
    {
        public int callbackOrder => 1000;
        public void OnProcessScene(Scene scene, BuildReport report)
        { if (report != null) LocalSlideAnimationSetup.Apply(scene); }
    }
}
