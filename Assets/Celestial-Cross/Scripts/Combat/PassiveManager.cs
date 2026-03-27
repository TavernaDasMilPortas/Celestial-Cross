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

        CombatLogger.Log($"<color=cyan>[PassiveManager]</color> {gameObject.name} disparando hook: <b>{hook}</b>", LogCategory.Passive);

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
                // O AbilityExecutor é agora responsável por lidar com quais etapas são executadas em quais ganchos.
                AbilityExecutor.Instance?.ExecuteAbility(unit, blueprint, hook);
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
        AbilityExecutor.Instance?.ExecuteAbility(source, conditionBlueprint, CombatHook.OnAfterApplyCondition);

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

