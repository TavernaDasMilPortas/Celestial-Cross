using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TargetingRuleData))]
public class TargetingRuleDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float line = EditorGUIUtility.singleLineHeight;
        float gap = 2f;

        var mode = property.FindPropertyRelative("mode");
        var minTargets = property.FindPropertyRelative("minTargets");
        var maxTargets = property.FindPropertyRelative("maxTargets");
        var canTargetSelf = property.FindPropertyRelative("canTargetSelf");
        var targetFaction = property.FindPropertyRelative("targetFaction");

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, line), mode);
        y += line + gap;
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, line), minTargets);
        y += line + gap;
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, line), maxTargets);
        y += line + gap;
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, line), canTargetSelf);
        y += line + gap;
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, line), targetFaction);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + 2f) * 5;
    }
}
