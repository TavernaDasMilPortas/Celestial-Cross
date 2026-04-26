using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class VariableModifierNode : AbilityNode
    {
        private TextField variableNameField;
        private EnumField operationDropdown;
        private FloatField valueField;
        private TextField valueVariableField;

        private VariableModifierNodeData nodeData = new VariableModifierNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Modifier";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.4f, 0.8f, 0.9f));

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

            operationDropdown = new EnumField("Operation", VariableModifierNodeData.Operation.Set);
            operationDropdown.value = nodeData.operation;
            operationDropdown.RegisterValueChangedCallback(evt => nodeData.operation = (VariableModifierNodeData.Operation)evt.newValue);
            extensionContainer.Add(operationDropdown);

            valueField = new FloatField("Value");
            valueField.value = nodeData.value;
            valueField.RegisterValueChangedCallback(evt => nodeData.value = evt.newValue);
            extensionContainer.Add(valueField);

            valueVariableField = new TextField("Val. Variable");
            valueVariableField.value = nodeData.valueVariableReference;
            valueVariableField.RegisterValueChangedCallback(evt => nodeData.valueVariableReference = evt.newValue);
            extensionContainer.Add(valueVariableField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<VariableModifierNodeData>(json);
            variableNameField.value = nodeData.variableName;
            operationDropdown.value = nodeData.operation;
            valueField.value = nodeData.value;
            valueVariableField.value = nodeData.valueVariableReference;
        }

        public override string GetDescription()
        {
            string val = string.IsNullOrEmpty(nodeData.valueVariableReference) ? nodeData.value.ToString() : $"[{nodeData.valueVariableReference}]";
            return $"Modifica '{nodeData.variableName}' ({nodeData.operation} {val}).";
        }
    }
}
