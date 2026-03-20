using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Combat;

[RequireComponent(typeof(Unit))]
public class PassiveManager : MonoBehaviour
{
    private Unit unit;
    
    // Lista de passivas fixas da unidade
    [SerializeField] private List<WeaverPassiveData> passiveAbilities = new();
    
    // Lista de condições temporárias ( runtime )
    private List<WeaverConditionInstance> activeConditions = new();

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
        // Para hooks de turno/rodada, a própria unidade é source e target (auto-buffs)
        TriggerHook(CombatHook.OnTurnStart, new CombatContext(current, current));
        CleanupExpiredConditions();
    }

    void HandleRoundStarted(int round)
    {
        TriggerHook(CombatHook.OnRoundStart, new CombatContext(unit, unit));
    }

    void CleanupExpiredConditions()
    {
        for (int i = activeConditions.Count - 1; i >= 0; i--)
        {
            if (activeConditions[i].IsExpired)
            {
                RemoveCondition(activeConditions[i]);
            }
        }
    }

    /// <summary>
    /// Ponto central para disparar hooks de combate nesta unidade.
    /// </summary>
    public void TriggerHook(CombatHook hook, CombatContext context)
    {
        // 1. Processar Passivas
        foreach (var passive in passiveAbilities)
        {
            if (passive != null && passive.trigger == hook)
            {
                Debug.Log($"[PassiveManager] Hook {hook} disparou passiva: {passive.displayName}");
                passive.Execute(context);
            }
        }

        // 2. Processar Condições
        for (int i = activeConditions.Count - 1; i >= 0; i--)
        {
            activeConditions[i].OnHookTriggered(hook, context);
        }
    }

    public void AddPassive(WeaverPassiveEntry entry)
    {
        if (entry == null) return;
        
        Debug.Log($"[PassiveManager] Adicionando passiva virtual: {entry.entryName} (Gatilho: {entry.trigger})");

        // Criamos um WeaverPassiveData "virtual" para reaproveitar a lógica
        WeaverPassiveData virtualPassive = ScriptableObject.CreateInstance<WeaverPassiveData>();
        virtualPassive.displayName = entry.entryName;
        virtualPassive.trigger = entry.trigger;
        virtualPassive.effects = entry.effects;
        
        passiveAbilities.Add(virtualPassive);
    }

    public void ApplyCondition(WeaverConditionData data, Unit source)
    {
        // Lógica de stack/atualização virá depois
        WeaverConditionInstance instance = new WeaverConditionInstance(data, unit, source);
        activeConditions.Add(instance);
        instance.OnApplied();
        
        TriggerHook(CombatHook.OnAfterApplyCondition, new CombatContext(source, unit));
    }

    public void RemoveCondition(WeaverConditionInstance instance)
    {
        if (activeConditions.Contains(instance))
        {
            instance.OnRemoved();
            activeConditions.Remove(instance);
            TriggerHook(CombatHook.OnAfterRemoveCondition, new CombatContext(unit, unit));
        }
    }
}
