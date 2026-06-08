using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class LimitPerTurnNode : AbilityNode
    {
        private IntegerField limitField;

        private LimitPerTurnNodeData nodeData = new LimitPerTurnNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Limit Per Turn";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.6f, 0.4f, 0.9f));

            var inPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inPort.portName = "In";
            inputContainer.Add(inPort);

            var outputTruePort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputTruePort.portName = "True";
            outputContainer.Add(outputTruePort);

            var outputFalsePort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputFalsePort.portName = "False";
            outputContainer.Add(outputFalsePort);

            limitField = new IntegerField("Max Executions");
            limitField.value = nodeData.maxExecutionsPerTurn;
            limitField.RegisterValueChangedCallback(evt => nodeData.maxExecutionsPerTurn = evt.newValue);
            extensionContainer.Add(limitField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<LimitPerTurnNodeData>(json);
            limitField.value = nodeData.maxExecutionsPerTurn;
        }
    }
}
