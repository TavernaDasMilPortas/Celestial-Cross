using UnityEngine;
using UnityEditor;
using CelestialCross.Combat;

namespace CelestialCross.Editor
{
    [CustomEditor(typeof(CombatLogger))]
    public class CombatLoggerEditor : UnityEditor.Editor
    {
        private Vector2 scrollPos;
        private bool autoScroll = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CombatLogger logger = (CombatLogger)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("=== CELESTIAL CROSS COMBAT MONITOR ===", EditorStyles.boldLabel);
            
            // --- MONITOR DE STATUS ATUAL (NOVO) ---
            DrawStatusMonitor();

            EditorGUILayout.Space(5);

            // --- FILTROS ---
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Event Logs & Filters", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            logger.showDamage = GUILayout.Toggle(logger.showDamage, "Damage", "Button", GUILayout.ExpandWidth(true));
            logger.showHealing = GUILayout.Toggle(logger.showHealing, "Heal", "Button", GUILayout.ExpandWidth(true));
            logger.showGraphs = GUILayout.Toggle(logger.showGraphs, "Graph", "Button", GUILayout.ExpandWidth(true));
            logger.showAI = GUILayout.Toggle(logger.showAI, "AI", "Button", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            logger.showPassives = GUILayout.Toggle(logger.showPassives, "Passive", "Button", GUILayout.ExpandWidth(true));
            logger.showConditions = GUILayout.Toggle(logger.showConditions, "Cond", "Button", GUILayout.ExpandWidth(true));
            logger.showAbilities = GUILayout.Toggle(logger.showAbilities, "System", "Button", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            logger.showEmptyTriggers = GUILayout.Toggle(logger.showEmptyTriggers, "Show Empty Triggers", "Button", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Clear", GUILayout.ExpandWidth(true))) logger.Clear();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            autoScroll = EditorGUILayout.Toggle("Auto Scroll", autoScroll);

            EditorGUILayout.Space(5);

            // --- AREA DE LOG ---
            GUIStyle logStyle = new GUIStyle(EditorStyles.label);
            logStyle.richText = true;
            logStyle.wordWrap = true;
            logStyle.fontSize = 11;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(400));
            
            foreach (var entry in logger.entries)
            {
                if (!ShouldShow(logger, entry)) continue;

                // Definir cor de fundo baseada na categoria para "segundo chat" feeling
                GUI.backgroundColor = entry.category switch {
                    LogCategory.Passive => new Color(0.8f, 0.4f, 0.8f, 0.2f),
                    LogCategory.Condition => new Color(0.8f, 0.8f, 0.2f, 0.2f),
                    LogCategory.Graph => new Color(0.4f, 0.4f, 0.8f, 0.2f),
                    LogCategory.AI => new Color(1.0f, 0.6f, 0.0f, 0.15f),
                    _ => Color.white
                };

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                string hexColor = GetHexColor(entry.category);
                string tag = $"<b><color={hexColor}>[{entry.category}]</color></b>";
                EditorGUILayout.LabelField($"<color=grey>[{entry.timestamp}]</color> {tag} {entry.message}", logStyle);
                
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = Color.white; // Reset
            }

            if (autoScroll && Event.current.type == EventType.Repaint)
                scrollPos.y = float.MaxValue;

            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(logger);
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStatusMonitor()
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 12;
            headerStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("UNIT STATUS (CURRENT TURN)", headerStyle);
            GUILayout.Space(5);

            if (CombatLogger.CurrentUnit != null)
            {
                var unit = CombatLogger.CurrentUnit;
                var stats = unit.Stats;

                EditorGUILayout.LabelField($"<b>{unit.DisplayName}</b>", new GUIStyle(EditorStyles.label) { richText = true, alignment = TextAnchor.MiddleCenter });
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                DrawStatBox("ATK", stats.attack, "#ff4d4d");
                DrawStatBox("DEF", stats.defense, "#ffd700");
                DrawStatBox("SPD", stats.speed, "#a29bfe");
                DrawStatBox("HP", $"{unit.Health.CurrentHealth}/{stats.health}", "#4dff88");
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Nenhuma unidade ativa.", EditorStyles.centeredGreyMiniLabel);
            }

            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        private void DrawStatBox(string label, object value, string colorHex)
        {
            string text = $"<color={colorHex}><b>{label}:</b> {value}</color>";
            GUILayout.Label(text, new GUIStyle(EditorStyles.helpBox) { richText = true, margin = new RectOffset(2, 2, 2, 2) });
        }

        private string GetHexColor(LogCategory category)
        {
            return category switch
            {
                LogCategory.Damage => "#FF5555",
                LogCategory.Healing => "#55FF55",
                LogCategory.Passive => "#FF55FF",
                LogCategory.Condition => "#FFFF55",
                LogCategory.Ability => "#55FFFF",
                LogCategory.Graph => "#a29bfe",
                LogCategory.AI => "#ffa502",
                _ => "#FFFFFF"
            };
        }

        private bool ShouldShow(CombatLogger logger, LogEntry entry)
        {
            if (entry.isTriggerOnly && !logger.showEmptyTriggers) return false;

            return entry.category switch
            {
                LogCategory.Damage => logger.showDamage,
                LogCategory.Healing => logger.showHealing,
                LogCategory.Passive => logger.showPassives,
                LogCategory.Condition => logger.showConditions,
                LogCategory.Ability => logger.showAbilities,
                LogCategory.Graph => logger.showGraphs,
                LogCategory.AI => logger.showAI,
                _ => true
            };
        }
    }
}