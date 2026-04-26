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

            UpdateDynamicFields();

            RefreshExpandedState();
            RefreshPorts();
        }

        private void UpdateDynamicFields()
        {
            if (nodeData.type == AbilityType.Passive /* assumindo que exista Condition ou similar, mas usando o q tem no AbilityType atual */) 
            {
                // Se for um tipo que requer duração, adicionamos o campo se não estiver na tela
                if (!extensionContainer.Contains(durationField))
                {
                    extensionContainer.Add(durationField);
                }
            }
            else
            {
                // Caso contrário removemos
                if (extensionContainer.Contains(durationField))
                {
                    extensionContainer.Remove(durationField);
                }
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
                UpdateDynamicFields();
            }
        }

        public override string GetDescription()
        {
            return $"Esta é uma habilidade do tipo {nodeData.type}.";
        }
    }
}
