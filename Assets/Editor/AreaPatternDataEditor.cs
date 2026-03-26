using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AreaPatternData))]
public class AreaPatternDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AreaPatternData pattern = (AreaPatternData)target;
        serializedObject.Update();

        var widthProp = serializedObject.FindProperty("width");
        var heightProp = serializedObject.FindProperty("height");
        var originXProp = serializedObject.FindProperty("originX");
        var originYProp = serializedObject.FindProperty("originY");
        var canRotateProp = serializedObject.FindProperty("canRotate");
        var rotationTypeProp = serializedObject.FindProperty("rotationType");
        var diagonalPatternProp = serializedObject.FindProperty("diagonalPattern");
        var rowsProp = serializedObject.FindProperty("rows");

        if (widthProp != null) EditorGUILayout.PropertyField(widthProp);
        if (heightProp != null) EditorGUILayout.PropertyField(heightProp);
        if (originXProp != null) EditorGUILayout.PropertyField(originXProp);
        if (originYProp != null) EditorGUILayout.PropertyField(originYProp);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rotation Settings", EditorStyles.boldLabel);
        if (canRotateProp != null) EditorGUILayout.PropertyField(canRotateProp);
        
        if (canRotateProp != null && canRotateProp.boolValue)
        {
            if (rotationTypeProp != null) EditorGUILayout.PropertyField(rotationTypeProp);
            
            // Forçamos a aplicação das propriedades para que o enumValueIndex atualize antes de checar as diagonais
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            if (rotationTypeProp != null && rotationTypeProp.enumValueIndex == (int)RotationType.EightDirections)
            {
                pattern.EnsureDiagonalShape();
                serializedObject.Update();
                var diagRowsProp = serializedObject.FindProperty("diagonalPattern");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Diagonal Pattern Matrix (NE)", EditorStyles.boldLabel);

                if (diagRowsProp != null && diagRowsProp.arraySize == pattern.height)
                {
                    for (int y = pattern.height - 1; y >= 0; y--)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var rowProp = diagRowsProp.GetArrayElementAtIndex(y);
                        var cellsProp = rowProp.FindPropertyRelative("cells");

                        if (cellsProp != null && cellsProp.arraySize == pattern.width)
                        {
                            for (int x = 0; x < pattern.width; x++)
                            {
                                var cell = cellsProp.GetArrayElementAtIndex(x);
                                bool isOrigin = (x == pattern.originX && y == pattern.originY);
                                Color oldColor = GUI.backgroundColor;
                                if (isOrigin) GUI.backgroundColor = Color.cyan;
                                cell.boolValue = EditorGUILayout.Toggle(cell.boolValue, GUILayout.Width(20));
                                GUI.backgroundColor = oldColor;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();

        pattern.EnsureShape();
        serializedObject.Update();

        // Refetch rowsProp after Update
        rowsProp = serializedObject.FindProperty("rows");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pattern Matrix", EditorStyles.boldLabel);

        if (rowsProp == null || rowsProp.arraySize != pattern.height)
        {
            EditorGUILayout.HelpBox("Initializing rows...", MessageType.Info);
            return;
        }

        for (int y = pattern.height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            var rowProp = rowsProp.GetArrayElementAtIndex(y);
            var cellsProp = rowProp.FindPropertyRelative("cells");

            if (cellsProp == null || cellsProp.arraySize != pattern.width)
            {
                EditorGUILayout.EndHorizontal();
                continue;
            }

            for (int x = 0; x < pattern.width; x++)
            {
                var cell = cellsProp.GetArrayElementAtIndex(x);

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
