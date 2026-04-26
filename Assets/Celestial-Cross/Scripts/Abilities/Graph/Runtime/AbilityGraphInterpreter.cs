using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Combat.Execution;
using Celestial_Cross.Scripts.Abilities.Conditions;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;

namespace Celestial_Cross.Scripts.Abilities.Graph.Runtime
{
    public class AbilityGraphInterpreter : MonoBehaviour
    {
        public static AbilityGraphInterpreter Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AbilityGraphInterpreter");
                    _instance = go.AddComponent<AbilityGraphInterpreter>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        private static AbilityGraphInterpreter _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public IEnumerator ExecuteGraphCoroutine(Unit caster, AbilityGraphSO graph, CombatHook hook, Action onComplete, int level = 1)
        {
            Debug.Log($"<color=cyan>[Interpreter]</color> Iniciando grafo: {graph.name} com {graph.NodeData.Count} nós.");

            if (graph == null || graph.NodeData.Count == 0)
            {
                Debug.LogWarning("[Interpreter] O grafo está vazio ou nulo!");
                onComplete?.Invoke();
                yield break;
            }

            var context = new CombatContext(caster);
            context.abilityLevel = level;

            // Inicializar Blackboard com valores do SO
            foreach (var variable in graph.Variables)
            {
                context.Variables[variable.name] = variable.initialValue;
            }
            var nodeMap = graph.NodeData.ToDictionary(n => n.Guid);
            var connections = graph.NodeLinks;

            AbilityNodeData currentNodeData = null;

            if (hook == CombatHook.OnManualCast)
            {
                // Para ações manuais, sempre começar pelo StartNode
                currentNodeData = graph.NodeData.FirstOrDefault(n => n.NodeType == "StartNode");
            }
            else
            {
                // Para hooks passivos/condições, procurar um TriggerNode que bata com o hook
                currentNodeData = graph.NodeData.FirstOrDefault(n => {
                    if (n.NodeType != "TriggerNode") return false;
                    if (string.IsNullOrEmpty(n.JsonData)) return false;
                    var triggerData = JsonUtility.FromJson<TriggerNodeData>(n.JsonData);
                    return triggerData.trigger == hook;
                });
            }

            if (currentNodeData == null)
            {
                // Se não encontrou ponto de entrada, silenciosamente sair (não é erro para passivas sem esse hook)
                if (hook != CombatHook.OnManualCast)
                {
                    onComplete?.Invoke();
                    yield break;
                }
                Debug.LogError("[Interpreter] Nó inicial (StartNode) não encontrado no grafo!");
                onComplete?.Invoke();
                yield break;

            }

            while (currentNodeData != null)
            {
                Debug.Log($"[Interpreter] Executando nó: {currentNodeData.NodeType} ({currentNodeData.Guid})");
                
                string nextPort = "Out";
                yield return StartCoroutine(ProcessNode(graph, currentNodeData, context, hook, (port) => nextPort = port));

                Debug.Log($"[Interpreter] Procurando link saindo de {currentNodeData.Guid} pela porta '{nextPort}'");
                var link = connections.FirstOrDefault(l => l.BaseNodeGuid == currentNodeData.Guid && l.PortName == nextPort);
                
                if (link == null)
                {
                    // Debug para ver o que TEM de link
                    var possibleLinks = connections.Where(l => l.BaseNodeGuid == currentNodeData.Guid).ToList();
                    if (possibleLinks.Count > 0)
                    {
                        string linkInfo = string.Join(", ", possibleLinks.Select(l => $"'{l.PortName}' -> {l.TargetNodeGuid}"));
                        Debug.LogWarning($"[Interpreter] Link não encontrado para '{nextPort}'. Links disponíveis deste nó: {linkInfo}");
                    }
                }

                if (link != null)
                {
                    currentNodeData = nodeMap[link.TargetNodeGuid];
                }
                else
                {
                    Debug.Log("[Interpreter] Fim do caminho alcançado.");
                    currentNodeData = null;
                }
            }

            Debug.Log("<color=cyan>[Interpreter]</color> Execução finalizada.");
            onComplete?.Invoke();
        }

        private IEnumerator ProcessNode(AbilityGraphSO graph, AbilityNodeData node, CombatContext context, CombatHook currentHook, Action<string> onResultPort)
        {
            string resultPort = "Out";

            switch (node.NodeType)
            {
                case "TriggerNode":
                    var triggerData = JsonUtility.FromJson<TriggerNodeData>(node.JsonData);
                    // O TriggerNode é passivo no grafo, mas aqui ele valida se o hook bate
                    break;

                case "TargetNode":
                    var targetData = JsonUtility.FromJson<TargetNodeData>(node.JsonData);
                    
                    // Prioridade: Novo sistema de referências por ID. Se não achar, usa o campo legado.
                    AreaPatternData pattern = graph.GetAsset<AreaPatternData>(targetData.patternReferenceId);
                    if (pattern == null) pattern = node.areaPattern;
                    
                    targetData.areaPattern = pattern; 
                    yield return StartCoroutine(HandleTargeting(targetData, context, currentHook));
                    break;

                case "DamageEffectNode":
                    var dmgData = JsonUtility.FromJson<DamageNodeData>(node.JsonData);
                    ExecuteDamage(dmgData, context);
                    break;

                case "HealEffectNode":
                    var healData = JsonUtility.FromJson<HealNodeData>(node.JsonData);
                    ExecuteHeal(healData, context);
                    break;

                case "ConditionalFlowNode":
                    // O Branch node em si não tem dados, ele apenas roteia
                    // A lógica de decisão geralmente vem do nó anterior ou de um input de bool
                    // No nosso sistema simples, o branch segue o resultado de uma condição conectada
                    // MAS, se o nó for um Branch, ele precisa saber qual porta seguir.
                    // Por simplicidade, vamos assumir que o Branch avalia o 'context' atual.
                    break;

                case "AttributeConditionNode":
                    var attrCond = JsonUtility.FromJson<AttributeConditionNodeData>(node.JsonData);
                    bool attrResult = EvaluateAttributeCondition(attrCond, context);
                    resultPort = attrResult ? "True" : "False";
                    break;

                case "DistanceConditionNode":
                    var distCond = JsonUtility.FromJson<DistanceConditionNodeData>(node.JsonData);
                    bool distResult = EvaluateDistanceCondition(distCond, context);
                    resultPort = distResult ? "True" : "False";
                    break;

                case "MoveEffectNode":
                    var moveData = JsonUtility.FromJson<MoveEffectNodeData>(node.JsonData);
                    yield return StartCoroutine(ExecuteMove(moveData, context));
                    break;

                case "VfxNode":
                    var vfxData = JsonUtility.FromJson<VfxNodeData>(node.JsonData);
                    ExecuteVfx(vfxData, context);
                    break;
                
                case "CostNode":
                    var costData = JsonUtility.FromJson<CostNodeData>(node.JsonData);
                    ExecuteCost(costData, context);
                    break;

                case "LoopNode":
                    var loopData = JsonUtility.FromJson<LoopNodeData>(node.JsonData);
                    int maxIterations = loopData.iterations;
                    if (!string.IsNullOrEmpty(loopData.iterationsVariable))
                        maxIterations = (int)GetVariable(context, loopData.iterationsVariable, maxIterations);
                    
                    if (!context.loopCounters.ContainsKey(node.Guid))
                        context.loopCounters[node.Guid] = 0;
                    
                    if (context.loopCounters[node.Guid] < maxIterations)
                    {
                        context.loopCounters[node.Guid]++;
                        resultPort = "Loop";
                    }
                    else
                    {
                        context.loopCounters[node.Guid] = 0; // Reset para possíveis loops aninhados futuros
                        resultPort = "Exit";
                    }
                    break;

                case "VariableModifierNode":
                    var modData = JsonUtility.FromJson<VariableModifierNodeData>(node.JsonData);
                    float modVal = modData.value;
                    if (!string.IsNullOrEmpty(modData.valueVariableReference))
                        modVal = GetVariable(context, modData.valueVariableReference, modVal);
                    
                    ModifyVariable(context, modData.variableName, modData.operation, modVal);
                    break;

                case "LevelBranchNode":
                    resultPort = $"Level {context.abilityLevel}";
                    break;

                case "StatModifierEffectNode":
                    var statModData = JsonUtility.FromJson<StatModifierNodeData>(node.JsonData);
                    ExecuteStatModifier(statModData, context);
                    break;

                case "ApplyModifierNode":
                    var applyModData = JsonUtility.FromJson<ApplyModifierNodeData>(node.JsonData);
                    ExecuteApplyModifier(applyModData, node, graph, context);
                    break;
            }

            onResultPort?.Invoke(resultPort);
            yield return null;
        }

        #region Targeting Logic

        private IEnumerator HandleTargeting(TargetNodeData data, CombatContext context, CombatHook currentHook)
        {
            if (data.reusePrevious && context.targets != null && context.targets.Count > 0) yield break;

            if (data.sourceType == GraphTargetSourceType.Manual && currentHook == CombatHook.OnManualCast)
            {
                Debug.Log("[Interpreter] Iniciando Seleção Manual...");
                
                // Criar regra de targeting a partir dos dados do nó
                TargetingRuleData rule = new TargetingRuleData();
                rule.mode = data.mode == GraphTargetMode.Single ? TargetingMode.Unit : TargetingMode.Area;
                rule.origin = data.origin == GraphTargetOrigin.Unit ? TargetOrigin.Unit : TargetOrigin.Point;
                rule.allowMultiple = data.multipleTargets;
                rule.maxTargets = data.maxTargets;
                rule.targetFaction = (TargetFaction)data.factionType;

                TargetSelector selector = context.source.gameObject.AddComponent<TargetSelector>();
                selector.Begin(context.source, data.range, rule, data.areaPattern, data.preferredDirection, null, data.autoRotate);

                bool selectionConfirmed = false;
                List<Unit> selected = new List<Unit>();

                Action<List<Unit>> onTargets = (targets) => { 
                    selected = targets; 
                    selectionConfirmed = true; 
                };

                selector.OnTargetsConfirmed += onTargets;

                yield return new WaitUntil(() => selectionConfirmed);

                selector.OnTargetsConfirmed -= onTargets;
                context.targets = selected;
                
                UnityEngine.Object.Destroy(selector);
                Debug.Log($"[Interpreter] Seleção manual finalizada: {context.targets.Count} alvos.");
            }
            else
            {
                // Auto Strategy
                context.targets = AutoTargetResolver.Resolve(context.source, data);
                Debug.Log($"[Interpreter] Auto Targeting: {context.targets.Count} alvos encontrados.");
            }
        }

        #endregion

        #region Effect Implementations

        private void ExecuteDamage(DamageNodeData data, CombatContext context)
        {
            foreach (var target in context.targets)
            {
                if (target == null) continue;
                var stepContext = new CombatContext(context.source, target);
                
                float baseVal = 0;
                var attr = (AttributeCondition.AttributeType)data.baseAttribute;
                switch(attr)
                {
                    case AttributeCondition.AttributeType.HP: baseVal = target.Health.CurrentHealth; break;
                    case AttributeCondition.AttributeType.Attack: baseVal = context.source.Stats.attack; break;
                    default: baseVal = context.source.Stats.attack; break;
                }

                float amount = data.amount;
                if (!string.IsNullOrEmpty(data.variableReference))
                    amount = GetVariable(context, data.variableReference, amount);

                int finalAmount = (data.valueType == Celestial_Cross.Scripts.Abilities.ValueType.Flat) ? (int)amount : (int)(baseVal * (amount / 100f));
                stepContext.amount = finalAmount;

                DamageProcessor.ProcessAndApplyDamage(stepContext, true);
            }
        }

        private void ExecuteHeal(HealNodeData data, CombatContext context)
        {
            foreach (var target in context.targets)
            {
                if (target == null) continue;
                
                float amount = data.amount;
                if (!string.IsNullOrEmpty(data.variableReference))
                    amount = GetVariable(context, data.variableReference, amount);

                int finalAmount = (data.valueType == Celestial_Cross.Scripts.Abilities.ValueType.Flat) ? (int)amount : (int)(context.source.Stats.attack * (amount / 100f));
                
                var stepContext = new CombatContext(context.source, target, finalAmount);
                DamageProcessor.ProcessAndApplyHeal(stepContext, data.canCrit);
            }
        }

        private IEnumerator ExecuteMove(MoveEffectNodeData data, CombatContext context)
        {
            foreach (var target in context.targets)
            {
                if (target == null) continue;
                // Implementar lógica de empurrão/puxão/teleporte...
                Debug.Log($"[Interpreter] Movendo {target.name}: {data.moveType}");
            }
            yield return new WaitForSeconds(0.2f);
        }

        private void ExecuteVfx(VfxNodeData data, CombatContext context)
        {
            Debug.Log($"[Interpreter] VFX: {data.vfxId}");
        }

        private void ExecuteCost(CostNodeData data, CombatContext context)
        {
            int mana = data.manaCost;
            if (!string.IsNullOrEmpty(data.manaVariable))
                mana = (int)GetVariable(context, data.manaVariable, mana);

            int stamina = data.staminaCost;
            if (!string.IsNullOrEmpty(data.staminaVariable))
                stamina = (int)GetVariable(context, data.staminaVariable, stamina);

            // TODO: Implementar campos de Mana e Stamina no CombatStats ou Unit de forma definitiva
            // Por enquanto simulamos a dedução se os campos existirem futuramente
            Debug.Log($"[Interpreter] Custo aplicado: {mana} Mana, {stamina} Stamina");
        }

        private void ExecuteStatModifier(StatModifierNodeData data, CombatContext context)
        {
            foreach(var target in context.targets)
            {
                if (target == null) continue;
                var passiveManager = target.GetComponent<PassiveManager>();
                if (passiveManager == null) continue;

                // We dynamically create a wrapper Blueprint to hold graph's modifier logic
                var dynamicBlueprint = ScriptableObject.CreateInstance<AbilityBlueprint>();
                dynamicBlueprint.name = "GraphBuff_" + (string.IsNullOrEmpty(data.variableReference) ? Guid.NewGuid().ToString().Substring(0,4) : data.variableReference);
                dynamicBlueprint.isPersistentCondition = false;
                dynamicBlueprint.durationInTurns = 1; // Default duration, maybe Graph supplies DurationPort later
                dynamicBlueprint.canStack = data.canStack;
                dynamicBlueprint.maxStacks = data.maxStacks;
                
                // Add flat stat modifier to blueprint
                var mod = new Celestial_Cross.Scripts.Abilities.PassiveEffect_ConditionalStatBonus() 
                {
                    triggerHook = data.isBuff ? CombatHook.OnRoundStart : CombatHook.OnTurnStart, // Simplification
                    statBonus = new CombatStats()
                };
                
                float multiplier = 1f;
                if (!string.IsNullOrEmpty(data.variableReference))
                    multiplier = GetVariable(context, data.variableReference, 1f);
                
                // Exemplo simplificado para conversão
                foreach(var stat in data.stats)
                {
                    // Mapeia os índices de StatType para CombatStats
                    if (stat.statIndex == 1) // Supondo AttackFlat
                        mod.statBonus.attack = (int)(stat.value * multiplier);
                    if (stat.statIndex == 3) // Supondo DefenseFlat
                        mod.statBonus.defense = (int)(stat.value * multiplier);
                }

                dynamicBlueprint.modifiers.Add(mod);

                // NOTE: Proper dynamic insertion of stat mods requires a specific Modifier class implementations
                // For now, the user wants the stack fields configured. We use ApplyCondition with stacking logic!
                passiveManager.ApplyCondition(dynamicBlueprint, context.source);
            }
        }

        private void ExecuteApplyModifier(ApplyModifierNodeData data, AbilityNodeData nodeData, AbilityGraphSO graph, CombatContext context)
        {
            // O modifierId referencia um AbilityGraphSO nas dependencies do grafo
            var conditionGraph = graph.GetAsset<AbilityGraphSO>(data.modifierId);
            if (conditionGraph == null)
            {
                Debug.LogWarning($"[Interpreter] ApplyModifierNode: Nenhum grafo encontrado para ID '{data.modifierId}'. Verifique as Dependencies do grafo.");
                return;
            }

            foreach (var target in context.targets)
            {
                if (target == null) continue;
                var passiveManager = target.GetComponent<PassiveManager>();
                if (passiveManager == null) continue;

                for (int i = 0; i < data.stacks; i++)
                {
                    passiveManager.ApplyGraphCondition(conditionGraph, context.source);
                }
                Debug.Log($"[Interpreter] Condição '{conditionGraph.name}' aplicada em {target.name} ({data.stacks} stack(s)).");
            }
        }

        #endregion

        #region Condition Evaluators

        private bool EvaluateAttributeCondition(AttributeConditionNodeData data, CombatContext context)
        {
            Unit unit = data.targetToCheck == AttributeCondition.TargetType.Caster ? context.source : (context.targets.Count > 0 ? context.targets[0] : null);
            if (unit == null) return false;

            float val = 0;
            switch(data.attribute)
            {
                case AttributeCondition.AttributeType.HP: 
                    if (data.mode == AttributeCondition.ValueMode.Percentage)
                        val = (float)unit.Health.CurrentHealth / unit.Health.MaxHealth * 100f;
                    else
                        val = unit.Health.CurrentHealth; 
                    break;
                case AttributeCondition.AttributeType.Attack: val = unit.Stats.attack; break;
                case AttributeCondition.AttributeType.Defense: val = unit.Stats.defense; break;
            }

            return data.comparison switch
            {
                AttributeCondition.Comparison.GreaterThan => val > data.threshold,
                AttributeCondition.Comparison.LessThan => val < data.threshold,
                AttributeCondition.Comparison.Equal => Mathf.Approximately(val, data.threshold),
                AttributeCondition.Comparison.GreaterOrEqual => val >= data.threshold,
                AttributeCondition.Comparison.LessOrEqual => val <= data.threshold,
                _ => false
            };
        }

        private bool EvaluateDistanceCondition(DistanceConditionNodeData data, CombatContext context)
        {
            if (context.targets.Count == 0) return false;
            int dist = Mathf.Max(Mathf.Abs(context.source.GridPosition.x - context.targets[0].GridPosition.x), 
                                Mathf.Abs(context.source.GridPosition.y - context.targets[0].GridPosition.y));

            return data.checkType switch
            {
                DistanceCondition.DistanceType.Min => dist >= data.distance,
                DistanceCondition.DistanceType.Max => dist <= data.distance,
                DistanceCondition.DistanceType.Exact => dist == data.distance,
                _ => false
            };
        }

        #endregion

        #region Blackboard Helpers

        private float GetVariable(CombatContext context, string varName, float defaultValue)
        {
            if (context.Variables.TryGetValue(varName, out float val))
                return val;
            return defaultValue;
        }

        private void ModifyVariable(CombatContext context, string varName, VariableModifierNodeData.Operation op, float value)
        {
            if (!context.Variables.ContainsKey(varName))
                context.Variables[varName] = 0;

            switch (op)
            {
                case VariableModifierNodeData.Operation.Set: context.Variables[varName] = value; break;
                case VariableModifierNodeData.Operation.Add: context.Variables[varName] += value; break;
                case VariableModifierNodeData.Operation.Multiply: context.Variables[varName] *= value; break;
            }
            Debug.Log($"[Interpreter] Variável '{varName}' atualizada para {context.Variables[varName]} (Op: {op})");
        }

        #endregion
    }

    public static class AutoTargetResolver
    {
        public static List<Unit> Resolve(Unit source, TargetNodeData data)
        {
            var allUnits = UnityEngine.Object.FindObjectsByType<Unit>(FindObjectsSortMode.None).ToList();
            
            // Filtro de Facção
            var filteredUnits = allUnits.Where(u => {
                if (data.factionType == GraphFactionType.Any) return true;
                bool isAlly = u.Team == source.Team;
                if (data.factionType == GraphFactionType.Ally) return isAlly;
                if (data.factionType == GraphFactionType.Enemy) return !isAlly;
                return true;
            }).ToList();

            var results = new List<Unit>();

            switch (data.strategy)
            {
                case GraphAutoStrategyType.Self:
                    results.Add(source);
                    break;
                case GraphAutoStrategyType.ClosestUnit:
                    var closest = filteredUnits.Where(u => u != source).OrderBy(u => Vector2Int.Distance(source.GridPosition, u.GridPosition)).FirstOrDefault();
                    if (closest != null) results.Add(closest);
                    break;
                // ... outras estratégias usariam filteredUnits
            }

            return results;
        }
    }
}
