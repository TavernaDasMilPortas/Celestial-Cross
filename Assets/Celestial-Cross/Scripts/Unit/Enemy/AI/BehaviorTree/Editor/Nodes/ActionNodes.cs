using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime;
using UnityEditor.UIElements;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor.Nodes
{
    public class BTActionBaseEditorNode : BTEditorNode
    {
        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(nodeName, position);
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.4f, 0.1f, 0.9f));
            AddInputPort("Parent", Port.Capacity.Single);
            RefreshExpandedState();
            RefreshPorts();
        }
    }

    public class BTActionMoveEditorNode : BTActionBaseEditorNode 
    {
        private ActionMoveData data = new ActionMoveData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(BTLocalizationManager.GetString("Action Move"), position);
            AddDataInputPort("Target", Port.Capacity.Single);

            var intentField = new EnumField("Intent", data.intent);
            intentField.RegisterValueChangedCallback(evt => { data.intent = (BTMoveIntent)evt.newValue; JsonData = GetJsonData(); });
            extensionContainer.Add(intentField);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);

        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<ActionMoveData>(json);
                if (extensionContainer.Q<EnumField>("Intent") is EnumField f) f.value = data.intent;
            }
        }
    }
    
    public class BTActionWaitEditorNode : BTActionBaseEditorNode 
    {
        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(BTLocalizationManager.GetString("Action Wait"), position);
        }
    }

    public class BTActionUseAbilityEditorNode : BTActionBaseEditorNode
    {
        private ActionUseAbilityData data = new ActionUseAbilityData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(BTLocalizationManager.GetString("Action Use Ability"), position);
            AddDataInputPort("Target", Port.Capacity.Single);

            var categoryField = new EnumField("Category", data.category);
            categoryField.RegisterValueChangedCallback(evt => { data.category = (AIAbilityHint.AbilityCategory)evt.newValue; JsonData = GetJsonData(); });
            extensionContainer.Add(categoryField);
            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);
        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<ActionUseAbilityData>(json);
                if (extensionContainer.Q<EnumField>("Category") is EnumField f) f.value = data.category;
            }
        }
    }

    public class BTActionUseBestAbilityEditorNode : BTActionBaseEditorNode
    {
        private ActionUseBestAbilityData data = new ActionUseBestAbilityData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(BTLocalizationManager.GetString("Action Use Best Ability"), position);
            AddDataInputPort("Target", Port.Capacity.Single);

            var thresholdField = new FloatField("Min Score");
            thresholdField.value = data.minimumScoreThreshold;
            thresholdField.RegisterValueChangedCallback(evt => { data.minimumScoreThreshold = evt.newValue; JsonData = GetJsonData(); });
            extensionContainer.Add(thresholdField);
            
            RefreshExpandedState();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);
        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<ActionUseBestAbilityData>(json);
                if (extensionContainer.Q<FloatField>("Min Score") is FloatField f) f.value = data.minimumScoreThreshold;
            }
        }
    }
}
