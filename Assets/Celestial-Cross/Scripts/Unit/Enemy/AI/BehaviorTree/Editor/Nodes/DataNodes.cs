using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime;
using UnityEditor.UIElements;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor.Nodes
{
    public class BTDataEditorNode : BTEditorNode
    {
        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(nodeName, position);
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.3f, 0.4f, 0.9f));
            RefreshExpandedState();
            RefreshPorts();
        }
    }

    public class BTGetTargetEditorNode : BTDataEditorNode
    {
        private BTGetTargetData data = new BTGetTargetData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(BTLocalizationManager.GetString("Get Target"), position);
            
            AddDataOutputPort("Target", Port.Capacity.Multi);

            var factionField = new EnumField("Faction", data.faction);
            factionField.RegisterValueChangedCallback(evt => { data.faction = (BTTargetFaction)evt.newValue; JsonData = GetJsonData(); });
            
            var strategyField = new EnumField("Strategy", data.strategy);
            strategyField.RegisterValueChangedCallback(evt => { data.strategy = (BTTargetStrategy)evt.newValue; JsonData = GetJsonData(); });

            var tagField = new TextField("Required Tag") { value = data.requiredTag };
            tagField.RegisterValueChangedCallback(evt => { data.requiredTag = evt.newValue; JsonData = GetJsonData(); });

            extensionContainer.Add(factionField);
            extensionContainer.Add(strategyField);
            extensionContainer.Add(tagField);
            RefreshExpandedState();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);
        
        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<BTGetTargetData>(json);
                if (extensionContainer.Q<EnumField>("Faction") is EnumField f) f.value = data.faction;
                if (extensionContainer.Q<EnumField>("Strategy") is EnumField s) s.value = data.strategy;
                if (extensionContainer.Q<TextField>("Required Tag") is TextField t) t.value = data.requiredTag;
            }
        }
    }

    public class BTGetNumericDataEditorNode : BTDataEditorNode
    {
        private BTGetNumericData data = new BTGetNumericData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(BTLocalizationManager.GetString("Get Numeric Data"), position);
            
            AddDataOutputPort("Value", Port.Capacity.Multi);

            var typeField = new EnumField(BTLocalizationManager.GetString("Data Type"), data.dataType);
            typeField.RegisterValueChangedCallback(evt => { data.dataType = (BTNumericDataType)evt.newValue; JsonData = GetJsonData(); });

            extensionContainer.Add(typeField);
            RefreshExpandedState();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);
        
        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<BTGetNumericData>(json);
                if (extensionContainer.Q<EnumField>(BTLocalizationManager.GetString("Data Type")) is EnumField f) f.value = data.dataType;
            }
        }
    }
}
