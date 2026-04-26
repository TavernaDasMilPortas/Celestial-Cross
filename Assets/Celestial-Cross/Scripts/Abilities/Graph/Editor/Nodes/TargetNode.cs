using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class TargetNode : AbilityNode
    {
        private Toggle reusePreviousToggle;
        private EnumField sourceDropdown;
        
        // Manual Fields
        private VisualElement manualContainer;
        private EnumField modeDropdown;
        private IntegerField rangeField;
        private Toggle multipleTargetsToggle;
        private IntegerField maxTargetsField;
        private ObjectField areaPatternField;
        private EnumField preferredDirectionDropdown;
        private Toggle autoRotateToggle;
        private EnumField originDropdown;

        // Auto Fields
        private VisualElement autoContainer;
        private EnumField strategyDropdown;
        private EnumField attributeDropdown; // Para lowest/highest
        private EnumField factionDropdown; // Para random
        private IntegerField targetCountField;

        private Celestial_Cross.Scripts.Abilities.Graph.Runtime.TargetNodeData nodeData = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.TargetNodeData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Targeting";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.4f, 0.1f, 0.9f));

            // Portas
            var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Single, typeof(float));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            // Reuse Previous
            reusePreviousToggle = new Toggle("Reuse Previous Targets");
            reusePreviousToggle.RegisterValueChangedCallback(evt => {
                nodeData.reusePrevious = evt.newValue;
                UpdateUI();
            });
            extensionContainer.Add(reusePreviousToggle);

            // Source Type
            sourceDropdown = new EnumField("Source", GraphTargetSourceType.Manual);
            sourceDropdown.RegisterValueChangedCallback(evt => {
                nodeData.sourceType = (GraphTargetSourceType)evt.newValue;
                UpdateUI();
            });
            extensionContainer.Add(sourceDropdown);

            // Manual Container
            manualContainer = new VisualElement();
            modeDropdown = new EnumField("Mode", GraphTargetMode.Single);
            modeDropdown.RegisterValueChangedCallback(evt => { nodeData.mode = (GraphTargetMode)evt.newValue; UpdateUI(); });
            manualContainer.Add(modeDropdown);

            rangeField = new IntegerField("Range");
            rangeField.value = nodeData.range;
            rangeField.RegisterValueChangedCallback(evt => nodeData.range = evt.newValue);
            manualContainer.Add(rangeField);

            multipleTargetsToggle = new Toggle("Multiple Targets");
            multipleTargetsToggle.value = nodeData.multipleTargets;
            multipleTargetsToggle.RegisterValueChangedCallback(evt => {
                nodeData.multipleTargets = evt.newValue;
                UpdateUI();
            });
            manualContainer.Add(multipleTargetsToggle);

            maxTargetsField = new IntegerField("Max Targets");
            maxTargetsField.value = nodeData.maxTargets;
            maxTargetsField.RegisterValueChangedCallback(evt => nodeData.maxTargets = evt.newValue);
            manualContainer.Add(maxTargetsField);

            areaPatternField = new ObjectField("Area Pattern") { objectType = typeof(AreaPatternData) };
            areaPatternField.RegisterValueChangedCallback(evt => nodeData.areaPattern = (AreaPatternData)evt.newValue);

            preferredDirectionDropdown = new EnumField("Preferred Dir", Direction.N); // Using global Direction enum
            // TODO: nodeData.preferredDirection if needed
            
            autoRotateToggle = new Toggle("Auto Rotate");
            autoRotateToggle.RegisterValueChangedCallback(evt => nodeData.autoRotate = evt.newValue);
            
            originDropdown = new EnumField("Origin", GraphTargetOrigin.Unit);
            originDropdown.RegisterValueChangedCallback(evt => nodeData.origin = (GraphTargetOrigin)evt.newValue);
            manualContainer.Add(originDropdown);
            extensionContainer.Add(manualContainer);

            // Auto Container
            autoContainer = new VisualElement();
            strategyDropdown = new EnumField("Strategy", GraphAutoStrategyType.ClosestUnit);
            strategyDropdown.RegisterValueChangedCallback(evt => { nodeData.strategy = (GraphAutoStrategyType)evt.newValue; UpdateUI(); });
            autoContainer.Add(strategyDropdown);

            attributeDropdown = new EnumField("Attribute", Celestial_Cross.Scripts.Abilities.ValueType.Flat); 
            factionDropdown = new EnumField("Faction", GraphFactionType.Any); 
            factionDropdown.RegisterValueChangedCallback(evt => nodeData.factionType = (GraphFactionType)evt.newValue);
            targetCountField = new IntegerField("Target Count");
            targetCountField.RegisterValueChangedCallback(evt => nodeData.targetCount = evt.newValue);
            
            extensionContainer.Add(autoContainer);

            UpdateUI();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void UpdateUI()
        {
            sourceDropdown.style.display = nodeData.reusePrevious ? DisplayStyle.None : DisplayStyle.Flex;
            manualContainer.style.display = (!nodeData.reusePrevious && nodeData.sourceType == GraphTargetSourceType.Manual) ? DisplayStyle.Flex : DisplayStyle.None;
            autoContainer.style.display = (!nodeData.reusePrevious && nodeData.sourceType == GraphTargetSourceType.AutoStrategy) ? DisplayStyle.Flex : DisplayStyle.None;

            // Dinâmica interna do Manual
            if (nodeData.mode == GraphTargetMode.Area)
            {
                if (!manualContainer.Contains(areaPatternField))
                {
                    manualContainer.Insert(1, areaPatternField);
                    manualContainer.Insert(2, preferredDirectionDropdown);
                    manualContainer.Insert(3, autoRotateToggle);
                }
            }
            else
            {
                if (manualContainer.Contains(areaPatternField))
                {
                    manualContainer.Remove(areaPatternField);
                    manualContainer.Remove(preferredDirectionDropdown);
                    manualContainer.Remove(autoRotateToggle);
                }
            }

            // Múltiplos alvos
            maxTargetsField.style.display = nodeData.multipleTargets ? DisplayStyle.Flex : DisplayStyle.None;

            // Dinâmica interna do Auto
            if (nodeData.strategy == GraphAutoStrategyType.LowestAttribute || nodeData.strategy == GraphAutoStrategyType.HighestAttribute)
            {
                if (!autoContainer.Contains(attributeDropdown)) autoContainer.Add(attributeDropdown);
            }
            else
            {
                if (autoContainer.Contains(attributeDropdown)) autoContainer.Remove(attributeDropdown);
            }

            // Faction Filter - Sempre visível no Auto (exceto Self)
            if (nodeData.strategy != GraphAutoStrategyType.Self)
            {
                if (!autoContainer.Contains(factionDropdown)) autoContainer.Add(factionDropdown);
            }
            else
            {
                if (autoContainer.Contains(factionDropdown)) autoContainer.Remove(factionDropdown);
            }

            if (nodeData.strategy == GraphAutoStrategyType.RandomTarget)
            {
                if (!autoContainer.Contains(targetCountField)) autoContainer.Add(targetCountField);
            }
            else
            {
                if (autoContainer.Contains(targetCountField)) autoContainer.Remove(targetCountField);
            }

            RefreshExpandedState();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.TargetNodeData>(json);
            
            reusePreviousToggle.value = nodeData.reusePrevious;
            sourceDropdown.value = nodeData.sourceType;
            
            // Manual
            modeDropdown.value = nodeData.mode;
            rangeField.value = nodeData.range;
            multipleTargetsToggle.value = nodeData.multipleTargets;
            maxTargetsField.value = nodeData.maxTargets;
            areaPatternField.value = nodeData.areaPattern;
            autoRotateToggle.value = nodeData.autoRotate;
            originDropdown.value = nodeData.origin;

            // Auto
            strategyDropdown.value = nodeData.strategy;
            factionDropdown.value = nodeData.factionType;
            targetCountField.value = nodeData.targetCount;

            UpdateUI();
        }

        public override string GetDescription()
        {
            if (nodeData.reusePrevious) return "Usa os mesmos alvos da etapa anterior.";

            if (nodeData.sourceType == GraphTargetSourceType.Manual)
            {
                string multiText = nodeData.multipleTargets ? $"até {nodeData.maxTargets} " : "";
                string modeText = nodeData.mode == GraphTargetMode.Single ? $"{multiText}unidade(s)" : "múltiplos tiles em área";
                return $"Escolhe manualmente {modeText} a um range de {nodeData.range} a partir de {nodeData.origin}.";
            }
            else
            {
                string stratText = nodeData.strategy switch
                {
                    GraphAutoStrategyType.ClosestUnit => "a unidade mais próxima",
                    GraphAutoStrategyType.FarthestUnit => "a unidade mais distante",
                    GraphAutoStrategyType.LowestAttribute => "a unidade com menor atributo",
                    GraphAutoStrategyType.HighestAttribute => "a unidade com maior atributo",
                    GraphAutoStrategyType.Self => "a si mesmo",
                    GraphAutoStrategyType.MainTarget => "o alvo principal",
                    GraphAutoStrategyType.RandomTarget => $"até {nodeData.targetCount} alvos aleatórios",
                    _ => "um alvo automático"
                };
                return $"Seleciona automaticamente {stratText}.";
            }
        }
        public override void OnSave(AbilityNodeData data)
        {
            data.areaPattern = nodeData.areaPattern;
        }

        public override void OnLoad(AbilityNodeData data)
        {
            nodeData.areaPattern = data.areaPattern;
            areaPatternField.value = nodeData.areaPattern;
        }
    }
}
