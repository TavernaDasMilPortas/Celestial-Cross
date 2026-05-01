using System;
using System.Collections.Generic;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities.Conditions;
using Celestial_Cross.Scripts.Abilities; 
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Graph.Runtime
{
    // --- Enums Globais para o Grafo ---
    public enum GraphTargetSourceType { Manual, AutoStrategy }
    public enum GraphTargetMode { Single, Area }
    public enum GraphTargetOrigin { Unit, Point }
    public enum GraphAutoStrategyType { ClosestUnit, FarthestUnit, LowestAttribute, HighestAttribute, Self, MainTarget, RandomTarget }
    public enum GraphFactionType { Ally, Enemy, Any }

    // --- Dados dos Nós ---

    [Serializable]
    public class StartNodeData
    {
        public AbilityType type;
        public AbilitySubtype subtype;
        public bool isBuff = true;
    }


    [Serializable]
    public class TargetNodeData
    {
        public bool reusePrevious = false;
        public GraphTargetSourceType sourceType = GraphTargetSourceType.Manual;
        public GraphTargetMode mode = GraphTargetMode.Single;
        public int range = 3;
        public bool multipleTargets = false;
        public int maxTargets = 1;
        public GraphTargetOrigin origin = GraphTargetOrigin.Unit;
        public GraphAutoStrategyType strategy = GraphAutoStrategyType.ClosestUnit;
        public int attributeType = 0; 
        public GraphFactionType factionType = GraphFactionType.Any; 
        public int targetCount = 1;
        public bool autoRotate = true;
        public Direction preferredDirection = Direction.N;
        public string patternReferenceId;
        public AreaPatternData areaPattern;
        public string rangeVariable;
        public string maxTargetsVariable;
    }

    [Serializable]
    public class DamageNodeData
    {
        public string variableReference; // Se preenchido, usa o valor da variável
        public int baseAttribute = (int)AttributeCondition.AttributeType.Attack;
        public bool scaleWithDistance = false;
        public float distanceScaleFactor = 0.1f;
    }

    [Serializable]
    public class HealNodeData
    {
        public string variableReference;
        public int baseAttribute = 0;
        public bool canCrit = true;
        public bool allowOverheal = false;
    }

    [Serializable]
    public class TriggerNodeData
    {
        public CombatHook trigger = CombatHook.OnManualCast;
    }

    // --- Logic & Flow ---

    [Serializable]
    public class LoopNodeData
    {
        public int iterations = 1;
        public string iterationsVariable; // Opcional: usar uma variável para o número de iterações
    }

    [Serializable]
    public class VariableModifierNodeData
    {
        public enum Operation { Set, Add, Multiply }
        public string variableName;
        public Operation operation = Operation.Set;
        public float value;
        public string valueVariableReference; // Opcional: usar outra variável como valor
    }

    [Serializable]
    public class LevelBranchNodeData
    {
        public int levelCount = 3;
    }

    [Serializable]
    public class CostNodeData
    {
        public int manaCost;
        public string manaVariable;
        public int staminaCost;
        public string staminaVariable;
    }

    // --- Condições ---

    [Serializable]
    public class AttributeConditionNodeData
    {
        public AttributeCondition.TargetType targetToCheck;
        public AttributeCondition.AttributeType attribute;
        public AttributeCondition.Comparison comparison;
        public AttributeCondition.ValueMode mode;
        public float threshold;
    }

    [Serializable]
    public class DistanceConditionNodeData
    {
        public DistanceCondition.DistanceType checkType;
        public int distance;
        public bool integrateFaction;
        public GraphFactionType faction;
    }

    [Serializable]
    public class RangeConditionNodeData
    {
        public RangeCondition.RangeOrigin origin = RangeCondition.RangeOrigin.Caster;
        public int range = 1;
        public RangeCondition.UnitFilter filter = RangeCondition.UnitFilter.Both;
        public int targetCount = 2;
        public RangeCondition.Comparison comparison = RangeCondition.Comparison.GreaterOrEqual;
    }

    [Serializable]
    public class FactionConditionNodeData
    {
        public AttributeCondition.TargetType target = AttributeCondition.TargetType.Target;
        public FactionTarget faction = FactionTarget.Enemy;
    }

    [Serializable]
    public class SpeedAdvantageConditionNodeData
    {
        public int requiredDifference = 10;
        public bool greaterOrEqual = true;
    }

    [Serializable]
    public class TurnOrderConditionNodeData
    {
        public TurnOrderCondition.OrderType type = TurnOrderCondition.OrderType.FirstInRound;
        public int specificIndex = 0;
    }

    [Serializable]
    public class CleanseStatusNodeData
    {
        public bool allPositive = false;
        public bool allNegative = false;
    }

    // --- Efeitos ---

    [Serializable]
    public class MoveEffectNodeData
    {
        public enum MoveMode { MoveCaster, MoveTarget }
        public MoveMode moveMode = MoveMode.MoveCaster;
        public int range = 3;
        public string rangeVariable;
        public bool manualDestination = true;
        public bool allowOccupiedTiles = false;
        
        // Mantemos legado para suporte básico ou futuras adições
        public enum MoveType { Push, Pull, TeleportToTarget, DashToTarget }
        public MoveType moveType;
    }

    [Serializable]
    public class StatModifierNodeData
    {
        [Serializable]
        public class StatEntry { public int statIndex; public float value; }
        public List<StatEntry> stats = new List<StatEntry>();
        public bool isBuff = true;
        public string variableReference; // Multiplicador unificado ou base para os buffs
        public bool canStack = false;
        public int maxStacks = 1;
    }

    [Serializable]
    public class ApplyModifierNodeData
    {
        public string modifierId;
        public int stacks = 1;
        public bool canStack = false;
        public int maxStacks = 1;
    }

    [Serializable]
    public class VfxNodeData

    {
        public string vfxId;
        public bool attachToTarget;
        public float duration;
    }

    [Serializable]
    public class DurationNodeData
    {
        public Celestial_Cross.Scripts.Abilities.Modifiers.DurationType type = Celestial_Cross.Scripts.Abilities.Modifiers.DurationType.Turns;
        public int value = 1;
    }
}

