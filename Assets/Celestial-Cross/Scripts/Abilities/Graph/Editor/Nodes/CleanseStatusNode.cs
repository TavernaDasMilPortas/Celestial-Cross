using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class CleanseStatusNode : AbilityNode
    {
        private ObjectField specificStatusField;
        private Toggle allPositiveToggle;
        private Toggle allNegativeToggle;

        [System.Serializable]
        public class CleanseData
        {
            public bool allPositive = false;
            public bool allNegative = false;
        }

        private CleanseData nodeData = new CleanseData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Cleanse / Remove Status";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.5f, 0.2f, 0.8f, 0.9f));

            var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            specificStatusField = new ObjectField("Specific Status") { objectType = typeof(ScriptableObject) };
            extensionContainer.Add(specificStatusField);

            allPositiveToggle = new Toggle("Remove All Buffs");
            allPositiveToggle.RegisterValueChangedCallback(evt => nodeData.allPositive = evt.newValue);
            extensionContainer.Add(allPositiveToggle);

            allNegativeToggle = new Toggle("Remove All Debuffs");
            allNegativeToggle.RegisterValueChangedCallback(evt => nodeData.allNegative = evt.newValue);
            extensionContainer.Add(allNegativeToggle);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<CleanseData>(json);
            allPositiveToggle.value = nodeData.allPositive;
            allNegativeToggle.value = nodeData.allNegative;
        }
    }
}
