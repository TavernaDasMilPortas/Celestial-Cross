using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Units;
using CelestialCross.Artifacts;
using Celestial_Cross.Scripts.Abilities.Graph;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(UnitHoverDetector))]
[RequireComponent(typeof(PassiveManager))]
public abstract class Unit : MonoBehaviour
{
    [Header("Base Data")]
    public UnitData unitData { get; set; }
    public CelestialCross.Data.Pets.PetSpeciesSO petSpeciesData { get; set; }
    public CelestialCross.Data.Pets.RuntimePetData runtimePetData { get; set; }
    public CelestialCross.Data.RuntimeUnitData runtimeUnitData { get; set; }
    public CelestialCross.UnitVisuals.PetVisualController petVisual { get; private set; }
    
    [Header("Test/Debug Artifacts")]
    [SerializeField]
    public List<ArtifactInstance> equippedArtifacts = new List<ArtifactInstance>();

    [Header("Runtime Artifacts (Option B)")]
    [SerializeField]
    private List<ArtifactInstanceData> equippedArtifactData = new List<ArtifactInstanceData>();

    [SerializeField]
    private List<StatModifier> cachedArtifactStatModifiers = new List<StatModifier>();

    [SerializeField]
    private List<AbilityBlueprint> cachedArtifactSetPassives = new List<AbilityBlueprint>();

    public IReadOnlyList<AbilityBlueprint> ArtifactSetPassives => cachedArtifactSetPassives;

    [SerializeField]
    private List<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO> cachedArtifactSetPassiveGraphs = new List<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO>();

    public IReadOnlyList<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO> ArtifactSetPassiveGraphs => cachedArtifactSetPassiveGraphs;
    
    public Team Team;

    [Header("Runtime")]
    public Vector2Int GridPosition;

    [Header("Runtime Stats")]
    // Atributos de modificadores (buffs/debuffs) agora são calculados dinamicamente via PassiveManager

    public bool hasMovedThisTurn { get; set; }
    public bool hasActedThisTurn { get; set; }

    public int MaxAP { get; set; } = 1;
    public int CurrentAP { get; set; } = 0;

    // =========================
    // PROPERTIES
    // =========================

    public UnitData Data => unitData;
    public CelestialCross.Data.Pets.RuntimePetData EquippedPet => runtimePetData;
    public UnitData UnitData => unitData;

    public string DisplayName =>
        unitData != null ? unitData.displayName : name;

    public CombatStats Stats
    {
        get
        {
            int level = runtimeUnitData != null ? runtimeUnitData.Level : 1;
            int refMaxLevel = (LevelingConfig.Instance != null) ? LevelingConfig.Instance.globalMaxLevel : 100;
            
            CombatStats baseStats = unitData != null
                ? unitData.GetStatsAtLevel(level, refMaxLevel)
                : new CombatStats(1, 0, 0, 0, 0, 0, 50, 0);

            if (runtimePetData != null)
            {
                baseStats.health += runtimePetData.Health;
                baseStats.attack += runtimePetData.Attack;
                baseStats.defense += runtimePetData.Defense;
                baseStats.speed += runtimePetData.Speed;
                baseStats.criticalChance = Mathf.Clamp(baseStats.criticalChance + runtimePetData.CriticalChance, 0, 100);
                baseStats.effectAccuracy = Mathf.Clamp(baseStats.effectAccuracy + runtimePetData.EffectAccuracy, 0, 100);
            }
            

            // Accumulate Artifact modifiers
            float healthFlat = 0f, healthPct = 0f;
            float atkFlat = 0f, atkPct = 0f;
            float defFlat = 0f, defPct = 0f;
            float spdFlat = 0f;
            float critChanceFlat = 0f;
            float critDamageFlat = 0f;
            float effectAccFlat = 0f;
            float effectResFlat = 0f;

            // Prefer cache built from saved-data artifacts (Option B). If empty, fallback to debug ScriptableObjects.
            if (cachedArtifactStatModifiers != null && cachedArtifactStatModifiers.Count > 0)
            {
                for (int i = 0; i < cachedArtifactStatModifiers.Count; i++)
                {
                    ProcessArtifactStat(cachedArtifactStatModifiers[i], ref healthFlat, ref healthPct, ref atkFlat, ref atkPct, ref defFlat, ref defPct, ref spdFlat, ref critChanceFlat, ref critDamageFlat, ref effectAccFlat, ref effectResFlat);
                }
            }
            else if (equippedArtifacts != null)
            {
                Dictionary<ArtifactSet, int> setCounts = new Dictionary<ArtifactSet, int>();

                foreach (var artifact in equippedArtifacts)
                {
                    if (artifact != null)
                    {
                        ProcessArtifactStat(artifact.mainStat, ref healthFlat, ref healthPct, ref atkFlat, ref atkPct, ref defFlat, ref defPct, ref spdFlat, ref critChanceFlat, ref critDamageFlat, ref effectAccFlat, ref effectResFlat);
                        foreach (var sub in artifact.subStats)
                        {
                            ProcessArtifactStat(sub, ref healthFlat, ref healthPct, ref atkFlat, ref atkPct, ref defFlat, ref defPct, ref spdFlat, ref critChanceFlat, ref critDamageFlat, ref effectAccFlat, ref effectResFlat);
                        }

                        if (artifact.artifactSet != null)
                        {
                            if (!setCounts.ContainsKey(artifact.artifactSet))
                                setCounts[artifact.artifactSet] = 0;
                            setCounts[artifact.artifactSet]++;
                        }
                    }
                }

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
                                ProcessArtifactStat(statMod, ref healthFlat, ref healthPct, ref atkFlat, ref atkPct, ref defFlat, ref defPct, ref spdFlat, ref critChanceFlat, ref critDamageFlat, ref effectAccFlat, ref effectResFlat);
                            }
                        }
                    }
                }
            }

            // uBase deve ser os atributos escalados para o nível atual (sem o pet), e não o Level 1.
            CombatStats uBase = unitData != null ? unitData.GetStatsAtLevel(level, refMaxLevel) : baseStats;
            
            // Subtrai o uBase para encontrar exatamente quanto o pet deu de atributo
            int petHealth = baseStats.health - uBase.health;
            int petAttack = baseStats.attack - uBase.attack;
            int petDefense = baseStats.defense - uBase.defense;

            // Aplica as porcentagens dos artefatos em cima do status escalado pelo nível!
            int finalHealth = (int)(Mathf.Round(uBase.health * (1f + (healthPct / 100f))) + healthFlat) + petHealth;
            int finalAttack = (int)(Mathf.Round(uBase.attack * (1f + (atkPct / 100f))) + atkFlat) + petAttack;
            int finalDefense = (int)(Mathf.Round(uBase.defense * (1f + (defPct / 100f))) + defFlat) + petDefense;
            int finalSpeed = (int)Mathf.Round(baseStats.speed + spdFlat); 
            int finalCrit = (int)Mathf.Round(baseStats.criticalChance + critChanceFlat);
            int finalCritDmg = (int)Mathf.Round(baseStats.criticalDamage + critDamageFlat);
            int finalAcc = (int)Mathf.Round(baseStats.effectAccuracy + effectAccFlat);
            int finalRes = (int)Mathf.Round(baseStats.effectResistance + effectResFlat);

            CombatStats finalArtifactStats = new CombatStats(finalHealth, finalAttack, finalDefense, finalSpeed, finalCrit, finalAcc, finalCritDmg, finalRes);
            
            // Somar bônus de condições ativas do PassiveManager
            CombatStats conditionStats = new CombatStats(0, 0, 0, 0, 0, 0, 0, 0);
            if (PassiveManager != null)
            {
                conditionStats = PassiveManager.GetTotalStatBonuses(uBase);
            }

            return finalArtifactStats + conditionStats;
        }
    }

    private void ProcessArtifactStat(StatModifier stat, ref float hF, ref float hP, ref float aF, ref float aP, ref float dF, ref float dP, ref float spdF, ref float crF, ref float crDF, ref float eaf, ref float erf)
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
            case StatType.CriticalDamage: crDF += stat.value; break;
            case StatType.EffectHitRate: eaf += stat.value; break;
            case StatType.EffectResistance: erf += stat.value; break;
        }
    }

    public int Speed => Stats.speed;
    public int MaxHealth => Stats.health;

    public Health Health { get; private set; }
    public PassiveManager PassiveManager { get; private set; }
    public UnitVariableStore VariableStore { get; private set; }
    public UnitLoadout Loadout { get; private set; }

    public void ConfigureLoadout(UnitLoadout loadout)
    {
        Loadout = loadout;
    }

    protected List<IUnitAction> actions = new();
    public IReadOnlyList<IUnitAction> Actions => actions;
    
    protected IUnitAction currentAction;
    public IUnitAction CurrentAction => currentAction;
    
    // Armazena o path visualizado no targeting para sincronizar perfeitamente a execução real
    [HideInInspector] public System.Collections.Generic.List<UnityEngine.Vector2Int> lastCalculatedPath;

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

        VariableStore = new UnitVariableStore(this);
    }
 
    protected virtual void Start()
    {
        // Safe fallback in case it wasn't spawned by a manager that calls Initialize
        if (PhaseManager.Instance != null && !PhaseManager.Instance.IsUnitRegistered(this))
        {
            Initialize();
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
        
        GetComponentInChildren<UnitVisualController>()?.Init(this);

        // Garante que passivas de set (artefatos) estejam ativas no PassiveManager.
        ApplyArtifactSetPassives();
        
        // Aplica passivas de habilidades inatas (Via Grafos) apenas se for inimigo. 
        // Players usam slots, pets, artefatos e constelações.
        if (unitData != null)
        {
            MaxAP = unitData.maxAP;
            CurrentAP = MaxAP;
            bool isEnemy = this is Celestial_Cross.Scripts.Units.Enemy.EnemyUnit;
            if (isEnemy)
            {
                var graphs = unitData.GetAbilityGraphs();
                if (graphs != null)
                {
                    foreach (var g in graphs)
                    {
                        if (g != null && g.IsPassive)
                        {
                            PassiveManager?.ApplyGraphCondition(g, this);
                        }
                    }
                }
            }
        }

        // Aplica passivas da árvore de habilidades (SkillTreeConfig) e do Loadout
        var treeConfig = unitData != null ? unitData.skillTreeConfig : null;
        if (treeConfig != null)
        {
            // 1. Ataque básico se for passivo
            if (treeConfig.basicAttack != null && treeConfig.basicAttack.IsPassive)
            {
                PassiveManager?.ApplyGraphCondition(treeConfig.basicAttack, this);
            }
            // 2. Movimentação se for passiva
            if (treeConfig.movementSkill != null && treeConfig.movementSkill.IsPassive)
            {
                PassiveManager?.ApplyGraphCondition(treeConfig.movementSkill, this);
            }

            // 3. Habilidades passivas selecionadas nos slots 1 e 2 do Loadout
            if (Loadout != null)
            {
                if (!string.IsNullOrEmpty(Loadout.Slot1SkillId))
                {
                    var pool1 = (treeConfig.slot1Skills != null && treeConfig.slot1Skills.Count > 0)
                        ? treeConfig.slot1Skills
                        : treeConfig.combatSkills;

                    var g = pool1.Find(x => x != null && x.name == Loadout.Slot1SkillId);
                    if (g != null && g.IsPassive)
                    {
                        PassiveManager?.ApplyGraphCondition(g, this);
                        Debug.Log($"[Unit] Passiva do Slot 1 '{g.name}' aplicada em {DisplayName}.");
                    }
                }

                if (!string.IsNullOrEmpty(Loadout.Slot2SkillId))
                {
                    var pool2 = (treeConfig.slot2Skills != null && treeConfig.slot2Skills.Count > 0)
                        ? treeConfig.slot2Skills
                        : treeConfig.combatSkills;

                    var g = pool2.Find(x => x != null && x.name == Loadout.Slot2SkillId);
                    if (g != null && g.IsPassive)
                    {
                        PassiveManager?.ApplyGraphCondition(g, this);
                        Debug.Log($"[Unit] Passiva do Slot 2 '{g.name}' aplicada em {DisplayName}.");
                    }
                }
            }
        }

        if (petSpeciesData != null)
        {
            // Aplica a habilidade do pet como passiva caso aplicável
            if (petSpeciesData.PassiveSkills != null) { foreach(var pass in petSpeciesData.PassiveSkills) { if (pass != null) PassiveManager?.ApplyCondition(pass, this); } }

            // Aplica grafos de habilidades passivas do pet
            if (petSpeciesData.AbilityGraphs != null)
            {
                foreach (var graph in petSpeciesData.AbilityGraphs)
                {
                    if (graph != null && graph.IsPassive)
                    {
                        PassiveManager?.ApplyGraphCondition(graph, this);
                    }
                }
            }

            Debug.Log($"<color=green>[Unit Stats]</color> {DisplayName} combinou status com o pet <b>{(runtimePetData != null ? runtimePetData.DisplayName : petSpeciesData.SpeciesName)}</b>. Total -> HP: {MaxHealth} | Atk: {Stats.attack} | Def: {Stats.defense} | Spd: {Stats.speed} | Crit: {Stats.criticalChance}%");

            // Instancia o visual do Pet
            GameObject petObj = null;

            if (petSpeciesData.IdleAnimation != null)
            {
                // Carrega o Prefab Base universal diretamente da pasta Resources!
                GameObject basePrefab = Resources.Load<GameObject>("BasePetPrefab");
                if (basePrefab != null)
                {
                    petObj = Instantiate(basePrefab, transform);
                    petObj.name = $"PetVisual_{petSpeciesData.SpeciesName}";
                    
                    var anim = petObj.GetComponent<Animator>();

                    if (anim != null && anim.runtimeAnimatorController != null)
                    {
                        // Injeta os clipes específicos deste Pet usando um Override Controller
                        AnimatorOverrideController overrideController = new AnimatorOverrideController(anim.runtimeAnimatorController);
                        
                        var clipOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                        foreach (var clip in overrideController.animationClips)
                        {
                            if (clip.name == "BasePet_Idle" && petSpeciesData.IdleAnimation != null)
                                clipOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, petSpeciesData.IdleAnimation));
                            else if (clip.name == "BasePet_Skill" && petSpeciesData.SkillAnimation != null)
                                clipOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, petSpeciesData.SkillAnimation));
                        }
                        
                        overrideController.ApplyOverrides(clipOverrides);
                        anim.runtimeAnimatorController = overrideController;
                    }
                }
                else
                {
                    Debug.LogWarning("BasePetPrefab não encontrado. Crie um prefab com esse nome dentro de uma pasta chamada 'Resources'.");
                }
            }

            if (petObj != null)
            {
                petVisual = petObj.GetComponent<CelestialCross.UnitVisuals.PetVisualController>();
                if (petVisual == null) petVisual = petObj.AddComponent<CelestialCross.UnitVisuals.PetVisualController>();
                
                petVisual.Setup(transform, petSpeciesData.MovementType, petSpeciesData.CombatScale);
                
                // Sincroniza o flip inicial
                var unitVisual = GetComponentInChildren<UnitVisualController>();
                if (unitVisual != null)
                {
                    var sr = unitVisual.GetComponent<SpriteRenderer>();
                    if (sr != null) petVisual.FaceDirection(sr.flipX);
                }
            }
        }

        // Aplica passivas de constelação (Fase 2)
        if (unitData != null && runtimeUnitData != null)
        {
            var constPassives = CelestialCross.System.ConstellationService.GetUnlockedPassives(unitData, runtimeUnitData.ConstellationLevel);
            foreach (var graph in constPassives)
            {
                if (graph != null) PassiveManager?.ApplyGraphCondition(graph, this);
            }
        }
    }

    public void ConfigureArtifactsFromSaveData(List<ArtifactInstanceData> artifacts, ArtifactSetCatalog setCatalog)
    {
        equippedArtifactData = artifacts ?? new List<ArtifactInstanceData>();
        RebuildArtifactCaches(setCatalog);
    }

    private void RebuildArtifactCaches(ArtifactSetCatalog setCatalog)
    {
        cachedArtifactStatModifiers.Clear();
        cachedArtifactSetPassives.Clear();
        cachedArtifactSetPassiveGraphs.Clear();
 
        if (equippedArtifactData == null)
            return;
 
        // 1) Main/Sub stats
        for (int i = 0; i < equippedArtifactData.Count; i++)
        {
            var artifact = equippedArtifactData[i];
            if (artifact == null) continue;
 
            if (artifact.mainStat != null)
                cachedArtifactStatModifiers.Add(new StatModifier(artifact.mainStat.statType, artifact.mainStat.value));
 
            if (artifact.subStats != null)
            {
                for (int s = 0; s < artifact.subStats.Count; s++)
                {
                    var sub = artifact.subStats[s];
                    if (sub != null)
                        cachedArtifactStatModifiers.Add(new StatModifier(sub.statType, sub.value));
                }
            }
        }
 
        // 2) Set bonuses (stats + passives)
        if (setCatalog == null)
            return;
 
        var setCounts = new Dictionary<string, int>();
        for (int i = 0; i < equippedArtifactData.Count; i++)
        {
            var artifact = equippedArtifactData[i];
            if (artifact == null || string.IsNullOrWhiteSpace(artifact.artifactSetId))
                continue;
 
            if (!setCounts.ContainsKey(artifact.artifactSetId))
                setCounts[artifact.artifactSetId] = 0;
            setCounts[artifact.artifactSetId]++;
        }
 
        foreach (var kvp in setCounts)
        {
            var set = setCatalog.GetSetById(kvp.Key);
            if (set == null) continue;
 
            int piecesEquipped = kvp.Value;
            foreach (var bonus in set.setBonuses)
            {
                if (piecesEquipped < bonus.piecesRequired)
                    continue;
 
                if (bonus.statBonuses != null)
                    cachedArtifactStatModifiers.AddRange(bonus.statBonuses);
 
                if (bonus.passiveAbility != null && !cachedArtifactSetPassives.Contains(bonus.passiveAbility))
                    cachedArtifactSetPassives.Add(bonus.passiveAbility);

                if (bonus.passiveGraph != null && !cachedArtifactSetPassiveGraphs.Contains(bonus.passiveGraph))
                    cachedArtifactSetPassiveGraphs.Add(bonus.passiveGraph);
            }
        }
    }
 
    private void ApplyArtifactSetPassives()
    {
        if (PassiveManager == null)
            return;
            
        // Se estivermos usando a lista de testes do Editor (equippedArtifacts) e não o cache do Save:
        if ((cachedArtifactSetPassives == null || cachedArtifactSetPassives.Count == 0) &&
            (cachedArtifactSetPassiveGraphs == null || cachedArtifactSetPassiveGraphs.Count == 0) &&
            equippedArtifacts != null && equippedArtifacts.Count > 0)
        {
            Dictionary<ArtifactSet, int> setCounts = new Dictionary<ArtifactSet, int>();
            foreach (var artifact in equippedArtifacts)
            {
                if (artifact != null && artifact.artifactSet != null)
                {
                    if (!setCounts.ContainsKey(artifact.artifactSet)) setCounts[artifact.artifactSet] = 0;
                    setCounts[artifact.artifactSet]++;
                }
            }

            foreach (var kvp in setCounts)
            {
                foreach (var bonus in kvp.Key.setBonuses)
                {
                    if (kvp.Value >= bonus.piecesRequired)
                    {
                        if (bonus.passiveAbility != null && !cachedArtifactSetPassives.Contains(bonus.passiveAbility))
                            cachedArtifactSetPassives.Add(bonus.passiveAbility);
                        if (bonus.passiveGraph != null && !cachedArtifactSetPassiveGraphs.Contains(bonus.passiveGraph))
                            cachedArtifactSetPassiveGraphs.Add(bonus.passiveGraph);
                    }
                }
            }
        }
 
        if (cachedArtifactSetPassives != null)
        {
            for (int i = 0; i < cachedArtifactSetPassives.Count; i++)
            {
                var passive = cachedArtifactSetPassives[i];
                if (passive == null) continue;
                PassiveManager.ApplyCondition(passive, this);
            }
        }

        if (cachedArtifactSetPassiveGraphs != null)
        {
            for (int i = 0; i < cachedArtifactSetPassiveGraphs.Count; i++)
            {
                var passiveGraph = cachedArtifactSetPassiveGraphs[i];
                if (passiveGraph == null) continue;
                PassiveManager.ApplyGraphCondition(passiveGraph, this);
            }
        }
    }
 
    public void InitializeActions()
    {
        if (unitData == null) 
        { 
            if (Application.isPlaying) Debug.LogError($"[Unit] {name} não possui UnitData."); 
            return; 
        }
        actions.Clear();
        foreach (var action in GetComponents<IUnitAction>()) 
        {
            Destroy(action as Component);
        }
        
        var addedGraphs = new HashSet<AbilityGraphSO>();

        // 1) Ataque Básico e Movimentação da Árvore (Apenas se não forem passivos)
        var treeConfig = unitData.skillTreeConfig;
        if (treeConfig != null)
        {
            if (treeConfig.basicAttack != null && !treeConfig.basicAttack.IsPassive)
            {
                actions.Add(new GraphActionWrapper(this, treeConfig.basicAttack));
                addedGraphs.Add(treeConfig.basicAttack);
            }
            if (treeConfig.movementSkill != null && !treeConfig.movementSkill.IsPassive)
            {
                actions.Add(new GraphActionWrapper(this, treeConfig.movementSkill));
                addedGraphs.Add(treeConfig.movementSkill);
            }
        }

        // 2) Habilidades ativas dos slots 1 e 2
        if (Loadout != null && treeConfig != null)
        {
            #pragma warning disable 612, 618
            string slot1Id = Loadout.Slot1SkillId;

            if (!string.IsNullOrEmpty(slot1Id))
            {
                var pool = (treeConfig.slot1Skills != null && treeConfig.slot1Skills.Count > 0)
                    ? treeConfig.slot1Skills
                    : treeConfig.combatSkills;

                var g = pool.Find(x => x != null && x.name == slot1Id);
                if (g != null && !g.IsPassive && !addedGraphs.Contains(g))
                {
                    var wrapper = new GraphActionWrapper(this, g);
                    wrapper.SlotId = "Slot1";
                    actions.Add(wrapper);
                    addedGraphs.Add(g);
                    Debug.Log($"[Unit] Habilidade do Slot 1 '{g.name}' injetada em {DisplayName} (via Loadout).");
                }
            }

            string slot2Id = Loadout.Slot2SkillId;

            if (!string.IsNullOrEmpty(slot2Id))
            {
                var pool = (treeConfig.slot2Skills != null && treeConfig.slot2Skills.Count > 0)
                    ? treeConfig.slot2Skills
                    : treeConfig.combatSkills;

                var g = pool.Find(x => x != null && x.name == slot2Id);
                if (g != null && !g.IsPassive && !addedGraphs.Contains(g))
                {
                    var wrapper = new GraphActionWrapper(this, g);
                    wrapper.SlotId = "Slot2";
                    actions.Add(wrapper);
                    addedGraphs.Add(g);
                    Debug.Log($"[Unit] Habilidade do Slot 2 '{g.name}' injetada em {DisplayName} (via Loadout).");
                }
            }
            #pragma warning restore 612, 618
        }

        // 3) Outras habilidades da própria unidade (apenas ativas)
        bool isEnemy = this is Celestial_Cross.Scripts.Units.Enemy.EnemyUnit;
        if (isEnemy)
        {
            var graphs = unitData.GetAbilityGraphs();
            if (graphs != null)
            {
                foreach (var g in graphs)
                {
                    if (g != null && !g.IsPassive && !addedGraphs.Contains(g))
                    {
                        actions.Add(new GraphActionWrapper(this, g));
                        addedGraphs.Add(g);
                    }
                }
            }
        }

        // 4) Habilidades dos Pets (apenas ativas)
        if (petSpeciesData != null)
        {
            if (petSpeciesData.ActiveSkills != null)
            {
                foreach (var act in petSpeciesData.ActiveSkills)
                {
                    if (act != null && !act.isPassive)
                    {
                        actions.Add(new BlueprintActionWrapper(this, act));
                    }
                }
            }
            if (petSpeciesData.AbilityGraphs != null)
            {
                foreach (var graph in petSpeciesData.AbilityGraphs)
                {
                    if (graph != null && !graph.IsPassive && !addedGraphs.Contains(graph))
                    {
                        actions.Add(new GraphActionWrapper(this, graph));
                        addedGraphs.Add(graph);
                    }
                }
            }
        }

        // 5) Executáveis nativos (se houver)
        foreach (var definition in unitData.GetExecutableDefinitions()) {
            var component = gameObject.AddComponent(definition.GetRuntimeActionType()) as IUnitAction;
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
        if (CelestialCross.Tutorial.TutorialMockCombat.ShouldMock)
            return CelestialCross.Tutorial.TutorialMockCombat.GetMockedResult();

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

        // Limpar o Grid
        if (GridMap.Instance != null)
        {
            var tile = GridMap.Instance.GetTile(GridPosition);
            if (tile != null && tile.OccupyingUnit == this)
            {
                tile.IsOccupied = false;
                tile.OccupyingUnit = null;
            }
        }

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





