using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime;
using UnityEditor.UIElements;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor.Nodes
{
    public class BTInverterEditorNode : BTEditorNode
    {
        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(nodeName, position);
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.1f, 0.4f, 0.9f)); // Dark purple
            AddInputPort("Parent", Port.Capacity.Single);
            AddOutputPort("Child", Port.Capacity.Single);
            RefreshExpandedState();
            RefreshPorts();
        }
    }

    public class BTRepeaterEditorNode : BTEditorNode
    {
        private BTRepeaterData data = new BTRepeaterData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(nodeName, position);
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.1f, 0.4f, 0.9f)); // Dark purple
            AddInputPort("Parent", Port.Capacity.Single);
            AddOutputPort("Child", Port.Capacity.Single);

            var countField = new IntegerField("Count") { value = data.count };
            countField.RegisterValueChangedCallback(evt => { data.count = evt.newValue; JsonData = GetJsonData(); });
            extensionContainer.Add(countField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);
        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<BTRepeaterData>(json);
                if (extensionContainer.Q<IntegerField>("Count") is IntegerField f) f.value = data.count;
            }
        }
    }

    public class BTRandomChanceEditorNode : BTEditorNode
    {
        private BTRandomChanceData data = new BTRandomChanceData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(nodeName, position);
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.1f, 0.4f, 0.9f)); // Dark purple
            AddInputPort("Parent", Port.Capacity.Single);
            AddOutputPort("Child", Port.Capacity.Single);

            var chanceField = new FloatField("Chance (%)") { value = data.chancePercent };
            chanceField.RegisterValueChangedCallback(evt => { data.chancePercent = evt.newValue; JsonData = GetJsonData(); });
            extensionContainer.Add(chanceField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);
        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<BTRandomChanceData>(json);
                if (extensionContainer.Q<FloatField>("Chance (%)") is FloatField f) f.value = data.chancePercent;
            }
        }
    }

    public class BTCooldownEditorNode : BTEditorNode
    {
        private BTCooldownData data = new BTCooldownData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(nodeName, position);
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.1f, 0.4f, 0.9f)); // Dark purple
            AddInputPort("Parent", Port.Capacity.Single);
            AddOutputPort("Child", Port.Capacity.Single);

            var cooldownField = new IntegerField("Turns") { value = data.cooldownTurns };
            cooldownField.RegisterValueChangedCallback(evt => { data.cooldownTurns = evt.newValue; JsonData = GetJsonData(); });
            extensionContainer.Add(cooldownField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);
        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<BTCooldownData>(json);
                if (extensionContainer.Q<IntegerField>("Turns") is IntegerField f) f.value = data.cooldownTurns;
            }
        }
    }
}
