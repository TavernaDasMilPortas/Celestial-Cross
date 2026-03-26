using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Units;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(UnitHoverDetector))]
[RequireComponent(typeof(UnitOutlineController))]
[RequireComponent(typeof(PassiveManager))]
[RequireComponent(typeof(CombatLogger))]
public abstract class Unit : MonoBehaviour
{
    [Header("Base Data")]
    [SerializeField] protected UnitData unitData;
    [SerializeField] protected PetData equippedPet;

    [Header("Runtime")]
    public Vector2Int GridPosition;

    [Header("Runtime Stats")]
    [SerializeField] protected CombatStats modifierStats = new CombatStats(0, 0, 0, 0, 0, 0);

    // =========================
    // PROPERTIES
    // =========================

    public UnitData Data => unitData;
    public PetData EquippedPet => equippedPet;

    public string DisplayName =>
        unitData != null ? unitData.displayName : name;

    public CombatStats Stats
    {
        get
        {
            CombatStats baseStats = unitData != null
                ? unitData.GetCombinedStats(equippedPet)
                : new CombatStats(1, 0, 0, 0, 0, 0);

            return baseStats + modifierStats;
        }
    }

    public int Speed => Stats.speed;
    public int MaxHealth => Stats.health;

    public Health Health { get; private set; }
    public PassiveManager PassiveManager { get; private set; }

    protected List<IUnitAction> actions = new();
    public IReadOnlyList<IUnitAction> Actions => actions;
    
    protected IUnitAction currentAction;
    public IUnitAction CurrentAction => currentAction;

    public System.Action<IUnitAction> OnActionChanged;

    // INITIALIZATION
    // =========================
    protected virtual void Awake()
    {
        Health = GetComponent<Health>();
        PassiveManager = GetComponent<PassiveManager>();
        
        // Garante que o PassiveManager exista em todos os Units!
        if (PassiveManager == null) 
        {
            PassiveManager = gameObject.AddComponent<PassiveManager>();
            Debug.Log($"<color=yellow>[Unit]</color> PassiveManager adicionado dinamicamente a {gameObject.name}");
        }
    }
 
    protected virtual void Start()
    {
        if (Health != null) Health.SetMaxHealth(MaxHealth);
        InitializeActions();
    }
 
    public void InitializeActions()
    {
        if (unitData == null) { Debug.LogError($"[Unit] {name} n�o possui UnitData."); return; }
        actions.Clear();
        foreach (var action in GetComponents<IUnitAction>()) Destroy(action as Component);
        var blueprints = unitData.GetAbilities();
        if (blueprints != null) foreach (var bp in blueprints) if (bp != null) actions.Add(new BlueprintActionWrapper(this, bp));
        if (equippedPet != null && equippedPet.ability != null) actions.Add(new BlueprintActionWrapper(this, equippedPet.ability));
        foreach (var definition in unitData.GetExecutableDefinitions(equippedPet)) {
            if (definition == null) continue;
            System.Type actionType = definition.GetRuntimeActionType();
            if (actionType == null) continue;
            var action = gameObject.AddComponent(actionType) as IUnitAction;
            if (action != null) {
                definition.Configure(action);
                actions.Add(action);
            }
        }
    }
 
    // =========================
    // HELPER / UI / AI
    // =========================
 
    public void LogCanConfirm(bool canConfirm) { }
 
    public int GetAttacksAgainst(Unit target) {
        return 1;
    }










    public int GetAttacksAgainst(Unit target, IUnitAction action) => GetAttacksAgainst(target);

    public AttackResult CalculateAttack(Unit target)
    {
        if (target == null) return new AttackResult(Stats.attack, false);
        return DamageModel.ResolveHit(Stats, target.Stats);
    }

    public AttackResult CalculateAttack(Unit target, out bool isCrit, IUnitAction action)
    {
        AttackResult res = CalculateAttack(target);
        isCrit = res.isCritical;
        return res;
    }

    // =========================
    // ACTION FLOW
    // =========================

    public void SelectAction(int index)
    {
        if (index < 0 || index >= actions.Count) return;

        currentAction?.Cancel();
        GridMap.Instance?.ResetAllTileVisuals();

        currentAction = actions[index];
        OnActionChanged?.Invoke(currentAction);

        PassiveManager?.TriggerHook(CombatHook.OnBeforeAction, new CombatContext(this, this, 0, currentAction));
        currentAction.EnterAction();
        CameraController.Instance?.SetActionFocus(currentAction);
    }

    public void UpdateAction() => currentAction?.UpdateAction();
    public void ConfirmAction() => currentAction?.Confirm();
    public void CancelAction()
    {
        currentAction?.Cancel();
        CameraController.Instance?.ResetFocus();
    }

    public void TriggerPassives(CombatHook hook, CombatContext context)
    {
        PassiveManager?.TriggerHook(hook, context);
    }
}

