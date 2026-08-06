using System;
using System.Collections.Generic;
using System.Linq;
using CombatSimulator.Core.Models;
using CombatSimulator.Core.Interpreter.NodeProcessors;
using Newtonsoft.Json;

namespace CombatSimulator.Core.Interpreter
{
    public static class HeadlessGraphInterpreter
    {
        public static void ExecuteGraph(
            AbilityGraphData graph,
            SimCombatContext context,
            SimCombatHook hook,
            List<BattleEvent> events)
        {
            if (graph == null || graph.Nodes.Count == 0) return;

            foreach (var v in graph.Variables)
            {
                if (!context.Variables.ContainsKey(v.Name))
                    context.Variables[v.Name] = v.InitialValue;
            }

            var nodeMap = graph.Nodes.ToDictionary(n => n.Guid);

            NodeExport currentNode = null;
            if (hook == SimCombatHook.OnManualCast)
            {
                currentNode = graph.Nodes.FirstOrDefault(n => n.NodeType == "StartNode");
            }
            else
            {
                currentNode = graph.Nodes.FirstOrDefault(n =>
                {
                    if (n.NodeType != "TriggerNode") return false;
                    var data = Deserialize<TriggerNodeData>(n.JsonData);
                    return data != null && data.trigger == hook;
                });
            }

            if (currentNode == null) return;

            int safety = 0;
            while (currentNode != null && safety++ < 1000)
            {
                string nextPort = ProcessNodeStatic(graph, currentNode, context, hook, events);

                if (nextPort == "Scheduled") break;

                var link = graph.Links.FirstOrDefault(l =>
                    l.BaseNodeGuid == currentNode.Guid && l.PortName == nextPort);

                currentNode = link != null && nodeMap.ContainsKey(link.TargetNodeGuid)
                    ? nodeMap[link.TargetNodeGuid]
                    : null;
            }
        }

        public static string ProcessNodeStatic(
            AbilityGraphData graph,
            NodeExport node,
            SimCombatContext context,
            SimCombatHook hook,
            List<BattleEvent> events)
        {
            switch (node.NodeType)
            {
                case "StartNode":
                case "TriggerNode":
                case "RamificationSpecNode":
                case "VfxNode":
                case "DurationNode":
                case "CostNode":
                    return "Out";

                case "TargetNode":
                    return TargetNodeProcessor.Process(graph, node, context);

                case "DamageEffectNode":
                    return DamageNodeProcessor.Process(node, context, events);

                case "HealEffectNode":
                    return HealNodeProcessor.Process(node, context, events);

                case "MoveEffectNode":
                    return EffectProcessors.ProcessMove(node, context, events);

                case "StatModifierEffectNode":
                    return EffectProcessors.ProcessStatModifier(node, graph, context, events);

                case "ApplyModifierNode":
                    return EffectProcessors.ProcessApplyModifier(node, graph, context, events);

                case "ModifyAPNode":
                    return EffectProcessors.ProcessModifyAP(node, context, events);

                case "CleanseStatusNode":
                    return EffectProcessors.ProcessCleanse(node, context, events);

                case "SacrificeHealthNode":
                    return EffectProcessors.ProcessSacrifice(node, context, events);

                case "LimitPerTurnNode":
                    return EffectProcessors.ProcessLimit(node, context);

                case "ScheduleExecutionNode":
                    return EffectProcessors.ProcessSchedule(node, graph, context);

                case "ConditionalFlowNode":
                    return ControlFlowProcessors.ProcessConditionalFlow(graph, node, context, hook, events);

                case "LoopNode":
                    return ControlFlowProcessors.ProcessLoop(node, context);

                case "LevelBranchNode":
                    return $"Level {context.AbilityLevel}";

                case "RamificationNode":
                    return ControlFlowProcessors.ProcessRamification(node, graph, context);

                case "VariableModifierNode":
                    return VariableProcessors.ProcessModifier(node, context);

                case "UnitVariableNode":
                    return VariableProcessors.ProcessUnitVariable(node, context);

                case "AttributeConditionNode":
                    return ConditionProcessors.EvaluateAttribute(node, context) ? "True" : "False";

                case "DistanceConditionNode":
                    return ConditionProcessors.EvaluateDistance(node, context) ? "True" : "False";

                case "RangeConditionNode":
                    return ConditionProcessors.EvaluateRange(node, context) ? "True" : "False";

                case "FactionConditionNode":
                    return ConditionProcessors.EvaluateFaction(node, context) ? "True" : "False";

                case "SpeedAdvantageConditionNode":
                    return ConditionProcessors.EvaluateSpeedAdvantage(node, context) ? "True" : "False";

                case "TurnOrderConditionNode":
                    return ConditionProcessors.EvaluateTurnOrder(node, context) ? "True" : "False";

                default:
                    return "Out";
            }
        }

        public static T Deserialize<T>(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData)) return default;
            try { return JsonConvert.DeserializeObject<T>(jsonData); }
            catch { return default; }
        }
    }
}
