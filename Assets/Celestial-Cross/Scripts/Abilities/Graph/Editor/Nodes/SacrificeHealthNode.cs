using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class SacrificeHealthNode : AbilityNode
    {
        private Toggle usePercentageToggle;
        private FloatField amountField;
        private TextField outputVarField;

        private SacrificeHealthNodeData nodeData = new SacrificeHealthNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Sacrifice Health";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.8f, 0.2f, 0.2f, 0.9f));

            var inPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inPort.portName = "In";
            inputContainer.Add(inPort);

            var outPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outPort.portName = "Out";
            outputContainer.Add(outPort);

            usePercentageToggle = new Toggle("Use % of Max HP?");
            usePercentageToggle.value = nodeData.usePercentage;
            usePercentageToggle.RegisterValueChangedCallback(evt => nodeData.usePercentage = evt.newValue);
            extensionContainer.Add(usePercentageToggle);

            amountField = new FloatField("Amount");
            amountField.value = nodeData.amount;
            amountField.RegisterValueChangedCallback(evt => nodeData.amount = evt.newValue);
            extensionContainer.Add(amountField);

            outputVarField = new TextField("Output Var (Optional)");
            outputVarField.value = nodeData.outputVariable;
            outputVarField.RegisterValueChangedCallback(evt => nodeData.outputVariable = evt.newValue);
            extensionContainer.Add(outputVarField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<SacrificeHealthNodeData>(json);
            usePercentageToggle.value = nodeData.usePercentage;
            amountField.value = nodeData.amount;
            outputVarField.value = nodeData.outputVariable;
        }

        public override string GetDescription()
        {
            string symbol = nodeData.usePercentage ? "%" : " flat";
            return $"Sacrifica {nodeData.amount}{symbol} de Vida. Saída: '{nodeData.outputVariable}'.";
        }
    }
}
