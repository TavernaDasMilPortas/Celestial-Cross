using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AttackAction : UnitActionBase
{
    public override int Range { get; set; }
    public int Damage { get; set; }
    public override string GetDetailStats() => $"Dano: {Damage}";
    public TargetingRuleData TargetingRule { get; set; } = new TargetingRuleData();
    public AreaPatternData AreaPattern { get; set; }
    public int AreaRotationSteps { get; set; }


    protected override ActionContext CreateContext()
    {
        return new ActionContext(unit);
    }

    protected override void OnEnter()
    {
        Debug.Log($"[AttackAction] {unit.DisplayName} | Range {Range} | Flat Bonus {Damage} | Mode {TargetingRule.mode}");

        StartTargetSelection(Range, TargetingRule);
        
        // Configuração adicional do seletor (Área)
        targetSelector.OnSelectedTargetsChanged += OnSelectionChanged;
        targetSelector.UpdateAreaConfig(AreaPattern, AreaRotationSteps);

        unit.LogCanConfirm(false);
    }

    protected override void OnUpdate()
    {
        // input é tratado pelo TargetSelector
    }

    protected override void Resolve()
    {
        foreach (var target in context.targets)
        {
            if (target == null || target.Health == null)
                continue;

            int hits = unit.GetAttacksAgainst(target);
            int totalDamage = 0;

            bool anyCrit = false;
            for (int i = 0; i < hits; i++)
            {
                AttackResult result = unit.CalculateAttack(
                    target,
                    new DamageBonus { flat = Damage, percent = 0f },
                    new DamageReduction { flat = 0, percent = 0f }
                );

                totalDamage += result.damage;
                if (result.isCritical) anyCrit = true;
            }

            target.Health.TakeDamage(totalDamage, anyCrit);
        }
    }

    protected override void OnCancel()
    {
        // Limpeza básica via UnitActionBase
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

        AttackResult sample = unit.CalculateAttack(
            lastTarget,
            new DamageBonus { flat = Damage, percent = 0f },
            new DamageReduction { flat = 0, percent = 0f }
        );

        ActionForecast forecast = new ActionForecast
        {
            Source = unit,
            Target = lastTarget,
            Damage = sample.damage,
            IsCritical = sample.isCritical,
            AttackCount = unit.GetAttacksAgainst(lastTarget),
            CriticalChance = unit.Stats.criticalChance // Simple display
        };

        InvokeForecastUpdated(forecast);
    }

    protected override void OnTargetsConfirmed(List<Unit> targets)
    {
        context.targets = ExpandAreaTargetsIfNeeded(targets, targetSelector != null ? targetSelector.SelectedPoints : null);
        state = ActionState.ReadyToConfirm;

        Debug.Log($"[AttackAction] Alvos confirmados: {context.targets.Count}");

        unit.LogCanConfirm(true);
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
                foreach (var cell in AreaResolver.ResolveCells(point, AreaPattern, AreaRotationSteps))
                    affectedCells.Add(cell);
            }
        }
        else
        {
            foreach (var target in targets)
            {
                foreach (var cell in AreaResolver.ResolveCells(target.GridPosition, AreaPattern, AreaRotationSteps))
                    affectedCells.Add(cell);
            }
        }

        context.affectedAreaCells.AddRange(affectedCells);
        
        List<Unit> affectedUnits = FindObjectsOfType<Unit>()
            .Where(u => affectedCells.Contains(u.GridPosition))
            .Where(u => TargetingRule.canTargetSelf || u != unit)
            .Distinct()
            .ToList();

        Debug.Log($"[AttackAction] Área aplicada | Células: {affectedCells.Count} | Units afetadas: {affectedUnits.Count}");
        return affectedUnits;
    }

}
