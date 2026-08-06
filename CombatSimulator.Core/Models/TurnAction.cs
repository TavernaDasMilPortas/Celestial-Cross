using System.Collections.Generic;

namespace CombatSimulator.Core.Models
{
    public enum TurnActionType { Move, UseAbility, Wait, Surrender }

    public class TurnAction
    {
        public string MatchId { get; set; }
        public string ActingUnitId { get; set; }
        public TurnActionType Type { get; set; }
        public string AbilityId { get; set; }
        public int AbilityLevel { get; set; }
        public SimpleVector2Int? TargetPosition { get; set; }
        public string TargetUnitId { get; set; }
        public List<SimpleVector2Int> TargetPositions { get; set; } = new List<SimpleVector2Int>();
        public int ExpectedActionIndex { get; set; }
    }
}
