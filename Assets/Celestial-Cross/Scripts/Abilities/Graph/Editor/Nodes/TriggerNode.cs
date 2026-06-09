using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class TriggerNode : AbilityNode
    {
        private EnumField hookDropdown;

        private Celestial_Cross.Scripts.Abilities.Graph.Runtime.TriggerNodeData nodeData = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.TriggerNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Trigger Event";
            
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.4f, 0.6f, 0.9f));

            // Portas
            var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Single, typeof(float));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            // UI
            hookDropdown = new EnumField("Trigger Hook", CombatHook.OnManualCast);
            hookDropdown.RegisterValueChangedCallback(evt => nodeData.trigger = (CombatHook)evt.newValue);
            extensionContainer.Add(hookDropdown);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData()
        {
            return JsonUtility.ToJson(nodeData);
        }

        public override void LoadFromJson(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                nodeData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.TriggerNodeData>(json);
                hookDropdown.value = nodeData.trigger;
            }
        }
    }
}
