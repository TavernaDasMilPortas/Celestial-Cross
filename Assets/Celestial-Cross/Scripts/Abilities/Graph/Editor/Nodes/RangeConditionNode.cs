using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Conditions;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class RangeConditionNode : AbilityNode
    {
        private EnumField originDropdown;
        private IntegerField rangeField;
        private EnumField filterDropdown;
        private IntegerField targetCountField;
        private EnumField comparisonDropdown;

        [System.Serializable]
        public class RangeData
        {
            public RangeCondition.RangeOrigin origin = RangeCondition.RangeOrigin.Caster;
            public int range = 1;
            public RangeCondition.UnitFilter filter = RangeCondition.UnitFilter.Both;
            public int targetCount = 2;
            public RangeCondition.Comparison comparison = RangeCondition.Comparison.GreaterOrEqual;
        }

        private RangeData nodeData = new RangeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Range Count Condition";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.6f, 0.4f, 0.9f));

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "Bool Out";
            outputContainer.Add(outputPort);

            originDropdown = new EnumField("Origin", RangeCondition.RangeOrigin.Caster);
            originDropdown.RegisterValueChangedCallback(evt => nodeData.origin = (RangeCondition.RangeOrigin)evt.newValue);
            extensionContainer.Add(originDropdown);

            rangeField = new IntegerField("Search Range");
            rangeField.value = nodeData.range;
            rangeField.RegisterValueChangedCallback(evt => nodeData.range = evt.newValue);
            extensionContainer.Add(rangeField);

            filterDropdown = new EnumField("Filter Units", RangeCondition.UnitFilter.Both);
            filterDropdown.RegisterValueChangedCallback(evt => nodeData.filter = (RangeCondition.UnitFilter)evt.newValue);
            extensionContainer.Add(filterDropdown);

            comparisonDropdown = new EnumField("If Count is", RangeCondition.Comparison.GreaterOrEqual);
            comparisonDropdown.RegisterValueChangedCallback(evt => nodeData.comparison = (RangeCondition.Comparison)evt.newValue);
            extensionContainer.Add(comparisonDropdown);

            targetCountField = new IntegerField("Than");
            targetCountField.value = nodeData.targetCount;
            targetCountField.RegisterValueChangedCallback(evt => nodeData.targetCount = evt.newValue);
            extensionContainer.Add(targetCountField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<RangeData>(json);
            originDropdown.value = nodeData.origin;
            rangeField.value = nodeData.range;
            filterDropdown.value = nodeData.filter;
            comparisonDropdown.value = nodeData.comparison;
            targetCountField.value = nodeData.targetCount;
        }
    }
}
