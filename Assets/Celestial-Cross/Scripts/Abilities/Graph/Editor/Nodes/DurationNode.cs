using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Modifiers;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class DurationNode : AbilityNode
    {
        private EnumField durationTypeDropdown;
        private IntegerField valueField;

        private DurationNodeData nodeData = new DurationNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Duration / Expiry";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.4f, 0.4f, 0.9f));

            // Porta de Saída de Dados (não de fluxo)
            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Multi, typeof(ModifierDurationSettings));
            outputPort.portName = "Settings Out";
            outputContainer.Add(outputPort);

            durationTypeDropdown = new EnumField("Duration Type", DurationType.Turns);
            durationTypeDropdown.RegisterValueChangedCallback(evt => {
                nodeData.type = (DurationType)evt.newValue;
                UpdateUI();
            });
            extensionContainer.Add(durationTypeDropdown);

            valueField = new IntegerField("Value (Turns/Charges)");
            valueField.value = nodeData.value;
            valueField.RegisterValueChangedCallback(evt => nodeData.value = evt.newValue);
            
            UpdateUI();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void UpdateUI()
        {
            bool showValue = nodeData.type == DurationType.Turns || nodeData.type == DurationType.Charges;
            if (showValue)
            {
                if (!extensionContainer.Contains(valueField)) extensionContainer.Add(valueField);
            }
            else
            {
                if (extensionContainer.Contains(valueField)) extensionContainer.Remove(valueField);
            }
            RefreshExpandedState();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<DurationNodeData>(json);
            durationTypeDropdown.value = nodeData.type;
            valueField.value = nodeData.value;
            UpdateUI();
        }
    }
}
