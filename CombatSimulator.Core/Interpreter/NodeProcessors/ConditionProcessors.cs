using System;
using System.Linq;
using CombatSimulator.Core.Models;

namespace CombatSimulator.Core.Interpreter.NodeProcessors
{
    public static class ConditionProcessors
    {
        public static bool EvaluateAttribute(NodeExport node, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<AttributeConditionNodeData>(node.JsonData);
            if (data == null) return false;

            var unit = data.targetToCheck == TargetType.Caster ? context.GetSource() : context.GetTarget();
            if (unit == null) return false;

            float attrValue = data.attribute switch
            {
                AttributeType.HP => data.mode == ValueMode.Percentage ? (unit.CurrentHealth * 100f / Math.Max(1, unit.MaxHealth)) : unit.CurrentHealth,
                AttributeType.Attack => unit.Stats.Attack,
                AttributeType.Defense => unit.Stats.Defense,
                AttributeType.Speed => unit.Stats.Speed,
                AttributeType.EffectAccuracy => unit.Stats.EffectAccuracy,
                AttributeType.CriticalChance => unit.Stats.CriticalChance,
                _ => 0
            };

            return CompareValues(attrValue, data.comparison, data.threshold);
        }

        public static bool EvaluateDistance(NodeExport node, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<DistanceConditionNodeData>(node.JsonData);
            if (data == null) return false;

            var source = context.GetSource();
            var target = context.GetTarget();
            if (source == null || target == null) return false;

            int dist = Math.Abs(source.GridPosition.X - target.GridPosition.X) + Math.Abs(source.GridPosition.Y - target.GridPosition.Y);
            return data.checkType switch
            {
                DistanceType.Min => dist >= data.distanceValue,
                DistanceType.Max => dist <= data.distanceValue,
                DistanceType.Exact => dist == data.distanceValue,
                _ => false
            };
        }

        public static bool EvaluateRange(NodeExport node, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<RangeConditionNodeData>(node.JsonData);
            if (data == null) return false;

            var origin = data.origin == RangeOrigin.Caster ? context.GetSource() : context.GetTarget();
            if (origin == null) return false;

            int count = 0;
            foreach (var unit in context.State.Units)
            {
                if (!unit.IsAlive || unit.UnitId == origin.UnitId) continue;
                if (data.filter == UnitFilter.Allies && unit.Team != origin.Team) continue;
                if (data.filter == UnitFilter.Enemies && unit.Team == origin.Team) continue;

                int dist = Math.Abs(unit.GridPosition.X - origin.GridPosition.X) + Math.Abs(unit.GridPosition.Y - origin.GridPosition.Y);
                if (dist <= data.range) count++;
            }

            return CompareValues(count, data.comparison, data.targetCount);
        }

        public static bool EvaluateFaction(NodeExport node, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<FactionConditionNodeData>(node.JsonData);
            if (data == null) return false;

            var checkUnit = data.target == TargetType.Caster ? context.GetSource() : context.GetTarget();
            var source = context.GetSource();
            if (checkUnit == null || source == null) return false;

            bool sameTeam = checkUnit.Team == source.Team;
            return data.faction == FactionTarget.Ally ? sameTeam : !sameTeam;
        }

        public static bool EvaluateSpeedAdvantage(NodeExport node, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<SpeedAdvantageConditionNodeData>(node.JsonData);
            if (data == null) return false;

            var source = context.GetSource();
            var target = context.GetTarget();
            if (source == null || target == null) return false;

            int diff = source.Stats.Speed - target.Stats.Speed;
            return data.greaterOrEqual ? diff >= data.requiredDifference : diff > data.requiredDifference;
        }

        public static bool EvaluateTurnOrder(NodeExport node, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<TurnOrderConditionNodeData>(node.JsonData);
            if (data == null) return false;

            var queue = context.State.TurnQueue;
            if (queue == null || queue.Count == 0) return false;

            return data.type switch
            {
                OrderType.FirstInRound => queue.Count > 0 && queue[0] == context.SourceUnitId,
                OrderType.LastInRound => queue.Count > 0 && queue[queue.Count - 1] == context.SourceUnitId,
                OrderType.SpecificIndex => data.specificIndex >= 0 && data.specificIndex < queue.Count && queue[data.specificIndex] == context.SourceUnitId,
                _ => false
            };
        }

        private static bool CompareValues(float value, Comparison comparison, float threshold)
        {
            return comparison switch
            {
                Comparison.GreaterThan => value > threshold,
                Comparison.LessThan => value < threshold,
                Comparison.Equal => Math.Abs(value - threshold) < 0.001f,
                Comparison.GreaterOrEqual => value >= threshold,
                Comparison.LessOrEqual => value <= threshold,
                _ => false
            };
        }
    }
}
