using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class CostNode : AbilityNode
    {
        public enum ResourceType { Mana, Stamina, HP, ActionPoints, Cooldown }
        public enum Timing { OnCast, OnHit }

        private EnumField resourceDropdown;
        private IntegerField amountField;
        private EnumField timingDropdown;

        [System.Serializable]
        public class CostData
        {
            public ResourceType resource = ResourceType.Mana;
            public int amount = 10;
            public Timing timing = Timing.OnCast;
        }

        private CostData nodeData = new CostData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Cost / Countdown";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.4f, 0.4f, 0.4f, 0.9f));

            var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            resourceDropdown = new EnumField("Resource", ResourceType.Mana);
            resourceDropdown.RegisterValueChangedCallback(evt => nodeData.resource = (ResourceType)evt.newValue);
            extensionContainer.Add(resourceDropdown);

            amountField = new IntegerField("Amount");
            amountField.RegisterValueChangedCallback(evt => nodeData.amount = evt.newValue);
            extensionContainer.Add(amountField);

            timingDropdown = new EnumField("Timing", Timing.OnCast);
            timingDropdown.RegisterValueChangedCallback(evt => nodeData.timing = (Timing)evt.newValue);
            extensionContainer.Add(timingDropdown);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<CostData>(json);
            resourceDropdown.value = nodeData.resource;
            amountField.value = nodeData.amount;
            timingDropdown.value = nodeData.timing;
        }
    }
}
