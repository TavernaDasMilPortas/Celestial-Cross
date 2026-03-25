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
            EditorGUILayout.LabelField("=== RPG COMBAT LOG SYSTEM PRO ===", EditorStyles.boldLabel);
            
            // --- FILTROS ---
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Visual Filters", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            logger.showDamage = GUILayout.Toggle(logger.showDamage, "Damage", "Button", GUILayout.ExpandWidth(true));
            logger.showHealing = GUILayout.Toggle(logger.showHealing, "Heal", "Button", GUILayout.ExpandWidth(true));
            logger.showAbilities = GUILayout.Toggle(logger.showAbilities, "Skill", "Button", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            logger.showPassives = GUILayout.Toggle(logger.showPassives, "Passive", "Button", GUILayout.ExpandWidth(true));
            logger.showConditions = GUILayout.Toggle(logger.showConditions, "Cond", "Button", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Clear", GUILayout.ExpandWidth(true))) logger.Clear();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            autoScroll = EditorGUILayout.Toggle("Auto Scroll", autoScroll);

            EditorGUILayout.Space(5);

            // --- AREA DE LOG ---
            GUIStyle logStyle = new GUIStyle(EditorStyles.label);
            logStyle.richText = true;
            logStyle.wordWrap = true;
            logStyle.fontSize = 12;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(400));
            
            foreach (var entry in logger.entries)
            {
                if (!ShouldShow(logger, entry.category)) continue;

                string colorCode = entry.color == "red" ? "#FF5555" : 
                                  entry.color == "green" ? "#55FF55" : 
                                  entry.color == "magenta" ? "#FF55FF" : 
                                  entry.color == "cyan" ? "#55FFFF" : 
                                  entry.color == "yellow" ? "#FFFF55" : "#FFFFFF";

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                string tag = $"<b><color={colorCode}>[{entry.category}]</color></b>";
                EditorGUILayout.LabelField($"<color=grey>[{entry.timestamp}]</color> {tag} {entry.message}", logStyle);
                
                EditorGUILayout.EndHorizontal();
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

        private bool ShouldShow(CombatLogger logger, LogCategory category)
        {
            return category switch
            {
                LogCategory.Damage => logger.showDamage,
                LogCategory.Healing => logger.showHealing,
                LogCategory.Passive => logger.showPassives,
                LogCategory.Condition => logger.showConditions,
                LogCategory.Ability => logger.showAbilities,
                _ => true
            };
        }
    }
}