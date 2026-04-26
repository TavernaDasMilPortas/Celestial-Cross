using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Conditions;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class AttributeConditionNode : AbilityNode
    {
        private EnumField targetDropdown;
        private EnumField attributeDropdown;
        private EnumField operatorDropdown;
        private EnumField modeDropdown;
        private FloatField thresholdField;

        private Celestial_Cross.Scripts.Abilities.Graph.Runtime.AttributeConditionNodeData nodeData = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.AttributeConditionNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Attribute Condition";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.6f, 0.4f, 0.9f));

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "Bool Out";
            outputContainer.Add(outputPort);

            targetDropdown = new EnumField("Check", AttributeCondition.TargetType.Target);
            targetDropdown.RegisterValueChangedCallback(evt => nodeData.targetToCheck = (AttributeCondition.TargetType)evt.newValue);
            extensionContainer.Add(targetDropdown);

            attributeDropdown = new EnumField("Attribute", AttributeCondition.AttributeType.HP);
            attributeDropdown.RegisterValueChangedCallback(evt => nodeData.attribute = (AttributeCondition.AttributeType)evt.newValue);
            extensionContainer.Add(attributeDropdown);

            operatorDropdown = new EnumField("Operator", AttributeCondition.Comparison.LessOrEqual);
            operatorDropdown.RegisterValueChangedCallback(evt => nodeData.comparison = (AttributeCondition.Comparison)evt.newValue);
            extensionContainer.Add(operatorDropdown);

            modeDropdown = new EnumField("Mode", AttributeCondition.ValueMode.Flat);
            modeDropdown.RegisterValueChangedCallback(evt => nodeData.mode = (AttributeCondition.ValueMode)evt.newValue);
            extensionContainer.Add(modeDropdown);

            thresholdField = new FloatField("Threshold");
            thresholdField.value = nodeData.threshold;
            thresholdField.RegisterValueChangedCallback(evt => nodeData.threshold = evt.newValue);
            extensionContainer.Add(thresholdField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.AttributeConditionNodeData>(json);
            targetDropdown.value = nodeData.targetToCheck;
            attributeDropdown.value = nodeData.attribute;
            operatorDropdown.value = nodeData.comparison;
            modeDropdown.value = nodeData.mode;
            thresholdField.value = nodeData.threshold;
        }

        public override string GetDescription()
        {
            return $"Se {nodeData.targetToCheck}.{nodeData.attribute} for {nodeData.comparison} a {nodeData.threshold} ({nodeData.mode}).";
        }
    }
}
