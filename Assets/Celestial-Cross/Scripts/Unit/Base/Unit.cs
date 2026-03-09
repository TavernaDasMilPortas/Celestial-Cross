using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(UnitHoverDetector))]
[RequireComponent(typeof(UnitOutlineController))]
public abstract class Unit : MonoBehaviour
{
    [Header("Base Data")]
    [SerializeField] private UnitData unitData;
    [SerializeField] private PetData equippedPet;

    [Header("Runtime")]
    public Vector2Int GridPosition;

    // =========================
    // PROPERTIES
    // =========================

    public UnitData Data => unitData;

    public string DisplayName =>
        unitData != null ? unitData.displayName : name;

    public CombatStats Stats =>
        unitData != null
            ? unitData.GetCombinedStats(equippedPet)
            : new CombatStats(1, 0, 0, 0, 0, 0);

    public int Speed => Stats.speed;

    public int MaxHealth => Stats.health;

    public Health Health { get; private set; }

    // =========================
    // ACTIONS
    // =========================

    protected List<IUnitAction> actions = new();
    protected IUnitAction currentAction;

    // =========================
    // UNITY
    // =========================

    protected virtual void Awake()
    {
        Health = GetComponent<Health>();

        if (Health != null)
            Health.SetMaxHealth(MaxHealth);

        SetupActionsFromData();
    }

    // =========================
    // COMBAT
    // =========================

    public int GetAttacksAgainst(Unit target)
    {
        if (target == null)
            return 1;

        return DamageModel.GetAttackCountBySpeed(Stats, target.Stats);
    }

    public AttackResult CalculateAttack(Unit target, DamageBonus damageBonus, DamageReduction damageReduction)
    {
        if (target == null)
            return new AttackResult(1, false);

        return DamageModel.ResolveHit(Stats, target.Stats, damageBonus, damageReduction);
    }

    public PetData EquippedPet => equippedPet;

    public void EquipPet(PetData pet)
    {
        equippedPet = pet;

        if (Health != null)
            Health.SetMaxHealth(MaxHealth);
    }

    // =========================
    // ACTION SETUP
    // =========================

    void SetupActionsFromData()
    {
        if (unitData == null)
        {
            Debug.LogError($"[Unit] {name} não possui UnitData.");
            return;
        }

        actions.Clear();

        foreach (var action in GetComponents<IUnitAction>())
            Destroy(action as Component);

        Debug.Log($"[Unit] Criando ações para {DisplayName}");

        foreach (var actionData in unitData.actions)
        {
            if (actionData == null)
                continue;

            System.Type actionType = actionData.GetRuntimeActionType();
            if (actionType == null)
            {
                Debug.LogError("[Unit] ActionData sem RuntimeActionType.");
                continue;
            }

            var action = gameObject.AddComponent(actionType) as IUnitAction;
            if (action == null)
            {
                Debug.LogError($"[Unit] Falha ao criar {actionType.Name}");
                continue;
            }

            actionData.Configure(action);
            actions.Add(action);

            Debug.Log($"[Unit] Action adicionada: {actionType.Name}");
        }
    }

    // =========================
    // ACTION FLOW
    // =========================

    public void SelectAction(int index)
    {
        if (index < 0 || index >= actions.Count)
            return;

        currentAction?.Cancel();
        currentAction = actions[index];
        currentAction.EnterAction();
    }

    public void UpdateAction()
    {
        currentAction?.UpdateAction();
    }

    public void ConfirmAction()
    {
        currentAction?.Confirm();
    }

    public void CancelAction()
    {
        currentAction?.Cancel();
    }

    // =========================
    // FEEDBACK
    // =========================

    public void LogCanConfirm(bool canConfirm)
    {
        Debug.Log(
            canConfirm
                ? $"[{DisplayName}] Pronto para confirmar (ENTER)"
                : $"[{DisplayName}] Selecione alvo"
        );
    }
}
