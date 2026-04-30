using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph
{
    [CreateAssetMenu(fileName = "NewAbilityGraph", menuName = "Celestial Cross/Abilities/Ability Graph Object")]
    public class AbilityGraphSO : ScriptableObject
    {
        [Header("UI & Identity")]
        public string abilityName;
        public Sprite abilityIcon;
        [TextArea(3, 5)]
        public string abilityDescription;
        public int displayRange;

        [Header("Graph Data")]
        public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
        public List<AbilityNodeData> NodeData = new List<AbilityNodeData>();
        
        [Header("Assets & Dependencies")]
        public List<Dependency> Dependencies = new List<Dependency>();

        [Header("Blackboard & Leveling")]
        public List<GraphVariable> Variables = new List<GraphVariable>();
        public int MaxLevel = 1;

        [Header("AI Hints")]
        public Celestial_Cross.Scripts.Units.Enemy.AI.AIAbilityHint aiHint;

        [TextArea(5, 15)]
        public string GeneratedDescription;

        // --- Helpers que lêem metadados do StartNode ---
        
        private StartNodeData GetStartNodeData()
        {
            var startNode = NodeData.FirstOrDefault(n => n.NodeType == "StartNode");
            if (startNode != null && !string.IsNullOrEmpty(startNode.JsonData))
                return JsonUtility.FromJson<StartNodeData>(startNode.JsonData);
            return new StartNodeData { type = AbilityType.Active };
        }

        public AbilityType GetAbilityType() => GetStartNodeData().type;
        public bool IsPassive => GetAbilityType() == AbilityType.Passive;
        public bool IsCondition => GetAbilityType() == AbilityType.Condition;
        public bool IsActive => GetAbilityType() == AbilityType.Active;
        public int GetDuration() 
        {
            var node = NodeData.FirstOrDefault(n => n.NodeType == "DurationNode");
            if (node != null && !string.IsNullOrEmpty(node.JsonData))
            {
                var data = JsonUtility.FromJson<DurationNodeData>(node.JsonData);
                return (int)data.value;
            }
            return 0;
        }

        public bool GetCanStack() 
        {
            // Procura em StatModifier ou ApplyModifier
            var node = NodeData.FirstOrDefault(n => n.NodeType == "StatModifierEffectNode" || n.NodeType == "ApplyModifierNode");
            if (node != null && !string.IsNullOrEmpty(node.JsonData))
            {
                if (node.NodeType == "StatModifierEffectNode")
                    return JsonUtility.FromJson<StatModifierNodeData>(node.JsonData).canStack;
                else
                    return JsonUtility.FromJson<ApplyModifierNodeData>(node.JsonData).canStack;
            }
            return false;
        }

        public int GetMaxStacks() 
        {
            var node = NodeData.FirstOrDefault(n => n.NodeType == "StatModifierEffectNode" || n.NodeType == "ApplyModifierNode");
            if (node != null && !string.IsNullOrEmpty(node.JsonData))
            {
                if (node.NodeType == "StatModifierEffectNode")
                    return JsonUtility.FromJson<StatModifierNodeData>(node.JsonData).maxStacks;
                else
                    return JsonUtility.FromJson<ApplyModifierNodeData>(node.JsonData).maxStacks;
            }
            return 1;
        }

        public bool GetIsPersistent() 
        {
            var node = NodeData.FirstOrDefault(n => n.NodeType == "DurationNode");
            if (node != null && !string.IsNullOrEmpty(node.JsonData))
            {
                var data = JsonUtility.FromJson<DurationNodeData>(node.JsonData);
                return data.type == Celestial_Cross.Scripts.Abilities.Modifiers.DurationType.Infinite;
            }
            return false;
        }
        
        public bool GetIsBuff() => GetStartNodeData().isBuff;


        // --- Asset helpers ---

        public T GetAsset<T>(string id) where T : ScriptableObject
        {
            var reference = Dependencies.Find(r => r.id == id);
            return reference?.asset as T;
        }

        public float GetVariableInitialValue(string varName)
        {
            var variable = Variables.Find(v => v.name == varName);
            return variable != null ? variable.initialValue : 0f;
        }

        [System.Serializable]
        public class Dependency
        {
            public string id;
            public string type; // Pattern, Condition, etc
            public ScriptableObject asset;
        }

        [System.Serializable]
        public class GraphVariable
        {
            public string name;
            public float initialValue;
            public string description;
        }
    }

    [System.Serializable]
    public class NodeLinkData
    {
        public string BaseNodeGuid;
        public string PortName;
        public string TargetNodeGuid;
        public string TargetPortName;
    }

    [System.Serializable]
    public class AbilityNodeData
    {
        public string Guid;
        public string NodeType; // Ex: "StartNode", "DamageEffectNode"
        public string NodeTitle;
        public Vector2 Position;
        
        // Aqui guardaremos os dados dinâmicos de cada nó em JSON para não perdermos valores
        // ao fechar o editor, já que Unity não serializa polimorfismo de forma simples nativamente
        [TextArea(3, 10)]
        public string JsonData; 

        // Referências a objetos do Unity (ScriptableObjects, Sprites, etc) 
        // precisam estar fora do JSON para serem salvas corretamente pelo Unity.
        // [DEPRECATED] Use AbilityGraphSO.Dependencies instead
        public AreaPatternData areaPattern;
    }
}

