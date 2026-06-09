using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime;
using UnityEditor.UIElements;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor.Nodes
{
    public class BTConditionBaseEditorNode : BTEditorNode
    {
        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(nodeName, position);
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.4f, 0.4f, 0.1f, 0.9f));
            AddInputPort("Parent", Port.Capacity.Single);
            RefreshExpandedState();
            RefreshPorts();
        }
    }

    public class BTConditionTargetInRangeEditorNode : BTConditionBaseEditorNode 
    {
        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(nodeName, position);
            AddDataInputPort("Target", UnityEditor.Experimental.GraphView.Port.Capacity.Single);
            RefreshExpandedState();
            RefreshPorts();
        }
    }

    public class BTConditionTargetHasBuffEditorNode : BTConditionBaseEditorNode
    {
        private ConditionTargetHasBuffData data = new ConditionTargetHasBuffData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(nodeName, position);
            var buffToggle = new Toggle("Is Buff?") { value = data.isBuff };
            buffToggle.RegisterValueChangedCallback(evt => { data.isBuff = evt.newValue; JsonData = GetJsonData(); });
            extensionContainer.Add(buffToggle);

            var idField = new TextField("Modifier ID") { value = data.modifierId };
            idField.RegisterValueChangedCallback(evt => { data.modifierId = evt.newValue; JsonData = GetJsonData(); });
            extensionContainer.Add(idField);

            RefreshExpandedState();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);
        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<ConditionTargetHasBuffData>(json);
                if (extensionContainer.Q<Toggle>("Is Buff?") is Toggle f1) f1.value = data.isBuff;
                if (extensionContainer.Q<TextField>("Modifier ID") is TextField f2) f2.value = data.modifierId;
            }
        }
    }

    public class BTConditionAoEHitCountEditorNode : BTConditionBaseEditorNode
    {
        private ConditionAoEHitCountData data = new ConditionAoEHitCountData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(nodeName, position);
            var hitField = new IntegerField("Min Hit Count") { value = data.minHitCount };
            hitField.RegisterValueChangedCallback(evt => { data.minHitCount = evt.newValue; JsonData = GetJsonData(); });
            extensionContainer.Add(hitField);
            RefreshExpandedState();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);
        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<ConditionAoEHitCountData>(json);
                if (extensionContainer.Q<IntegerField>("Min Hit Count") is IntegerField f1) f1.value = data.minHitCount;
            }
        }
    }

    public class BTConditionAbilityReadyEditorNode : BTConditionBaseEditorNode
    {
        private ConditionAbilityReadyData data = new ConditionAbilityReadyData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(nodeName, position);
            var catField = new EnumField("Category", data.category);
            catField.RegisterValueChangedCallback(evt => { data.category = (AIAbilityHint.AbilityCategory)evt.newValue; JsonData = GetJsonData(); });
            extensionContainer.Add(catField);
            RefreshExpandedState();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);
        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<ConditionAbilityReadyData>(json);
                if (extensionContainer.Q<EnumField>("Category") is EnumField f1) f1.value = data.category;
            }
        }
    }

    public class BTCheckValueEditorNode : BTConditionBaseEditorNode
    {
        private BTCheckValueData data = new BTCheckValueData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(BTLocalizationManager.GetString("Check Value"), position);
            
            AddDataInputPort("Value", Port.Capacity.Single);

            var opField = new EnumField(BTLocalizationManager.GetString("Operator"), data.operatorType);
            opField.RegisterValueChangedCallback(evt => { data.operatorType = (BTComparisonOperator)evt.newValue; JsonData = GetJsonData(); });

            var valField = new FloatField(BTLocalizationManager.GetString("Threshold")) { value = data.threshold };
            valField.RegisterValueChangedCallback(evt => { data.threshold = evt.newValue; JsonData = GetJsonData(); });

            extensionContainer.Add(opField);
            extensionContainer.Add(valField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);
        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<BTCheckValueData>(json);
                if (extensionContainer.Q<EnumField>(BTLocalizationManager.GetString("Operator")) is EnumField f1) f1.value = data.operatorType;
                if (extensionContainer.Q<FloatField>(BTLocalizationManager.GetString("Threshold")) is FloatField f2) f2.value = data.threshold;
            }
        }
    }
}
