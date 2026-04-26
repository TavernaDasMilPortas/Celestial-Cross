using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class ConditionalFlowNode : AbilityNode
    {
        private List<Port> conditionPorts = new List<Port>();
        private List<Port> truePorts = new List<Port>();

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
            var addBtn = new Button(AddBranch) { text = "Add Branch" };
            titleContainer.Add(addBtn);

            // Default Else Port
            var elsePort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            elsePort.portName = "Else / False";
            outputContainer.Add(elsePort);

            AddBranch(); // Start with one branch

            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddBranch()
        {
            int index = conditionPorts.Count;
            
            // Input Data Port for Boolean Condition
            var condPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Single, typeof(bool));
            condPort.portName = $"Cond {index}";
            inputContainer.Add(condPort);
            conditionPorts.Add(condPort);

            // Output Flow Port for True case
            var truePort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            truePort.portName = $"True {index}";
            outputContainer.Add(truePort);
            truePorts.Add(truePort);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => ""; // No specific data yet, just ports

        public override void LoadFromJson(string json) { }
    }
}
