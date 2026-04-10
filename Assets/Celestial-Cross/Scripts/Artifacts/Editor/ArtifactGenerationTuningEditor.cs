using System;
using UnityEditor;
using UnityEngine;

namespace CelestialCross.Artifacts.Editor
{
    [CustomEditor(typeof(ArtifactGenerationTuning))]
    public class ArtifactGenerationTuningEditor : UnityEditor.Editor
    {
        private bool showRarity;
        private bool showStats;

        private Vector2 statsScroll;

        private readonly global::System.Collections.Generic.Dictionary<StatType, bool> statFoldouts =
            new global::System.Collections.Generic.Dictionary<StatType, bool>();

        public override void OnInspectorGUI()
        {
            var tuning = (ArtifactGenerationTuning)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("useTuning"));

            GUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Garantir Todos os Status"))
            {
                Undo.RecordObject(tuning, "Ensure All Stats");
                tuning.EnsureAllStatsPresent();
                EditorUtility.SetDirty(tuning);
            }

            if (GUILayout.Button("Restaurar Padrões"))
            {
                Undo.RecordObject(tuning, "Reset Defaults");
                tuning.ResetToGeneratorDefaults();
                EditorUtility.SetDirty(tuning);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Botões:\n" +
                "- 'Garantir Todos os Status': cria entradas para TODOS os StatType, para você conseguir editar tudo.\n" +
                "- 'Restaurar Padrões': volta para os valores originais do gerador (antes do tuning).",
                MessageType.None);

            GUILayout.Space(8);

            showRarity = EditorGUILayout.Foldout(showRarity, "Quantidade de Substats Iniciais (Raridade)", true);
            if (showRarity)
            {
                DrawRarityRanges();
            }

            GUILayout.Space(8);

            showStats = EditorGUILayout.Foldout(showStats, "Faixas de Valores por Status (Estrelas)", true);
            if (showStats)
            {
                if (GUILayout.Button("Ordenar Lista de Status"))
                {
                    Undo.RecordObject(tuning, "Sort Stat List");
                    tuning.statRanges.Sort((a, b) => a.statType.CompareTo(b.statType));
                    EditorUtility.SetDirty(tuning);
                }

                GUILayout.Space(4);

                // Scroll para caber em telas menores.
                statsScroll = EditorGUILayout.BeginScrollView(statsScroll, GUILayout.MinHeight(220));
                DrawStats();
                EditorGUILayout.EndScrollView();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRarityRanges()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("commonInitialSubstats"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("uncommonInitialSubstats"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rareInitialSubstats"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("epicInitialSubstats"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("legendaryInitialSubstats"));
        }

        private void DrawStats()
        {
            SerializedProperty listProp = serializedObject.FindProperty("statRanges");
            if (listProp == null)
                return;

            if (listProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("A lista de status está vazia. Clique em 'Garantir Todos os Status'.", MessageType.Warning);
                return;
            }

            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty element = listProp.GetArrayElementAtIndex(i);
                SerializedProperty statTypeProp = element.FindPropertyRelative("statType");

                StatType statType = (StatType)statTypeProp.enumValueIndex;
                if (!statFoldouts.ContainsKey(statType))
                    statFoldouts[statType] = false;

                statFoldouts[statType] = EditorGUILayout.Foldout(statFoldouts[statType], statType.ToString(), true);

                if (!statFoldouts[statType])
                    continue;

                EditorGUI.indentLevel++;
                DrawRangeTable(element, "mainBaseByStars", "Main Base");
                DrawRangeTable(element, "mainUpgradeByStars", "Main Upgrade");
                DrawRangeTable(element, "subInitialByStars", "Sub Initial");
                DrawRangeTable(element, "subUpgradeByStars", "Sub Upgrade");
                EditorGUI.indentLevel--;

                GUILayout.Space(6);
            }
        }

        private void DrawRangeTable(SerializedProperty statRangesElement, string arrayField, string label)
        {
            SerializedProperty arr = statRangesElement.FindPropertyRelative(arrayField);
            if (arr == null)
                return;

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            if (arr.arraySize != 6)
            {
                EditorGUILayout.HelpBox("O tamanho deve ser 6 (1★ a 6★).", MessageType.Warning);
                return;
            }

            for (int starIndex = 0; starIndex < 6; starIndex++)
            {
                SerializedProperty range = arr.GetArrayElementAtIndex(starIndex);
                SerializedProperty minProp = range.FindPropertyRelative("min");
                SerializedProperty maxProp = range.FindPropertyRelative("max");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{starIndex + 1}★", GUILayout.Width(32));
                minProp.floatValue = EditorGUILayout.FloatField("Mín", minProp.floatValue);
                maxProp.floatValue = EditorGUILayout.FloatField("Máx", maxProp.floatValue);
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(4);
        }
    }
}
