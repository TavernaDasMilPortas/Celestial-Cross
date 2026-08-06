using System.Collections.Generic;

namespace CombatSimulator.Core.Models
{
    public class BattleEvent
    {
        public string EventType { get; set; }
        public string SourceUnitId { get; set; }
        public string TargetUnitId { get; set; }
        public int? Amount { get; set; }
        public bool? IsCritical { get; set; }
        public string StatusId { get; set; }
        public int? Stacks { get; set; }
        public int? Duration { get; set; }
        public SimpleVector2Int? Position { get; set; }
        public string AbilityId { get; set; }
        public Dictionary<string, string> Extra { get; set; } = new Dictionary<string, string>();
    }
}
