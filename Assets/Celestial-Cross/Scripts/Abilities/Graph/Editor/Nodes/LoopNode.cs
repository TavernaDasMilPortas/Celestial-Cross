using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class LoopNode : AbilityNode
    {
        private IntegerField iterationsField;
        private TextField iterationsVariableField;

        private LoopNodeData nodeData = new LoopNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Loop";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f, 0.9f));

            // In Port
            var inPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inPort.portName = "In";
            inputContainer.Add(inPort);

            // Loop Out Port
            var loopPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            loopPort.portName = "Loop";
            outputContainer.Add(loopPort);

            // Exit Out Port
            var exitPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            exitPort.portName = "Exit";
            outputContainer.Add(exitPort);

            iterationsField = new IntegerField("Iterations");
            iterationsField.value = nodeData.iterations;
            iterationsField.RegisterValueChangedCallback(evt => nodeData.iterations = evt.newValue);
            extensionContainer.Add(iterationsField);

            iterationsVariableField = new TextField("Iter. Variable");
            iterationsVariableField.value = nodeData.iterationsVariable;
            iterationsVariableField.RegisterValueChangedCallback(evt => nodeData.iterationsVariable = evt.newValue);
            extensionContainer.Add(iterationsVariableField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<LoopNodeData>(json);
            iterationsField.value = nodeData.iterations;
            iterationsVariableField.value = nodeData.iterationsVariable;
        }

        public override string GetDescription()
        {
            string iter = string.IsNullOrEmpty(nodeData.iterationsVariable) ? nodeData.iterations.ToString() : $"[{nodeData.iterationsVariable}]";
            return $"Repete o ramo 'Loop' {iter} vezes.";
        }
    }
}
