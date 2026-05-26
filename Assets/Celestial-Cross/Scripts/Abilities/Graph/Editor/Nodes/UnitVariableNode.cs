using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class UnitVariableNode : AbilityNode
    {
        private EnumField variableDropdown;
        private EnumField operationDropdown;
        private EnumField scopeDropdown;
        private FloatField valueField;
        private TextField contextVariableField;
        private TextField outputVariableField;
        private Label readOnlyWarning;

        private UnitVariableNodeData nodeData = new UnitVariableNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Unit Variable";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.4f, 0.4f, 0.7f, 0.9f));

            var inPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inPort.portName = "In";
            inputContainer.Add(inPort);

            var outPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outPort.portName = "Out";
            outputContainer.Add(outPort);

            variableDropdown = new EnumField("Variable", UnitVariable.ExtraRange);
            variableDropdown.value = nodeData.variable;
            variableDropdown.RegisterValueChangedCallback(evt => {
                nodeData.variable = (UnitVariable)evt.newValue;
                RefreshUIState();
            });
            extensionContainer.Add(variableDropdown);

            operationDropdown = new EnumField("Operation", UnitVariableOperation.Get);
            operationDropdown.value = nodeData.operation;
            operationDropdown.RegisterValueChangedCallback(evt => {
                nodeData.operation = (UnitVariableOperation)evt.newValue;
                RefreshUIState();
            });
            extensionContainer.Add(operationDropdown);

            scopeDropdown = new EnumField("Scope", UnitVariableScope.Global);
            scopeDropdown.value = nodeData.scope;
            scopeDropdown.RegisterValueChangedCallback(evt => nodeData.scope = (UnitVariableScope)evt.newValue);
            extensionContainer.Add(scopeDropdown);

            valueField = new FloatField("Value");
            valueField.value = nodeData.value;
            valueField.RegisterValueChangedCallback(evt => nodeData.value = evt.newValue);
            extensionContainer.Add(valueField);

            contextVariableField = new TextField("Context Var Ref");
            contextVariableField.value = nodeData.contextVariableReference;
            contextVariableField.RegisterValueChangedCallback(evt => nodeData.contextVariableReference = evt.newValue);
            extensionContainer.Add(contextVariableField);

            outputVariableField = new TextField("Output Var");
            outputVariableField.value = nodeData.outputVariable;
            outputVariableField.RegisterValueChangedCallback(evt => nodeData.outputVariable = evt.newValue);
            extensionContainer.Add(outputVariableField);

            readOnlyWarning = new Label("⚠ Read-only stat");
            readOnlyWarning.style.color = new StyleColor(Color.red);
            readOnlyWarning.style.display = DisplayStyle.None;
            extensionContainer.Add(readOnlyWarning);

            RefreshUIState();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void RefreshUIState()
        {
            bool isGet = nodeData.operation == UnitVariableOperation.Get;
            bool isReadOnly = UnitVariableHelper.IsReadOnly(nodeData.variable);

            outputVariableField.style.display = isGet ? DisplayStyle.Flex : DisplayStyle.None;
            valueField.style.display = !isGet ? DisplayStyle.Flex : DisplayStyle.None;
            contextVariableField.style.display = !isGet ? DisplayStyle.Flex : DisplayStyle.None;

            readOnlyWarning.style.display = (!isGet && isReadOnly) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<UnitVariableNodeData>(json);
            
            variableDropdown.value = nodeData.variable;
            operationDropdown.value = nodeData.operation;
            scopeDropdown.value = nodeData.scope;
            valueField.value = nodeData.value;
            contextVariableField.value = nodeData.contextVariableReference;
            outputVariableField.value = nodeData.outputVariable;

            RefreshUIState();
        }

        public override string GetDescription()
        {
            string scopeStr = nodeData.scope.ToString();
            if (nodeData.operation == UnitVariableOperation.Get)
            {
                return $"Lê '{nodeData.variable}' ({scopeStr}) -> '{nodeData.outputVariable}'.";
            }
            else
            {
                string val = string.IsNullOrEmpty(nodeData.contextVariableReference) ? nodeData.value.ToString() : $"[{nodeData.contextVariableReference}]";
                return $"Escreve '{nodeData.variable}' ({scopeStr}): {nodeData.operation} {val}.";
            }
        }
    }
}
