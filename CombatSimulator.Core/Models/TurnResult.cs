using System.Collections.Generic;

namespace CombatSimulator.Core.Models
{
    public class TurnResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public BattleState NewState { get; set; }
        public List<BattleEvent> Events { get; set; } = new List<BattleEvent>();
    }
}
