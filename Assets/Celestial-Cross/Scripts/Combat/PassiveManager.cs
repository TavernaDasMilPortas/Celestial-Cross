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
    private List<AbilityBlueprint> activeRuntimeConditions = new();
    private HashSet<AbilityBlueprint> executingAbilities = new();

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
        abilities.AddRange(activeRuntimeConditions);

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
                            CombatLogger.Log("$<color=cyan>[PassiveManager]</color> {gameObject.name}: Passiva <b>{blueprint.name}</b> ativada no hook {hook} (Modifier)", LogCategory.Passive);
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
                                
                                CombatLogger.Log("$<color=cyan>[PassiveManager]</color> {gameObject.name}: Passiva <b>{blueprint.name}</b> ativada no hook {hook} (Effect: {effect.GetType().Name})", LogCategory.Passive);

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

        // Adiciona a condição na lista de runtime
        if (!activeRuntimeConditions.Contains(conditionBlueprint))
        {
            activeRuntimeConditions.Add(conditionBlueprint);
        }
        
        Debug.Log($"[PassiveManager] Aplicando condição (Blueprint): {conditionBlueprint.name}");


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
        if (activeRuntimeConditions.Contains(conditionBlueprint))
        {
            activeRuntimeConditions.Remove(conditionBlueprint);
        }
    }
}










