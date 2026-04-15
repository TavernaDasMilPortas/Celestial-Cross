#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridMap))]
public class GridMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridMap map = (GridMap)target;

        GUILayout.Space(10);
        
        GUI.color = Color.green;
        if (GUILayout.Button("Generate Map", GUILayout.Height(30)))
        {
            map.Generate();
        }

        GUI.color = new Color(1f, 0.8f, 0.4f);
        if (GUILayout.Button("Test Highlight Area (3x3)", GUILayout.Height(30)))
        {
            map.TestHighlightArea();
        }

        GUI.color = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("Clear Map"))
        {
            map.Clear();
        }
        
        GUI.color = Color.white;
    }
}
#endif
