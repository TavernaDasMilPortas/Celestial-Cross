using System;
using System.Collections.Generic;
using UnityEngine;

namespace CelestialCross.Dialogue.Graph
{
    /// <summary>
    /// Tipos de nós suportados pelo sistema de diálogo.
    /// </summary>
    public enum NodeType
    {
        Start,
        Speech,
        Choice,
        Condition,
        Action,
        End
    }

    /// <summary>
    /// Armazena os dados de um nó individual no grafo.
    /// </summary>
    [Serializable]
    public class DialogueNodeData
    {
        public string guid;
        public NodeType nodeType;
        public Vector2 position;

        // Dados de Speech/Fala
        public string speakerName;
        [TextArea(3, 10)]
        public string dialogueText;
        public Sprite characterSprite;
        // public AudioClip voiceClip; // Futura expansão

        // Dados de Choice/Escolha
        public List<ChoiceData> choices = new List<ChoiceData>();

        // Dados de Condição
        public string conditionVariable;
        public string conditionValue;
        public ConditionType conditionType;

        // Dados de Ação (Set Variable)
        public string actionVariable;
        public string actionValue;
        public ActionType actionType;

        // Novos campos para compatibilidade com o editor robusto
        public string variableName;
        public string compareValue;
    }

    [Serializable]
    public class ChoiceData
    {
        public string text;
        public string targetNodeGuid;
        public string requiredFlag; // Legado/Simples
    }

    [Serializable]
    public enum ConditionType { Equals, GreaterThan, LessThan, Boolean }

    [Serializable]
    public enum ActionType { Set, Add, Subtract }

    /// <summary>
    /// Armazena as conexões entre os nós.
    /// </summary>
    [Serializable]
    public class NodeLinkData
    {
        public string baseNodeGuid;
        public string portName;
        public string targetNodeGuid;
    }

    /// <summary>
    /// O ScriptableObject que contém todo o grafo de diálogo.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogueGraph", menuName = "Celestial Cross/Dialogue/Graph")]
    public class DialogueGraph : ScriptableObject
    {
        public List<NodeLinkData> nodeLinks = new List<NodeLinkData>();
        public List<DialogueNodeData> nodeData = new List<DialogueNodeData>();
        
        // Blackboard para variáveis locais do grafo (como Amor Doce)
        public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();
    }

    [Serializable]
    public class ExposedProperty
    {
        public string propertyName = "New Variable";
        public string propertyValue = "";
        public PropertyType type = PropertyType.String;
    }

    public enum PropertyType { String, Int, Bool }
}
