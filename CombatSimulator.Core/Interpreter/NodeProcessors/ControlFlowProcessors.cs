using System;
using System.Collections.Generic;
using System.Linq;
using CombatSimulator.Core.Models;

namespace CombatSimulator.Core.Interpreter.NodeProcessors
{
    public static class ControlFlowProcessors
    {
        public static string ProcessConditionalFlow(
            AbilityGraphData graph, NodeExport node, SimCombatContext context, 
            SimCombatHook hook, List<BattleEvent> events)
        {
            var data = HeadlessGraphInterpreter.Deserialize<ConditionalFlowNodeData>(node.JsonData);
            if (data == null) data = new ConditionalFlowNodeData();

            // Find condition nodes linked INTO this node's Cond ports
            var condLinks = graph.Links.Where(l => l.TargetNodeGuid == node.Guid && l.TargetPortName.StartsWith("Cond")).ToList();
            if (condLinks.Count == 0) return "True";

            var nodeMap = graph.Nodes.ToDictionary(n => n.Guid);
            bool allTrue = true;
            bool anyTrue = false;

            foreach (var link in condLinks)
            {
                if (!nodeMap.TryGetValue(link.BaseNodeGuid, out var sourceNode)) continue;

                // Evaluate the condition sub-node
                string subResult = HeadlessGraphInterpreter.ProcessNodeStatic(graph, sourceNode, context, hook, events);
                bool isTrue = subResult == "True" || subResult == "Bool Out";
                if (!isTrue) allTrue = false;
                else anyTrue = true;
            }

            bool finalResult = data.mode == ConditionalFlowNodeData.LogicMode.And ? allTrue : anyTrue;
            return finalResult ? "True" : "False";
        }

        public static string ProcessLoop(NodeExport node, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<LoopNodeData>(node.JsonData);
            if (data == null) return "Exit";

            int maxIterations = data.iterations;
            if (!string.IsNullOrEmpty(data.iterationsVariable) && context.Variables.TryGetValue(data.iterationsVariable, out float iterVar))
                maxIterations = (int)iterVar;

            if (!context.LoopCounters.ContainsKey(node.Guid))
                context.LoopCounters[node.Guid] = 0;

            if (context.LoopCounters[node.Guid] < maxIterations)
            {
                context.LoopCounters[node.Guid]++;
                return "Loop";
            }
            else
            {
                context.LoopCounters[node.Guid] = 0;
                return "Exit";
            }
        }

        public static string ProcessRamification(NodeExport node, AbilityGraphData graph, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<RamificationNodeData>(node.JsonData);
            if (data == null) return "Base";

            var source = context.GetSource();
            if (source?.Loadout == null || data.flows == null) return "Base";

            var sel = source.Loadout.BranchSelections.Find(s => s.SkillId == graph.Name);
            if (sel != null && sel.SelectedBranchIds.Count > data.tierIndex)
            {
                string selectedId = sel.SelectedBranchIds[data.tierIndex];
                if (!string.IsNullOrEmpty(selectedId))
                {
                    var matchingFlow = data.flows.Find(f => f.flowId == selectedId);
                    if (matchingFlow != null)
                        return matchingFlow.flowName;
                }
            }

            return "Base";
        }
    }
}
