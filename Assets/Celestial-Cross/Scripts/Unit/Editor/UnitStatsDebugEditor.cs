using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Unit), true)]
public class UnitStatsDebugEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Desenha o inspetor normal

        Unit unit = (Unit)target;

        if (unit == null || unit.unitData == null) return;

        GUILayout.Space(15);
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.padding = new RectOffset(10, 10, 10, 10);
        
        GUILayout.BeginVertical(style);
        GUILayout.Label("📊 Calculadora Final em Tempo Real (Não precisa de PlayMode!)", EditorStyles.boldLabel);
        
        var baseStats = unit.unitData.baseStats;
        var finalStats = unit.Stats;

        DrawStatComparison("HP Max", baseStats.health, finalStats.health);
        DrawStatComparison("Ataque", baseStats.attack, finalStats.attack);
        DrawStatComparison("Defesa", baseStats.defense, finalStats.defense);
        DrawStatComparison("Velocidade", baseStats.speed, finalStats.speed);
        DrawStatComparison("Chande de Crítico", baseStats.criticalChance, finalStats.criticalChance, "%");
        DrawStatComparison("Precisão de Efeito", baseStats.effectAccuracy, finalStats.effectAccuracy, "%");

        GUILayout.EndVertical();

        // Força a redraivar a tela quando houver mudanças nas arrays do unity
        if (GUI.changed)
        {
            EditorUtility.SetDirty(unit);
        }
    }

    private void DrawStatComparison(string statName, float baseValue, float finalValue, string suffix = "")
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{statName}:", GUILayout.Width(130));
        
        if (finalValue > baseValue)
        {
            GUILayout.Label($"{baseValue}{suffix}  <color=green>→ {finalValue}{suffix}</color>", new GUIStyle(GUI.skin.label) { richText = true });
        }
        else
        {
            GUILayout.Label($"{baseValue}{suffix}  → {finalValue}{suffix}");
        }
        
        GUILayout.EndHorizontal();
    }
}