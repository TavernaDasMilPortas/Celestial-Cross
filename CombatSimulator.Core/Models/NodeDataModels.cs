using System;
using System.Collections.Generic;

namespace CombatSimulator.Core.Models
{
    public enum GraphTargetSourceType { Manual, AutoStrategy }
    public enum GraphTargetMode { Single, Area }
    public enum GraphTargetOrigin { Unit, Point }
    public enum GraphAutoStrategyType { ClosestUnit, FarthestUnit, LowestAttribute, HighestAttribute, Self, MainTarget, RandomTarget }
    public enum GraphFactionType { Ally, Enemy, Any }
    public enum ModifierBonusType { Flat, Percent }
    public enum ModifierValueMode { Value, Variable }

    public class StartNodeData
    {
        public AbilityType type;
        public AbilitySubtype subtype;
        public bool isBuff = true;
    }

    public class TargetNodeData
    {
        public bool targetsSelf = false;
        public bool reusePrevious = false;
        public GraphTargetSourceType sourceType = GraphTargetSourceType.Manual;
        public GraphTargetMode mode = GraphTargetMode.Single;
        public int range = 3;
        public bool multipleTargets = false;
        public bool allowSameTargetMultipleTimes = false;
        public int maxTargets = 1;
        public GraphTargetOrigin origin = GraphTargetOrigin.Unit;
        public GraphAutoStrategyType strategy = GraphAutoStrategyType.ClosestUnit;
        public AttributeType attributeType = AttributeType.HP; 
        public GraphFactionType factionType = GraphFactionType.Any; 
        public int targetCount = 1;
        public bool autoRotate = true;
        public Direction preferredDirection = Direction.N;
        public string patternReferenceId;
        public AreaPatternData areaPattern;
        public string rangeVariable;
        public string maxTargetsVariable;
        public bool useExtraRangeVariable = false;
    }

    public class DamageNodeData
    {
        public string variableReference;
        public List<StatScalingData> scalings = new List<StatScalingData>();
        public bool scaleWithDistance = false;
        public float distanceScaleFactor = 0.1f;
    }

    public class HealNodeData
    {
        public string variableReference;
        public List<StatScalingData> scalings = new List<StatScalingData>();
        public bool canCrit = true;
        public bool allowOverheal = false;
    }

    public class TriggerNodeData
    {
        public SimCombatHook trigger = SimCombatHook.OnManualCast;
    }

    public class LoopNodeData
    {
        public int iterations = 1;
        public string iterationsVariable;
    }

    public class VariableModifierNodeData
    {
        public enum Operation { Set, Add, Multiply, Divide }
        public string variableName;
        public Operation operation = Operation.Set;
        public float value;
        public string valueVariableReference;
        
        public bool useCasterAttribute = false;
        public string casterAttribute = "HealthFlat";
    }

    public class UnitVariableNodeData
    {
        public UnitVariable variable = UnitVariable.ExtraRange;
        public UnitVariableOperation operation = UnitVariableOperation.Get;
        public UnitVariableScope scope = UnitVariableScope.Global;
        public float value;
        public string contextVariableReference;
        public string outputVariable;
    }

    public class LevelBranchNodeData
    {
        public int levelCount = 3;
    }

    public class ModifyAPNodeData
    {
        public int amount = 1;
        public bool modifyMax = false;
    }

    public class CostNodeData
    {
        public int manaCost = 0;
        public string manaVariable;
        public int staminaCost = 0;
        public string staminaVariable;
    }

    public class SacrificeHealthNodeData
    {
        public bool usePercentage = true;
        public float amount = 10f;
        public string outputVariable; 
    }

    public class AttributeConditionNodeData
    {
        public TargetType targetToCheck;
        public AttributeType attribute;
        public Comparison comparison;
        public ValueMode mode;
        public float threshold;
    }

    public class DistanceConditionNodeData
    {
        public DistanceType checkType;
        public int distanceValue;
        public bool checkFaction;
        public FactionTarget faction;
    }

    public class RangeConditionNodeData
    {
        public RangeOrigin origin = RangeOrigin.Caster;
        public int range = 1;
        public UnitFilter filter = UnitFilter.Both;
        public int targetCount = 2;
        public Comparison comparison = Comparison.GreaterOrEqual;
    }

    public class FactionConditionNodeData
    {
        public TargetType target = TargetType.Target;
        public FactionTarget faction = FactionTarget.Enemy;
    }

    public class SpeedAdvantageConditionNodeData
    {
        public int requiredDifference = 10;
        public bool greaterOrEqual = true;
    }

    public class TurnOrderConditionNodeData
    {
        public OrderType type = OrderType.FirstInRound;
        public int specificIndex = 0;
    }

    public class CleanseStatusNodeData
    {
        public bool allPositive = false;
        public bool allNegative = false;
    }

    public class ConditionalFlowNodeData
    {
        public enum LogicMode { And, Or }
        public LogicMode mode = LogicMode.And;
    }

    public class LimitPerTurnNodeData
    {
        public int maxExecutionsPerTurn = 1;
    }

    public class ScheduleExecutionNodeData
    {
        public int delayTurns = 1;
    }

    public class MoveEffectNodeData
    {
        public enum MoveMode { MoveCaster, MoveTarget }
        public MoveMode moveMode = MoveMode.MoveCaster;
        public int range = 3;
        public string rangeVariable;
        public bool manualDestination = true;
        public bool allowOccupiedTiles = false;
        
        public enum MoveType { Push, Pull, TeleportToTarget, DashToTarget }
        public MoveType moveType;
    }

    public class StatModifierNodeData
    {
        public class StatEntry 
        { 
            public int statIndex; 
            public float value;
            public string statTypeName = "AttackFlat";
            public ModifierBonusType bonusType = ModifierBonusType.Flat;
            public ModifierValueMode valueMode = ModifierValueMode.Value;
            public string valueVariable;
        }
        public List<StatEntry> stats = new List<StatEntry>();
        public bool isBuff = true;
        public bool canStack = false;
        public int maxStacks = 1;
    }

    public class ApplyModifierNodeData
    {
        public string modifierId;
        public int stacks = 1;
        public float hitChance = 100f; 
    }

    public class DurationNodeData
    {
        public DurationType type = DurationType.Turns;
        public int value = 1;
    }

    public class RamificationNodeData
    {
        public int tierIndex;
        public List<RamificationFlowData> flows = new List<RamificationFlowData>();
    }

    public class RamificationFlowData
    {
        public string flowId;
        public string flowName;
    }
}
