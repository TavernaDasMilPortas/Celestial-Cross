using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class ModifyAPNode : AbilityNode
    {
        private IntegerField amountField;
        private Toggle addMaxToggle;

        private ModifyAPNodeData nodeData = new ModifyAPNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Modify AP";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.8f, 0.4f, 0.2f, 0.9f));

            var inPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inPort.portName = "In";
            inputContainer.Add(inPort);

            var outPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outPort.portName = "Out";
            outputContainer.Add(outPort);

            amountField = new IntegerField("Amount");
            amountField.value = nodeData.amount;
            amountField.RegisterValueChangedCallback(evt => nodeData.amount = evt.newValue);
            extensionContainer.Add(amountField);

            addMaxToggle = new Toggle("Modify Max AP instead?");
            addMaxToggle.value = nodeData.modifyMax;
            addMaxToggle.RegisterValueChangedCallback(evt => nodeData.modifyMax = evt.newValue);
            extensionContainer.Add(addMaxToggle);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<ModifyAPNodeData>(json);
            amountField.value = nodeData.amount;
            addMaxToggle.value = nodeData.modifyMax;
        }

        public override string GetDescription()
        {
            string target = nodeData.modifyMax ? "Max AP" : "Current AP";
            return $"Modifica {target} em {nodeData.amount}.";
        }
    }
}
