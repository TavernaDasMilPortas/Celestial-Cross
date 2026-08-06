using System.Collections.Generic;
using System.Linq;
using CombatSimulator.Core.Simulation;

namespace CombatSimulator.Core.Models
{
    public class SimCombatContext
    {
        public BattleState State { get; set; }
        public string SourceUnitId { get; set; }
        public string TargetUnitId { get; set; }
        public List<string> TargetUnitIds { get; set; } = new List<string>();
        public int Amount { get; set; }
        public bool IsCritical { get; set; }

        public int AbilityLevel { get; set; } = 1;
        public string SlotId { get; set; } = "";
        public SimpleVector2Int? TargetPos { get; set; }

        public Dictionary<string, float> Variables { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, int> LoopCounters { get; set; } = new Dictionary<string, int>();
        public SeededRandom Rng { get; set; }

        // Helpers
        public UnitState GetSource() => State.Units.FirstOrDefault(u => u.UnitId == SourceUnitId);
        public UnitState GetTarget() => State.Units.FirstOrDefault(u => u.UnitId == TargetUnitId);
        public List<UnitState> GetTargets() => State.Units.Where(u => TargetUnitIds.Contains(u.UnitId)).ToList();

        public SimCombatContext Clone()
        {
            var clone = new SimCombatContext
            {
                State = State,
                SourceUnitId = SourceUnitId,
                TargetUnitId = TargetUnitId,
                TargetUnitIds = new List<string>(TargetUnitIds),
                Amount = Amount,
                IsCritical = IsCritical,
                AbilityLevel = AbilityLevel,
                SlotId = SlotId,
                TargetPos = TargetPos,
                Variables = new Dictionary<string, float>(Variables),
                LoopCounters = new Dictionary<string, int>(LoopCounters),
                Rng = Rng
            };
            return clone;
        }
    }
}
