using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class StartNode : AbilityNode
    {
        private EnumField abilityTypeDropdown;
        private IntegerField durationField;
        private Toggle canStackToggle;
        private IntegerField maxStacksField;
        private Toggle isPersistentToggle;

        private Celestial_Cross.Scripts.Abilities.Graph.Runtime.StartNodeData nodeData = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.StartNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Start";
            EntryPoint = true;
            
            // Header color
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f, 0.9f));

            // Porta de Saída
            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            // Dropdown de Tipo de Habilidade
            abilityTypeDropdown = new EnumField("Ability Type", AbilityType.Active);
            abilityTypeDropdown.RegisterValueChangedCallback(evt => 
            {
                nodeData.type = (AbilityType)evt.newValue;
                UpdateDynamicFields();
            });
            
            extensionContainer.Add(abilityTypeDropdown);

            // Campo dinâmico de Duração
            durationField = new IntegerField("Duration (Turns)");
            durationField.RegisterValueChangedCallback(evt => nodeData.duration = evt.newValue);

            // Persistent toggle
            isPersistentToggle = new Toggle("Persistent (Infinite)");
            isPersistentToggle.RegisterValueChangedCallback(evt => {
                nodeData.isPersistent = evt.newValue;
                UpdateDynamicFields();
            });

            // Stack toggle
            canStackToggle = new Toggle("Can Stack");
            canStackToggle.RegisterValueChangedCallback(evt => {
                nodeData.canStack = evt.newValue;
                UpdateDynamicFields();
            });

            // Max stacks field
            maxStacksField = new IntegerField("Max Stacks (0=Inf)");
            maxStacksField.RegisterValueChangedCallback(evt => nodeData.maxStacks = evt.newValue);

            UpdateDynamicFields();

            RefreshExpandedState();
            RefreshPorts();
        }

        private void UpdateDynamicFields()
        {
            bool showConditionFields = (nodeData.type == AbilityType.Condition || nodeData.type == AbilityType.Passive);

            // Duration: shown for Condition/Passive, hidden if isPersistent
            if (showConditionFields && !nodeData.isPersistent)
            {
                if (!extensionContainer.Contains(durationField))
                    extensionContainer.Add(durationField);
            }
            else
            {
                if (extensionContainer.Contains(durationField))
                    extensionContainer.Remove(durationField);
            }

            // isPersistent: shown for Condition/Passive
            if (showConditionFields)
            {
                if (!extensionContainer.Contains(isPersistentToggle))
                    extensionContainer.Add(isPersistentToggle);
            }
            else
            {
                if (extensionContainer.Contains(isPersistentToggle))
                    extensionContainer.Remove(isPersistentToggle);
            }

            // canStack: shown for Condition/Passive
            if (showConditionFields)
            {
                if (!extensionContainer.Contains(canStackToggle))
                    extensionContainer.Add(canStackToggle);
            }
            else
            {
                if (extensionContainer.Contains(canStackToggle))
                    extensionContainer.Remove(canStackToggle);
            }

            // maxStacks: shown only if canStack is on
            if (showConditionFields && nodeData.canStack)
            {
                if (!extensionContainer.Contains(maxStacksField))
                    extensionContainer.Add(maxStacksField);
            }
            else
            {
                if (extensionContainer.Contains(maxStacksField))
                    extensionContainer.Remove(maxStacksField);
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
                nodeData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.StartNodeData>(json);
                abilityTypeDropdown.value = nodeData.type;
                durationField.value = nodeData.duration;
                isPersistentToggle.value = nodeData.isPersistent;
                canStackToggle.value = nodeData.canStack;
                maxStacksField.value = nodeData.maxStacks;
                UpdateDynamicFields();
            }
        }

        public override string GetDescription()
        {
            string desc = $"Esta é uma habilidade do tipo {nodeData.type}.";
            if (nodeData.type == AbilityType.Condition)
            {
                desc += nodeData.isPersistent ? " Persistente." : $" Duração: {nodeData.duration} turnos.";
                if (nodeData.canStack) desc += $" Stacks: max {(nodeData.maxStacks == 0 ? "∞" : nodeData.maxStacks.ToString())}.";
            }
            return desc;
        }
    }
}
