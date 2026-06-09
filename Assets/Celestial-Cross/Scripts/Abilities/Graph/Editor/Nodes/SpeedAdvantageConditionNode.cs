using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Conditions;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class SpeedAdvantageConditionNode : AbilityNode
    {
        private IntegerField diffField;
        private Toggle geToggle;

        [System.Serializable]
        public class SpeedData
        {
            public int requiredDifference = 10;
            public bool greaterOrEqual = true;
        }

        private SpeedData nodeData = new SpeedData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Speed Advantage";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.6f, 0.4f, 0.9f));

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "Bool Out";
            outputContainer.Add(outputPort);

            diffField = new IntegerField("Required Diff");
            diffField.value = nodeData.requiredDifference;
            diffField.RegisterValueChangedCallback(evt => nodeData.requiredDifference = evt.newValue);
            extensionContainer.Add(diffField);

            geToggle = new Toggle("Caster >= Target + X?");
            geToggle.value = nodeData.greaterOrEqual;
            geToggle.RegisterValueChangedCallback(evt => nodeData.greaterOrEqual = evt.newValue);
            extensionContainer.Add(geToggle);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<SpeedData>(json);
            diffField.value = nodeData.requiredDifference;
            geToggle.value = nodeData.greaterOrEqual;
        }
    }
}
