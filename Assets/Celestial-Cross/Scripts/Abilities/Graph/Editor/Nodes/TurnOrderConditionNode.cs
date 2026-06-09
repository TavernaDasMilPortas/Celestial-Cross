using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Conditions;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class TurnOrderConditionNode : AbilityNode
    {
        private EnumField typeDropdown;
        private IntegerField indexField;

        [System.Serializable]
        public class TurnOrderData
        {
            public TurnOrderCondition.OrderType type = TurnOrderCondition.OrderType.FirstInRound;
            public int specificIndex = 0;
        }

        private TurnOrderData nodeData = new TurnOrderData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Turn Order Condition";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.6f, 0.4f, 0.9f));

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "Bool Out";
            outputContainer.Add(outputPort);

            typeDropdown = new EnumField("Order Type", TurnOrderCondition.OrderType.FirstInRound);
            typeDropdown.RegisterValueChangedCallback(evt => {
                nodeData.type = (TurnOrderCondition.OrderType)evt.newValue;
                UpdateUI();
            });
            extensionContainer.Add(typeDropdown);

            indexField = new IntegerField("Specific Index");
            indexField.value = nodeData.specificIndex;
            indexField.RegisterValueChangedCallback(evt => nodeData.specificIndex = evt.newValue);

            UpdateUI();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void UpdateUI()
        {
            if (nodeData.type == TurnOrderCondition.OrderType.SpecificIndex)
            {
                if (!extensionContainer.Contains(indexField)) extensionContainer.Add(indexField);
            }
            else
            {
                if (extensionContainer.Contains(indexField)) extensionContainer.Remove(indexField);
            }
            RefreshExpandedState();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<TurnOrderData>(json);
            typeDropdown.value = nodeData.type;
            indexField.value = nodeData.specificIndex;
            UpdateUI();
        }
    }
}
