using System;
using System.Collections.Generic;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    // --- Composites ---
    
    [Serializable]
    public class BTCompositeData
    {
        // Stores the names of the explicit step ports (e.g., "Passo_0", "Passo_1")
        public List<string> ports = new List<string>();
    }

    // --- Decorators ---
    
    [Serializable]
    public class BTRandomChanceData
    {
        public float chancePercent = 50f;
    }

    [Serializable]
    public class BTRepeaterData
    {
        public int count = 1;
    }

    [Serializable]
    public class BTCooldownData
    {
        public int cooldownTurns = 1;
    }

    // --- Actions ---

    [Serializable]
    public class ActionUseAbilityData
    {
        public AIAbilityHint.AbilityCategory category = AIAbilityHint.AbilityCategory.Damage;
    }

    [Serializable]
    public class ActionUseBestAbilityData
    {
        public float minimumScoreThreshold = 5f;
    }

    public enum BTMoveIntent { Approach, Flee, Flank, Wander }

    [Serializable]
    public class ActionMoveData
    {
        public BTMoveIntent intent = BTMoveIntent.Approach;
    }

    // --- Conditions ---

    [Serializable]
    public class ConditionTargetHasBuffData
    {
        public bool isBuff = true;
        public string modifierId = "";
    }
    
    [Serializable]
    public class ConditionAoEHitCountData
    {
        public int minHitCount = 2;
    }
    
    [Serializable]
    public class ConditionAbilityReadyData
    {
        public AIAbilityHint.AbilityCategory category = AIAbilityHint.AbilityCategory.Damage;
    }

    // --- New Modular Conditions & Flow ---

    public enum BTComparisonOperator { LessThan, LessOrEqual, Equal, GreaterOrEqual, Greater, ModuloZero }

    [Serializable]
    public class BTCheckValueData
    {
        public BTComparisonOperator operatorType = BTComparisonOperator.Equal;
        public float threshold = 0f;
    }

    [Serializable]
    public class BTValueSwitchCaseData
    {
        public string portName;
        public BTComparisonOperator operatorType;
        public float threshold;
    }

    [Serializable]
    public class BTValueSwitchData
    {
        public List<BTValueSwitchCaseData> cases = new List<BTValueSwitchCaseData>();
    }

    // --- Data ---
    
    public enum BTTargetFaction { Enemy, Ally, Self }
    public enum BTTargetStrategy { Closest, Farthest, LowestHealth, HighestHealth, Random }

    [Serializable]
    public class BTGetTargetData
    {
        public BTTargetFaction faction = BTTargetFaction.Enemy;
        public BTTargetStrategy strategy = BTTargetStrategy.Closest;
        public string requiredTag = ""; // Optional filter, e.g. "Mage", "Healer"
    }

    public enum BTNumericDataType { SelfHPPercent, TargetHPPercent, LowestAllyHPPercent, DistanceToTarget, TurnNumber, AliveAllyCount }

    [Serializable]
    public class BTGetNumericData
    {
        public BTNumericDataType dataType = BTNumericDataType.SelfHPPercent;
    }

    [Serializable]
    public class BTSwitchData // Keep this if they still want String-based blackboard switches, though ValueSwitch is better
    {
        public string blackboardKey = "State";
        public List<string> cases = new List<string>();
    }
}
