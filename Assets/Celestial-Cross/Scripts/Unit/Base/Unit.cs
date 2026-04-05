using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Units;
using CelestialCross.Artifacts;

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
    
    [Header("Test/Debug Artifacts")]
    [SerializeField]
    public List<ArtifactInstance> equippedArtifacts = new List<ArtifactInstance>();
    
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

            // Accumulate Artifact modifiers
            float healthFlat = 0f, healthPct = 0f;
            float atkFlat = 0f, atkPct = 0f;
            float defFlat = 0f, defPct = 0f;
            float spdFlat = 0f;
            float critChanceFlat = 0f;
            float effectAccFlat = 0f;

            if (equippedArtifacts != null)
            {
                Dictionary<ArtifactSet, int> setCounts = new Dictionary<ArtifactSet, int>();

                foreach (var artifact in equippedArtifacts)
                {
                    if (artifact != null)
                    {
                        ProcessArtifactStat(artifact.mainStat, ref healthFlat, ref healthPct, ref atkFlat, ref atkPct, ref defFlat, ref defPct, ref spdFlat, ref critChanceFlat, ref effectAccFlat);
                        foreach (var sub in artifact.subStats)
                        {
                            ProcessArtifactStat(sub, ref healthFlat, ref healthPct, ref atkFlat, ref atkPct, ref defFlat, ref defPct, ref spdFlat, ref critChanceFlat, ref effectAccFlat);
                        }

                        // Registra quantidade desse "Set"
                        if (artifact.artifactSet != null)
                        {
                            if (!setCounts.ContainsKey(artifact.artifactSet))
                                setCounts[artifact.artifactSet] = 0;
                            setCounts[artifact.artifactSet]++;
                        }
                    }
                }

                // Aplica Bônus de Status provindos de Séries Equipadas
                foreach (var kvp in setCounts)
                {
                    ArtifactSet setFamily = kvp.Key;
                    int piecesEquipped = kvp.Value;

                    foreach (var bonus in setFamily.setBonuses)
                    {
                        if (piecesEquipped >= bonus.piecesRequired)
                        {
                            foreach (var statMod in bonus.statBonuses)
                            {
                                ProcessArtifactStat(statMod, ref healthFlat, ref healthPct, ref atkFlat, ref atkPct, ref defFlat, ref defPct, ref spdFlat, ref critChanceFlat, ref effectAccFlat);
                            }
                        }
                    }
                }
            }

            int finalHealth = (int)(Mathf.Round(baseStats.health + healthFlat) * (1f + (healthPct / 100f)));
            int finalAttack = (int)(Mathf.Round(baseStats.attack + atkFlat) * (1f + (atkPct / 100f)));
            int finalDefense = (int)(Mathf.Round(baseStats.defense + defFlat) * (1f + (defPct / 100f)));
            int finalSpeed = (int)Mathf.Round(baseStats.speed + spdFlat); // Speed generally hasn't a percent variant in standard logic
            int finalCrit = (int)Mathf.Round(baseStats.criticalChance + critChanceFlat);
            int finalAcc = (int)Mathf.Round(baseStats.effectAccuracy + effectAccFlat);

            CombatStats finalArtifactStats = new CombatStats(finalHealth, finalAttack, finalDefense, finalSpeed, finalCrit, finalAcc);

            return finalArtifactStats + modifierStats;
        }
    }

    private void ProcessArtifactStat(StatModifier stat, ref float hF, ref float hP, ref float aF, ref float aP, ref float dF, ref float dP, ref float spdF, ref float crF, ref float eaf)
    {
        switch (stat.statType)
        {
            case StatType.HealthFlat: hF += stat.value; break;
            case StatType.HealthPercent: hP += stat.value; break;
            case StatType.AttackFlat: aF += stat.value; break;
            case StatType.AttackPercent: aP += stat.value; break;
            case StatType.DefenseFlat: dF += stat.value; break;
            case StatType.DefensePercent: dP += stat.value; break;
            case StatType.Speed: spdF += stat.value; break;
            case StatType.CriticalRate: crF += stat.value; break;
            case StatType.EffectHitRate: eaf += stat.value; break;
            // CriticalDamage and EffectResistance can be added here if Unit.CombatStats supports them in the future.
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

