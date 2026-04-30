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
        private EnumField abilitySubtypeDropdown;

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

            // Dropdown de Subtipo (Aparece se for Active)
            abilitySubtypeDropdown = new EnumField("Subtype", AbilitySubtype.None);
            abilitySubtypeDropdown.RegisterValueChangedCallback(evt => nodeData.subtype = (AbilitySubtype)evt.newValue);
            extensionContainer.Add(abilitySubtypeDropdown);


            UpdateDynamicFields();

            RefreshExpandedState();
            RefreshPorts();
        }

        private void UpdateDynamicFields()
        {
            if (nodeData.type == AbilityType.Active)
            {
                if (!extensionContainer.Contains(abilitySubtypeDropdown))
                    extensionContainer.Add(abilitySubtypeDropdown);
            }
            else
            {
                if (extensionContainer.Contains(abilitySubtypeDropdown))
                    extensionContainer.Remove(abilitySubtypeDropdown);
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
                abilitySubtypeDropdown.value = nodeData.subtype;
                UpdateDynamicFields();
            }
        }

        public override string GetDescription()
        {
            string desc = $"Esta é uma habilidade do tipo {nodeData.type}.";
            if (nodeData.type == AbilityType.Active) desc += $" Subtipo: {nodeData.subtype}.";
            return desc;
        }
    }
}
