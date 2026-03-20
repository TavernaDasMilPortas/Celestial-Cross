using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AbilityData))]
public class AbilityDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Common", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityType"));

        var typeProp = serializedObject.FindProperty("abilityType");
        var weaverPassivesProp = serializedObject.FindProperty("weaverPassives");
        var activeProp = serializedObject.FindProperty("active");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Weaver Settings", EditorStyles.boldLabel);
        
        // Sempre mostrar as passivas weaver (podem existir em ativas tbm)
        EditorGUILayout.PropertyField(weaverPassivesProp, new GUIContent("Weaver Passives"), true);

        if ((AbilityType)typeProp.enumValueIndex == AbilityType.Active)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Active Setup", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(activeProp.FindPropertyRelative("actionName"));
            EditorGUILayout.PropertyField(activeProp.FindPropertyRelative("actionDefinition"), true);

            if (activeProp.FindPropertyRelative("actionDefinition").managedReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Defina um Action Definition para habilidade ativa.", MessageType.Warning);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
