using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Abilities.Graph;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;
using Celestial_Cross.Scripts.Combat.Execution;

[RequireComponent(typeof(Unit))]
public class PassiveManager : MonoBehaviour
{
    private Unit unit;
    
    // Lista de condições baseadas em Graph durante a batalha
    private readonly List<RuntimeGraphCondition> activeGraphConditions = new();
    
    // Lista de condições legacy baseadas em Blueprint durante a batalha
    private readonly List<RuntimeCondition> activeRuntimeConditions = new();
    private HashSet<object> executingAbilities = new();

    [System.Serializable]
    private class RuntimeCondition
    {
        public AbilityBlueprint blueprint;
        public bool isPersistent;
        public int remainingTurns;
        public int stacks;

        public RuntimeCondition(AbilityBlueprint blueprint, bool isPersistent, int remainingTurns, int stacks = 1)
        {
            this.blueprint = blueprint;
            this.isPersistent = isPersistent;
            this.remainingTurns = remainingTurns;
            this.stacks = stacks;
        }
    }

    [System.Serializable]
    private class RuntimeGraphCondition
    {
        public AbilityGraphSO graph;
        public bool isPersistent;
        public int remainingTurns;
        public int stacks;

        public RuntimeGraphCondition(AbilityGraphSO graph, bool isPersistent, int remainingTurns, int stacks = 1)
        {
            this.graph = graph;
            this.isPersistent = isPersistent;
            this.remainingTurns = remainingTurns;
            this.stacks = stacks;
        }
    }

    void Awake()
    {
        unit = GetComponent<Unit>();
    }

    void OnEnable()
    {
        TurnManager.OnTurnStarted += HandleTurnStarted;
        TurnManager.OnTurnEnded += HandleTurnEnded;
        TurnManager.OnRoundStarted += HandleRoundStarted;
    }

    void OnDisable()
    {
        TurnManager.OnTurnStarted -= HandleTurnStarted;
        TurnManager.OnTurnEnded -= HandleTurnEnded;
        TurnManager.OnRoundStarted -= HandleRoundStarted;
    }

    void HandleTurnStarted(Unit current)
    {
        if (current != unit) return;
        TriggerHook(CombatHook.OnTurnStart, new CombatContext(unit, unit));     
    }

    void HandleTurnEnded()
    {
        if (TurnManager.Instance != null && TurnManager.Instance.CurrentUnit == unit)
        {
            TriggerHook(CombatHook.OnTurnEnd, new CombatContext(unit, unit));
            TickConditionsOnTurnEnd();
        }
    }

    void HandleRoundStarted(int round)
    {
        TriggerHook(CombatHook.OnRoundStart, new CombatContext(unit, unit));    
    }

    public void TriggerHook(CombatHook hook, CombatContext context)
    {
        if (unit == null) unit = GetComponent<Unit>();
        if (unit == null || unit.Data == null) return;

        // --- 1. Legacy Blueprint passives ---
        var blueprintAbilities = new List<AbilityBlueprint>();
        if (unit.petSpeciesData != null)
        {
            if (unit.petSpeciesData.PassiveSkills != null) blueprintAbilities.AddRange(unit.petSpeciesData.PassiveSkills);
            if (unit.petSpeciesData.ActiveSkills != null) blueprintAbilities.AddRange(unit.petSpeciesData.ActiveSkills);
        }
        
        for (int i = 0; i < activeRuntimeConditions.Count; i++)
        {
            var cond = activeRuntimeConditions[i];
            if (cond?.blueprint != null)
                blueprintAbilities.Add(cond.blueprint);
        }

        foreach (var blueprint in blueprintAbilities)
        {
            if (blueprint == null) continue;
            if (executingAbilities.Contains(blueprint)) continue;
            executingAbilities.Add(blueprint);
            try
            {
                int stacks = 1;
                var runtimeCond = FindRuntimeCondition(blueprint);
                if (runtimeCond != null) stacks = runtimeCond.stacks;
                
                if (context.Variables == null) context.Variables = new Dictionary<string, float>();
                context.Variables["stacks"] = stacks;

                if (blueprint.modifiers != null)
                {
                    foreach (var mod in blueprint.modifiers)
                    {
                        if (mod != null && mod.triggerHook == hook)
                        {
                            if (mod.EvaluateConditions(context))
                            {
                                CombatLogger.Log($"<color=cyan>[PassiveManager]</color> {gameObject.name}: Passiva <b>{blueprint.name}</b> ativada (Modifier)", LogCategory.Passive);
                                mod.ApplyModifier(context);
                                CheckAndTriggerPetVisual(blueprint);
                            }
                            else
                            {
                                CombatLogger.Log($"<color=#ffd700>[Condição]</color> Passiva <b>{blueprint.name}</b> ignorada (Requisitos não atendidos)", LogCategory.Condition);
                            }
                        }
                    }
                }

                if (blueprint.modifierSteps != null)
                {
                    foreach (var step in blueprint.modifierSteps)
                    {
                        if (step == null || step.trigger != hook) continue;

                        List<Unit> targets = new List<Unit>();
                        if (step.targetingStrategy != null)
                            targets = step.targetingStrategy.GetTargets(context);
                        else
                            targets.Add(context.target ?? unit);

                        foreach (var target in targets)
                        {
                            foreach (var effect in step.effects)
                            {
                                if (effect == null) continue;
                                
                                CombatLogger.Log($"<color=cyan>[PassiveManager]</color> {gameObject.name}: Passiva <b>{blueprint.name}</b> ativada (Effect: {effect.GetType().Name})", LogCategory.Passive);

                                var originalTarget = context.target;
                                context.target = target;
                                effect.Execute(context);
                                context.target = originalTarget;
                                CheckAndTriggerPetVisual(blueprint);
                            }
                        }
                    }
                }

                if (blueprint.abilityGraph != null)
                {
                    ExecuteGraphForHook(blueprint.abilityGraph, hook, context, stacks);
                }
            }
            finally
            {
                executingAbilities.Remove(blueprint);
            }
        }

        // --- 2. Graph-based conditions ---
        for (int i = 0; i < activeGraphConditions.Count; i++)
        {
            var cond = activeGraphConditions[i];
            if (cond?.graph == null) continue;
            if (executingAbilities.Contains(cond.graph)) continue;

            executingAbilities.Add(cond.graph);
            try
            {
                if (context.Variables == null) context.Variables = new Dictionary<string, float>();
                context.Variables["stacks"] = cond.stacks;

                ExecuteGraphForHook(cond.graph, hook, context, cond.stacks);
            }
            finally
            {
                executingAbilities.Remove(cond.graph);
            }
        }
    }

    private void ExecuteGraphForHook(AbilityGraphSO graph, CombatHook hook, CombatContext context, int stacks)
    {
        if (AbilityGraphInterpreter.Instance == null) return;
        
        CombatLogger.Log($"<color=cyan>[Passiva]</color> Gatilho <b>{hook}</b> detectado em <b>{gameObject.name}</b>. Executando grafo: <b>{graph.name}</b>", LogCategory.Passive, true);

        StartCoroutine(AbilityGraphInterpreter.Instance.ExecuteGraphCoroutine(
            unit, graph, hook, null
        ));
    }

    // --- Apply / Remove Condition (Graph-based) ---

    public void ApplyGraphCondition(AbilityGraphSO conditionGraph, Unit source)
    {
        if (conditionGraph == null) return;
        var context = new CombatContext(source, unit);

        TriggerHook(CombatHook.OnBeforeApplyCondition, context);
        if (source != null)
        {
            var sourcePassive = source.GetComponent<PassiveManager>();
            sourcePassive?.TriggerHook(CombatHook.OnBeforeApplyCondition, context);
        }

        // Se for uma Passiva (Inata), ela deve ser persistente por padrão. 
        // Se for uma Condição (Buff/Debuff), ela segue a duração do grafo.
        bool persistent = conditionGraph.IsPassive || conditionGraph.GetIsPersistent() || conditionGraph.GetDuration() <= 0;
        int duration = persistent ? 0 : conditionGraph.GetDuration();
        bool canStack = conditionGraph.GetCanStack();
        int maxStacks = conditionGraph.GetMaxStacks();

        var existing = FindGraphCondition(conditionGraph);
        if (existing != null)
        {
            existing.isPersistent = persistent;
            existing.remainingTurns = duration;
            if (canStack)
            {
                existing.stacks++;
                if (maxStacks > 0 && existing.stacks > maxStacks)
                    existing.stacks = maxStacks;
            }
        }
        else
        {
            activeGraphConditions.Add(new RuntimeGraphCondition(conditionGraph, persistent, duration));
        }

        int currentStacks = existing != null ? existing.stacks : 1;
        Debug.Log($"[PassiveManager] Aplicando condição (Graph): {conditionGraph.name} | Persistent: {persistent} | Turns: {duration} | Stacks: {currentStacks}");

        TriggerHook(CombatHook.OnAfterApplyCondition, context);
        if (source != null)
        {
            var sourcePassive = source.GetComponent<PassiveManager>();
            sourcePassive?.TriggerHook(CombatHook.OnAfterApplyCondition, context);
        }
    }

    public void RemoveGraphCondition(AbilityGraphSO conditionGraph)
    {
        var existing = FindGraphCondition(conditionGraph);
        if (existing != null)
            activeGraphConditions.Remove(existing);
    }

    public void RemoveAllPositiveConditions()
    {
        for (int i = activeGraphConditions.Count - 1; i >= 0; i--)
        {
            if (activeGraphConditions[i].graph.GetIsBuff())
                activeGraphConditions.RemoveAt(i);
        }
        for (int i = activeRuntimeConditions.Count - 1; i >= 0; i--)
        {
            if (activeRuntimeConditions[i].blueprint.isBuff)
                activeRuntimeConditions.RemoveAt(i);
        }
    }

    public void RemoveAllNegativeConditions()
    {
        for (int i = activeGraphConditions.Count - 1; i >= 0; i--)
        {
            if (!activeGraphConditions[i].graph.GetIsBuff())
                activeGraphConditions.RemoveAt(i);
        }
        for (int i = activeRuntimeConditions.Count - 1; i >= 0; i--)
        {
            if (!activeRuntimeConditions[i].blueprint.isBuff)
                activeRuntimeConditions.RemoveAt(i);
        }
    }


    private RuntimeGraphCondition FindGraphCondition(AbilityGraphSO graph)
    {
        if (graph == null) return null;
        for (int i = 0; i < activeGraphConditions.Count; i++)
        {
            var cond = activeGraphConditions[i];
            if (cond != null && cond.graph == graph)
                return cond;
        }
        return null;
    }

    // --- Legacy Blueprint conditions ---

    public void ApplyCondition(AbilityBlueprint conditionBlueprint, Unit source)
    {
        var context = new CombatContext(source, unit);

        TriggerHook(CombatHook.OnBeforeApplyCondition, context);
        if (source != null)
        {
            var sourcePassive = source.GetComponent<PassiveManager>();
            sourcePassive?.TriggerHook(CombatHook.OnBeforeApplyCondition, context);
        }

        if (conditionBlueprint != null)
        {
            bool persistent = conditionBlueprint.isPersistentCondition || conditionBlueprint.durationInTurns <= 0;
            int duration = persistent ? 0 : conditionBlueprint.durationInTurns;

            var existing = FindRuntimeCondition(conditionBlueprint);
            if (existing != null)
            {
                existing.isPersistent = persistent;
                existing.remainingTurns = duration;
                if (conditionBlueprint.canStack)
                {
                    existing.stacks++;
                    if (conditionBlueprint.maxStacks > 0 && existing.stacks > conditionBlueprint.maxStacks)
                        existing.stacks = conditionBlueprint.maxStacks;
                }
            }
            else
            {
                activeRuntimeConditions.Add(new RuntimeCondition(conditionBlueprint, persistent, duration));
            }

            int currentStacks = existing != null ? existing.stacks : 1;
            Debug.Log($"[PassiveManager] Aplicando condição (Blueprint): {conditionBlueprint.name} | Persistent: {persistent} | Turns: {duration} | Stacks: {currentStacks}");
        }

        TriggerHook(CombatHook.OnAfterApplyCondition, context);
        if (source != null)
        {
            var sourcePassive = source.GetComponent<PassiveManager>();
            sourcePassive?.TriggerHook(CombatHook.OnAfterApplyCondition, context);
        }
    }

    public void RemoveCondition(AbilityBlueprint conditionBlueprint)
    {
        var existing = FindRuntimeCondition(conditionBlueprint);
        if (existing != null)
            activeRuntimeConditions.Remove(existing);
    }

    private RuntimeCondition FindRuntimeCondition(AbilityBlueprint conditionBlueprint)
    {
        if (conditionBlueprint == null) return null;
        foreach (var cond in activeRuntimeConditions)
        {
            // Usar nome para identificação permite que Blueprints gerados dinamicamente 
            // sejam atualizados corretamente mesmo sendo novas instâncias de ScriptableObject.
            if (cond != null && cond.blueprint != null && cond.blueprint.name == conditionBlueprint.name)
                return cond;
        }
        return null;
    }

    private void TickConditionsOnTurnEnd()
    {
        // Tick graph conditions
        for (int i = activeGraphConditions.Count - 1; i >= 0; i--)
        {
            var cond = activeGraphConditions[i];
            if (cond == null || cond.graph == null)
            {
                activeGraphConditions.RemoveAt(i);
                continue;
            }
            if (cond.isPersistent) continue;

            cond.remainingTurns--;
            if (cond.remainingTurns <= 0)
            {
                Debug.Log($"[PassiveManager] Condição expirada (Graph): {cond.graph.name}");
                activeGraphConditions.RemoveAt(i);
            }
        }

        // Tick legacy blueprint conditions
        for (int i = activeRuntimeConditions.Count - 1; i >= 0; i--)
        {
            var cond = activeRuntimeConditions[i];
            if (cond == null || cond.blueprint == null)
            {
                activeRuntimeConditions.RemoveAt(i);
                continue;
            }
            if (cond.isPersistent) continue;

            cond.remainingTurns--;
            if (cond.remainingTurns <= 0)
            {
                Debug.Log($"[PassiveManager] Condição expirada: {cond.blueprint.name}");
                activeRuntimeConditions.RemoveAt(i);
            }
        }
    }

    private void CheckAndTriggerPetVisual(AbilityBlueprint blueprint)
    {
        if (unit == null || unit.petVisual == null || unit.petSpeciesData == null) return;
        
        // Verifica se o blueprint pertence ao pet
        bool isPetBlueprint = false;
        if (unit.petSpeciesData.PassiveSkills != null && unit.petSpeciesData.PassiveSkills.Contains(blueprint)) isPetBlueprint = true;
        if (!isPetBlueprint && unit.petSpeciesData.ActiveSkills != null && unit.petSpeciesData.ActiveSkills.Contains(blueprint)) isPetBlueprint = true;

        if (isPetBlueprint)
        {
            unit.petVisual.PlaySkill();
            Debug.Log($"[PassiveManager] Pet Animation Triggered by Blueprint {blueprint.name}");
        }
    }
    public CombatStats GetTotalStatBonuses(CombatStats baseStats)
    {
        CombatStats total = new CombatStats(0, 0, 0, 0, 0, 0);
        float atkPct = 0, defPct = 0, hpPct = 0, spdPct = 0;

        if (activeRuntimeConditions == null) return total;

        foreach (var cond in activeRuntimeConditions)
        {
            if (cond == null || cond.blueprint == null || cond.blueprint.modifiers == null) continue;

            int stacks = cond.stacks;

            foreach (var mod in cond.blueprint.modifiers)
            {
                if (mod is PassiveEffect_ConditionalStatBonus flatMod)
                {
                    total.attack += flatMod.statBonus.attack * stacks;
                    total.defense += flatMod.statBonus.defense * stacks;
                    // Evitar bugs de HP 1 que é usado para flags internas em alguns lugares
                    total.health += (flatMod.statBonus.health > 1 ? flatMod.statBonus.health : 0) * stacks;
                    total.speed += flatMod.statBonus.speed * stacks;
                    total.criticalChance += flatMod.statBonus.criticalChance * stacks;
                }
                else if (mod is PassiveEffect_PercentStatBonus percentMod)
                {
                    foreach (var p in percentMod.modifiers)
                    {
                        switch (p.statType)
                        {
                            case CelestialCross.Artifacts.StatType.AttackPercent: atkPct += p.percentBonus * stacks; break;
                            case CelestialCross.Artifacts.StatType.DefensePercent: defPct += p.percentBonus * stacks; break;
                            case CelestialCross.Artifacts.StatType.HealthPercent: hpPct += p.percentBonus * stacks; break;
                            case CelestialCross.Artifacts.StatType.Speed: spdPct += p.percentBonus * stacks; break;
                        }
                    }
                }
            }
        }

        // Aplicar bônus percentuais sobre o status base da unidade
        total.attack += Mathf.RoundToInt(baseStats.attack * (atkPct / 100f));
        total.defense += Mathf.RoundToInt(baseStats.defense * (defPct / 100f));
        total.health += Mathf.RoundToInt(baseStats.health * (hpPct / 100f));
        total.speed += Mathf.RoundToInt(baseStats.speed * (spdPct / 100f));

        return total;
    }
}
