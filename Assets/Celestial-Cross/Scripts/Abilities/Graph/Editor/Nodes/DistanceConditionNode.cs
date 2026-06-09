using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Conditions;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class DistanceConditionNode : AbilityNode
    {
        private EnumField distanceTypeDropdown;
        private IntegerField distanceValueField;
        private Toggle checkFactionToggle;
        private EnumField factionDropdown;

        [System.Serializable]
        public class DistanceData
        {
            public DistanceCondition.DistanceType checkType = DistanceCondition.DistanceType.Max;
            public int distanceValue = 5;
            public bool checkFaction = false;
            public FactionTarget faction = FactionTarget.Enemy;
        }

        private DistanceData nodeData = new DistanceData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Distance & Faction";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.6f, 0.4f, 0.9f));

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "Bool Out";
            outputContainer.Add(outputPort);

            // Distância
            distanceTypeDropdown = new EnumField("Check Type", DistanceCondition.DistanceType.Max);
            distanceTypeDropdown.RegisterValueChangedCallback(evt => nodeData.checkType = (DistanceCondition.DistanceType)evt.newValue);
            extensionContainer.Add(distanceTypeDropdown);

            distanceValueField = new IntegerField("Distance Value");
            distanceValueField.value = nodeData.distanceValue;
            distanceValueField.RegisterValueChangedCallback(evt => nodeData.distanceValue = evt.newValue);
            extensionContainer.Add(distanceValueField);

            // Facção (Integrada)
            checkFactionToggle = new Toggle("Check Faction?");
            checkFactionToggle.value = nodeData.checkFaction;
            checkFactionToggle.RegisterValueChangedCallback(evt => {
                nodeData.checkFaction = evt.newValue;
                UpdateUI();
            });
            extensionContainer.Add(checkFactionToggle);

            factionDropdown = new EnumField("Target Faction", FactionTarget.Enemy);
            factionDropdown.RegisterValueChangedCallback(evt => nodeData.faction = (FactionTarget)evt.newValue);

            UpdateUI();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void UpdateUI()
        {
            if (nodeData.checkFaction)
            {
                if (!extensionContainer.Contains(factionDropdown)) extensionContainer.Add(factionDropdown);
            }
            else
            {
                if (extensionContainer.Contains(factionDropdown)) extensionContainer.Remove(factionDropdown);
            }
            RefreshExpandedState();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<DistanceData>(json);
            distanceTypeDropdown.value = nodeData.checkType;
            distanceValueField.value = nodeData.distanceValue;
            checkFactionToggle.value = nodeData.checkFaction;
            factionDropdown.value = nodeData.faction;
            UpdateUI();
        }
    }
}
