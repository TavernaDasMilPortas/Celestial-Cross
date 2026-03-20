using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitData))]
public class UnitDataEditor : Editor
{
    SerializedProperty nativeActionsProp;
    SerializedProperty characterAbilitiesProp;

    void OnEnable()
    {
        nativeActionsProp = serializedObject.FindProperty("nativeActions");
        characterAbilitiesProp = serializedObject.FindProperty("characterAbilities");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Character Core", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseStats"));
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Abilities (External SO)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(characterAbilitiesProp, true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Native Actions (Inline)", EditorStyles.boldLabel);

        // Renderiza lista de ações de forma customizada para ter botões de Add/Remove limpos
        for (int i = 0; i < nativeActionsProp.arraySize; i++)
        {
            SerializedProperty element = nativeActionsProp.GetArrayElementAtIndex(i);
            
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.PropertyField(element, true);

            if (GUILayout.Button("X", GUILayout.Width(24)))
            {
                nativeActionsProp.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Attack"))
        {
            AddAction(new AttackActionData());
        }
        if (GUILayout.Button("+ Add Move"))
        {
            AddAction(new MoveActionData());
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    void AddAction(UnitActionData action)
    {
        int index = nativeActionsProp.arraySize;
        nativeActionsProp.InsertArrayElementAtIndex(index);
        nativeActionsProp.GetArrayElementAtIndex(index).managedReferenceValue = action;
    }
}
