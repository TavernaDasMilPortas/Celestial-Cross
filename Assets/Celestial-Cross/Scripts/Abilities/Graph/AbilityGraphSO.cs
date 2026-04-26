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
        public int GetDuration() => GetStartNodeData().duration;
        public bool GetCanStack() => GetStartNodeData().canStack;
        public int GetMaxStacks() => GetStartNodeData().maxStacks;
        public bool GetIsPersistent() => GetStartNodeData().isPersistent;

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

