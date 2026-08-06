using System.Collections.Generic;

namespace CombatSimulator.Core.Models
{
    // From AbilityType.cs (global namespace in Unity)
    public enum AbilityType { Passive = 0, Active = 1, Condition = 2 }
    public enum AbilitySubtype { None = 0, Attack = 1, Movement = 2, Buff = 3, Debuff = 4 }
    
    // From CombatHook.cs - EXACT match (already correct)
    public enum SimCombatHook
    {
        OnManualCast,
        OnRoundStart, OnRoundEnd,
        OnTurnStart, OnTurnEnd,
        OnBeforeAction, OnAfterAction,
        OnBeforeTakeDamage, OnAfterTakeDamage,
        OnBeforeDealDamage, OnAfterDealDamage,
        OnBeforeTakeHeal, OnAfterTakeHeal,
        OnBeforeDealHeal, OnAfterDealHeal,
        OnBeforeApplyCondition, OnAfterApplyCondition,
        OnBeforeRemoveCondition, OnAfterRemoveCondition,
        OnDeath, OnKill,
        OnMoveStart, OnMoveEnd
    }

    // From AreaResolver.cs (global namespace)
    public enum Direction { N = 0, NE = 1, E = 2, SE = 3, S = 4, SW = 5, W = 6, NW = 7 }
    
    // From UnitVariable.cs
    public enum UnitVariable
    {
        Health = 0, Attack = 1, Defense = 2, Speed = 3,
        CriticalChance = 4, CriticalDamage = 5,
        EffectAccuracy = 6, EffectResistance = 7,
        BonusDamagePercent = 8, DamageReductionPercent = 9,
        HealingBonusPercent = 10, ExtraRange = 11, ExtraMoveRange = 12,
        Counter1 = 13, Counter2 = 14, Counter3 = 15
    }
    public enum UnitVariableOperation { Get = 0, Set = 1, Add = 2, Subtract = 3, Multiply = 4, Divide = 5 }
    public enum UnitVariableScope { Global = 0, Slot = 1 }
    
    // From FactionCondition.cs
    public enum FactionTarget { Ally = 0, Enemy = 1 }
    
    // From AttributeCondition.cs
    public enum TargetType { Caster = 0, Target = 1 }
    public enum AttributeType { HP = 0, Attack = 1, Defense = 2, Speed = 3, EffectAccuracy = 4, CriticalChance = 5 }
    public enum Comparison { GreaterThan = 0, LessThan = 1, Equal = 2, GreaterOrEqual = 3, LessOrEqual = 4 }
    public enum ValueMode { Flat = 0, Percentage = 1 }
    
    // From DistanceCondition.cs
    public enum DistanceType { Min = 0, Max = 1, Exact = 2 }
    
    // From RangeCondition.cs
    public enum RangeOrigin { Caster = 0, Target = 1 }
    public enum UnitFilter { Allies = 0, Enemies = 1, Both = 2 }
    // Note: RangeCondition has its OWN Comparison enum but we use the unified one above
    
    // From TurnOrderCondition.cs
    public enum OrderType { FirstInRound = 0, LastInRound = 1, SpecificIndex = 2 }
    
    // From LegacyTypes.cs
    public enum DurationType { Turns = 0, Charges = 1, Infinite = 2 }
    
    // Supporting structs
    public class AreaPatternData
    {
        public List<SimpleVector2Int> PatternCells { get; set; } = new List<SimpleVector2Int>();
    }

    // From StatScalingData.cs - MUST include useTargetStat and percentage fields
    public class StatScalingData
    {
        public string StatTypeName { get; set; }  // e.g. "AttackFlat", "HealthFlat"
        public float Percentage { get; set; }      // scaling percentage (e.g. 100 = 100%)
        public bool UseTargetStat { get; set; }    // false = caster stat, true = target stat
    }
}
