using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using CelestialCross.Artifacts; // Onde o StatType está definido
using Celestial_Cross.Scripts.Abilities.Modifiers;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class StatModifierEffectNode : AbilityNode
    {
        private VisualElement modifierListContainer;
        private Toggle scaleWithDistanceToggle;
        private FloatField distanceFactorField;
        private TextField variableReferenceField;
        
        // Stack Fields
        private Toggle canStackToggle;
        private IntegerField maxStacksField;

        [System.Serializable]
        public class StatEntry
        {
            public StatType stat;
            public float value;
        }

        [System.Serializable]
        public class StatModData
        {
            public List<StatEntry> modifiers = new List<StatEntry>();
            public bool scaleWithDistance;
            public float distanceScaleFactor = 0.1f;
            public string variableReference;
            public bool canStack = false;
            public int maxStacks = 1;
        }

        private StatModData nodeData = new StatModData();

        public override void Initialize(string nodeGuid, Vector2 position)
        {
            base.Initialize(nodeGuid, position);
            title = "Stat Modifier";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.8f, 0.6f, 0.2f, 0.9f));

            // Portas de Fluxo
            var inputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            var outputPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            // Porta de Dados para Duração
            var durationPort = InstantiatePort(Orientation.Horizontal, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Single, typeof(ModifierDurationSettings));
            durationPort.portName = "Duration";
            inputContainer.Add(durationPort);

            // UI
            modifierListContainer = new VisualElement();
            extensionContainer.Add(modifierListContainer);

            var addBtn = new Button(AddModifier) { text = "Add Stat Modifier" };
            extensionContainer.Add(addBtn);

            variableReferenceField = new TextField("Buff Base Var");
            variableReferenceField.value = nodeData.variableReference;
            variableReferenceField.RegisterValueChangedCallback(evt => nodeData.variableReference = evt.newValue);
            extensionContainer.Add(variableReferenceField);

            canStackToggle = new Toggle("Can Stack");
            canStackToggle.value = nodeData.canStack;
            canStackToggle.RegisterValueChangedCallback(evt => {
                nodeData.canStack = evt.newValue;
                UpdateUI();
            });
            extensionContainer.Add(canStackToggle);

            maxStacksField = new IntegerField("Max Stacks (0=Inf)");
            maxStacksField.value = nodeData.maxStacks;
            maxStacksField.RegisterValueChangedCallback(evt => nodeData.maxStacks = evt.newValue);

            scaleWithDistanceToggle = new Toggle("Scale With Distance");
            scaleWithDistanceToggle.RegisterValueChangedCallback(evt => {
                nodeData.scaleWithDistance = evt.newValue;
                UpdateUI();
            });
            extensionContainer.Add(scaleWithDistanceToggle);

            distanceFactorField = new FloatField("Distance Factor");
            distanceFactorField.RegisterValueChangedCallback(evt => nodeData.distanceScaleFactor = evt.newValue);
            
            if (nodeData.modifiers.Count == 0) AddModifier(); // Inicia com um vazio

            UpdateUI();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddModifier()
        {
            var entry = new StatEntry { stat = StatType.AttackFlat, value = 5 };
            nodeData.modifiers.Add(entry);
            CreateModifierUI(entry);
        }

        private void CreateModifierUI(StatEntry entry)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            var statEnum = new EnumField(entry.stat);
            statEnum.style.flexGrow = 1;
            statEnum.RegisterValueChangedCallback(evt => entry.stat = (StatType)evt.newValue);
            
            var valField = new FloatField();
            valField.value = entry.value;
            valField.style.width = 50;
            valField.RegisterValueChangedCallback(evt => entry.value = evt.newValue);

            var removeBtn = new Button(() => {
                nodeData.modifiers.Remove(entry);
                modifierListContainer.Remove(row);
                RefreshExpandedState();
            }) { text = "X" };

            row.Add(statEnum);
            row.Add(valField);
            row.Add(removeBtn);
            modifierListContainer.Add(row);
            RefreshExpandedState();
        }

        private void UpdateUI()
        {
            if (nodeData.scaleWithDistance)
            {
                if (!extensionContainer.Contains(distanceFactorField)) extensionContainer.Add(distanceFactorField);
            }
            else
            {
                if (extensionContainer.Contains(distanceFactorField)) extensionContainer.Remove(distanceFactorField);
            }

            if (nodeData.canStack)
            {
                if (!extensionContainer.Contains(maxStacksField)) extensionContainer.Add(maxStacksField);
            }
            else
            {
                if (extensionContainer.Contains(maxStacksField)) extensionContainer.Remove(maxStacksField);
            }

            RefreshExpandedState();
        }

        public override string GetJsonData() => JsonUtility.ToJson(nodeData);

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            nodeData = JsonUtility.FromJson<StatModData>(json);
            
            modifierListContainer.Clear();
            foreach(var mod in nodeData.modifiers) CreateModifierUI(mod);
            
            scaleWithDistanceToggle.value = nodeData.scaleWithDistance;
            distanceFactorField.value = nodeData.distanceScaleFactor;
            variableReferenceField.value = nodeData.variableReference;
            canStackToggle.value = nodeData.canStack;
            maxStacksField.value = nodeData.maxStacks;
            UpdateUI();
        }

        public void SetVariableReference(string varName)
        {
            nodeData.variableReference = varName;
            if (variableReferenceField != null) variableReferenceField.value = varName;
        }
    }
}
