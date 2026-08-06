using System.Collections.Generic;

namespace CombatSimulator.Core.Models
{
    public class ActiveCondition
    {
        public string GraphId { get; set; }
        public int RemainingTurns { get; set; }
        public int Stacks { get; set; }
        public int MaxStacks { get; set; }
        public bool IsPersistent { get; set; }
        public bool IsBuff { get; set; }
        public bool IsExecuting { get; set; }
        public List<StatModEntry> StatMods { get; set; } = new List<StatModEntry>();
    }

    public class StatModEntry
    {
        public string StatTypeName { get; set; }
        public float Value { get; set; }
        public bool IsPercent { get; set; }
    }
}
