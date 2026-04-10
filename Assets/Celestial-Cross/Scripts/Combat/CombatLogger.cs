using UnityEngine;
using System;
using System.Collections.Generic;

namespace CelestialCross.Combat
{
    public enum LogCategory { Damage, Healing, Passive, Condition, Ability, System }

    [Serializable]
    public struct LogEntry
    {
        public string timestamp;
        public LogCategory category;
        public string message;
        public string color;

        public LogEntry(string msg, LogCategory cat)
        {
            timestamp = DateTime.Now.ToString("HH:mm:ss");
            message = msg;
            category = cat;
            color = cat switch
            {
                LogCategory.Damage => "red",
                LogCategory.Healing => "green",
                LogCategory.Passive => "magenta",
                LogCategory.Condition => "yellow",
                LogCategory.Ability => "cyan",
                _ => "white"
            };
        }
    }

    public class CombatLogger : MonoBehaviour
    {
        public static CombatLogger Instance { get; private set; }

        public List<LogEntry> entries = new List<LogEntry>();
        public int maxEntries = 50;

        // Filtros para o Inspector (usados pelo Editor script)
        [HideInInspector] public bool showDamage = true;
        [HideInInspector] public bool showHealing = true;
        [HideInInspector] public bool showPassives = true;
        [HideInInspector] public bool showConditions = true;
        [HideInInspector] public bool showAbilities = true;

        private void Awake()
        {
            if (Instance == null) 
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public static void Log(string message, LogCategory category = LogCategory.System)
        {
            // Remove tags HTML de logs antigos para nÃ£o duplicar no novo sistema se vierem strings sujas
            string cleanMsg = global::System.Text.RegularExpressions.Regex.Replace(message, "<.*?>", string.Empty);

            if (Instance == null)
            {
                Debug.Log($"[{category}] {cleanMsg}");
                return;
            }

            Instance.entries.Add(new LogEntry(cleanMsg, category));
            if (Instance.entries.Count > Instance.maxEntries)
                Instance.entries.RemoveAt(0);
        }

        public void Clear()
        {
            entries.Clear();
        }
    }
}
