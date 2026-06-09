using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class ConditionalFlowNode : AbilityNode
    {
        private List<Port> conditionPorts = new List<Port>();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Branch (Conditional)";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.6f, 0.9f));

            // Flow In
            var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Single, typeof(float));
            inputPort.portName = "Flow In";
            inputContainer.Add(inputPort);

            // Add Condition Button
            var addBtn = new Button(AddConditionInput) { text = "Add Condition" };
            titleContainer.Add(addBtn);

            // Output Ports
            var truePort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            truePort.portName = "True";
            outputContainer.Add(truePort);

            var falsePort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            falsePort.portName = "False";
            outputContainer.Add(falsePort);

            AddConditionInput(); // Start with one condition

            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddConditionInput()
        {
            int index = conditionPorts.Count;
            
            // Input Data Port for Boolean Condition
            var condPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Single, typeof(bool));
            condPort.portName = $"Cond {index}";
            inputContainer.Add(condPort);
            conditionPorts.Add(condPort);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => ""; 

        public override void LoadFromJson(string json) { }
    }
}

