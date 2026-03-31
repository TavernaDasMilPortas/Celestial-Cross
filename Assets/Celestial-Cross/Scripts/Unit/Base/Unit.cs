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
public abstract class Unit : MonoBehaviour
{
    [Header("Base Data")]
    public UnitData unitData { get; set; }
    public PetData petData { get; set; }
    public Team Team;

    [Header("Runtime")]
    public Vector2Int GridPosition;

    [Header("Runtime Stats")]
    [SerializeField] protected CombatStats modifierStats = new CombatStats(0, 0, 0, 0, 0, 0);

    // =========================
    // PROPERTIES
    // =========================

    public UnitData Data => unitData;
    public PetData EquippedPet => petData;
    public UnitData UnitData => unitData;

    public string DisplayName =>
        unitData != null ? unitData.displayName : name;

    public CombatStats Stats
    {
        get
        {
            CombatStats baseStats = unitData != null
                ? unitData.GetCombinedStats(petData)
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
 
    public virtual void Initialize()
    {
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.RegisterUnit(this);
        }
        if (Health != null) Health.SetMaxHealth(MaxHealth);
        InitializeActions();

        if (petData != null)
        {
            Debug.Log($"<color=green>[Unit Stats]</color> {DisplayName} combinou status com o pet <b>{petData.name}</b>. Total -> HP: {MaxHealth} | Atk: {Stats.attack} | Def: {Stats.defense} | Spd: {Stats.speed} | Crit: {Stats.criticalChance}%");
        }
    }
 
    public void InitializeActions()
    {
        if (unitData == null) { Debug.LogError($"[Unit] {name} não possui UnitData."); return; }
        actions.Clear();
        foreach (var action in GetComponents<IUnitAction>()) Destroy(action as Component);
        var blueprints = unitData.GetAbilities();
        if (blueprints != null) foreach (var bp in blueprints) if (bp != null ) actions.Add(new BlueprintActionWrapper(this, bp));
        if (petData != null && petData.ability != null ) actions.Add(new BlueprintActionWrapper(this, petData.ability));
        foreach (var definition in unitData.GetExecutableDefinitions(petData)) {
            var component = gameObject.AddComponent(definition.GetType()) as IUnitAction;
            if (component != null)
            {
                definition.Configure(component);
                actions.Add(component);
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

    public void Die()
    {
        // 1. Desativar componentes
        GetComponent<Collider>().enabled = false;
        // Adicione aqui outros componentes a serem desativados, como IA, scripts de movimento, etc.

        // 2. Ativar animação/efeito de morte
        // Ex: GetComponent<Animator>().SetTrigger("Die");
        Debug.Log($"{DisplayName} foi derrotado(a).");

        // 3. Adicionar ao cemitério
        if (GraveyardManager.Instance != null)
        {
            GraveyardManager.Instance.AddDeadUnit(this);
        }

        // 4. Notificar o PhaseManager
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.UnregisterUnit(this);
        }

        // 5. Desativar o GameObject após um tempo para a animação tocar
        // Destroy(gameObject, 2f); // Exemplo: Destruir após 2 segundos
        gameObject.SetActive(false); // Ou simplesmente desativar
    }
}

