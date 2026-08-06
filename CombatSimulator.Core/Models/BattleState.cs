using System;
using System.Collections.Generic;

namespace CombatSimulator.Core.Models
{
    public class BattleState
    {
        public string MatchId { get; set; }
        public string OwnerPlayFabId { get; set; }
        public string OpponentPlayFabId { get; set; }
        public int RoundNumber { get; set; }
        public List<string> TurnQueue { get; set; } = new List<string>();
        public string CurrentTurnUnitId { get; set; }
        public List<UnitState> Units { get; set; } = new List<UnitState>();
        public int ActionIndex { get; set; }
        public long RngSeed { get; set; }
        public bool IsFinished { get; set; }
        public TeamType WinnerTeam { get; set; }
        public string StageId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime LastActionAtUtc { get; set; }
        public Dictionary<string, ScheduledAction> ScheduledActions { get; set; } = new Dictionary<string, ScheduledAction>();
        public Dictionary<string, AbilityGraphData> AbilityLibrary { get; set; } = new Dictionary<string, AbilityGraphData>();

        public BattleState DeepClone()
        {
            // Simplified deep clone for the state
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<BattleState>(json);
        }
    }

    public class ScheduledAction
    {
        public string UnitId { get; set; }
        public string GraphId { get; set; }
        public int TurnsRemaining { get; set; }
        public Dictionary<string, float> CapturedVariables { get; set; }
    }
}
