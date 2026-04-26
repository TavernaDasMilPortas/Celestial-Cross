using System.Collections.Generic;
using UnityEngine;

namespace Celestial_Cross.Scripts.Abilities.Graph
{
    [CreateAssetMenu(fileName = "NewAbilityGraph", menuName = "Celestial Cross/Abilities/Ability Graph Object")]
    public class AbilityGraphSO : ScriptableObject
    {
        public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
        public List<AbilityNodeData> NodeData = new List<AbilityNodeData>();
        
        [TextArea(5, 15)]
        public string GeneratedDescription;
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
        public AreaPatternData areaPattern;
    }
}
