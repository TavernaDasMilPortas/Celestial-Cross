using UnityEngine;
using System;
using System.Collections.Generic;

namespace CelestialCross.Combat
{
    public enum LogCategory { Damage, Healing, Passive, Condition, Ability, System, Graph }

    [Serializable]
    public struct LogEntry
    {
        public string timestamp;
        public LogCategory category;
        public string message;
        public string color;
        public bool isTriggerOnly;

        public LogEntry(string msg, LogCategory cat, bool isTrigger = false)
        {
            timestamp = DateTime.Now.ToString("HH:mm:ss");
            message = msg;
            category = cat;
            isTriggerOnly = isTrigger;
            color = cat switch
            {
                LogCategory.Damage => "#ff4d4d",    // Vermelho vibrante
                LogCategory.Healing => "#4dff88",   // Verde cura
                LogCategory.Passive => "#da70d6",   // Orquídea
                LogCategory.Condition => "#ffd700", // Dourado
                LogCategory.Ability => "#00ffff",   // Ciano
                LogCategory.Graph => "#a29bfe",     // Roxo suave (Grafos)
                _ => "#ffffff"
            };
        }
    }

    public class CombatLogger : MonoBehaviour
    {
        public static CombatLogger Instance { get; private set; }
        public static Unit CurrentUnit;

        public List<LogEntry> entries = new List<LogEntry>();
        public int maxEntries = 100;

        // Filtros para o Inspector
        [HideInInspector] public bool showDamage = true;
        [HideInInspector] public bool showHealing = true;
        [HideInInspector] public bool showPassives = true;
        [HideInInspector] public bool showConditions = true;
        [HideInInspector] public bool showAbilities = true;
        [HideInInspector] public bool showGraphs = true;
        [HideInInspector] public bool showEmptyTriggers = true;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        public static void Log(string message, LogCategory category = LogCategory.System, bool isTrigger = false)
        {
            if (Instance == null)
            {
                string cleanMsg = global::System.Text.RegularExpressions.Regex.Replace(message, "<.*?>", string.Empty);
                Debug.Log($"[{category}] {cleanMsg}");
                return;
            }

            Instance.entries.Add(new LogEntry(message, category, isTrigger));
            if (Instance.entries.Count > Instance.maxEntries)
                Instance.entries.RemoveAt(0);
            
            // Log no console também para facilitar debug sem abrir a janela de log
            string consoleMsg = global::System.Text.RegularExpressions.Regex.Replace(message, "<.*?>", string.Empty);
            // Debug.Log($"[CombatLog] {consoleMsg}"); 
        }

        public void Clear()
        {
            entries.Clear();
        }
    }
}
