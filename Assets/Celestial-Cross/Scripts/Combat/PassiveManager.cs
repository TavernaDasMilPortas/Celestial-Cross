using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Abilities.Graph;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;
using Celestial_Cross.Scripts.Combat.Execution;

[RequireComponent(typeof(Unit))]
public class PassiveManager : MonoBehaviour
{
    private Unit unit;
    
    private readonly List<RuntimeGraphCondition> activeGraphConditions = new();
    private HashSet<object> executingAbilities = new();

    public class RuntimeStatCondition
    {
        public string conditionName;
        public bool isBuff;
        public bool isPersistent;
        public int remainingTurns;
        public int stacks;
        public int maxStacks;
        public bool canStack;
        public List<StatModifierNodeData.StatEntry> stats;
        public string displayName;
        public Sprite icon;
    }
    private readonly List<RuntimeStatCondition> activeStatConditions = new();

    [System.Serializable]
    private class RuntimeGraphCondition
    {
        public AbilityGraphSO graph;
        public bool isPersistent;
        public int remainingTurns;
        public int stacks;

        public RuntimeGraphCondition(AbilityGraphSO graph, bool isPersistent, int remainingTurns, int stacks = 1)
        {
            this.graph = graph;
            this.isPersistent = isPersistent;
            this.remainingTurns = remainingTurns;
            this.stacks = stacks;
        }
    }

    public struct PassiveInfo
    {
        public string name;
        public string description;
        public Sprite icon;
        public bool isPersistent;
        public int remainingTurns;
        public int stacks;
    }

    public List<PassiveInfo> GetActiveConditionsInfo()
    {
        var list = new List<PassiveInfo>();
        foreach (var c in activeGraphConditions)
        {
            if (c.graph != null)
            {
                list.Add(new PassiveInfo
                {
                    name = string.IsNullOrEmpty(c.graph.abilityName) ? c.graph.name : c.graph.abilityName,
                    description = c.graph.abilityDescription,
                    icon = c.graph.abilityIcon,
                    isPersistent = c.isPersistent,
                    remainingTurns = c.remainingTurns,
                    stacks = c.stacks
                });
            }
        }
        foreach (var c in activeStatConditions)
        {
            list.Add(new PassiveInfo
            {
                name = c.displayName,
                description = "Modificador de Status",
                icon = c.icon,
                isPersistent = c.isPersistent,
                remainingTurns = c.remainingTurns,
                stacks = c.stacks
            });
        }
        return list;
    }

    public struct StaticPassiveInfo
    {
        public string name;
        public string description;
        public Sprite icon;
        public string source;
    }

    public struct StatModifierInfo
    {
        public string statText;
        public string source;
        public string remaining;
        public bool isPositive;
        public Sprite icon;
    }

    public List<StaticPassiveInfo> GetStaticPassives()
    {
        var list = new List<StaticPassiveInfo>();
        if (unit == null) unit = GetComponent<Unit>();
        if (unit == null) return list;

        if (unit.unitData != null && unit.unitData.skillTreeConfig != null)
        {
            var tree = unit.unitData.skillTreeConfig;
            if (unit.Loadout != null)
            {
                if (!string.IsNullOrEmpty(unit.Loadout.Slot1SkillId))
                {
                    var pool1 = (tree.slot1Skills != null && tree.slot1Skills.Count > 0) ? tree.slot1Skills : tree.combatSkills;
                    var g = pool1.Find(x => x != null && x.name == unit.Loadout.Slot1SkillId);
                    if (g != null && g.IsPassive) list.Add(CreateStaticPassiveInfo(g, "Slot 1"));
                }
                if (!string.IsNullOrEmpty(unit.Loadout.Slot2SkillId))
                {
                    var pool2 = (tree.slot2Skills != null && tree.slot2Skills.Count > 0) ? tree.slot2Skills : tree.combatSkills;
                    var g = pool2.Find(x => x != null && x.name == unit.Loadout.Slot2SkillId);
                    if (g != null && g.IsPassive) list.Add(CreateStaticPassiveInfo(g, "Slot 2"));
                }
            }
            if (tree.basicAttack != null && tree.basicAttack.IsPassive) list.Add(CreateStaticPassiveInfo(tree.basicAttack, "Ataque Básico"));
            if (tree.movementSkill != null && tree.movementSkill.IsPassive) list.Add(CreateStaticPassiveInfo(tree.movementSkill, "Movimentação"));
        }

        if (unit.unitData != null && unit.runtimeUnitData != null)
        {
            var constPassives = CelestialCross.System.ConstellationService.GetUnlockedPassives(unit.unitData, unit.runtimeUnitData.ConstellationLevel);
            if (constPassives != null)
            {
                foreach (var g in constPassives) if (g != null) list.Add(CreateStaticPassiveInfo(g, "Constelação"));
            }
        }

        if (unit.petSpeciesData != null)
        {
            if (unit.petSpeciesData.AbilityGraphs != null)
            {
                foreach (var g in unit.petSpeciesData.AbilityGraphs)
                {
                    if (g != null && g.IsPassive) list.Add(CreateStaticPassiveInfo(g, "Pet (Grafo)"));
                }
            }
        }

        if (unit.equippedArtifacts != null)
        {
            if (unit.ArtifactSetPassiveGraphs != null)
            {
                foreach (var g in unit.ArtifactSetPassiveGraphs)
                {
                    if (g != null) list.Add(CreateStaticPassiveInfo(g, "Set de Artefatos (Grafo)"));
                }
            }
        }

        return list;
    }
    
    private StaticPassiveInfo CreateStaticPassiveInfo(AbilityGraphSO g, string source)
    {
        return new StaticPassiveInfo
        {
            name = string.IsNullOrEmpty(g.abilityName) ? g.name : g.abilityName,
            description = g.abilityDescription,
            icon = g.abilityIcon,
            source = source
        };
    }

    public List<StatModifierInfo> GetActiveStatModifiers()
    {
        var list = new List<StatModifierInfo>();
        foreach (var cond in activeStatConditions)
        {
            string sourceName = cond.displayName;
            string timeStr = cond.isPersistent ? "Permanente" : $"{cond.remainingTurns} Turnos";
            int stacks = cond.stacks;

            if (cond.stats != null)
            {
                foreach (var stat in cond.stats)
                {
                    string statName = stat.statTypeName.ToString();
                    float val = stat.value * stacks;
                    string prefix = val > 0 ? "+" : "";
                    string suffix = stat.bonusType == Celestial_Cross.Scripts.Abilities.Graph.Runtime.ModifierBonusType.Percent ? "%" : "";
                    
                    if (val != 0)
                    {
                        list.Add(new StatModifierInfo
                        {
                            statText = $"{prefix}{val}{suffix} de {statName}",
                            source = sourceName,
                            remaining = timeStr,
                            isPositive = val > 0,
                            icon = cond.icon
                        });
                    }
                }
            }
        }
        return list;
    }

    public List<string> GetActiveConditionNames()
    {
        var names = new List<string>();
        foreach (var c in activeGraphConditions) if (c.graph != null) names.Add(c.graph.name);
        foreach (var c in activeStatConditions) names.Add(c.conditionName);
        return names;
    }

    void Awake() { unit = GetComponent<Unit>(); }
    void OnEnable() { TurnManager.OnTurnStarted += HandleTurnStarted; TurnManager.OnTurnEnded += HandleTurnEnded; TurnManager.OnRoundStarted += HandleRoundStarted; }
    void OnDisable() { TurnManager.OnTurnStarted -= HandleTurnStarted; TurnManager.OnTurnEnded -= HandleTurnEnded; TurnManager.OnRoundStarted -= HandleRoundStarted; }

    void HandleTurnStarted(Unit current) { if (current != unit) return; TriggerHook(CombatHook.OnTurnStart, new CombatContext(unit, unit)); }
    void HandleTurnEnded() { if (TurnManager.Instance != null && TurnManager.Instance.CurrentUnit == unit) { TriggerHook(CombatHook.OnTurnEnd, new CombatContext(unit, unit)); TickConditionsOnTurnEnd(); } }
    void HandleRoundStarted(int round) { TriggerHook(CombatHook.OnRoundStart, new CombatContext(unit, unit)); }

    public void TriggerHook(CombatHook hook, CombatContext context)
    {
        if (unit == null) unit = GetComponent<Unit>();
        if (unit == null || unit.Data == null) return;

        for (int i = 0; i < activeGraphConditions.Count; i++)
        {
            var cond = activeGraphConditions[i];
            if (cond?.graph == null) continue;
            if (executingAbilities.Contains(cond.graph)) continue;

            executingAbilities.Add(cond.graph);
            try
            {
                if (context.Variables == null) context.Variables = new Dictionary<string, float>();
                context.Variables["stacks"] = cond.stacks;

                ExecuteGraphForHook(cond.graph, hook, context, cond.stacks);
            }
            finally
            {
                executingAbilities.Remove(cond.graph);
            }
        }
    }

    private void ExecuteGraphForHook(AbilityGraphSO graph, CombatHook hook, CombatContext context, int stacks)
    {
        if (AbilityGraphInterpreter.Instance == null) return;
        CombatLogger.Log($"<color=cyan>[Passiva - Sync]</color> Gatilho <b>{hook}</b> detectado em <b>{gameObject.name}</b>. Executando grafo: <b>{graph.name}</b>", LogCategory.Passive, true);
        AbilityGraphInterpreter.Instance.ExecuteGraphSync(unit, graph, hook);
    }

    public void ApplyGraphCondition(AbilityGraphSO conditionGraph, Unit source)
    {
        if (conditionGraph == null) return;
        var context = new CombatContext(source, unit);
        TriggerHook(CombatHook.OnBeforeApplyCondition, context);
        source?.GetComponent<PassiveManager>()?.TriggerHook(CombatHook.OnBeforeApplyCondition, context);

        bool persistent = conditionGraph.IsPassive || conditionGraph.GetIsPersistent() || conditionGraph.GetDuration() <= 0;
        int duration = persistent ? 0 : conditionGraph.GetDuration();
        bool canStack = conditionGraph.GetCanStack();
        int maxStacks = conditionGraph.GetMaxStacks();

        var existing = FindGraphCondition(conditionGraph);
        if (existing != null)
        {
            existing.isPersistent = persistent;
            existing.remainingTurns = duration;
            if (canStack)
            {
                existing.stacks++;
                if (maxStacks > 0 && existing.stacks > maxStacks) existing.stacks = maxStacks;
            }
        }
        else
        {
            activeGraphConditions.Add(new RuntimeGraphCondition(conditionGraph, persistent, duration));
        }

        TriggerHook(CombatHook.OnAfterApplyCondition, context);
        source?.GetComponent<PassiveManager>()?.TriggerHook(CombatHook.OnAfterApplyCondition, context);
    }

    public void RemoveGraphCondition(AbilityGraphSO conditionGraph)
    {
        var existing = FindGraphCondition(conditionGraph);
        if (existing != null) activeGraphConditions.Remove(existing);
    }
    
    private RuntimeGraphCondition FindGraphCondition(AbilityGraphSO graph)
    {
        if (graph == null) return null;
        for (int i = 0; i < activeGraphConditions.Count; i++)
        {
            var cond = activeGraphConditions[i];
            if (cond != null && cond.graph == graph) return cond;
        }
        return null;
    }

    public void ApplyStatModifierCondition(string conditionName, bool isBuff, bool canStack, int maxStacks, int duration, bool isPersistent, List<StatModifierNodeData.StatEntry> stats, Unit source, CombatContext context, Sprite icon, string displayName)
    {
        TriggerHook(CombatHook.OnBeforeApplyCondition, context);
        source?.GetComponent<PassiveManager>()?.TriggerHook(CombatHook.OnBeforeApplyCondition, context);

        var existing = activeStatConditions.Find(c => c.conditionName == conditionName);
        if (existing != null)
        {
            existing.isPersistent = isPersistent;
            existing.remainingTurns = duration;
            if (canStack)
            {
                existing.stacks++;
                if (maxStacks > 0 && existing.stacks > maxStacks) existing.stacks = maxStacks;
            }
        }
        else
        {
            activeStatConditions.Add(new RuntimeStatCondition
            {
                conditionName = conditionName,
                isBuff = isBuff,
                isPersistent = isPersistent,
                remainingTurns = duration,
                stacks = 1,
                maxStacks = maxStacks,
                canStack = canStack,
                stats = stats,
                icon = icon,
                displayName = displayName
            });
        }

        TriggerHook(CombatHook.OnAfterApplyCondition, context);
        source?.GetComponent<PassiveManager>()?.TriggerHook(CombatHook.OnAfterApplyCondition, context);
    }

    public void RemoveAllPositiveConditions()
    {
        for (int i = activeGraphConditions.Count - 1; i >= 0; i--) if (activeGraphConditions[i].graph.GetIsBuff()) activeGraphConditions.RemoveAt(i);
        for (int i = activeStatConditions.Count - 1; i >= 0; i--) if (activeStatConditions[i].isBuff) activeStatConditions.RemoveAt(i);
    }

    public void RemoveAllNegativeConditions()
    {
        for (int i = activeGraphConditions.Count - 1; i >= 0; i--) if (!activeGraphConditions[i].graph.GetIsBuff()) activeGraphConditions.RemoveAt(i);
        for (int i = activeStatConditions.Count - 1; i >= 0; i--) if (!activeStatConditions[i].isBuff) activeStatConditions.RemoveAt(i);
    }

    public bool HasPositiveEffects()
    {
        foreach (var c in activeGraphConditions) if (c.graph.GetIsBuff()) return true;
        foreach (var c in activeStatConditions) if (c.isBuff) return true;
        return false;
    }

    public bool HasNegativeEffects()
    {
        foreach (var c in activeGraphConditions) if (!c.graph.GetIsBuff()) return true;
        foreach (var c in activeStatConditions) if (!c.isBuff) return true;
        return false;
    }

    private void TickConditionsOnTurnEnd()
    {
        for (int i = activeGraphConditions.Count - 1; i >= 0; i--)
        {
            var cond = activeGraphConditions[i];
            if (cond == null || cond.graph == null) { activeGraphConditions.RemoveAt(i); continue; }
            if (cond.isPersistent) continue;
            cond.remainingTurns--;
            if (cond.remainingTurns <= 0) activeGraphConditions.RemoveAt(i);
        }

        for (int i = activeStatConditions.Count - 1; i >= 0; i--)
        {
            var cond = activeStatConditions[i];
            if (cond == null) { activeStatConditions.RemoveAt(i); continue; }
            if (cond.isPersistent) continue;
            cond.remainingTurns--;
            if (cond.remainingTurns <= 0) activeStatConditions.RemoveAt(i);
        }
    }

    public CombatStats GetTotalStatBonuses(CombatStats baseStats, CombatContext context = null)
    {
        CombatStats total = new CombatStats(0, 0, 0, 0, 0, 0, 0, 0);
        float atkPct = 0, defPct = 0, hpPct = 0, spdPct = 0;
        float critDmgFlat = 0, effAccFlat = 0, effResFlat = 0;

        foreach (var cond in activeStatConditions)
        {
            if (cond == null || cond.stats == null) continue;
            int stacks = cond.stacks;

            foreach (var stat in cond.stats)
            {
                float val = stat.value; // For now assuming simple value access

                if (stat.bonusType == Celestial_Cross.Scripts.Abilities.Graph.Runtime.ModifierBonusType.Flat)
                {
                    switch (stat.statTypeName)
                    {
                        case nameof(CelestialCross.Artifacts.StatType.AttackFlat): total.attack += Mathf.RoundToInt(val * stacks); break;
                        case nameof(CelestialCross.Artifacts.StatType.DefenseFlat): total.defense += Mathf.RoundToInt(val * stacks); break;
                        case nameof(CelestialCross.Artifacts.StatType.HealthFlat): total.health += Mathf.RoundToInt((val > 1 ? val : 0) * stacks); break;
                        case nameof(CelestialCross.Artifacts.StatType.Speed): total.speed += Mathf.RoundToInt(val * stacks); break;
                        case nameof(CelestialCross.Artifacts.StatType.CriticalRate): total.criticalChance += Mathf.RoundToInt(val * stacks); break;
                        case nameof(CelestialCross.Artifacts.StatType.CriticalDamage): total.criticalDamage += Mathf.RoundToInt(val * stacks); break;
                        case nameof(CelestialCross.Artifacts.StatType.EffectHitRate): total.effectAccuracy += Mathf.RoundToInt(val * stacks); break;
                        case nameof(CelestialCross.Artifacts.StatType.EffectResistance): total.effectResistance += Mathf.RoundToInt(val * stacks); break;
                    }
                }
                else
                {
                    switch (stat.statTypeName)
                    {
                        case nameof(CelestialCross.Artifacts.StatType.AttackPercent): atkPct += val * stacks; break;
                        case nameof(CelestialCross.Artifacts.StatType.DefensePercent): defPct += val * stacks; break;
                        case nameof(CelestialCross.Artifacts.StatType.HealthPercent): hpPct += val * stacks; break;
                        case nameof(CelestialCross.Artifacts.StatType.Speed): spdPct += val * stacks; break;
                        case nameof(CelestialCross.Artifacts.StatType.CriticalDamage): critDmgFlat += val * stacks; break;
                        case nameof(CelestialCross.Artifacts.StatType.EffectHitRate): effAccFlat += val * stacks; break;
                        case nameof(CelestialCross.Artifacts.StatType.EffectResistance): effResFlat += val * stacks; break;
                    }
                }
            }
        }

        total.attack += Mathf.RoundToInt(baseStats.attack * (atkPct / 100f));
        total.defense += Mathf.RoundToInt(baseStats.defense * (defPct / 100f));
        total.health += Mathf.RoundToInt(baseStats.health * (hpPct / 100f));
        total.speed += Mathf.RoundToInt(baseStats.speed * (spdPct / 100f));
        total.criticalDamage += Mathf.RoundToInt(critDmgFlat);
        total.effectAccuracy += Mathf.RoundToInt(effAccFlat);
        total.effectResistance += Mathf.RoundToInt(effResFlat);

        return total;
    }
}
