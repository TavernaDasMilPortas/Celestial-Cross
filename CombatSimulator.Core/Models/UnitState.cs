using System.Collections.Generic;

namespace CombatSimulator.Core.Models
{
    public enum TeamType { Player, Enemy }

    public class UnitState
    {
        public string UnitId { get; set; }
        public string DisplayName { get; set; }
        public TeamType Team { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
        public int CurrentAP { get; set; }
        public int MaxAP { get; set; }
        public bool IsAlive { get; set; } = true;
        public bool HasMovedThisTurn { get; set; }
        public bool HasActedThisTurn { get; set; }
        public SimpleVector2Int GridPosition { get; set; }
        public CombatStatsPure Stats { get; set; }
        public List<ActiveCondition> ActiveConditions { get; set; } = new List<ActiveCondition>();
        public Dictionary<string, int> AbilityCooldowns { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, float> UnitVariables { get; set; } = new Dictionary<string, float>();
        public List<string> AbilityIds { get; set; } = new List<string>();
        public LoadoutData Loadout { get; set; }

        public UnitState Clone()
        {
            // Simplified clone for state immutability
            // A more robust serialization/deserialization or manual deep copy is needed for real usage
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<UnitState>(json);
        }
    }

    public class LoadoutData
    {
        public List<BranchSelection> BranchSelections { get; set; } = new List<BranchSelection>();
    }

    public class BranchSelection
    {
        public string SkillId { get; set; }
        public List<string> SelectedBranchIds { get; set; } = new List<string>();
    }
}
