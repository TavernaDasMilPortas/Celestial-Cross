using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class ScheduleExecutionNode : AbilityNode
    {
        private IntegerField delayField;

        private ScheduleExecutionNodeData nodeData = new ScheduleExecutionNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Schedule Execution";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.4f, 0.1f, 0.9f));

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float)); // Or any type
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            delayField = new IntegerField("Delay Turns");
            delayField.value = nodeData.delayTurns;
            delayField.RegisterValueChangedCallback(evt => nodeData.delayTurns = evt.newValue);
            extensionContainer.Add(delayField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<ScheduleExecutionNodeData>(json);
            delayField.value = nodeData.delayTurns;
        }
    }
}
