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
        // Force finding the Unit component if it's not set
        if (unit == null) unit = GetComponent<Unit>();
        if (unit == null) return;

        // CRITICAL DEBUGS
        Debug.Log($"<color=cyan>[PassiveManager]</color> {gameObject.name} disparando hook: <b>{hook}</b>");

        if (unit.Data == null) 
        {
            Debug.LogError($"<color=red>[PassiveManager]</color> {gameObject.name} NÃO TEM UnitData associado!");
            return;
        }

        var abilities = new List<AbilityBlueprint>();
        if (unit.Data != null && unit.Data.GetAbilities() != null)
            abilities.AddRange(unit.Data.GetAbilities());
        
        abilities.AddRange(activeRuntimeConditions);

        if (abilities.Count == 0)
        {
            Debug.LogWarning($"<color=yellow>[PassiveManager]</color> {gameObject.name} não tem habilidades inatas nem condições em runtime.");
            return;
        }

        Debug.Log($"<color=cyan>[PassiveManager]</color> {gameObject.name} analisando {abilities.Count} habilidades...");

        foreach (var blueprint in abilities)
        {
            if (blueprint == null) 
            {
                Debug.LogWarning($"<color=yellow>[PassiveManager]</color> Encontrado um slot NULO na lista de habilidades de {gameObject.name}");
                continue;
            }

            // Log entry for EVERY blueprint found
            Debug.Log($"<color=white>[PassiveManager]</color> Asset na lista: <b>{blueprint.name}</b> | Tipo: <b>{blueprint.GetType().Namespace}.{blueprint.GetType().Name}</b>");

            if (blueprint is PassiveAbilityBlueprint passiveBlueprint)
            {
                Debug.Log($"<color=green>[PassiveManager]</color> PASSIVA IDENTIFICADA: {blueprint.name}. Efeitos: {passiveBlueprint.passiveEffects?.Count ?? 0}");
                if (passiveBlueprint.passiveEffects == null) continue;

                foreach (var effect in passiveBlueprint.passiveEffects)
                {
                    if (effect == null) 
                    {
                        Debug.LogWarning($"<color=yellow>[PassiveManager]</color> Efeito nulo dentro de {blueprint.name}");
                        continue;
                    }
                    
                    bool matches = (effect.triggerHook == hook);
                    Debug.Log($"<color=white>[PassiveManager]</color> - Efeito {effect.GetType().Name} | Hook: {effect.triggerHook} | Match: {matches}");
                    
                    if (matches)
                    {
                        Debug.Log($"<color=green>[PassiveManager]</color> -> EXECUTANDO {effect.GetType().Name}!");
                        effect.Execute(context);
                    }
                }
            }
        }

        // 2. Processar Passiva do Pet
        if (unit.EquippedPet != null && unit.EquippedPet.ability != null)
        {
            var petAbility = unit.EquippedPet.ability;
            if (petAbility is PassiveAbilityBlueprint petPassive)
            {
                foreach (var effect in petPassive.passiveEffects)
                {
                    if (effect.triggerHook == hook)
                    {
                        effect.Execute(context);
                    }
                }
            }
        }
    }

    public void ApplyCondition(AbilityBlueprint conditionBlueprint, Unit source)
    {
        var context = new CombatContext(source, unit);

        // Hooks ANTES de aplicar a condio
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

        // No novo sistema, uma "Condio" pode ser simplesmente uma AbilityBlueprint 
        // sendo executada em hooks especficos (OnTurnStart, etc).
        // Por enquanto, vamos apenas registrar que a unidade tem essa habilidade extra.
        Debug.Log($"[PassiveManager] Aplicando condio (Blueprint): {conditionBlueprint.name}");
        AbilityExecutor.Instance?.ExecuteAbility(source, conditionBlueprint, CombatHook.OnAfterApplyCondition);

        // Hooks DEPOS de aplicar a condio
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
            var context = new CombatContext(unit, unit);
            TriggerHook(CombatHook.OnBeforeRemoveCondition, context);
            
            activeRuntimeConditions.Remove(conditionBlueprint);
            Debug.Log($"[PassiveManager] Removendo condição (Blueprint): {conditionBlueprint.name}");
            
            TriggerHook(CombatHook.OnAfterRemoveCondition, context);
        }
    }
}

