using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Combat.Execution;

[RequireComponent(typeof(Unit))]
public class PassiveManager : MonoBehaviour
{
    private Unit unit;
    
    // Lista de condições temporárias durante a batalha
    private readonly List<RuntimeCondition> activeRuntimeConditions = new();
    private HashSet<AbilityBlueprint> executingAbilities = new();

    [System.Serializable]
    private class RuntimeCondition
    {
        public AbilityBlueprint blueprint;
        public bool isPersistent;
        public int remainingTurns;

        public RuntimeCondition(AbilityBlueprint blueprint, bool isPersistent, int remainingTurns)
        {
            this.blueprint = blueprint;
            this.isPersistent = isPersistent;
            this.remainingTurns = remainingTurns;
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

            // Decrementa duração de condições no fim do turno da unidade afetada.
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

        var abilities = new List<AbilityBlueprint>();
        if (unit.Data.GetAbilities() != null)
        {
            abilities.AddRange(unit.Data.GetAbilities());
        }
        
        if (unit.EquippedPet != null && unit.Data.GetPetAbility(unit.EquippedPet) != null)
        {
            abilities.Add(unit.Data.GetPetAbility(unit.EquippedPet));
        }
        
        for (int i = 0; i < activeRuntimeConditions.Count; i++)
        {
            var cond = activeRuntimeConditions[i];
            if (cond?.blueprint != null)
                abilities.Add(cond.blueprint);
        }

        foreach (var blueprint in abilities)
        {
            if (blueprint != null)
            {
                if (executingAbilities.Contains(blueprint)) continue;
                executingAbilities.Add(blueprint);
                try
                {
                // 1. Processamos as Modifiers síncronas (como buffs de atributos/danos que modificam o context flat)
                if (blueprint.modifiers != null)
                {
                    foreach (var mod in blueprint.modifiers)
                    {
                        if (mod != null && mod.triggerHook == hook && mod.EvaluateConditions(context))
                        {
                            CombatLogger.Log($"<color=cyan>[PassiveManager]</color> {gameObject.name}: Passiva <b>{blueprint.name}</b> ativada no hook {hook} (Modifier)", LogCategory.Passive);
                            mod.ApplyModifier(context);
                        }
                    }
                }

                // 2. Processamos os EffectSteps vinculados a este hook sem invocar a Coroutine do Executor,
                // de forma síncrona, usando o mesmo CombatContext passado para o PassiveManager!
                if (blueprint.modifierSteps != null)
                {
                    foreach (var step in blueprint.modifierSteps)
                    {
                        if (step == null || step.trigger != hook) continue;

                        List<Unit> targets = new List<Unit>();
                        if (step.targetingStrategy != null)
                            targets = step.targetingStrategy.GetTargets(context);
                        else
                            targets.Add(context.target ?? unit); // Default target ou self

                        foreach (var target in targets)
                        {
                            foreach (var effect in step.effects)
                            {
                                if (effect == null) continue;
                                
                                CombatLogger.Log($"<color=cyan>[PassiveManager]</color> {gameObject.name}: Passiva <b>{blueprint.name}</b> ativada no hook {hook} (Effect: {effect.GetType().Name})", LogCategory.Passive);

                                var originalTarget = context.target;
                                context.target = target;
                                
                                effect.Execute(context);
                                
                                context.target = originalTarget;
                            }
                        }
                    }
                }
                }
                finally
                {
                    executingAbilities.Remove(blueprint);
                }
            }
        }
    }

    public void ApplyCondition(AbilityBlueprint conditionBlueprint, Unit source)
    {
        var context = new CombatContext(source, unit);

        // Hooks ANTES de aplicar a condição
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

            // Atualiza existente ou adiciona nova
            var existing = FindRuntimeCondition(conditionBlueprint);
            if (existing != null)
            {
                existing.isPersistent = persistent;
                existing.remainingTurns = duration;
            }
            else
            {
                activeRuntimeConditions.Add(new RuntimeCondition(conditionBlueprint, persistent, duration));
            }

            Debug.Log($"[PassiveManager] Aplicando condição (Blueprint): {conditionBlueprint.name} | Persistent: {persistent} | Turns: {duration}");
        }


        // Hooks DEPOIS de aplicar a condição
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

    private RuntimeCondition FindRuntimeCondition(AbilityBlueprint blueprint)
    {
        if (blueprint == null) return null;
        for (int i = 0; i < activeRuntimeConditions.Count; i++)
        {
            var cond = activeRuntimeConditions[i];
            if (cond != null && cond.blueprint == blueprint)
                return cond;
        }
        return null;
    }

    private void TickConditionsOnTurnEnd()
    {
        for (int i = activeRuntimeConditions.Count - 1; i >= 0; i--)
        {
            var cond = activeRuntimeConditions[i];
            if (cond == null || cond.blueprint == null)
            {
                activeRuntimeConditions.RemoveAt(i);
                continue;
            }

            if (cond.isPersistent)
                continue;

            cond.remainingTurns--;
            if (cond.remainingTurns <= 0)
            {
                Debug.Log($"[PassiveManager] Condição expirada: {cond.blueprint.name}");
                activeRuntimeConditions.RemoveAt(i);
            }
        }
    }
}











