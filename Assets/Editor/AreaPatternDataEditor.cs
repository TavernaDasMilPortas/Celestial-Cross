using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AreaPatternData))]
public class AreaPatternDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var widthProp = serializedObject.FindProperty("width");
        var heightProp = serializedObject.FindProperty("height");
        var originXProp = serializedObject.FindProperty("originX");
        var originYProp = serializedObject.FindProperty("originY");
        var allowRotationProp = serializedObject.FindProperty("allowRotation");
        var rowsProp = serializedObject.FindProperty("rows");

        EditorGUILayout.PropertyField(widthProp);
        EditorGUILayout.PropertyField(heightProp);
        EditorGUILayout.PropertyField(originXProp);
        EditorGUILayout.PropertyField(originYProp);
        EditorGUILayout.PropertyField(allowRotationProp);

        serializedObject.ApplyModifiedProperties();

        AreaPatternData pattern = (AreaPatternData)target;
        pattern.EnsureShape();
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pattern Matrix", EditorStyles.boldLabel);

        for (int y = pattern.height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            var row = rowsProp.GetArrayElementAtIndex(y).FindPropertyRelative("cells");

            for (int x = 0; x < pattern.width; x++)
            {
                var cell = row.GetArrayElementAtIndex(x);

                GUIStyle style = new GUIStyle(GUI.skin.toggle);
                if (x == pattern.originX && y == pattern.originY)
                    style.normal.textColor = Color.cyan;

                cell.boolValue = GUILayout.Toggle(cell.boolValue, GUIContent.none, style, GUILayout.Width(24));
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed)
            EditorUtility.SetDirty(pattern);

        serializedObject.ApplyModifiedProperties();
    }
}
