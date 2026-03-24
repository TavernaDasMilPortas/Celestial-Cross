using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Combat.Execution;

[RequireComponent(typeof(Unit))]
public class PassiveManager : MonoBehaviour
{
    private Unit unit;
    
    // Lista de condições temporárias ( runtime )
    // TODO: Refatorar WeaverConditionInstance para a nova arquitetura se necessário
    // private List<WeaverConditionInstance> activeConditions = new();

    void Awake()
    {
        unit = GetComponent<Unit>();
    }

    void OnEnable()
    {
        TurnManager.OnTurnStarted += HandleTurnStarted;
        TurnManager.OnRoundStarted += HandleRoundStarted;
    }

    void OnDisable()
    {
        TurnManager.OnTurnStarted -= HandleTurnStarted;
        TurnManager.OnRoundStarted -= HandleRoundStarted;
    }

    void HandleTurnStarted(Unit current)
    {
        if (current != unit) return;
        TriggerHook(CombatHook.OnTurnStart, new CombatContext(unit, unit));
    }

    void HandleRoundStarted(int round)
    {
        TriggerHook(CombatHook.OnRoundStart, new CombatContext(unit, unit));
    }

    public void TriggerHook(CombatHook hook, CombatContext context)
    {
        if (unit.Data == null) return;

        // 1. Processar Passivas da Unidade (Blueprints)
        foreach (var blueprint in unit.Data.GetAbilities())
        {
            if (blueprint != null)
            {
                AbilityExecutor.Instance?.ExecuteAbility(unit, blueprint, hook);
            }
        }

        // 2. Processar Passiva do Pet
        // Nota: Assumindo que o Pet está acessível via Unit ou passado no contexto
        // No momento vamos focar nas habilidades da UnitData.
    }

    public void ApplyCondition(AbilityBlueprint conditionBlueprint, Unit source)
    {
        // No novo sistema, uma "Condição" pode ser simplesmente uma AbilityBlueprint 
        // sendo executada em hooks específicos (OnTurnStart, etc).
        // Por enquanto, vamos apenas registrar que a unidade tem essa habilidade extra.
        Debug.Log($"[PassiveManager] Aplicando condição (Blueprint): {conditionBlueprint.name}");
        AbilityExecutor.Instance?.ExecuteAbility(source, conditionBlueprint, CombatHook.OnAfterApplyCondition);
    }
}

