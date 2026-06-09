using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Conditions;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class DamageEffectNode : AbilityNode
    {
        private VisualElement scalingsContainer;
        private TextField variableReferenceField;
        private Toggle scaleWithDistanceToggle;
        private FloatField distanceScaleFactorField;

        private Celestial_Cross.Scripts.Abilities.Graph.Runtime.DamageNodeData nodeData = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.DamageNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Damage Effect";
            
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.8f, 0.2f, 0.2f, 0.9f));

            // Porta de Entrada
            var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            // Porta de Saída
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

            scaleWithDistanceToggle = new Toggle("Scale With Distance");
            scaleWithDistanceToggle.value = nodeData.scaleWithDistance;
            scaleWithDistanceToggle.RegisterValueChangedCallback(evt => {
                nodeData.scaleWithDistance = evt.newValue;
                UpdateDynamicFields();
            });
            extensionContainer.Add(scaleWithDistanceToggle);

            distanceScaleFactorField = new FloatField("Distance Factor");
            distanceScaleFactorField.value = nodeData.distanceScaleFactor;
            distanceScaleFactorField.RegisterValueChangedCallback(evt => nodeData.distanceScaleFactor = evt.newValue);

            UpdateDynamicFields();

            if (nodeData.scalings.Count == 0) AddScaling();

            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddScaling()
        {
            var entry = new CelestialCross.Combat.StatScalingData
            {
                statType = CelestialCross.Artifacts.StatType.AttackFlat,
                percentage = 100f,
                useTargetStat = false
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

            // Armazenamos o objeto da struct na tag da UI para atualizações se necessário
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

        private void UpdateDynamicFields()
        {
            if (nodeData.scaleWithDistance)
            {
                if (!extensionContainer.Contains(distanceScaleFactorField))
                    extensionContainer.Add(distanceScaleFactorField);
            }
            else
            {
                if (extensionContainer.Contains(distanceScaleFactorField))
                    extensionContainer.Remove(distanceScaleFactorField);
            }

            RefreshExpandedState();
        }

        public override string GetJsonData()
        {
            return JsonUtility.ToJson(nodeData);
        }

        public override void LoadFromJson(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                nodeData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.DamageNodeData>(json);
                variableReferenceField.value = nodeData.variableReference;
                scaleWithDistanceToggle.value = nodeData.scaleWithDistance;
                distanceScaleFactorField.value = nodeData.distanceScaleFactor;
                
                scalingsContainer.Clear();
                if (nodeData.scalings != null)
                {
                    foreach (var scaling in nodeData.scalings)
                    {
                        CreateScalingUI(scaling);
                    }
                }
                
                UpdateDynamicFields();
            }
        }

        public override string GetDescription()
        {
            return $"Dano escalado ({nodeData.scalings?.Count ?? 0} stats).";
        }

        public void SetVariableReference(string varName)
        {
            nodeData.variableReference = varName;
            if (variableReferenceField != null) variableReferenceField.value = varName;
        }
    }
}
