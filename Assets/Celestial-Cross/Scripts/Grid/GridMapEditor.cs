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

        if (GUILayout.Button("Generate Map"))
        {
            map.Generate();
        }

        if (GUILayout.Button("Clear Map"))
        {
            map.Clear();
        }
    }
}
#endif
