#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AIBehaviorProfile))]
public class AIBehaviorProfileEditor : Editor
{
    private const string LANG_PREF = "AILang_PT";
    private bool isPortuguese = true;

    private void OnEnable()
    {
        isPortuguese = EditorPrefs.GetBool(LANG_PREF, true);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(isPortuguese ? "🌐 Idioma: Português" : "🌐 Language: English", GUILayout.Width(180), GUILayout.Height(30)))
        {
            isPortuguese = !isPortuguese;
            EditorPrefs.SetBool(LANG_PREF, isPortuguese);
            // Re-renderizar o painel e tudo mais que usa essa propriedade
            Repaint();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        DrawProp("profileName", "Nome do Perfil", "Profile Name", "Nome interno do perfil.", "Internal profile name.");
        
        GUILayout.Space(10);
        DrawProp("rules", "Regras de Comportamento", "Behavior Rules", "Avaliadas da maior para menor prioridade.", "Evaluated from highest to lowest priority.");
        
        GUILayout.Space(10);
        DrawProp("randomnessFactor", "Fator de Aleatoriedade", "Randomness Factor", "0 = Determinístico, 1 = Aleatório", "0 = Deterministic, 1 = Random");

        GUILayout.Space(15);
        EditorGUILayout.LabelField(isPortuguese ? "Comportamento Padrão (Fallback)" : "Fallback Behavior", EditorStyles.boldLabel);
        
        DrawProp("fallbackBehavior", "Comportamento (Fallback)", "Fallback Behavior");
        DrawProp("fallbackTargetPreference", "Seleção de Alvo (Fallback)", "Fallback Target Selection");
        
        var targetPref = serializedObject.FindProperty("fallbackTargetPreference");
        if (targetPref != null && (targetPref.enumValueIndex == (int)AITargetPreference.PrioritizeRole || targetPref.enumValueIndex == (int)AITargetPreference.PrioritizeClass))
        {
            EditorGUI.indentLevel++;
            DrawProp("fallbackPreferredRole", "Papel Alvo (Role)", "Target Role");
            DrawProp("fallbackPreferredClass", "Classe Alvo (Class)", "Target Class");
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawProp(string propName, string ptLabel, string enLabel, string ptTooltip = "", string enTooltip = "")
    {
        var prop = serializedObject.FindProperty(propName);
        if (prop != null)
        {
            GUIContent label = new GUIContent(isPortuguese ? ptLabel : enLabel, isPortuguese ? ptTooltip : enTooltip);
            EditorGUILayout.PropertyField(prop, label, true);
        }
    }
}
#endif