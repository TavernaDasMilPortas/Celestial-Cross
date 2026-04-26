using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Conditions;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class FactionConditionNode : AbilityNode
    {
        private EnumField targetDropdown;
        private EnumField factionDropdown;

        [System.Serializable]
        public class FactionData
        {
            public AttributeCondition.TargetType target = AttributeCondition.TargetType.Target;
            public FactionTarget faction = FactionTarget.Enemy;
        }

        private FactionData nodeData = new FactionData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Faction Condition";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.6f, 0.4f, 0.9f));

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "Bool Out";
            outputContainer.Add(outputPort);

            targetDropdown = new EnumField("Check", AttributeCondition.TargetType.Target);
            targetDropdown.RegisterValueChangedCallback(evt => nodeData.target = (AttributeCondition.TargetType)evt.newValue);
            extensionContainer.Add(targetDropdown);

            factionDropdown = new EnumField("Is Faction", FactionTarget.Enemy);
            factionDropdown.RegisterValueChangedCallback(evt => nodeData.faction = (FactionTarget)evt.newValue);
            extensionContainer.Add(factionDropdown);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<FactionData>(json);
            targetDropdown.value = nodeData.target;
            factionDropdown.value = nodeData.faction;
        }
    }
}
