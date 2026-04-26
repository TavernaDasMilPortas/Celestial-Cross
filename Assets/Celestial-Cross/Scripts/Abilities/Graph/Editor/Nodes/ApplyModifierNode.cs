using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class ApplyModifierNode : AbilityNode
    {
        private ObjectField modifierField;
        private EnumField applyTypeDropdown;

        public enum ApplyType { Add, Remove, Refresh }

        [System.Serializable]
        public class ApplyData
        {
            public ApplyType applyType = ApplyType.Add;
        }

        private ApplyData nodeData = new ApplyData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Apply Modifier/Status";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.5f, 0.2f, 0.8f, 0.9f));

            // Portas
            var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            // UI
            modifierField = new ObjectField("Status/Blueprint") { objectType = typeof(ScriptableObject) };
            extensionContainer.Add(modifierField);

            applyTypeDropdown = new EnumField("Action", ApplyType.Add);
            applyTypeDropdown.RegisterValueChangedCallback(evt => nodeData.applyType = (ApplyType)evt.newValue);
            extensionContainer.Add(applyTypeDropdown);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<ApplyData>(json);
            applyTypeDropdown.value = nodeData.applyType;
        }
    }
}
