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
    [SerializeField] private List<ActiveCombatEffect> activeEffects = new();

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
    public IReadOnlyList<IUnitAction> Actions => actions;
    protected IUnitAction currentAction;
    public IUnitAction CurrentAction => currentAction;
    public event System.Action<IUnitAction> OnActionChanged;
    bool combatStarted;

    // =========================
    // UNITY
    // =========================

    protected virtual void Awake()
    {
        Health = GetComponent<Health>();

        if (Health != null)
            Health.SetMaxHealth(Stats.health);

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

        StartCombat();
        target.StartCombat();

        DamageBonus finalBonus = DamageBonus.Combine(damageBonus, GetEffectDamageBonus());
        DamageReduction finalReduction = DamageReduction.Combine(
            damageReduction,
            target.GetEffectDamageReduction(isReceivingAttack: true)
        );

        return DamageModel.ResolveHit(Stats, target.Stats, finalBonus, finalReduction);
    }

    public PetData EquippedPet => equippedPet;

    public void EquipPet(PetData pet)
    {
        equippedPet = pet;

        if (Health != null)
            Health.SetMaxHealth(Stats.health);

        SetupActionsFromData();
    }

    public void StartCombat()
    {
        if (combatStarted)
            return;

        combatStarted = true;

        foreach (var effect in activeEffects)
        {
            if (effect == null)
                continue;

            effect.TriggerCombatStart();
        }
    }

    DamageBonus GetEffectDamageBonus()
    {
        DamageBonus total = new DamageBonus { flat = 0, percent = 0f };

        foreach (var effect in activeEffects)
        {
            if (effect == null)
                continue;

            total = DamageBonus.Combine(total, effect.GetOutgoingDamageBonus());
        }

        return total;
    }

    DamageReduction GetEffectDamageReduction(bool isReceivingAttack)
    {
        DamageReduction total = new DamageReduction { flat = 0, percent = 0f };

        foreach (var effect in activeEffects)
        {
            if (effect == null)
                continue;

            total = DamageReduction.Combine(total, effect.GetIncomingReduction(isReceivingAttack));
        }

        return total;
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

        foreach (var definition in unitData.GetExecutableDefinitions(equippedPet))
        {
            if (definition == null)
                continue;

            System.Type actionType = definition.GetRuntimeActionType();
            if (actionType == null)
            {
                Debug.LogError("[Unit] Definição executável sem RuntimeActionType.");
                continue;
            }

            var action = gameObject.AddComponent(actionType) as IUnitAction;
            if (action == null)
            {
                Debug.LogError($"[Unit] Falha ao criar {actionType.Name}");
                continue;
            }

            definition.Configure(action);
            actions.Add(action);

            Debug.Log($"[Unit] Ação/Habilidade adicionada: {actionType.Name}");
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
        GridMap.Instance?.ResetAllTileVisuals();

        currentAction = actions[index];
        OnActionChanged?.Invoke(currentAction);
        currentAction.EnterAction();

        CameraController.Instance?.SetActionFocus(currentAction);
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
        CameraController.Instance?.ResetFocus();
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
