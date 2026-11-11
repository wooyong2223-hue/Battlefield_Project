using Battlefield.Features.PlayerCharacter;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlefield.Editor.PlayerCharacter
{
    public static class SlideAnimationSceneSetup
    {
        public static void Apply(Scene scene)
        {
            var profile = AssetDatabase.LoadAssetAtPath<SlideAnimationSet>(MixamoSlideImporter.ProfilePath);
            if (profile == null || !profile.IsValid || !scene.IsValid() || !scene.isLoaded) return;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var traversal in root.GetComponentsInChildren<PlayerCharacterTraversal>(true))
            {
                var combat = traversal.GetComponent<PlayerCharacterCombatController>();
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

}
