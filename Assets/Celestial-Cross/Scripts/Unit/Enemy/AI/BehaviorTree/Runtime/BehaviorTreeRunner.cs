using System;
using System.Collections.Generic;
using UnityEngine;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public class BehaviorTreeRunner
    {
        public BTNode RootNode { get; private set; }

        public void Initialize(BehaviorTreeSO treeSo)
        {
            if (treeSo == null || treeSo.NodeData == null || treeSo.NodeData.Count == 0) return;

            Dictionary<string, BTNode> nodes = new Dictionary<string, BTNode>();

            // 1. Instantiate Nodes
            foreach (var nodeData in treeSo.NodeData)
            {
                string runtimeTypeName = MapEditorToRuntime(nodeData.NodeType);
                Type type = null;
                
                string[] namespaces = {
                    "Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime",
                    "Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Actions",
                    "Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Conditions",
                    "Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Data"
                };

                foreach (var ns in namespaces)
                {
                    type = Type.GetType(ns + "." + runtimeTypeName);
                    if (type != null) break;
                }

                if (type == null)
                {
                    Debug.LogWarning($"[BehaviorTreeRunner] Could not find runtime type for {nodeData.NodeType} -> {runtimeTypeName}");
                    continue;
                }

                var node = Activator.CreateInstance(type) as BTNode;
                node.Guid = nodeData.Guid;
                
                LoadNodeData(node, nodeData.JsonData);
                nodes[nodeData.Guid] = node;

                if (runtimeTypeName == "BTRoot")
                {
                    RootNode = node;
                }
            }

            // 2. Link Nodes
            foreach (var link in treeSo.NodeLinks)
            {
                if (!nodes.TryGetValue(link.ParentGuid, out var parent)) continue;
                if (!nodes.TryGetValue(link.ChildGuid, out var child)) continue;

                if (parent is BTComposite composite)
                {
                    composite.ChildNodes[link.ParentPort] = child;
                }
                else if (parent is BTDecorator decorator)
                {
                    decorator.Child = child;
                }
                else if (parent is BTRoot rootNode)
                {
                    rootNode.Child = child;
                }
                else if (parent is BTSwitch switchNode)
                {
                    switchNode.Cases[link.ParentPort] = child;
                }
                else if (parent is BTValueSwitch valSwitchNode)
                {
                    valSwitchNode.ChildNodes[link.ParentPort] = child;
                }
                else if (parent is Data.BTGetTargetNode || parent is Data.BTGetNumericDataNode)
                {
                    // Data nodes don't pass control to children
                }
                
                // If it's a data port connection, handle it
                if (!string.IsNullOrEmpty(link.ChildPort) && child.DataInputs != null)
                {
                    child.DataInputs[link.ChildPort] = parent;
                }
            }
        }

        private string MapEditorToRuntime(string editorType)
        {
            if (editorType == "BTRootEditorNode") return "BTRoot";
            if (editorType == "BTValueSwitchEditorNode") return "BTValueSwitch";
            if (editorType == "BTCheckValueEditorNode") return "BTCheckValue";
            if (editorType == "BTGetNumericDataEditorNode") return "BTGetNumericDataNode";
            if (editorType == "BTGetTargetEditorNode") return "BTGetTargetNode";
            if (editorType.StartsWith("BTAction")) return editorType.Replace("BTAction", "Action").Replace("EditorNode", "");
            if (editorType.StartsWith("BTCondition")) return editorType.Replace("BTCondition", "Condition").Replace("EditorNode", "");
            return editorType.Replace("EditorNode", "");
        }
        
        private void LoadNodeData(BTNode node, string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            
            if (node is Actions.ActionMove moveNode) moveNode.Data = JsonUtility.FromJson<ActionMoveData>(json);
            else if (node is Actions.ActionUseAbility useAbilNode) JsonUtility.FromJsonOverwrite(json, useAbilNode);
            else if (node is BTComposite composite) composite.Data = JsonUtility.FromJson<BTCompositeData>(json);
            else if (node is BTSwitch switchNode) switchNode.Data = JsonUtility.FromJson<BTSwitchData>(json);
            else if (node is BTValueSwitch valSwitch) valSwitch.Data = JsonUtility.FromJson<BTValueSwitchData>(json);
            else if (node is Conditions.BTCheckValue checkVal) checkVal.Data = JsonUtility.FromJson<BTCheckValueData>(json);
            else if (node is Data.BTGetTargetNode targetNode) targetNode.Data = JsonUtility.FromJson<BTGetTargetData>(json);
            else if (node is Data.BTGetNumericDataNode numericNode) numericNode.Data = JsonUtility.FromJson<BTGetNumericData>(json);
            else if (node is Conditions.ConditionTargetHasBuff targetBuff) JsonUtility.FromJsonOverwrite(json, targetBuff);
            else if (node is Conditions.ConditionAoEHitCount aoeHit) JsonUtility.FromJsonOverwrite(json, aoeHit);
            else if (node is Conditions.ConditionAbilityReady abilityReady) JsonUtility.FromJsonOverwrite(json, abilityReady);
            else if (node is BTCooldownDecorator cooldownDecorator)
            {
                var data = JsonUtility.FromJson<BTCooldownData>(json);
                if (data != null)
                {
                    cooldownDecorator.CooldownTurns = data.cooldownTurns;
                }
            }
        }

        public BTResult Evaluate(AIBlackboard blackboard)
        {
            if (RootNode == null) return BTResult.Failure;
            return RootNode.Evaluate(blackboard);
        }
    }
}
