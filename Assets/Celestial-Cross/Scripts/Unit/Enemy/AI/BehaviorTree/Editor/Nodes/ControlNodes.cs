using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor.Nodes
{
    public class BTRootEditorNode : BTEditorNode
    {
        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(nodeName, position);
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.35f, 0.1f, 0.1f, 0.9f));
            AddOutputPort("Child", Port.Capacity.Single);
            RefreshExpandedState();
            RefreshPorts();
        }
    }

    public abstract class BTCompositeEditorNode : BTEditorNode
    {
        protected Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.BTCompositeData data = new Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.BTCompositeData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(nodeName, position);
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.2f, 0.45f, 0.9f));
            AddInputPort("Parent", Port.Capacity.Single);

            var addBtn = new Button(() => { AddStepPort(); }) { text = BTLocalizationManager.GetString("Add Step") };
            extensionContainer.Add(addBtn);

            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddStepPort()
        {
            string portName = "Passo_" + data.ports.Count;
            data.ports.Add(portName);
            AddOutputPort(portName, Port.Capacity.Single);
            JsonData = GetJsonData();
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);

        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.BTCompositeData>(json);
                foreach (var port in data.ports)
                {
                    AddOutputPort(port, Port.Capacity.Single);
                }
            }
        }
    }

    public class BTSelectorEditorNode : BTCompositeEditorNode
    {
        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(BTLocalizationManager.GetString("Selector"), position);
        }
    }

    public class BTSequenceEditorNode : BTCompositeEditorNode
    {
        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(BTLocalizationManager.GetString("Sequence"), position);
        }
    }

    public class BTValueSwitchEditorNode : BTEditorNode
    {
        private Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.BTValueSwitchData data = new Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.BTValueSwitchData();

        public override void Initialize(string nodeName, Vector2 position)
        {
            base.Initialize(BTLocalizationManager.GetString("Condition Switch"), position);
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.4f, 0.2f, 0.4f, 0.9f));
            
            AddInputPort("Parent", Port.Capacity.Single);
            AddDataInputPort("Value", Port.Capacity.Single);

            var addBtn = new Button(() => { AddCasePort(); }) { text = BTLocalizationManager.GetString("Add Case") };
            extensionContainer.Add(addBtn);

            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddCasePort()
        {
            var caseData = new Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.BTValueSwitchCaseData
            {
                portName = "Case_" + data.cases.Count,
                operatorType = Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.BTComparisonOperator.Equal,
                threshold = 0f
            };
            data.cases.Add(caseData);
            RenderCase(caseData);
            JsonData = GetJsonData();
        }

        private void RenderCase(Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.BTValueSwitchCaseData caseData)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;

            var opField = new UnityEngine.UIElements.EnumField(caseData.operatorType);
            opField.RegisterValueChangedCallback(evt => {
                caseData.operatorType = (Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.BTComparisonOperator)evt.newValue;
                JsonData = GetJsonData();
            });
            opField.style.width = 60;

            var valField = new UnityEngine.UIElements.FloatField { value = caseData.threshold };
            valField.RegisterValueChangedCallback(evt => {
                caseData.threshold = evt.newValue;
                JsonData = GetJsonData();
            });
            valField.style.width = 40;

            container.Add(opField);
            container.Add(valField);

            extensionContainer.Add(container);
            AddOutputPort(caseData.portName, Port.Capacity.Single);
            RefreshPorts();
        }

        public override string GetJsonData() => JsonUtility.ToJson(data);

        public override void LoadFromJson(string json)
        {
            base.LoadFromJson(json);
            if (!string.IsNullOrEmpty(json))
            {
                data = JsonUtility.FromJson<Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.BTValueSwitchData>(json);
                foreach (var c in data.cases)
                {
                    RenderCase(c);
                }
            }
        }
    }
}
