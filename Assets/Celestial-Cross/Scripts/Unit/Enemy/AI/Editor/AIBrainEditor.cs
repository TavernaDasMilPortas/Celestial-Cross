#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AIBrain))]
public class AIBrainEditor : Editor
{
    private const string LANG_PREF = "AILang_PT";

    public override void OnInspectorGUI()
    {
        bool isPortuguese = EditorPrefs.GetBool(LANG_PREF, true);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(isPortuguese ? "🌐 Idioma: Português" : "🌐 Language: English", GUILayout.Width(180), GUILayout.Height(30)))
        {
            isPortuguese = !isPortuguese;
            EditorPrefs.SetBool(LANG_PREF, isPortuguese);
            Repaint();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(isPortuguese ? 
            "O AIBrain é o núcleo lógico do inimigo.\nLembre-se de configurar o [AIBehaviorProfile] no componente pai (EnemyUnit) e suas regras." : 
            "AIBrain is the logical core of the enemy.\nRemember to configure the [AIBehaviorProfile] on the parent component (EnemyUnit) and its rules.", MessageType.Info);

        DrawDefaultInspector();
    }
}
#endif