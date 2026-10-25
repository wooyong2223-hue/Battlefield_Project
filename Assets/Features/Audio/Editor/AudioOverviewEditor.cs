using System.Collections.Generic;
using Battlefield.Features.Audio;
using UnityEditor;
using UnityEngine;

namespace Battlefield.Features.Audio.Editor
{
    [CustomEditor(typeof(AudioOverview))]
    public sealed class AudioOverviewEditor : UnityEditor.Editor
    {
        private readonly Dictionary<AudioProfile, bool> _expandedProfiles = new();
        private readonly Dictionary<AudioProfile, bool> _expandedSettings = new();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty relatedPrefabs =
                serializedObject.FindProperty("_relatedPrefabs");
            EditorGUILayout.PropertyField(relatedPrefabs, true);
            serializedObject.ApplyModifiedProperties();

            AudioOverview overview = (AudioOverview)target;
            DrawProfiles(overview.gameObject);

            for (int index = 0; index < relatedPrefabs.arraySize; index++)
            {
                GameObject prefab = relatedPrefabs.GetArrayElementAtIndex(index)
                    .objectReferenceValue as GameObject;
                if (prefab != null)
                {
                    DrawProfiles(prefab);
                }
            }
        }

        private void DrawProfiles(GameObject root)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(root.name, EditorStyles.boldLabel);

            MonoBehaviour[] components =
                root.GetComponentsInChildren<MonoBehaviour>(true);
            bool found = false;

            foreach (MonoBehaviour component in components)
            {
                if (component == null)
                {
                    continue;
                }

                SerializedObject componentObject = new(component);
                SerializedProperty property = componentObject.GetIterator();
                bool enterChildren = true;

                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.propertyType !=
                        SerializedPropertyType.ObjectReference ||
                        property.objectReferenceValue is not AudioProfile profile)
                    {
                        continue;
                    }

                    found = true;
                    bool expanded = _expandedProfiles.TryGetValue(
                        profile, out bool previous) && previous;
                    expanded = EditorGUILayout.Foldout(
                        expanded,
                        $"{component.GetType().Name}: {profile.name}", true);
                    _expandedProfiles[profile] = expanded;

                    if (expanded)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            DrawConnections(component, componentObject);
                            bool showSettings = _expandedSettings.TryGetValue(
                                profile, out bool previousSettings) && previousSettings;
                            showSettings = EditorGUILayout.Foldout(
                                showSettings, "Audio Settings", true);
                            _expandedSettings[profile] = showSettings;
                            if (showSettings)
                            {
                                DrawProfile(profile);
                            }
                        }
                    }
                }
            }

            if (!found)
            {
                EditorGUILayout.HelpBox(
                    "No linked audio profiles were found.", MessageType.Info);
            }
        }

        private static void DrawConnections(
            MonoBehaviour component, SerializedObject componentObject)
        {
            List<GameObject> soundObjects = new();
            HashSet<GameObject> seenObjects = new();
            SerializedProperty property = componentObject.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.propertyType !=
                    SerializedPropertyType.ObjectReference ||
                    property.objectReferenceValue is not AudioSource source)
                {
                    continue;
                }

                if (seenObjects.Add(source.gameObject))
                {
                    soundObjects.Add(source.gameObject);
                }
            }

            for (int index = 0; index < soundObjects.Count; index++)
            {
                string label = soundObjects.Count == 1
                    ? "Sound Object"
                    : $"Sound Object {index + 1}";
                DrawGameObject(label, soundObjects[index]);
            }

            if (!seenObjects.Contains(component.gameObject))
            {
                DrawGameObject("Control Object", component.gameObject);
            }

            MonoScript script = MonoScript.FromMonoBehaviour(component);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(
                        "Script", script, typeof(MonoScript), false);
                }

                using (new EditorGUI.DisabledScope(script == null))
                {
                    if (GUILayout.Button("Open", GUILayout.Width(48f)))
                    {
                        AssetDatabase.OpenAsset(script);
                    }
                }
            }
        }

        private static void DrawGameObject(string label, GameObject gameObject)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(
                        label, gameObject, typeof(GameObject), true);
                }

                if (GUILayout.Button("Ping", GUILayout.Width(48f)))
                {
                    EditorGUIUtility.PingObject(gameObject);
                }
            }
        }

        private static void DrawProfile(AudioProfile profile)
        {
            SerializedObject profileObject = new(profile);
            profileObject.Update();
            SerializedProperty property = profileObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.name == "m_Script")
                {
                    continue;
                }

                EditorGUILayout.PropertyField(property, true);
            }

            profileObject.ApplyModifiedProperties();
        }
    }
}
