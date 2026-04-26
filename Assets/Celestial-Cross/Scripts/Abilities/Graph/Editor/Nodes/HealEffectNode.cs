using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class HealEffectNode : AbilityNode
    {
        private EnumField valueTypeDropdown;
        private IntegerField amountField;
        private EnumField baseAttributeDropdown;
        private Toggle canCritToggle;
        private Toggle allowOverhealToggle;
        private TextField variableReferenceField;

        private Celestial_Cross.Scripts.Abilities.Graph.Runtime.HealNodeData nodeData = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.HealNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Heal Effect";
            
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.8f, 0.4f, 0.9f));

            // Portas
            var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            // UI Elements
            valueTypeDropdown = new EnumField("Value Type", Celestial_Cross.Scripts.Abilities.ValueType.Flat);
            valueTypeDropdown.RegisterValueChangedCallback(evt => {
                nodeData.valueType = (Celestial_Cross.Scripts.Abilities.ValueType)evt.newValue;
                UpdateDynamicFields();
            });
            extensionContainer.Add(valueTypeDropdown);

            amountField = new IntegerField("Amount");
            amountField.value = nodeData.amount;
            amountField.RegisterValueChangedCallback(evt => nodeData.amount = evt.newValue);
            extensionContainer.Add(amountField);

            variableReferenceField = new TextField("Amount Var");
            variableReferenceField.value = nodeData.variableReference;
            variableReferenceField.RegisterValueChangedCallback(evt => nodeData.variableReference = evt.newValue);
            extensionContainer.Add(variableReferenceField);

            baseAttributeDropdown = new EnumField("Base Attribute", ValueType.Flat); 
            baseAttributeDropdown.RegisterValueChangedCallback(evt => {
                nodeData.baseAttribute = System.Convert.ToInt32(evt.newValue);
            });

            canCritToggle = new Toggle("Can Crit Heal");
            canCritToggle.value = nodeData.canCrit;
            canCritToggle.RegisterValueChangedCallback(evt => nodeData.canCrit = evt.newValue);
            extensionContainer.Add(canCritToggle);

            allowOverhealToggle = new Toggle("Allow Overheal");
            allowOverhealToggle.value = nodeData.allowOverheal;
            allowOverhealToggle.RegisterValueChangedCallback(evt => nodeData.allowOverheal = evt.newValue);
            extensionContainer.Add(allowOverhealToggle);

            UpdateDynamicFields();

            RefreshExpandedState();
            RefreshPorts();
        }

        private void UpdateDynamicFields()
        {
            if (nodeData.valueType == Celestial_Cross.Scripts.Abilities.ValueType.Percentage)
            {
                if (!extensionContainer.Contains(baseAttributeDropdown))
                    extensionContainer.Insert(2, baseAttributeDropdown); 
            }
            else
            {
                if (extensionContainer.Contains(baseAttributeDropdown))
                    extensionContainer.Remove(baseAttributeDropdown);
            }
            RefreshExpandedState();
        }

        public override string GetJsonData()
        {
            return JsonUtility.ToJson(nodeData);
        }

        public override void LoadFromJson(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                nodeData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.HealNodeData>(json);
                valueTypeDropdown.value = nodeData.valueType;
                amountField.value = nodeData.amount;
                variableReferenceField.value = nodeData.variableReference;
                canCritToggle.value = nodeData.canCrit;
                allowOverhealToggle.value = nodeData.allowOverheal;
                UpdateDynamicFields();
            }
        }

        public override string GetDescription()
        {
            string overhealText = nodeData.allowOverheal ? " (pode sobre-curar)" : "";
            return $"Cura {nodeData.amount}{nodeData.valueType} de vida{overhealText}.";
        }

        public void SetVariableReference(string varName)
        {
            nodeData.variableReference = varName;
            if (variableReferenceField != null) variableReferenceField.value = varName;
        }
    }
}
