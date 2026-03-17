using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TargetingRuleData))]
public class TargetingRuleDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        
        // Draw the main foldout label
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, true);
        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            SerializedProperty modeProp = property.FindPropertyRelative("mode");
            SerializedProperty originProp = property.FindPropertyRelative("origin");
            SerializedProperty allowMultProp = property.FindPropertyRelative("allowMultiple");
            SerializedProperty minProp = property.FindPropertyRelative("minTargets");
            SerializedProperty maxProp = property.FindPropertyRelative("maxTargets");
            SerializedProperty selfProp = property.FindPropertyRelative("canTargetSelf");
            SerializedProperty factionProp = property.FindPropertyRelative("targetFaction");

            rect.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(rect, modeProp);

            rect.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(rect, originProp);

            rect.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(rect, allowMultProp);

            if (allowMultProp.boolValue)
            {
                rect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(rect, minProp);

                rect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(rect, maxProp);
            }

            rect.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(rect, selfProp);

            rect.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(rect, factionProp);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        float height = EditorGUIUtility.singleLineHeight + 2; // Foldout origin

        // Mode, Origin, AllowMultiple, CanTargetSelf, TargetFaction (5 props)
        height += (EditorGUIUtility.singleLineHeight + 2) * 5;

        SerializedProperty allowMultProp = property.FindPropertyRelative("allowMultiple");
        if (allowMultProp != null && allowMultProp.boolValue)
        {
            height += (EditorGUIUtility.singleLineHeight + 2) * 2; // min e max targets
        }

        return height;
    }
}
