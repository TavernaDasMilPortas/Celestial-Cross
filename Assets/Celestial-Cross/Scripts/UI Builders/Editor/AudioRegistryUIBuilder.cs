using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CelestialCross.Audio;
using System.IO;

namespace CelestialCross.EditorScripts
{
    public class AudioRegistryUIBuilder : EditorWindow
    {
        private SoundRegistrySO registrySO;
        private Vector2 scrollPos;
        private string filterText = "";

        [MenuItem("Celestial Cross/Audio/Registry Builder")]
        public static void ShowWindow()
        {
            var window = GetWindow<AudioRegistryUIBuilder>("Audio Builder");
            window.minSize = new Vector2(400, 500);
        }

        private void OnGUI()
        {
            GUILayout.Label("Audio Registry Configurator", EditorStyles.boldLabel);

            registrySO = (SoundRegistrySO)EditorGUILayout.ObjectField("Sound Registry SO", registrySO, typeof(SoundRegistrySO), false);

            if (registrySO == null)
            {
                EditorGUILayout.HelpBox("Please assign or create a SoundRegistrySO to start.", MessageType.Warning);
                if (GUILayout.Button("Create New Registry SO"))
                {
                    CreateNewRegistry();
                }
                return;
            }

            EditorGUILayout.Space();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("1. Populate Missing Enums", GUILayout.Height(30)))
            {
                PopulateMissingEnums();
            }
            if (GUILayout.Button("2. Auto-Assign Clips (CelerisLab)", GUILayout.Height(30)))
            {
                AutoAssignClips();
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            filterText = EditorGUILayout.TextField("Search", filterText);

            EditorGUILayout.Space();

            // Desenhando a lista
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            SerializedObject serializedObject = new SerializedObject(registrySO);
            SerializedProperty mappingsProp = serializedObject.FindProperty("Mappings");

            for (int i = 0; i < mappingsProp.arraySize; i++)
            {
                SerializedProperty mappingProp = mappingsProp.GetArrayElementAtIndex(i);
                SerializedProperty keyProp = mappingProp.FindPropertyRelative("Key");
                
                string keyName = ((SoundKey)keyProp.intValue).ToString();

                // Filtro
                if (!string.IsNullOrEmpty(filterText) && !keyName.ToLower().Contains(filterText.ToLower()))
                {
                    continue;
                }

                GUILayout.BeginVertical("box");
                
                GUILayout.Label(keyName, EditorStyles.boldLabel);
                
                SerializedProperty clipProp = mappingProp.FindPropertyRelative("Clip");
                EditorGUILayout.PropertyField(clipProp);
                
                if (clipProp.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Missing Audio Clip!", MessageType.Warning);
                }

                GUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }

        private void CreateNewRegistry()
        {
            registrySO = ScriptableObject.CreateInstance<SoundRegistrySO>();
            
            if (!AssetDatabase.IsValidFolder("Assets/Celestial-Cross/Scripts/Audio/Resources"))
            {
                global::System.IO.Directory.CreateDirectory(Application.dataPath + "/Celestial-Cross/Scripts/Audio/Resources");
            }
            
            AssetDatabase.CreateAsset(registrySO, "Assets/Celestial-Cross/Scripts/Audio/Resources/SoundRegistry.asset");
            AssetDatabase.SaveAssets();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = registrySO;
        }

        private void PopulateMissingEnums()
        {
            Undo.RecordObject(registrySO, "Populate Missing Enums");

            var allKeys = (SoundKey[])global::System.Enum.GetValues(typeof(SoundKey));
            
            if (registrySO.Mappings == null)
                registrySO.Mappings = new List<SoundMapping>();

            int addedCount = 0;
            foreach (var key in allKeys)
            {
                if (key == SoundKey.None) continue;

                if (!registrySO.Mappings.Exists(m => m.Key == key))
                {
                    registrySO.Mappings.Add(new SoundMapping { Key = key, VolumeMultiplier = 1f, Pitch = 1f });
                    addedCount++;
                }
            }

            EditorUtility.SetDirty(registrySO);
            Debug.Log($"[AudioRegistryUIBuilder] Added {addedCount} missing enum mappings.");
        }

        private void AutoAssignClips()
        {
            Undo.RecordObject(registrySO, "Auto Assign Audio Clips");

            string folderPath = "Assets/CelerisLab/CompleteUISFX";
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });

            int assignedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                
                if (clip == null) continue;

                // Sanitização reversa para tentar achar o Enum
                string clipName = clip.name;
                string sanitized = SanitizeForEnum(clipName);
                if (char.IsDigit(sanitized[0])) sanitized = "SFX_" + sanitized;

                // Tenta achar o Enum correspondente
                if (global::System.Enum.TryParse(sanitized, out SoundKey foundKey))
                {
                    var mapping = registrySO.Mappings.Find(m => m.Key == foundKey);
                    if (mapping != null && mapping.Clip == null)
                    {
                        mapping.Clip = clip;
                        assignedCount++;
                    }
                }
            }

            EditorUtility.SetDirty(registrySO);
            Debug.Log($"[AudioRegistryUIBuilder] Auto-assigned {assignedCount} audio clips based on filename matching.");
        }

        private string SanitizeForEnum(string name)
        {
            name = global::System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9]", "_");
            string[] parts = name.Split(new char[] { '_' }, global::System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
                }
            }
            return string.Join("", parts);
        }
    }
}
