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
        public IEnumerator ExecuteGraphCoroutine(Unit caster, AbilityGraphSO graph, CombatHook hook, Action onComplete, int level = 1, string slotId = "", Vector2Int? presetTargetPos = null, List<Vector2Int> presetTargetPositions = null)
        {
            if (graph == null || graph.NodeData.Count == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            string hookInfo = (hook == CombatHook.OnManualCast) ? "Ativa" : $"Gatilho: {hook}";
            CombatLogger.Log($"<color=#a29bfe>[Graph]</color> Iniciando <b>{graph.name}</b> ({hookInfo}) para {caster?.DisplayName}", LogCategory.Graph);

            var context = new CombatContext(caster);
            context.abilityLevel = level;
            context.slotId = slotId;

            if (presetTargetPositions != null && presetTargetPositions.Count > 0)
            {
                context.targetPos = presetTargetPositions[0];
                context.targets.Clear();
                foreach (var pos in presetTargetPositions)
                {
                    var unit = GridMap.Instance?.GetTile(pos)?.OccupyingUnit;
                    if (unit != null) context.targets.Add(unit);
                }
                if (context.targets.Count > 0) context.target = context.targets[0];
            }
            else if (presetTargetPos.HasValue)
            {
                context.targetPos = presetTargetPos.Value;
                var presetUnit = GridMap.Instance?.GetTile(presetTargetPos.Value)?.OccupyingUnit;
                if (presetUnit != null)
                {
                    context.target = presetUnit;
                    context.targets.Add(presetUnit);
                }
            }

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

                if (nextPort == "Scheduled") 
                {
                    Debug.Log("[Interpreter] Fluxo suspenso por ScheduleExecution.");
                    break;
                }

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

        public IEnumerator ExecuteFromNode(Unit caster, AbilityGraphSO graph, AbilityNodeData startNode, CombatContext context, Action onComplete = null)
        {
            if (graph == null || startNode == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            CombatLogger.Log($"<color=#a29bfe>[Graph]</color> Retomando <b>{graph.name}</b> a partir do nó {startNode.NodeType}", LogCategory.Graph);

            var nodeMap = graph.NodeData.ToDictionary(n => n.Guid);
            var connections = graph.NodeLinks;
            AbilityNodeData currentNodeData = startNode;

            while (currentNodeData != null)
            {
                Debug.Log($"[Interpreter] Executando nó: {currentNodeData.NodeType} ({currentNodeData.Guid})");
                
                string nextPort = "Out";
                yield return StartCoroutine(ProcessNode(graph, currentNodeData, context, CombatHook.OnManualCast, (port) => nextPort = port));

                if (nextPort == "Scheduled") 
                {
                    Debug.Log("[Interpreter] Fluxo suspenso por ScheduleExecution.");
                    break;
                }

                var link = connections.FirstOrDefault(l => l.BaseNodeGuid == currentNodeData.Guid && l.PortName == nextPort);
                if (link != null)
                {
                    currentNodeData = nodeMap[link.TargetNodeGuid];
                }
                else
                {
                    currentNodeData = null;
                }
            }

            Debug.Log("<color=cyan>[Interpreter]</color> Execução retomada finalizada.");
            onComplete?.Invoke();
        }

        public void ExecuteGraphSync(Unit caster, AbilityGraphSO graph, CombatHook hook, int level = 1, string slotId = "")
        {
            if (graph == null || graph.NodeData.Count == 0) return;

            string hookInfo = (hook == CombatHook.OnManualCast) ? "Ativa" : $"Gatilho: {hook}";
            CombatLogger.Log($"<color=#a29bfe>[Graph - Sync]</color> Iniciando <b>{graph.name}</b> ({hookInfo}) para {caster?.DisplayName}", LogCategory.Graph);

            var context = new CombatContext(caster);
            context.abilityLevel = level;
            context.slotId = slotId;

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
                currentNodeData = graph.NodeData.FirstOrDefault(n => n.NodeType == "StartNode");
            }
            else
            {
                currentNodeData = graph.NodeData.FirstOrDefault(n => {
                    if (n.NodeType != "TriggerNode") return false;
                    if (string.IsNullOrEmpty(n.JsonData)) return false;
                    var triggerData = JsonUtility.FromJson<TriggerNodeData>(n.JsonData);
                    return triggerData.trigger == hook;
                });
            }

            if (currentNodeData == null) return;

            int safetyCounter = 0;
            while (currentNodeData != null && safetyCounter < 1000)
            {
                safetyCounter++;
                Debug.Log($"[Interpreter - Sync] Executando nó: {currentNodeData.NodeType} ({currentNodeData.Guid})");

                string nextPort = ProcessNodeSync(graph, currentNodeData, context, hook);

                var link = connections.FirstOrDefault(l => l.BaseNodeGuid == currentNodeData.Guid && l.PortName == nextPort);
                if (link != null)
                {
                    currentNodeData = nodeMap[link.TargetNodeGuid];
                }
                else
                {
                    currentNodeData = null;
                }
            }

            Debug.Log("<color=cyan>[Interpreter - Sync]</color> Execução finalizada.");
        }

        private string ProcessNodeSync(AbilityGraphSO graph, AbilityNodeData node, CombatContext context, CombatHook currentHook)
        {
            string resultPort = "Out";

            CheckAndTriggerPetAnimation(graph, node, context);

            switch (node.NodeType)
            {
                case "TriggerNode":
                    break;

                case "TargetNode":
                    var targetData = JsonUtility.FromJson<TargetNodeData>(node.JsonData);
                    
                    AreaPatternData pattern = graph.GetAsset<AreaPatternData>(targetData.patternReferenceId);
                    if (pattern == null) pattern = node.areaPattern;
                    targetData.areaPattern = pattern; 

                    int resolvedRange = targetData.range;
                    if (!string.IsNullOrEmpty(targetData.rangeVariable))
                        resolvedRange = (int)GetVariable(context, targetData.rangeVariable, resolvedRange);

                    if (targetData.useExtraRangeVariable && context.source != null && context.source.VariableStore != null)
                    {
                        float extraRange = string.IsNullOrEmpty(context.slotId)
                            ? context.source.VariableStore.GetGlobalVar("ExtraRange")
                            : context.source.VariableStore.GetSlotVar(context.slotId, "ExtraRange");
                        resolvedRange += Mathf.RoundToInt(extraRange);
                    }

                    // No modo síncrono para passivas, resolvemos o auto targeting imediatamente
                    var dataCopy = targetData; 
                    dataCopy.range = resolvedRange;
                    context.targets = AutoTargetResolver.Resolve(context.source, dataCopy);
                    break;

                case "DamageEffectNode":
                    var dmgData = JsonUtility.FromJson<DamageNodeData>(node.JsonData);
                    ExecuteDamage(dmgData, context);
                    break;

                case "HealEffectNode":
                    var healData = JsonUtility.FromJson<HealNodeData>(node.JsonData);
                    ExecuteHeal(healData, context);
                    break;

                case "ModifyAPNode":
                    var modifyAPData = JsonUtility.FromJson<ModifyAPNodeData>(node.JsonData);
                    ExecuteModifyAP(modifyAPData, context);
                    break;

                case "ConditionalFlowNode":
                    bool allTrue = true;
                    var condLinks = graph.NodeLinks.Where(l => l.TargetNodeGuid == node.Guid && l.TargetPortName.StartsWith("Cond")).ToList();
                    
                    if (condLinks.Count > 0)
                    {
                        foreach (var link in condLinks)
                        {
                            var sourceNode = graph.NodeData.FirstOrDefault(n => n.Guid == link.BaseNodeGuid);
                            if (sourceNode != null)
                            {
                                string subResult = ProcessNodeSync(graph, sourceNode, context, currentHook);
                                if (subResult != "True" && subResult != "Bool Out")
                                {
                                    allTrue = false;
                                    break;
                                }
                            }
                        }
                        resultPort = allTrue ? "True" : "False";
                    }
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

                case "RangeConditionNode":
                    var rangeCond = JsonUtility.FromJson<RangeConditionNodeData>(node.JsonData);
                    bool rangeResult = EvaluateRangeCondition(rangeCond, context);
                    resultPort = rangeResult ? "True" : "False";
                    break;

                case "FactionConditionNode":
                    var factionCond = JsonUtility.FromJson<FactionConditionNodeData>(node.JsonData);
                    bool factionResult = EvaluateFactionCondition(factionCond, context);
                    resultPort = factionResult ? "True" : "False";
                    break;

                case "SpeedAdvantageConditionNode":
                    var speedCond = JsonUtility.FromJson<SpeedAdvantageConditionNodeData>(node.JsonData);
                    bool speedResult = EvaluateSpeedAdvantageCondition(speedCond, context);
                    resultPort = speedResult ? "True" : "False";
                    break;

                case "TurnOrderConditionNode":
                    var turnCond = JsonUtility.FromJson<TurnOrderConditionNodeData>(node.JsonData);
                    bool turnResult = EvaluateTurnOrderCondition(turnCond, context);
                    resultPort = turnResult ? "True" : "False";
                    break;

                case "VariableModifierNode":
                    var modData = JsonUtility.FromJson<VariableModifierNodeData>(node.JsonData);
                    float modVal = modData.value;
                    if (!string.IsNullOrEmpty(modData.valueVariableReference))
                        modVal = GetVariable(context, modData.valueVariableReference, modVal);
                    
                    if (modData.useCasterAttribute && context.source != null && context.source.VariableStore != null) 
                    {
                        float statVal = context.source.VariableStore.GetStat(modData.casterAttribute);
                        modVal = statVal * (modVal / 100f);
                    }
                    
                    ModifyVariable(context, modData.variableName, modData.operation, modVal);
                    break;

                case "UnitVariableNode":
                    var unitVarData = JsonUtility.FromJson<UnitVariableNodeData>(node.JsonData);
                    string varKey = unitVarData.variable.ToString();
                    
                    if (unitVarData.operation == UnitVariableOperation.Get)
                    {
                        float readVal = 0f;
                        if (context.source?.VariableStore != null)
                        {
                            readVal = unitVarData.scope == UnitVariableScope.Slot && !string.IsNullOrEmpty(context.slotId)
                                ? context.source.VariableStore.GetSlotVar(context.slotId, varKey)
                                : context.source.VariableStore.GetGlobalVar(varKey);
                        }
                        if (!string.IsNullOrEmpty(unitVarData.outputVariable))
                            context.Variables[unitVarData.outputVariable] = readVal;
                    }
                    else
                    {
                        if (UnitVariableHelper.IsReadOnly(unitVarData.variable))
                        {
                            Debug.LogWarning($"[Interpreter] Tentativa de escrever na variável read-only '{varKey}'");
                            break;
                        }
                        
                        float writeVal = unitVarData.value;
                        if (!string.IsNullOrEmpty(unitVarData.contextVariableReference))
                            writeVal = GetVariable(context, unitVarData.contextVariableReference, writeVal);
                        
                        float currentVal = 0f;
                        if (context.source?.VariableStore != null)
                        {
                            currentVal = unitVarData.scope == UnitVariableScope.Slot && !string.IsNullOrEmpty(context.slotId)
                                ? context.source.VariableStore.GetSlotVar(context.slotId, varKey)
                                : context.source.VariableStore.GetGlobalVar(varKey);
                        }
                        
                        float newVal = unitVarData.operation switch
                        {
                            UnitVariableOperation.Set => writeVal,
                            UnitVariableOperation.Add => currentVal + writeVal,
                            UnitVariableOperation.Subtract => currentVal - writeVal,
                            UnitVariableOperation.Multiply => currentVal * writeVal,
                            UnitVariableOperation.Divide => writeVal != 0 ? currentVal / writeVal : currentVal,
                            _ => writeVal
                        };
                        
                        if (context.source?.VariableStore != null)
                        {
                            if (unitVarData.scope == UnitVariableScope.Slot && !string.IsNullOrEmpty(context.slotId))
                                context.source.VariableStore.SetSlotVar(context.slotId, varKey, newVal);
                            else
                                context.source.VariableStore.SetGlobalVar(varKey, newVal);
                        }
                    }
                    break;

                case "RamificationNode":
                    var ramData = JsonUtility.FromJson<RamificationNodeData>(node.JsonData);
                    resultPort = "Base"; // Padrão
                    
                    if (context.source?.Loadout != null && ramData.flows != null)
                    {
                        var sel = context.source.Loadout.branchSelections.Find(
                            s => s.skillId == graph.name);
                        if (sel != null && sel.selectedBranchIds.Count > ramData.tierIndex)
                        {
                            string selectedId = sel.selectedBranchIds[ramData.tierIndex];
                            if (!string.IsNullOrEmpty(selectedId))
                            {
                                var matchingFlow = ramData.flows.Find(f => f.flowId == selectedId);
                                if (matchingFlow != null)
                                    resultPort = matchingFlow.flowName;
                            }
                        }
                    }
                    CombatLogger.Log($"  <color=#a29bfe>[Ramificação]</color> Tier {ramData.tierIndex}: <b>{resultPort}</b>", LogCategory.Graph);
                    break;

                case "RamificationSpecNode":
                    // Passthrough
                    break;

                case "LevelBranchNode":
                    resultPort = $"Level {context.abilityLevel}";
                    break;

                case "StatModifierEffectNode":
                    var statModData = JsonUtility.FromJson<StatModifierNodeData>(node.JsonData);
                    ExecuteStatModifier(statModData, node, graph, context);
                    break;

                case "ApplyModifierNode":
                    var applyModData = JsonUtility.FromJson<ApplyModifierNodeData>(node.JsonData);
                    ExecuteApplyModifier(applyModData, node, graph, context);
                    break;

                case "LimitPerTurnNode":
                    var limitData = JsonUtility.FromJson<LimitPerTurnNodeData>(node.JsonData);
                    string key = $"Limit_{node.Guid}_{TurnManager.Instance.RoundCounter}";
                    float uses = context.source?.VariableStore != null ? context.source.VariableStore.GetGlobalVar(key) : 0;
                    if (uses < limitData.maxExecutionsPerTurn)
                    {
                        context.source?.VariableStore?.SetGlobalVar(key, uses + 1);
                        resultPort = "True";
                    }
                    else
                    {
                        resultPort = "False";
                    }
                    break;

                case "VfxNode":
                    var vfxData = JsonUtility.FromJson<VfxNodeData>(node.JsonData);
                    ExecuteVfx(vfxData, context);
                    break;

                case "CostNode":
                    var costData = JsonUtility.FromJson<CostNodeData>(node.JsonData);
                    ExecuteCost(costData, context);
                    break;

                case "CleanseStatusNode":
                    var cleanseData = JsonUtility.FromJson<CleanseStatusNodeData>(node.JsonData);
                    ExecuteCleanse(cleanseData, context);
                    break;
                    
                case "SacrificeHealthNode":
                    var sacData = JsonUtility.FromJson<SacrificeHealthNodeData>(node.JsonData);
                    if (context.source != null && context.source.Health != null)
                    {
                        int hpLoss = sacData.usePercentage 
                            ? Mathf.RoundToInt(context.source.Health.MaxHealth * (sacData.amount / 100f))
                            : Mathf.RoundToInt(sacData.amount);
                        
                        context.source.Health.TakeDamage(hpLoss, false, context.source);
                        if (!string.IsNullOrEmpty(sacData.outputVariable))
                            context.Variables[sacData.outputVariable] = hpLoss;
                            
                        CombatLogger.Log($"<color=#ff4d4d>[Sacrifício]</color> {context.source.name} sacrificou {hpLoss} HP!", LogCategory.Passive);
                    }
                    break;
            }

            return resultPort;
        }

        private IEnumerator ProcessNode(AbilityGraphSO graph, AbilityNodeData node, CombatContext context, CombatHook currentHook, Action<string> onResultPort)
        {
            // Log de execução de nó
            // CombatLogger.Log($"  > Executando: {node.NodeType}", LogCategory.Graph);

            string resultPort = "Out";

            // Tenta disparar a animação do pet se o nó for um efeito ou algo "produtivo" (não condicional)
            CheckAndTriggerPetAnimation(graph, node, context);

            switch (node.NodeType)
            {
                case "TriggerNode":
                    var triggerData = JsonUtility.FromJson<TriggerNodeData>(node.JsonData);
                    // O TriggerNode é passivo no grafo, mas aqui ele valida se o hook bate
                    break;

                case "TargetNode":
                    var targetData = JsonUtility.FromJson<TargetNodeData>(node.JsonData);
                    
                    AreaPatternData pattern = graph.GetAsset<AreaPatternData>(targetData.patternReferenceId);
                    if (pattern == null) pattern = node.areaPattern;
                    targetData.areaPattern = pattern; 

                    int resolvedRange = targetData.range;
                    if (!string.IsNullOrEmpty(targetData.rangeVariable))
                        resolvedRange = (int)GetVariable(context, targetData.rangeVariable, resolvedRange);

                    if (targetData.useExtraRangeVariable && context.source != null && context.source.VariableStore != null)
                    {
                        float extraRange = string.IsNullOrEmpty(context.slotId)
                            ? context.source.VariableStore.GetGlobalVar("ExtraRange")
                            : context.source.VariableStore.GetSlotVar(context.slotId, "ExtraRange");
                        resolvedRange += Mathf.RoundToInt(extraRange);
                    }

                    yield return StartCoroutine(HandleTargeting(targetData, context, currentHook, resolvedRange));
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
                    // O usuário quer lógica AND: True se TODAS forem verdadeiras.
                    // Procuramos todas as portas "Cond X" conectadas.
                    bool allTrue = true;
                    var condLinks = graph.NodeLinks.Where(l => l.TargetNodeGuid == node.Guid && l.TargetPortName.StartsWith("Cond")).ToList();
                    
                    if (condLinks.Count == 0)
                    {
                        // Se não tem condições, vamos para True? Ou False? 
                        // Seguindo a lógica de "cumprimento de todas as condições", 0 condições = todas cumpridas.
                        resultPort = "True";
                    }
                    else
                    {
                        foreach (var link in condLinks)
                        {
                            var sourceNode = graph.NodeData.FirstOrDefault(n => n.Guid == link.BaseNodeGuid);
                            if (sourceNode != null)
                            {
                                string subResult = "False";
                                yield return StartCoroutine(ProcessNode(graph, sourceNode, context, currentHook, (res) => subResult = res));
                                if (subResult != "True" && subResult != "Bool Out")
                                {
                                    allTrue = false;
                                    break;
                                }
                            }
                        }
                        resultPort = allTrue ? "True" : "False";
                        CombatLogger.Log($"  <color=#ffd700>[Lógica]</color> Fluxo Condicional: <b>{resultPort}</b> (Todas as condições {(allTrue ? "cumpridas" : "não cumpridas")})", LogCategory.Condition);
                    }
                    break;

                case "AttributeConditionNode":
                    var attrCond = JsonUtility.FromJson<AttributeConditionNodeData>(node.JsonData);
                    bool attrResult = EvaluateAttributeCondition(attrCond, context);
                    resultPort = attrResult ? "True" : "False";
                    CombatLogger.Log($"  <color=#ffd700>[Condição]</color> Atributo {attrCond.attribute}: <b>{resultPort}</b>", LogCategory.Condition);
                    break;

                case "DistanceConditionNode":
                    var distCond = JsonUtility.FromJson<DistanceConditionNodeData>(node.JsonData);
                    bool distResult = EvaluateDistanceCondition(distCond, context);
                    resultPort = distResult ? "True" : "False";
                    CombatLogger.Log($"  <color=#ffd700>[Condição]</color> Distância ({distCond.checkType} {distCond.distanceValue}): <b>{resultPort}</b>", LogCategory.Condition);
                    break;

                case "RangeConditionNode":
                    var rangeCond = JsonUtility.FromJson<RangeConditionNodeData>(node.JsonData);
                    bool rangeResult = EvaluateRangeCondition(rangeCond, context);
                    resultPort = rangeResult ? "True" : "False";
                    break;

                case "FactionConditionNode":
                    var factionCond = JsonUtility.FromJson<FactionConditionNodeData>(node.JsonData);
                    bool factionResult = EvaluateFactionCondition(factionCond, context);
                    resultPort = factionResult ? "True" : "False";
                    CombatLogger.Log($"  <color=#ffd700>[Condição]</color> Facção ({factionCond.faction}): <b>{resultPort}</b>", LogCategory.Condition);
                    break;

                case "SpeedAdvantageConditionNode":
                    var speedCond = JsonUtility.FromJson<SpeedAdvantageConditionNodeData>(node.JsonData);
                    bool speedResult = EvaluateSpeedAdvantageCondition(speedCond, context);
                    resultPort = speedResult ? "True" : "False";
                    break;

                case "TurnOrderConditionNode":
                    var turnCond = JsonUtility.FromJson<TurnOrderConditionNodeData>(node.JsonData);
                    bool turnResult = EvaluateTurnOrderCondition(turnCond, context);
                    resultPort = turnResult ? "True" : "False";
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

                case "ModifyAPNode":
                    var modifyAPData = JsonUtility.FromJson<ModifyAPNodeData>(node.JsonData);
                    ExecuteModifyAP(modifyAPData, context);
                    break;

                case "CleanseStatusNode":
                    var cleanseData = JsonUtility.FromJson<CleanseStatusNodeData>(node.JsonData);
                    ExecuteCleanse(cleanseData, context);
                    break;
                    
                case "SacrificeHealthNode":
                    var sacData = JsonUtility.FromJson<SacrificeHealthNodeData>(node.JsonData);
                    if (context.source != null && context.source.Health != null)
                    {
                        int hpLoss = sacData.usePercentage 
                            ? Mathf.RoundToInt(context.source.Health.MaxHealth * (sacData.amount / 100f))
                            : Mathf.RoundToInt(sacData.amount);
                        
                        context.source.Health.TakeDamage(hpLoss, false, context.source);
                        if (!string.IsNullOrEmpty(sacData.outputVariable))
                            context.Variables[sacData.outputVariable] = hpLoss;
                            
                        CombatLogger.Log($"<color=#ff4d4d>[Sacrifício]</color> {context.source.name} sacrificou {hpLoss} HP!", LogCategory.Passive);
                    }
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
                    
                    if (modData.useCasterAttribute && context.source != null && context.source.VariableStore != null) 
                    {
                        float statVal = context.source.VariableStore.GetStat(modData.casterAttribute);
                        modVal = statVal * (modVal / 100f);
                    }
                    
                    ModifyVariable(context, modData.variableName, modData.operation, modVal);
                    break;

                case "UnitVariableNode":
                    var unitVarData = JsonUtility.FromJson<UnitVariableNodeData>(node.JsonData);
                    string varKey = unitVarData.variable.ToString();
                    
                    if (unitVarData.operation == UnitVariableOperation.Get)
                    {
                        float readVal = 0f;
                        if (context.source?.VariableStore != null)
                        {
                            readVal = unitVarData.scope == UnitVariableScope.Slot && !string.IsNullOrEmpty(context.slotId)
                                ? context.source.VariableStore.GetSlotVar(context.slotId, varKey)
                                : context.source.VariableStore.GetGlobalVar(varKey);
                        }
                        if (!string.IsNullOrEmpty(unitVarData.outputVariable))
                            context.Variables[unitVarData.outputVariable] = readVal;
                        CombatLogger.Log($"  <color=#a29bfe>[Variavel]</color> Lida variável de unidade '{varKey}' ({readVal}) para o contexto '{unitVarData.outputVariable}'", LogCategory.Graph);
                    }
                    else
                    {
                        if (UnitVariableHelper.IsReadOnly(unitVarData.variable))
                        {
                            Debug.LogWarning($"[Interpreter] Tentativa de escrever na variável read-only '{varKey}'");
                            break;
                        }
                        
                        float writeVal = unitVarData.value;
                        if (!string.IsNullOrEmpty(unitVarData.contextVariableReference))
                            writeVal = GetVariable(context, unitVarData.contextVariableReference, writeVal);
                        
                        float currentVal = 0f;
                        if (context.source?.VariableStore != null)
                        {
                            currentVal = unitVarData.scope == UnitVariableScope.Slot && !string.IsNullOrEmpty(context.slotId)
                                ? context.source.VariableStore.GetSlotVar(context.slotId, varKey)
                                : context.source.VariableStore.GetGlobalVar(varKey);
                        }
                        
                        float newVal = unitVarData.operation switch
                        {
                            UnitVariableOperation.Set => writeVal,
                            UnitVariableOperation.Add => currentVal + writeVal,
                            UnitVariableOperation.Subtract => currentVal - writeVal,
                            UnitVariableOperation.Multiply => currentVal * writeVal,
                            UnitVariableOperation.Divide => writeVal != 0 ? currentVal / writeVal : currentVal,
                            _ => writeVal
                        };
                        
                        if (context.source?.VariableStore != null)
                        {
                            if (unitVarData.scope == UnitVariableScope.Slot && !string.IsNullOrEmpty(context.slotId))
                                context.source.VariableStore.SetSlotVar(context.slotId, varKey, newVal);
                            else
                                context.source.VariableStore.SetGlobalVar(varKey, newVal);
                        }
                        CombatLogger.Log($"  <color=#a29bfe>[Variavel]</color> Variável de unidade '{varKey}' ({(unitVarData.scope == UnitVariableScope.Slot?"Slot":"Global")}) atualizada para {newVal}", LogCategory.Graph);
                    }
                    break;

                case "RamificationNode":
                    var ramData = JsonUtility.FromJson<RamificationNodeData>(node.JsonData);
                    resultPort = "Base"; // Padrão
                    
                    if (context.source?.Loadout != null && ramData.flows != null)
                    {
                        var sel = context.source.Loadout.branchSelections.Find(
                            s => s.skillId == graph.name);
                        if (sel != null && sel.selectedBranchIds.Count > ramData.tierIndex)
                        {
                            string selectedId = sel.selectedBranchIds[ramData.tierIndex];
                            if (!string.IsNullOrEmpty(selectedId))
                            {
                                var matchingFlow = ramData.flows.Find(f => f.flowId == selectedId);
                                if (matchingFlow != null)
                                    resultPort = matchingFlow.flowName;
                            }
                        }
                    }
                    CombatLogger.Log($"  <color=#a29bfe>[Ramificação]</color> Tier {ramData.tierIndex}: <b>{resultPort}</b>", LogCategory.Graph);
                    break;

                case "RamificationSpecNode":
                    // Passthrough
                    break;

                case "LevelBranchNode":
                    resultPort = $"Level {context.abilityLevel}";
                    CombatLogger.Log($"  <color=#a29bfe>[Fluxo]</color> Ramificação de Nível: Seguir porta <b>{resultPort}</b>", LogCategory.Graph);
                    break;

                case "StatModifierEffectNode":
                    var statModData = JsonUtility.FromJson<StatModifierNodeData>(node.JsonData);
                    ExecuteStatModifier(statModData, node, graph, context);
                    break;

                case "ApplyModifierNode":
                    var applyModData = JsonUtility.FromJson<ApplyModifierNodeData>(node.JsonData);
                    ExecuteApplyModifier(applyModData, node, graph, context);
                    break;

                case "LimitPerTurnNode":
                    var limitData = JsonUtility.FromJson<LimitPerTurnNodeData>(node.JsonData);
                    string key = $"Limit_{node.Guid}_{TurnManager.Instance.RoundCounter}";
                    float uses = context.source?.VariableStore != null ? context.source.VariableStore.GetGlobalVar(key) : 0;
                    if (uses < limitData.maxExecutionsPerTurn)
                    {
                        context.source?.VariableStore?.SetGlobalVar(key, uses + 1);
                        resultPort = "True";
                    }
                    else
                    {
                        resultPort = "False";
                    }
                    break;
                    
                case "ScheduleExecutionNode":
                    var schedData = JsonUtility.FromJson<ScheduleExecutionNodeData>(node.JsonData);
                    // Pega o guid do próximo nó a ser executado
                    var nextLink = graph.NodeLinks.FirstOrDefault(l => l.BaseNodeGuid == node.Guid && l.PortName == "Out");
                    if (nextLink != null)
                    {
                        var nextNode = graph.NodeData.FirstOrDefault(n => n.Guid == nextLink.TargetNodeGuid);
                        if (nextNode != null && PreparedActionManager.Instance != null)
                        {
                            PreparedActionManager.Instance.ScheduleAction(context.source, graph, context, nextNode, schedData.delayTurns);
                            resultPort = "Scheduled";
                        }
                    }
                    break;
            }

            onResultPort?.Invoke(resultPort);
            yield return null;
        }

        #region Targeting Logic

        private IEnumerator HandleTargeting(TargetNodeData data, CombatContext context, CombatHook currentHook, int resolvedRange)
        {
            if (data.reusePrevious && context.targets != null && context.targets.Count > 0) yield break;

            if (context.source != null && context.source.Team != Team.Player)
            {
                // Para a IA, se o Behavior Tree forneceu alvos explícitos, eles têm prioridade
                if (context.targets != null && context.targets.Count > 0)
                {
                    Debug.Log($"[Interpreter] Usando {context.targets.Count} alvos do Behavior Tree para a IA.");
                    
                    // Se for um ataque em área, calcular os alvos na área a partir de cada ponto central
                    if (data.mode == GraphTargetMode.Area && data.areaPattern != null)
                    {
                        var expandedTargets = new List<Unit>();
                        var hitPositions = new HashSet<Vector2Int>();
                        if (GridMap.Instance != null)
                        {
                            foreach (var t in context.targets)
                            {
                                var areaPoints = AreaResolver.ResolveCells(t.GridPosition, data.areaPattern, data.preferredDirection);
                                foreach (var pt in areaPoints)
                                {
                                    var tile = GridMap.Instance.GetTile(pt);
                                    if (tile != null && tile.OccupyingUnit != null)
                                    {
                                        bool isAlly = tile.OccupyingUnit.Team == context.source.Team;
                                        if (data.factionType == GraphFactionType.Ally && !isAlly) continue;
                                        if (data.factionType == GraphFactionType.Enemy && isAlly) continue;
                                        
                                        // Apenas adicionar se não bateu nessa unidade para esse ponto
                                        if (hitPositions.Add(tile.OccupyingUnit.GridPosition))
                                        {
                                            expandedTargets.Add(tile.OccupyingUnit);
                                        }
                                        else if (data.allowSameTargetMultipleTimes)
                                        {
                                            // Se permite múltiplas vezes, e já foi atingido, nós adicionamos mesmo assim
                                            expandedTargets.Add(tile.OccupyingUnit);
                                        }
                                    }
                                }
                            }
                        }
                        context.targets = expandedTargets;
                    }
                    yield break;
                }
                else if (context.targetPos.HasValue)
                {
                    Debug.Log($"[Interpreter] Usando alvo do Behavior Tree ({context.targetPos.Value}) para a IA.");
                    
                    // Se for um ataque em área, calcular os alvos na área a partir do ponto central
                    if (data.mode == GraphTargetMode.Area && data.areaPattern != null)
                    {
                        var areaPoints = AreaResolver.ResolveCells(context.targetPos.Value, data.areaPattern, data.preferredDirection);
                        context.targets.Clear();
                        if (GridMap.Instance != null)
                        {
                            foreach (var pt in areaPoints)
                            {
                                var tile = GridMap.Instance.GetTile(pt);
                                if (tile != null && tile.OccupyingUnit != null)
                                {
                                    bool isAlly = tile.OccupyingUnit.Team == context.source.Team;
                                    if (data.factionType == GraphFactionType.Ally && !isAlly) continue;
                                    if (data.factionType == GraphFactionType.Enemy && isAlly) continue;
                                    
                                    context.targets.Add(tile.OccupyingUnit);
                                }
                            }
                        }
                    }
                    yield break;
                }
            }

            if (data.sourceType == GraphTargetSourceType.Manual && currentHook == CombatHook.OnManualCast && context.source != null && context.source.Team == Team.Player)
            {
                TargetingRuleData rule = new TargetingRuleData();
                rule.mode = data.mode == GraphTargetMode.Single ? TargetingMode.Unit : TargetingMode.Area;
                rule.origin = data.origin == GraphTargetOrigin.Unit ? TargetOrigin.Unit : TargetOrigin.Point;
                rule.allowMultiple = data.multipleTargets;
                rule.maxTargets = data.maxTargets;
                rule.targetFaction = (TargetFaction)data.factionType;

                yield return StartCoroutine(PerformManualTargeting(context.source, context.source, resolvedRange, rule, context, (units, points) => context.targets = units, data.areaPattern, data.autoRotate, data.preferredDirection));
            }
            else
            {
                // Auto Strategy - Passamos o range resolvido
                var dataCopy = data; 
                dataCopy.range = resolvedRange;
                context.targets = AutoTargetResolver.Resolve(context.source, dataCopy);
                Debug.Log($"[Interpreter] Auto Targeting: {context.targets.Count} alvos encontrados.");
            }
        }

        private IEnumerator PerformManualTargeting(Unit source, Unit rangeOrigin, int range, TargetingRuleData rule, CombatContext context, Action<List<Unit>, List<Vector2Int>> onTargetsConfirmed, AreaPatternData pattern = null, bool autoRotate = false, Direction preferredDir = Direction.N, IEnumerable<GridTile> whitelist = null)
        {
            TargetSelector selector = source.gameObject.AddComponent<TargetSelector>();
            selector.Begin(rangeOrigin, range, rule, pattern, preferredDir, whitelist, autoRotate);

            bool selectionConfirmed = false;
            List<Unit> selectedUnits = new List<Unit>();
            List<Vector2Int> selectedPoints = new List<Vector2Int>();

            Action<List<Unit>> onTargets = (targets) => { 
                selectedUnits = targets; 
                selectedPoints = selector.SelectedPoints.ToList();
                selectionConfirmed = true; 
            };

            selector.OnTargetsConfirmed += onTargets;
            yield return new WaitUntil(() => selectionConfirmed);
            selector.OnTargetsConfirmed -= onTargets;

            onTargetsConfirmed?.Invoke(selectedUnits, selectedPoints);
            UnityEngine.Object.Destroy(selector);
        }

        #endregion

        #region Effect Implementations

        private void ExecuteDamage(DamageNodeData data, CombatContext context)
        {
            var targetsCopy = context.targets.ToList();
            foreach (var target in targetsCopy)
            {
                if (target == null) continue;
                var stepContext = new CombatContext(context.source, target);
                
                float baseVal = 0;

                if (data.scalings != null && data.scalings.Count > 0)
                {
                    foreach (var scaling in data.scalings)
                    {
                        var u = scaling.useTargetStat ? target : context.source;
                        if (u != null && u.VariableStore != null)
                        {
                            float statVal = u.VariableStore.GetStat(scaling.statType);
                            baseVal += statVal * (scaling.percentage / 100f);
                        }
                    }
                }
                else
                {
                    // Fallback to source attack
                    baseVal = context.source != null ? context.source.Stats.attack : 0;
                }

                float multiplier = 1.0f;
                if (!string.IsNullOrEmpty(data.variableReference))
                    multiplier = GetVariable(context, data.variableReference, multiplier);

                int finalAmount = Mathf.FloorToInt(baseVal * multiplier);
                stepContext.amount = finalAmount;

                CombatLogger.Log($"  <color=#ff4d4d>[Dano]</color> Causando <b>{finalAmount}</b> de dano em <b>{target.DisplayName}</b>", LogCategory.Damage);
                DamageProcessor.ProcessAndApplyDamage(stepContext, true);
            }
        }

        private void ExecuteHeal(HealNodeData data, CombatContext context)
        {
            var targetsCopy = context.targets.ToList();
            foreach (var target in targetsCopy)
            {
                if (target == null) continue;
                
                float baseVal = 0;
                
                if (data.scalings != null && data.scalings.Count > 0)
                {
                    foreach (var scaling in data.scalings)
                    {
                        var u = scaling.useTargetStat ? target : context.source;
                        if (u != null && u.VariableStore != null)
                        {
                            float statVal = u.VariableStore.GetStat(scaling.statType);
                            baseVal += statVal * (scaling.percentage / 100f);
                        }
                    }
                }
                else
                {
                    // Fallback to target max health
                    baseVal = target.Health.MaxHealth;
                }

                float multiplier = 1.0f;
                if (!string.IsNullOrEmpty(data.variableReference))
                    multiplier = GetVariable(context, data.variableReference, multiplier);

                int finalAmount = Mathf.FloorToInt(baseVal * multiplier);
                
                var stepContext = new CombatContext(context.source, target, finalAmount);
                CombatLogger.Log($"  <color=#4dff88>[Cura]</color> Curando <b>{finalAmount}</b> de HP em <b>{target.DisplayName}</b>", LogCategory.Healing);
                DamageProcessor.ProcessAndApplyHeal(stepContext, data.canCrit);
            }
        }

        private IEnumerator ExecuteMove(MoveEffectNodeData data, CombatContext context)
        {
            List<Unit> subjects = new List<Unit>();
            if (data.moveMode == MoveEffectNodeData.MoveMode.MoveCaster)
                subjects.Add(context.source);
            else
                subjects.AddRange(context.targets);

            int resolvedRange = data.range;
            if (!string.IsNullOrEmpty(data.rangeVariable))
                resolvedRange = (int)GetVariable(context, data.rangeVariable, resolvedRange);

            foreach (var subject in subjects)
            {
                if (subject == null) continue;
                Vector2Int destination = subject.GridPosition;

                if (data.manualDestination && AbilityExecutor.Instance != null)
                {
                    if (context.targetPos.HasValue)
                    {
                        destination = context.targetPos.Value;
                    }
                    else
                    {
                        TargetingRuleData rule = new TargetingRuleData();
                        rule.mode = TargetingMode.Area;
                        rule.origin = TargetOrigin.Point;
                        rule.allowMultiple = false;
                        rule.maxTargets = 1;

                        // Gerar whitelist se não permitir ocupados
                        List<GridTile> whitelist = null;
                        if (!data.allowOccupiedTiles && GridMap.Instance != null)
                        {
                            whitelist = GridMap.Instance.GetAllTiles().Where(t => t != null && t.IsWalkable && (!t.IsOccupied || t.OccupyingUnit == subject)).ToList();
                        }

                        Vector2Int selectedPoint = subject.GridPosition;
                        yield return StartCoroutine(PerformManualTargeting(context.source, subject, resolvedRange, rule, context, (units, points) => {
                            if (points.Count > 0) selectedPoint = points[0];
                        }, null, false, Direction.N, whitelist));
                        
                        destination = selectedPoint;
                    }
                }
                else
                {
                    // Lógica Automática / Legado
                    Vector2Int direction = subject.GridPosition - context.source.GridPosition;
                    if (direction == Vector2Int.zero) direction = Vector2Int.up;
                    direction = new Vector2Int(Mathf.Clamp(direction.x, -1, 1), Mathf.Clamp(direction.y, -1, 1));

                    switch (data.moveType)
                    {
                        case MoveEffectNodeData.MoveType.Push:
                            destination = subject.GridPosition + (direction * 1);
                            break;
                        case MoveEffectNodeData.MoveType.Pull:
                            destination = subject.GridPosition - (direction * 1);
                            break;
                        case MoveEffectNodeData.MoveType.DashToTarget:
                            if (context.targets.Count > 0)
                                destination = context.targets[0].GridPosition - direction;
                            break;
                        case MoveEffectNodeData.MoveType.TeleportToTarget:
                            if (context.targets.Count > 0)
                                destination = context.targets[0].GridPosition;
                            break;
                    }
                }

                // Grid Move Logic
                GridMap gridMap = GridMap.Instance;
                if (gridMap != null)
                {
                    var targetTile = gridMap.GetTile(destination);
                    if (targetTile == null || (!data.allowOccupiedTiles && targetTile.IsOccupied && targetTile.OccupyingUnit != subject))
                    {
                        Debug.LogWarning($"[Interpreter] Movimento cancelado.");
                        continue;
                    }

                    var oldTile = gridMap.GetTile(subject.GridPosition);
                    if (oldTile != null) { oldTile.IsOccupied = false; oldTile.OccupyingUnit = null; }

                    var moveAction = subject.GetComponent<MoveAction>();
                    if (moveAction != null)
                    {
                        yield return StartCoroutine(moveAction.MoveRoutine(destination));
                    }
                    else
                    {
                        subject.GridPosition = destination;
                        subject.transform.position = gridMap.GridToWorld(destination);
                    }

                    if (targetTile != null) { targetTile.IsOccupied = true; targetTile.OccupyingUnit = subject; }
                }

                CombatLogger.Log($"  <color=#55ffff>[Movimento]</color> <b>{subject.DisplayName}</b> movido para {destination}", LogCategory.Ability);
            }
            yield return new WaitForSeconds(0.1f);
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
            CombatLogger.Log($"  <color=#55ffff>[Custo]</color> Consumido: {mana} Mana, {stamina} Stamina", LogCategory.Ability);
        }

        private void ExecuteStatModifier(StatModifierNodeData data, AbilityNodeData node, AbilityGraphSO graph, CombatContext context)
        {
            foreach(var target in context.targets)
            {
                if (target == null) continue;
                var passiveManager = target.GetComponent<PassiveManager>();
                if (passiveManager == null) continue;

                // Criar um nome estável para o Blueprint baseado no Grafo e no Nó
                // Isso permite que o PassiveManager identifique bônus repetidos
                string stableName = $"GraphBuff_{graph.name}_{node.Guid.Substring(0, 4)}";
                
                var dynamicBlueprint = ScriptableObject.CreateInstance<AbilityBlueprint>();
                dynamicBlueprint.name = stableName;
                dynamicBlueprint.abilityName = string.IsNullOrEmpty(graph.abilityName) ? graph.name : graph.abilityName;
                dynamicBlueprint.abilityIcon = graph.abilityIcon;
                dynamicBlueprint.abilityDescription = graph.abilityDescription;
                dynamicBlueprint.isPersistentCondition = false;
                dynamicBlueprint.durationInTurns = 1; // Default duration, maybe Graph supplies DurationPort later
                dynamicBlueprint.canStack = data.canStack;
                dynamicBlueprint.maxStacks = data.maxStacks;
                
                // O multiplicador global foi removido. Cada bônus agora é independente.
                
                // Separar modificadores Flat e Percent
                var flatBonus = new CombatStats();
                var percentModifiers = new System.Collections.Generic.List<Celestial_Cross.Scripts.Abilities.PassiveEffect_PercentStatBonus.PercentStatModifier>();

                foreach(var stat in data.stats)
                {
                    // Converter nome do StatType string para enum
                    CelestialCross.Artifacts.StatType statType = CelestialCross.Artifacts.StatType.AttackFlat;
                    if (!string.IsNullOrEmpty(stat.statTypeName))
                    {
                        System.Enum.TryParse<CelestialCross.Artifacts.StatType>(stat.statTypeName, out statType);
                    }

                    float baseVal = stat.value;
                    if (stat.valueMode == ModifierValueMode.Variable && !string.IsNullOrEmpty(stat.valueVariable))
                    {
                        baseVal = GetVariable(context, stat.valueVariable, stat.value);
                    }

                    float modifiedValue = baseVal; // Não há mais multiplicador global

                    // Processar modificadores Flat
                    if (statType == CelestialCross.Artifacts.StatType.AttackFlat)
                    {
                        flatBonus.attack = (int)modifiedValue;
                    }
                    else if (statType == CelestialCross.Artifacts.StatType.DefenseFlat)
                    {
                        flatBonus.defense = (int)modifiedValue;
                    }
                    else if (statType == CelestialCross.Artifacts.StatType.HealthFlat)
                    {
                        flatBonus.health = (int)modifiedValue;
                    }
                    else if (statType == CelestialCross.Artifacts.StatType.CriticalRate)
                    {
                        flatBonus.criticalChance = (int)modifiedValue;
                    }
                    // Processar modificadores Percent
                    else if (statType == CelestialCross.Artifacts.StatType.AttackPercent 
                        || statType == CelestialCross.Artifacts.StatType.DefensePercent
                        || statType == CelestialCross.Artifacts.StatType.HealthPercent
                        || statType == CelestialCross.Artifacts.StatType.CriticalDamage
                        || statType == CelestialCross.Artifacts.StatType.EffectResistance
                        || statType == CelestialCross.Artifacts.StatType.EffectHitRate
                        || statType == CelestialCross.Artifacts.StatType.Speed)
                    {
                        percentModifiers.Add(new Celestial_Cross.Scripts.Abilities.PassiveEffect_PercentStatBonus.PercentStatModifier
                        {
                            statType = statType,
                            percentBonus = modifiedValue
                        });
                    }
                }

                // Adicionar modificador de bônus plano se houver valores
                if (flatBonus.attack > 0 || flatBonus.defense > 0 || flatBonus.health > 1 || flatBonus.criticalChance > 0)
                {
                    var flatMod = new Celestial_Cross.Scripts.Abilities.PassiveEffect_ConditionalStatBonus()
                    {
                        triggerHook = data.isBuff ? CombatHook.OnRoundStart : CombatHook.OnTurnStart,
                        statBonus = flatBonus
                    };
                    dynamicBlueprint.modifiers.Add(flatMod);
                }

                // Adicionar modificador de bônus percentual se houver
                if (percentModifiers.Count > 0)
                {
                    var percentMod = new Celestial_Cross.Scripts.Abilities.PassiveEffect_PercentStatBonus()
                    {
                        triggerHook = data.isBuff ? CombatHook.OnRoundStart : CombatHook.OnTurnStart,
                        modifiers = percentModifiers
                    };
                    dynamicBlueprint.modifiers.Add(percentMod);
                }

                // BUSCAR DURAÇÃO DO DURATION NODE (se conectado)
                var durationLink = graph.NodeLinks.FirstOrDefault(l => l.TargetNodeGuid == node.Guid && l.TargetPortName == "Duration");
                if (durationLink != null)
                {
                    var durationNode = graph.NodeData.FirstOrDefault(n => n.Guid == durationLink.BaseNodeGuid);
                    if (durationNode != null)
                    {
                        var durData = JsonUtility.FromJson<DurationNodeData>(durationNode.JsonData);
                        dynamicBlueprint.durationInTurns = (int)durData.value;
                        dynamicBlueprint.isPersistentCondition = (durData.type == Celestial_Cross.Scripts.Abilities.Modifiers.DurationType.Infinite);
                    }
                }

                passiveManager.ApplyCondition(dynamicBlueprint, context.source);

                // Construir log detalhado dos bônus
                string details = "";
                if (flatBonus.attack != 0) details += $"ATK+{flatBonus.attack} ";
                if (flatBonus.defense != 0) details += $"DEF+{flatBonus.defense} ";
                if (flatBonus.health > 1) details += $"HP+{flatBonus.health} ";
                foreach(var p in percentModifiers) details += $"{p.statType}+{p.percentBonus}% ";

                CombatLogger.Log($"  <color=#a29bfe>[Status]</color> Bônus aplicado em <b>{target.DisplayName}</b>: <color=#4dff88>{details}</color> ({dynamicBlueprint.durationInTurns} turnos)", LogCategory.Graph);
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

            bool isBuff = conditionGraph.GetIsBuff();

            var targetsCopy = context.targets.ToList();
            foreach (var target in targetsCopy)
            {
                if (target == null) continue;

                // Teste de Resistência para debuffs
                if (!isBuff)
                {
                    if (!EffectResistanceCheck.ShouldApplyEffect(context.source, target, 100f))
                    {
                        CombatLogger.Log($"  <color=#ffd700>[Resistido]</color> <b>{target.DisplayName}</b> resistiu à condição <b>{conditionGraph.name}</b>", LogCategory.Graph);
                        continue;
                    }
                }

                var passiveManager = target.GetComponent<PassiveManager>();
                if (passiveManager == null) continue;

                for (int i = 0; i < data.stacks; i++)
                {
                    passiveManager.ApplyGraphCondition(conditionGraph, context.source);
                }
                CombatLogger.Log($"  <color=#a29bfe>[Status]</color> Condição <b>{conditionGraph.name}</b> aplicada em <b>{target.DisplayName}</b> ({data.stacks} stacks)", LogCategory.Graph);
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
            
            Unit target = context.targets[0];
            
            // Verificação de Facção (se habilitada no nó)
            if (data.checkFaction)
            {
                bool factionMatch = data.faction switch
                {
                    FactionTarget.Ally => target.Team == context.source.Team,
                    FactionTarget.Enemy => target.Team != context.source.Team,
                    _ => true
                };
                if (!factionMatch) return false;
            }

            int dist = Mathf.Max(Mathf.Abs(context.source.GridPosition.x - target.GridPosition.x), 
                                Mathf.Abs(context.source.GridPosition.y - target.GridPosition.y));

            return data.checkType switch
            {
                DistanceCondition.DistanceType.Min => dist >= data.distanceValue,
                DistanceCondition.DistanceType.Max => dist <= data.distanceValue,
                DistanceCondition.DistanceType.Exact => dist == data.distanceValue,
                _ => false
            };
        }

        private bool EvaluateRangeCondition(RangeConditionNodeData data, CombatContext context)

        {
            Unit originUnit = (data.origin == RangeCondition.RangeOrigin.Caster) ? context.source : (context.targets.Count > 0 ? context.targets[0] : null);
            if (originUnit == null) return false;

            var allUnits = UnityEngine.Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
            int foundCount = 0;

            foreach (var u in allUnits)
            {
                if (u == null || u == originUnit) continue;

                int dist = Mathf.Max(Mathf.Abs(originUnit.GridPosition.x - u.GridPosition.x), Mathf.Abs(originUnit.GridPosition.y - u.GridPosition.y));
                if (dist > data.range) continue;

                bool isAlly = u.Team == originUnit.Team;
                bool matchesFilter = data.filter switch
                {
                    RangeCondition.UnitFilter.Allies => isAlly,
                    RangeCondition.UnitFilter.Enemies => !isAlly,
                    RangeCondition.UnitFilter.Both => true,
                    _ => false
                };

                if (matchesFilter) foundCount++;
            }

            return data.comparison switch
            {
                RangeCondition.Comparison.GreaterOrEqual => foundCount >= data.targetCount,
                RangeCondition.Comparison.LessOrEqual => foundCount <= data.targetCount,
                RangeCondition.Comparison.Exact => foundCount == data.targetCount,
                _ => false
            };
        }

        private bool EvaluateFactionCondition(FactionConditionNodeData data, CombatContext context)
        {
            Unit unit = data.target == AttributeCondition.TargetType.Caster ? context.source : (context.targets.Count > 0 ? context.targets[0] : null);
            if (unit == null) return false;

            bool isAlly = unit.Team == context.source.Team;
            return data.faction == FactionTarget.Ally ? isAlly : !isAlly;
        }

        private bool EvaluateSpeedAdvantageCondition(SpeedAdvantageConditionNodeData data, CombatContext context)
        {
            if (context.targets.Count == 0) return false;
            Unit target = context.targets[0];
            
            int casterSpeed = context.source.Stats.speed;
            int targetSpeed = target.Stats.speed;

            if (data.greaterOrEqual)
                return casterSpeed >= targetSpeed + data.requiredDifference;
            
            return Mathf.Abs(casterSpeed - targetSpeed) >= data.requiredDifference;
        }

        private bool EvaluateTurnOrderCondition(TurnOrderConditionNodeData data, CombatContext context)
        {
            if (TurnManager.Instance == null) return false;

            if (data.type == TurnOrderCondition.OrderType.FirstInRound)
            {
                return context.source == TurnManager.Instance.RoundStartUnit;
            }
            
            // SpecificIndex logic would go here if TurnManager supported it easily
            return false;
        }

        private void ExecuteCleanse(CleanseStatusNodeData data, CombatContext context)
        {
            foreach (var target in context.targets)
            {
                if (target == null) continue;
                var passiveManager = target.GetComponent<PassiveManager>();
                if (passiveManager == null) continue;

                if (data.allPositive) passiveManager.RemoveAllPositiveConditions();
                if (data.allNegative) passiveManager.RemoveAllNegativeConditions();
            }
        }

        #endregion

        private void ExecuteModifyAP(ModifyAPNodeData data, CombatContext context)
        {
            if (context.source == null) return;
            
            if (data.modifyMax)
            {
                context.source.MaxAP += data.amount;
            }
            else
            {
                context.source.CurrentAP += data.amount;
            }
            CelestialCross.Combat.CombatLogger.Log($"<color=orange>[AP]</color> {context.source.DisplayName} teve {(data.modifyMax ? "MaxAP" : "CurrentAP")} modificado em {data.amount}.", CelestialCross.Combat.LogCategory.System);
        }

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
                case VariableModifierNodeData.Operation.Divide: context.Variables[varName] = value != 0 ? context.Variables[varName] / value : context.Variables[varName]; break;
            }
            Debug.Log($"[Interpreter] Variável '{varName}' atualizada para {context.Variables[varName]} (Op: {op})");
        }

        #endregion

        #region Pet Animation Helpers

        private void CheckAndTriggerPetAnimation(AbilityGraphSO graph, AbilityNodeData node, CombatContext context)
        {
            if (context.hasTriggeredPetAnimation || context.source == null || context.source.petVisual == null) return;
            if (context.source.petSpeciesData == null || context.source.petSpeciesData.AbilityGraphs == null) return;

            // Só dispara se o grafo pertencer ao pet
            if (!context.source.petSpeciesData.AbilityGraphs.Contains(graph)) return;

            // Lista de nós que "confirmam" que a habilidade foi ativada (efeitos, vfx, etc)
            string[] triggerNodes = { 
                "DamageEffectNode", "HealEffectNode", "MoveEffectNode", "VfxNode", 
                "StatModifierEffectNode", "ApplyModifierNode", "CleanseStatusNode", "CostNode" 
            };

            if (triggerNodes.Contains(node.NodeType))
            {
                context.hasTriggeredPetAnimation = true;
                context.source.petVisual.PlaySkill();
                Debug.Log($"[Interpreter] Pet Animation Triggered for {context.source.DisplayName} by node {node.NodeType}");
            }
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
                if (u == null || !u.gameObject.activeInHierarchy || (u.Health != null && u.Health.CurrentHealth <= 0)) return false;
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
                case GraphAutoStrategyType.FarthestUnit:
                    var farthest = filteredUnits.Where(u => u != source).OrderByDescending(u => Vector2Int.Distance(source.GridPosition, u.GridPosition)).FirstOrDefault();
                    if (farthest != null) results.Add(farthest);
                    break;
                case GraphAutoStrategyType.LowestAttribute:
                    // Fallback para HP para manter simplificado se não passarmos o enum extra
                    var lowest = filteredUnits.Where(u => u != source).OrderBy(u => u.Health.CurrentHealth).FirstOrDefault();
                    if (lowest != null) results.Add(lowest);
                    break;
                case GraphAutoStrategyType.HighestAttribute:
                    var highest = filteredUnits.Where(u => u != source).OrderByDescending(u => u.Health.CurrentHealth).FirstOrDefault();
                    if (highest != null) results.Add(highest);
                    break;
                case GraphAutoStrategyType.RandomTarget:
                    var valids = filteredUnits.Where(u => u != source).ToList();
                    if (valids.Count > 0)
                    {
                        for (int i = 0; i < data.targetCount; i++)
                        {
                            results.Add(valids[UnityEngine.Random.Range(0, valids.Count)]);
                        }
                    }
                    break;
            }

            return results;
        }
    }
}
// refresh
