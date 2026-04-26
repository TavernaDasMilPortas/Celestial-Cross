using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class MoveEffectNode : AbilityNode
    {
        private Toggle moveTargetToggle;
        private IntegerField rangeField;
        private Toggle manualDestToggle;
        private Toggle allowOccupiedToggle;

        [System.Serializable]
        public class MoveNodeData
        {
            public bool moveTarget = false;
            public int range = 3;
            public bool manualDestination = true;
            public bool allowOccupiedTiles = false;
        }

        private MoveNodeData nodeData = new MoveNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Move Effect";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.4f, 0.6f, 0.8f, 0.9f));

            // Portas
            var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            // UI
            moveTargetToggle = new Toggle("Move Target?");
            moveTargetToggle.value = nodeData.moveTarget;
            moveTargetToggle.RegisterValueChangedCallback(evt => nodeData.moveTarget = evt.newValue);
            extensionContainer.Add(moveTargetToggle);

            rangeField = new IntegerField("Range");
            rangeField.value = nodeData.range;
            rangeField.RegisterValueChangedCallback(evt => nodeData.range = evt.newValue);
            extensionContainer.Add(rangeField);

            manualDestToggle = new Toggle("Manual Destination?");
            manualDestToggle.value = nodeData.manualDestination;
            manualDestToggle.RegisterValueChangedCallback(evt => nodeData.manualDestination = evt.newValue);
            extensionContainer.Add(manualDestToggle);

            allowOccupiedToggle = new Toggle("Allow Occupied?");
            allowOccupiedToggle.value = nodeData.allowOccupiedTiles;
            allowOccupiedToggle.RegisterValueChangedCallback(evt => nodeData.allowOccupiedTiles = evt.newValue);
            extensionContainer.Add(allowOccupiedToggle);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<MoveNodeData>(json);
            moveTargetToggle.value = nodeData.moveTarget;
            rangeField.value = nodeData.range;
            manualDestToggle.value = nodeData.manualDestination;
            allowOccupiedToggle.value = nodeData.allowOccupiedTiles;
        }
    }
}
