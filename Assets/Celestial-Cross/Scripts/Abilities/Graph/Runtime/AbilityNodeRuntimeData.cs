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
        public int duration;
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
        public AreaPatternData areaPattern;
    }

    [Serializable]
    public class DamageNodeData
    {
        public Celestial_Cross.Scripts.Abilities.ValueType valueType = Celestial_Cross.Scripts.Abilities.ValueType.Flat;
        public int amount = 10;
        public int baseAttribute = (int)AttributeCondition.AttributeType.Attack;
        public bool scaleWithDistance = false;
        public float distanceScaleFactor = 0.1f;
    }

    [Serializable]
    public class HealNodeData
    {
        public Celestial_Cross.Scripts.Abilities.ValueType valueType = Celestial_Cross.Scripts.Abilities.ValueType.Flat;
        public int amount = 10;
        public int baseAttribute = 0;
        public bool canCrit = true;
        public bool allowOverheal = false;
    }

    [Serializable]
    public class TriggerNodeData
    {
        public CombatHook trigger = CombatHook.OnManualCast;
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
    public class FactionConditionNodeData
    {
        public GraphFactionType faction;
    }

    // --- Efeitos ---

    [Serializable]
    public class MoveEffectNodeData
    {
        public enum MoveType { Push, Pull, TeleportToTarget, TeleportBehindTarget, DashToTarget }
        public MoveType moveType;
        public int distance;
    }

    [Serializable]
    public class StatModifierNodeData
    {
        [Serializable]
        public class StatEntry { public int statIndex; public float value; }
        public List<StatEntry> stats = new List<StatEntry>();
        public bool isBuff = true;
    }

    [Serializable]
    public class ApplyModifierNodeData
    {
        public string modifierId;
        public int stacks = 1;
    }

    [Serializable]
    public class VfxNodeData
    {
        public string vfxId;
        public bool attachToTarget;
        public float duration;
    }

    [Serializable]
    public class CostNodeData
    {
        public int manaCost;
        public int staminaCost;
    }

    [Serializable]
    public class DurationNodeData
    {
        public int durationType; 
        public float value;
    }
}
