using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class ReadUnitVariableNode : AbilityNode
    {
        private TextField variableNameField;
        private Toggle isSlotVariableToggle;
        private TextField outputVariableField;

        private ReadUnitVariableNodeData nodeData = new ReadUnitVariableNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Read Unit Var";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.5f, 0.3f, 0.9f));

            var inPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inPort.portName = "In";
            inputContainer.Add(inPort);

            var outPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outPort.portName = "Out";
            outputContainer.Add(outPort);

            variableNameField = new TextField("Var Name");
            variableNameField.value = nodeData.variableName;
            variableNameField.RegisterValueChangedCallback(evt => nodeData.variableName = evt.newValue);
            extensionContainer.Add(variableNameField);

            isSlotVariableToggle = new Toggle("Is Slot Var");
            isSlotVariableToggle.value = nodeData.isSlotVariable;
            isSlotVariableToggle.RegisterValueChangedCallback(evt => nodeData.isSlotVariable = evt.newValue);
            extensionContainer.Add(isSlotVariableToggle);

            outputVariableField = new TextField("Graph Output Var");
            outputVariableField.value = nodeData.outputVariable;
            outputVariableField.RegisterValueChangedCallback(evt => nodeData.outputVariable = evt.newValue);
            extensionContainer.Add(outputVariableField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<ReadUnitVariableNodeData>(json);
            variableNameField.value = nodeData.variableName;
            isSlotVariableToggle.value = nodeData.isSlotVariable;
            outputVariableField.value = nodeData.outputVariable;
        }

        public override string GetDescription()
        {
            string scope = nodeData.isSlotVariable ? "Slot" : "Global";
            return $"Lê a variável '{nodeData.variableName}' ({scope}) da Unidade e salva em '{nodeData.outputVariable}'.";
        }
    }
}
