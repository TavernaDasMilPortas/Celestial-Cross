using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CelestialCross.Dialogue.Graph.Editor
{
    public class DialogueNode : Node
    {
        public string guid;
        public NodeType nodeType = NodeType.Speech;
        public string speakerName;
        public string dialogueText;
        public Sprite characterSprite;
        public bool entryPoint = false;

        // Dados de Condição / Ação
        public string variableName;
        public string compareValue; // Também usado para ActionValue
        public ConditionType conditionType;
        public ActionType actionType;
    }
}
