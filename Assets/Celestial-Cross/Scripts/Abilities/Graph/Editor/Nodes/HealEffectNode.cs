using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;
using Celestial_Cross.Scripts.Abilities.Conditions;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class HealEffectNode : AbilityNode
    {
        private VisualElement scalingsContainer;
        private TextField variableReferenceField;
        private Toggle canCritToggle;
        private Toggle allowOverhealToggle;

        private Celestial_Cross.Scripts.Abilities.Graph.Runtime.HealNodeData nodeData = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.HealNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Heal Effect";
            
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.8f, 0.4f, 0.9f));

            // Portas
            var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            // UI Elements
            variableReferenceField = new TextField("Multiplier Var");
            variableReferenceField.value = nodeData.variableReference;
            variableReferenceField.RegisterValueChangedCallback(evt => nodeData.variableReference = evt.newValue);
            extensionContainer.Add(variableReferenceField);

            var addBtn = new Button(AddScaling) { text = "Add Scaling Stat" };
            extensionContainer.Add(addBtn);

            scalingsContainer = new VisualElement();
            extensionContainer.Add(scalingsContainer);

            canCritToggle = new Toggle("Can Crit Heal");
            canCritToggle.value = nodeData.canCrit;
            canCritToggle.RegisterValueChangedCallback(evt => nodeData.canCrit = evt.newValue);
            extensionContainer.Add(canCritToggle);

            allowOverhealToggle = new Toggle("Allow Overheal");
            allowOverhealToggle.value = nodeData.allowOverheal;
            allowOverhealToggle.RegisterValueChangedCallback(evt => nodeData.allowOverheal = evt.newValue);
            extensionContainer.Add(allowOverhealToggle);

            if (nodeData.scalings.Count == 0) AddScaling();

            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddScaling()
        {
            var entry = new CelestialCross.Combat.StatScalingData
            {
                statType = CelestialCross.Artifacts.StatType.HealthFlat,
                percentage = 100f,
                useTargetStat = true
            };
            nodeData.scalings.Add(entry);
            CreateScalingUI(entry);
        }

        private void CreateScalingUI(CelestialCross.Combat.StatScalingData entry)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Column;
            row.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            row.style.marginBottom = 4;
            row.style.paddingTop = 2;
            row.style.paddingBottom = 2;
            row.style.paddingLeft = 2;
            row.style.paddingRight = 2;
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = Color.gray;

            var topRow = new VisualElement();
            topRow.style.flexDirection = FlexDirection.Row;

            var statEnum = new EnumField(entry.statType);
            statEnum.style.flexGrow = 1;
            statEnum.RegisterValueChangedCallback(evt => {
                entry.statType = (CelestialCross.Artifacts.StatType)evt.newValue;
                UpdateEntryInList(row, entry);
            });
            
            var targetToggle = new Toggle("Use Target");
            targetToggle.value = entry.useTargetStat;
            targetToggle.RegisterValueChangedCallback(evt => {
                entry.useTargetStat = evt.newValue;
                UpdateEntryInList(row, entry);
            });

            var removeBtn = new Button(() => {
                nodeData.scalings.Remove(entry);
                scalingsContainer.Remove(row);
                RefreshExpandedState();
            }) { text = "X" };
            removeBtn.style.color = Color.red;

            topRow.Add(statEnum);
            topRow.Add(targetToggle);
            topRow.Add(removeBtn);
            row.Add(topRow);

            var bottomRow = new VisualElement();
            bottomRow.style.flexDirection = FlexDirection.Row;
            bottomRow.style.marginTop = 2;

            var pctField = new FloatField("Percentage (%)");
            pctField.value = entry.percentage;
            pctField.style.flexGrow = 1;
            pctField.RegisterValueChangedCallback(evt => {
                entry.percentage = evt.newValue;
                UpdateEntryInList(row, entry);
            });

            bottomRow.Add(pctField);
            row.Add(bottomRow);

            row.userData = entry;

            scalingsContainer.Add(row);
            RefreshExpandedState();
        }

        private void UpdateEntryInList(VisualElement row, CelestialCross.Combat.StatScalingData entry)
        {
            int index = scalingsContainer.IndexOf(row);
            if (index >= 0 && index < nodeData.scalings.Count)
            {
                nodeData.scalings[index] = entry;
            }
        }

        public override string GetJsonData()
        {
            return JsonUtility.ToJson(nodeData);
        }

        public override void LoadFromJson(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                nodeData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.HealNodeData>(json);
                variableReferenceField.value = nodeData.variableReference;
                canCritToggle.value = nodeData.canCrit;
                allowOverhealToggle.value = nodeData.allowOverheal;

                scalingsContainer.Clear();
                if (nodeData.scalings != null)
                {
                    foreach (var scaling in nodeData.scalings)
                    {
                        CreateScalingUI(scaling);
                    }
                }
            }
        }

        public override string GetDescription()
        {
            string overhealText = nodeData.allowOverheal ? " (pode sobre-curar)" : "";
            return $"Cura escalada ({nodeData.scalings?.Count ?? 0} stats){overhealText}.";
        }

        public void SetVariableReference(string varName)
        {
            nodeData.variableReference = varName;
            if (variableReferenceField != null) variableReferenceField.value = varName;
        }
    }
}
