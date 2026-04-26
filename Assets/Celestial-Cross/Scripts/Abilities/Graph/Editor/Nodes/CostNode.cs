using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class CostNode : AbilityNode
    {
        private IntegerField manaField;
        private TextField manaVarField;
        private IntegerField staminaField;
        private TextField staminaVarField;

        private CostNodeData nodeData = new CostNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Cost / Countdown";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.4f, 0.4f, 0.4f, 0.9f));

            var inPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inPort.portName = "In";
            inputContainer.Add(inPort);

            var outPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outPort.portName = "Out";
            outputContainer.Add(outPort);

            manaField = new IntegerField("Mana Cost");
            manaField.RegisterValueChangedCallback(evt => nodeData.manaCost = evt.newValue);
            extensionContainer.Add(manaField);

            manaVarField = new TextField("Mana Var");
            manaVarField.RegisterValueChangedCallback(evt => nodeData.manaVariable = evt.newValue);
            extensionContainer.Add(manaVarField);

            staminaField = new IntegerField("Stamina Cost");
            staminaField.RegisterValueChangedCallback(evt => nodeData.staminaCost = evt.newValue);
            extensionContainer.Add(staminaField);

            staminaVarField = new TextField("Stamina Var");
            staminaVarField.RegisterValueChangedCallback(evt => nodeData.staminaVariable = evt.newValue);
            extensionContainer.Add(staminaVarField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<CostNodeData>(json);
            manaField.value = nodeData.manaCost;
            manaVarField.value = nodeData.manaVariable;
            staminaField.value = nodeData.staminaCost;
            staminaVarField.value = nodeData.staminaVariable;
        }

        public override string GetDescription()
        {
            return $"Consome recursos ao ser executado.";
        }

        public void SetVariableReference(string manaVar, string staminaVar)
        {
            if (manaVar != null) { nodeData.manaVariable = manaVar; if (manaVarField != null) manaVarField.value = manaVar; }
            if (staminaVar != null) { nodeData.staminaVariable = staminaVar; if (staminaVarField != null) staminaVarField.value = staminaVar; }
        }
    }
}
