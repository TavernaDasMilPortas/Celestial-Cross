#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AIPatternData))]
public class AIPatternDataEditor : Editor
{
    private bool IsPortuguese => EditorPrefs.GetBool("AILang_PT", true);

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.Space(10);
        
        // Language Toggle centralizado e igual ao do AIBrain
        bool currentLang = IsPortuguese;
        bool newLang = GUILayout.Toggle(currentLang, currentLang ? " Idioma: Português (Mudar para Inglês)" : " Language: English (Change to Portuguese)", "Button");
        if (newLang != currentLang)
        {
            EditorPrefs.SetBool("AILang_PT", newLang);
            Repaint();
        }

        GUILayout.Space(15);
        
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
        GUILayout.Label(IsPortuguese ? "Padrões de Chefe (Boss Patterns)" : "Boss Patterns Data", headerStyle);
        GUILayout.Space(10);

        var initialProfile = serializedObject.FindProperty("initialProfile");
        EditorGUILayout.PropertyField(initialProfile, new GUIContent(IsPortuguese ? "Perfil Inicial (Começo da Batalha)" : "Initial Profile (Start of Battle)"));
        
        GUILayout.Space(15);
        
        // Tooltip explicativo
        EditorGUILayout.HelpBox(IsPortuguese ? 
            "As fases são testadas em ordem (do topo para baixo da lista). Ordene para que o gatilho mais emergencial (ex: 20% HP) venha antes de gatilhos mais tranquilos (Ex: 80% HP)." : 
            "Phases are evaluated top to bottom. Place the most critical triggers (e.g., 20% HP) before softer triggers (e.g., 80% HP).", 
            MessageType.Info);

        GUILayout.Space(5);

        var phases = serializedObject.FindProperty("phases");
        EditorGUILayout.PropertyField(phases, new GUIContent(IsPortuguese ? "Fases Adicionais (Gatilhos)" : "Additional Phases (Triggers)"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif