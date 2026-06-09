using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using CelestialCross.Artifacts; // Onde o StatType está definido
using Celestial_Cross.Scripts.Abilities.Modifiers;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor.Nodes
{
    public class StatModifierEffectNode : AbilityNode
    {
        private VisualElement modifierListContainer;
        private Toggle scaleWithDistanceToggle;
        private FloatField distanceFactorField;
        
        // Stack Fields
        private Toggle canStackToggle;
        private IntegerField maxStacksField;

        public enum CoreStat
        {
            Health, Attack, Defense, Speed, CriticalRate, CriticalDamage, EffectResistance, EffectHitRate
        }

        [System.Serializable]
        public class StatEntry
        {
            public CoreStat coreStat = CoreStat.Attack;
            public ModifierBonusType bonusType = ModifierBonusType.Flat;
            public ModifierValueMode valueMode = ModifierValueMode.Value;
            public float value;
            public string valueVariable;
            
            // Mantido apenas para compatibilidade de mapeamento interno se necessário
            public StatType stat => MapToStatType(coreStat, bonusType);

            private static StatType MapToStatType(CoreStat core, ModifierBonusType bonus)
            {
                switch (core)
                {
                    case CoreStat.Health: return bonus == ModifierBonusType.Flat ? StatType.HealthFlat : StatType.HealthPercent;
                    case CoreStat.Attack: return bonus == ModifierBonusType.Flat ? StatType.AttackFlat : StatType.AttackPercent;
                    case CoreStat.Defense: return bonus == ModifierBonusType.Flat ? StatType.DefenseFlat : StatType.DefensePercent;
                    case CoreStat.Speed: return StatType.Speed;
                    case CoreStat.CriticalRate: return StatType.CriticalRate;
                    case CoreStat.CriticalDamage: return StatType.CriticalDamage;
                    case CoreStat.EffectResistance: return StatType.EffectResistance;
                    case CoreStat.EffectHitRate: return StatType.EffectHitRate;
                    default: return StatType.AttackFlat;
                }
            }
        }

        [System.Serializable]
        public class StatModData
        {
            public List<StatEntry> modifiers = new List<StatEntry>();
            public bool scaleWithDistance;
            public float distanceScaleFactor = 0.1f;
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
            var entry = new StatEntry { coreStat = CoreStat.Attack, bonusType = ModifierBonusType.Flat, value = 5 };
            nodeData.modifiers.Add(entry);
            CreateModifierUI(entry);
        }

        private void CreateModifierUI(StatEntry entry)
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

            // 1. Status Dropdown
            var statEnum = new EnumField(entry.coreStat);
            statEnum.style.flexGrow = 1;
            statEnum.RegisterValueChangedCallback(evt => entry.coreStat = (CoreStat)evt.newValue);
            
            // 2. Bonus Type Dropdown
            var bonusEnum = new EnumField(entry.bonusType);
            bonusEnum.style.width = 70;
            bonusEnum.RegisterValueChangedCallback(evt => entry.bonusType = (ModifierBonusType)evt.newValue);

            var removeBtn = new Button(() => {
                nodeData.modifiers.Remove(entry);
                modifierListContainer.Remove(row);
                RefreshExpandedState();
            }) { text = "X" };
            removeBtn.style.color = Color.red;

            topRow.Add(statEnum);
            topRow.Add(bonusEnum);
            topRow.Add(removeBtn);
            row.Add(topRow);

            var bottomRow = new VisualElement();
            bottomRow.style.flexDirection = FlexDirection.Row;
            bottomRow.style.marginTop = 2;

            // 3. Value Mode Dropdown
            var modeEnum = new EnumField(entry.valueMode);
            modeEnum.style.width = 80;
            
            // Input Fields
            var valField = new FloatField();
            valField.value = entry.value;
            valField.style.flexGrow = 1;
            valField.RegisterValueChangedCallback(evt => entry.value = evt.newValue);

            var varField = new TextField();
            varField.value = entry.valueVariable;
            varField.style.flexGrow = 1;
            varField.RegisterValueChangedCallback(evt => entry.valueVariable = evt.newValue);

            void UpdateInputVisibility(ModifierValueMode mode)
            {
                if (mode == ModifierValueMode.Value)
                {
                    if (bottomRow.Contains(varField)) bottomRow.Remove(varField);
                    if (!bottomRow.Contains(valField)) bottomRow.Add(valField);
                }
                else
                {
                    if (bottomRow.Contains(valField)) bottomRow.Remove(valField);
                    if (!bottomRow.Contains(varField)) bottomRow.Add(varField);
                }
            }

            modeEnum.RegisterValueChangedCallback(evt => {
                entry.valueMode = (ModifierValueMode)evt.newValue;
                UpdateInputVisibility(entry.valueMode);
            });

            bottomRow.Add(modeEnum);
            UpdateInputVisibility(entry.valueMode);
            row.Add(bottomRow);

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

        public override string GetJsonData()
        {
            var runtimeData = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.StatModifierNodeData();
            runtimeData.isBuff = true;
            // runtimeData.variableReference = nodeData.variableReference; // Removido
            runtimeData.canStack = nodeData.canStack;
            runtimeData.maxStacks = nodeData.maxStacks;
            
            foreach(var mod in nodeData.modifiers)
            {
                var entry = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.StatModifierNodeData.StatEntry
                {
                    statIndex = (int)mod.stat,
                    value = mod.value,
                    statTypeName = mod.stat.ToString(),
                    bonusType = mod.bonusType,
                    valueMode = mod.valueMode,
                    valueVariable = mod.valueVariable
                };
                runtimeData.stats.Add(entry);
            }
            
            return JsonUtility.ToJson(runtimeData);
        }

        public override void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            
            // Tentar carregar como StatModifierNodeData (novo formato)
            var runtimeData = JsonUtility.FromJson<Celestial_Cross.Scripts.Abilities.Graph.Runtime.StatModifierNodeData>(json);
            
            // nodeData.variableReference = runtimeData.variableReference; // Removido
            nodeData.canStack = runtimeData.canStack;
            nodeData.maxStacks = runtimeData.maxStacks;
            nodeData.modifiers.Clear();
            
            foreach(var entry in runtimeData.stats)
            {
                StatType statType = StatType.AttackFlat;
                if (!string.IsNullOrEmpty(entry.statTypeName))
                {
                    System.Enum.TryParse<StatType>(entry.statTypeName, out statType);
                }
                
                // Converter StatType de volta para CoreStat e BonusType
                CoreStat core = CoreStat.Attack;
                ModifierBonusType bonus = entry.bonusType;

                string typeName = statType.ToString();
                if (typeName.Contains("Health")) core = CoreStat.Health;
                else if (typeName.Contains("Attack")) core = CoreStat.Attack;
                else if (typeName.Contains("Defense")) core = CoreStat.Defense;
                else if (typeName.Contains("Speed")) core = CoreStat.Speed;
                else if (typeName.Contains("CriticalRate")) core = CoreStat.CriticalRate;
                else if (typeName.Contains("CriticalDamage")) core = CoreStat.CriticalDamage;
                else if (typeName.Contains("EffectResistance")) core = CoreStat.EffectResistance;
                else if (typeName.Contains("EffectHitRate")) core = CoreStat.EffectHitRate;

                // Tentar detectar bônus pelo nome se não vier serializado (legado)
                if (typeName.Contains("Percent")) bonus = ModifierBonusType.Percent;
                else if (typeName.Contains("Flat")) bonus = ModifierBonusType.Flat;

                nodeData.modifiers.Add(new StatEntry 
                { 
                    coreStat = core, 
                    bonusType = bonus,
                    valueMode = entry.valueMode,
                    value = entry.value,
                    valueVariable = entry.valueVariable
                });
            }
            
            modifierListContainer.Clear();
            foreach(var mod in nodeData.modifiers) CreateModifierUI(mod);
            
            scaleWithDistanceToggle.value = nodeData.scaleWithDistance;
            distanceFactorField.value = nodeData.distanceScaleFactor;
            canStackToggle.value = nodeData.canStack;
            maxStacksField.value = nodeData.maxStacks;
            UpdateUI();
        }
    }
}
