using UnityEditor;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities;

public class AbilityBlueprintEditorWindow : EditorWindow
{
    private AbilityBlueprint _selectedBlueprint;
    private SerializedObject _serializedBlueprint;
    private SerializedProperty _effectStepsProperty;

    [MenuItem("Celestial Cross/Abilities/Ability Blueprint Editor")]
    public static void ShowWindow()
    {
        GetWindow<AbilityBlueprintEditorWindow>("Ability Blueprint Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Ability Blueprint Editor", EditorStyles.boldLabel);

        _selectedBlueprint = (AbilityBlueprint)EditorGUILayout.ObjectField("Blueprint", _selectedBlueprint, typeof(AbilityBlueprint), false);

        if (_selectedBlueprint != null)
        {
            if (_serializedBlueprint == null || _serializedBlueprint.targetObject != _selectedBlueprint)
            {
                _serializedBlueprint = new SerializedObject(_selectedBlueprint);
                _effectStepsProperty = _serializedBlueprint.FindProperty("effectSteps");
            }

            _serializedBlueprint.Update();

            EditorGUILayout.PropertyField(_effectStepsProperty, true);

            if (GUILayout.Button("Add New Effect Step"))
            {
                _effectStepsProperty.InsertArrayElementAtIndex(_effectStepsProperty.arraySize);
            }

            _serializedBlueprint.ApplyModifiedProperties();
        }
        else
        {
            _serializedBlueprint = null;
        }
    }
}
