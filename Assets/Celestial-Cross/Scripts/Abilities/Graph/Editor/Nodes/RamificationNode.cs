using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class RamificationNode : AbilityNode
    {
        private IntegerField tierIndexField;
        private Button addFlowButton;

        private RamificationNodeData nodeData = new RamificationNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Ramification";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.8f, 0.4f, 0.1f, 0.9f));

            var inPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inPort.portName = "In";
            inputContainer.Add(inPort);

            // Base port (always present)
            var basePort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            basePort.portName = "Base";
            outputContainer.Add(basePort);

            tierIndexField = new IntegerField("Tier Index");
            tierIndexField.value = nodeData.tierIndex;
            tierIndexField.RegisterValueChangedCallback(evt => nodeData.tierIndex = evt.newValue);
            extensionContainer.Add(tierIndexField);

            addFlowButton = new Button(AddFlow) { text = "+ Add Flow" };
            extensionContainer.Add(addFlowButton);

            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddFlow()
        {
            string newFlowId = "flow_" + Guid.NewGuid().ToString().Substring(0, 8);
            string newFlowName = "Novo Fluxo";

            var flowData = new RamificationFlowData { flowId = newFlowId, flowName = newFlowName };
            nodeData.flows.Add(flowData);

            CreateFlowPortAndField(flowData);

            // Try to auto-create and connect Spec Node
            var graphView = GetFirstAncestorOfType<AbilityGraphView>();
            if (graphView != null)
            {
                var newPort = outputContainer.Query<Port>().ToList().FirstOrDefault(p => p.portName == newFlowName);
                if (newPort != null)
                {
                    Vector2 offset = new Vector2(300, nodeData.flows.Count * 150);
                    // Requires AbilityGraphView to have CreateAndConnectSpecNode method
                    // graphView.CreateAndConnectSpecNode(this, newPort, offset);
                }
            }
        }

        private void CreateFlowPortAndField(RamificationFlowData flowData)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;

            var port = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            port.portName = flowData.flowName;
            
            var nameField = new TextField();
            nameField.value = flowData.flowName;
            nameField.style.flexGrow = 1;
            nameField.RegisterValueChangedCallback(evt =>
            {
                flowData.flowName = evt.newValue;
                port.portName = evt.newValue;
            });

            var deleteButton = new Button(() =>
            {
                nodeData.flows.Remove(flowData);
                outputContainer.Remove(container);
                RefreshPorts();
                
                // Disconnect any edges from this port
                var graphView = GetFirstAncestorOfType<AbilityGraphView>();
                if (graphView != null && port.connected)
                {
                    graphView.DeleteElements(port.connections);
                }
            }) { text = "✕" };

            container.Add(port);
            container.Add(nameField);
            container.Add(deleteButton);

            outputContainer.Add(container);
            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<RamificationNodeData>(json);
            
            tierIndexField.value = nodeData.tierIndex;

            // Clear old ports except Base (which is index 0)
            while (outputContainer.childCount > 1)
            {
                outputContainer.RemoveAt(1);
            }

            foreach (var flow in nodeData.flows)
            {
                CreateFlowPortAndField(flow);
            }
        }

        public override string GetDescription()
        {
            return $"Ramificação (Tier {nodeData.tierIndex}) com {nodeData.flows.Count} fluxos.";
        }
    }
}
