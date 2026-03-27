using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Celestial_Cross.Scripts.Combat.Execution;

public class AttackAction : UnitActionBase
{
    public override int Range { get; set; }
    public int Damage { get; set; }
    public override string GetDetailStats() => "Dano: {Damage}";
    public TargetingRuleData TargetingRule { get; set; } = new TargetingRuleData();
    public AreaPatternData AreaPattern { get; set; }
    public override AreaPatternData GetAreaPattern() => AreaPattern;
    public Direction PreferredDirection { get; set; }


    protected override ActionContext CreateContext()
    {
        return new ActionContext(unit);
    }

    protected override void OnEnter()
    {
        Debug.Log($"[AttackAction] {unit.DisplayName} | Range {Range} | Flat Bonus {Damage} | Mode {TargetingRule.mode}");

        StartTargetSelection(Range, TargetingRule);

        targetSelector.OnSelectedTargetsChanged += OnSelectionChanged;
        targetSelector.UpdateAreaConfig(AreaPattern, PreferredDirection);        

        unit.LogCanConfirm(false);
    }

    protected override void OnUpdate()
    {
    }

    protected override void Resolve()
    {
        foreach (var target in context.targets)
        {
            if (target == null || target.Health == null)
                continue;

            int hits = unit.GetAttacksAgainst(target);

            for (int i = 0; i < hits; i++)
            {
                var combatContext = new CelestialCross.Combat.CombatContext(unit, target, unit.Stats.attack + Damage);
                DamageProcessor.ProcessAndApplyDamage(combatContext, applyDefense: true);
            }
        }
    }

    protected override void OnCancel()
    {
    }


    void OnSelectionChanged(List<Unit> targets)
    {
        if (targets == null || targets.Count == 0)
        {
            InvokeForecastUpdated(default);
            return;
        }

        Unit lastTarget = targets.Last();
        if (lastTarget == null) return;

        AttackResult sample = unit.CalculateAttack(lastTarget);

        ActionForecast forecast = new ActionForecast
        {
            Source = unit,
            Target = lastTarget,
            Damage = sample.damage + Damage,
            IsCritical = sample.isCritical,
            AttackCount = unit.GetAttacksAgainst(lastTarget),
            CriticalChance = unit.Stats.criticalChance
        };

        InvokeForecastUpdated(forecast);
    }

    protected override void OnTargetsConfirmed(List<Unit> targets)
    {
        context.targets = ExpandAreaTargetsIfNeeded(targets, targetSelector != null ? targetSelector.SelectedPoints : null);
        state = ActionState.ReadyToConfirm;

        Debug.Log($"[AttackAction] Alvos confirmados: {context.targets.Count}");

        unit.LogCanConfirm(true);
        PerformFinalExecution();
    }

    List<Unit> ExpandAreaTargetsIfNeeded(List<Unit> targets, IReadOnlyList<Vector2Int> selectedPoints)
    {
        if ((targets == null || targets.Count == 0) && (selectedPoints == null || selectedPoints.Count == 0))
            return new List<Unit>();

        if (TargetingRule.mode != TargetingMode.Area || AreaPattern == null)    
            return targets;

        HashSet<Vector2Int> affectedCells = new();
        context.affectedAreaCells.Clear();

        if (TargetingRule.origin == TargetOrigin.Point && selectedPoints != null && selectedPoints.Count > 0)
        {
            foreach (var point in selectedPoints)
            {
                foreach (var cell in AreaResolver.ResolveCells(point, AreaPattern, PreferredDirection))
                    affectedCells.Add(cell);
            }
        }
        else
        {
            foreach (var target in targets)
            {
                foreach (var cell in AreaResolver.ResolveCells(target.GridPosition, AreaPattern, PreferredDirection))
                    affectedCells.Add(cell);
            }
        }

        context.affectedAreaCells.AddRange(affectedCells);

        List<Unit> affectedUnits = new List<Unit>(Object.FindObjectsByType<Unit>(FindObjectsSortMode.None))
            .Where(u => affectedCells.Contains(u.GridPosition))
            .Where(u => TargetingRule.canTargetSelf || u != unit)
            .Distinct()
            .ToList();

        Debug.Log($"[AttackAction] Área aplicada | Células: {affectedCells.Count} | Units afetadas: {affectedUnits.Count}");
        return affectedUnits;
    }

}
