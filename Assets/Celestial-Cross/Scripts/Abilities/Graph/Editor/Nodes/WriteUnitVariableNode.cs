using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class WriteUnitVariableNode : AbilityNode
    {
        private TextField variableNameField;
        private Toggle isSlotVariableToggle;
        private EnumField operationDropdown;
        private FloatField valueField;
        private TextField contextVariableField;

        private WriteUnitVariableNodeData nodeData = new WriteUnitVariableNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Write Unit Var";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.5f, 0.3f, 0.3f, 0.9f));

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

            operationDropdown = new EnumField("Operation", WriteUnitVariableNodeData.Operation.Set);
            operationDropdown.value = nodeData.operation;
            operationDropdown.RegisterValueChangedCallback(evt => nodeData.operation = (WriteUnitVariableNodeData.Operation)evt.newValue);
            extensionContainer.Add(operationDropdown);

            valueField = new FloatField("Value");
            valueField.value = nodeData.value;
            valueField.RegisterValueChangedCallback(evt => nodeData.value = evt.newValue);
            extensionContainer.Add(valueField);

            contextVariableField = new TextField("Context Var Ref");
            contextVariableField.value = nodeData.contextVariableReference;
            contextVariableField.RegisterValueChangedCallback(evt => nodeData.contextVariableReference = evt.newValue);
            extensionContainer.Add(contextVariableField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<WriteUnitVariableNodeData>(json);
            variableNameField.value = nodeData.variableName;
            isSlotVariableToggle.value = nodeData.isSlotVariable;
            operationDropdown.value = nodeData.operation;
            valueField.value = nodeData.value;
            contextVariableField.value = nodeData.contextVariableReference;
        }

        public override string GetDescription()
        {
            string val = string.IsNullOrEmpty(nodeData.contextVariableReference) ? nodeData.value.ToString() : $"[{nodeData.contextVariableReference}]";
            string scope = nodeData.isSlotVariable ? "Slot" : "Global";
            return $"Escreve '{nodeData.variableName}' ({scope}) da Unidade ({nodeData.operation} {val}).";
        }
    }
}
