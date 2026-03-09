using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitData))]
public class UnitDataEditor : Editor
{
    SerializedProperty actionsProp;

    void OnEnable()
    {
        actionsProp = serializedObject.FindProperty("actions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Magical Girl Setup", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseStats"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("characterAbility"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultPet"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Legacy Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        for (int i = 0; i < actionsProp.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(
                actionsProp.GetArrayElementAtIndex(i),
                GUIContent.none,
                true
            );

            if (GUILayout.Button("X", GUILayout.Width(24)))
            {
                actionsProp.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Add Action"))
        {
            actionsProp.arraySize++;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
