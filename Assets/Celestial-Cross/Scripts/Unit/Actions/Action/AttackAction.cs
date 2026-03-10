using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AttackAction : UnitActionBase, IRangeConfigurable
{
    public int Range { get; set; }
    public int Damage { get; set; }
    public TargetingRuleData TargetingRule { get; set; } = new TargetingRuleData();
    public AreaPatternData AreaPattern { get; set; }
    public int AreaRotationSteps { get; set; }

    TargetSelector targetSelector;

    protected override ActionContext CreateContext()
    {
        return new ActionContext(unit);
    }

    protected override void OnEnter()
    {
        Debug.Log($"[AttackAction] {unit.DisplayName} | Range {Range} | Flat Bonus {Damage} | Mode {TargetingRule.mode}");

        StartTargetSelection();
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
            if (target.Health == null)
                continue;

            int hits = unit.GetAttacksAgainst(target);
            int totalDamage = 0;

            for (int i = 0; i < hits; i++)
            {
                AttackResult result = unit.CalculateAttack(
                    target,
                    new DamageBonus { flat = Damage, percent = 0f },
                    new DamageReduction { flat = 0, percent = 0f }
                );

                totalDamage += result.damage;

                Debug.Log($"[AttackAction] Hit {i + 1}/{hits} | {unit.DisplayName} -> {target.DisplayName} | Damage: {result.damage} | Critical: {result.isCritical}");
            }

            target.Health.TakeDamage(totalDamage);
        }

        ClearSelection();
    }

    protected override void OnCancel()
    {
        ClearSelection();
    }

    void StartTargetSelection()
    {
        targetSelector = gameObject.AddComponent<TargetSelector>();

        targetSelector.Begin(
            sourceUnit: unit,
            selectionRange: Range,
            rule: TargetingRule,
            selectedAreaPattern: AreaPattern,
            selectedAreaRotationSteps: AreaRotationSteps
        );

        targetSelector.OnTargetsConfirmed += OnTargetsConfirmed;
        targetSelector.OnCanceled += OnSelectionCanceled;

        state = ActionState.SelectingTargets;
    }

    void OnTargetsConfirmed(List<Unit> targets)
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

        if ((TargetingRule.mode != TargetingMode.AreaFromTarget && TargetingRule.mode != TargetingMode.AreaFromPoint) || AreaPattern == null)
            return targets;

        HashSet<Vector2Int> affectedCells = new();

        if (TargetingRule.mode == TargetingMode.AreaFromPoint && selectedPoints != null && selectedPoints.Count > 0)
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

        List<Unit> affectedUnits = FindObjectsOfType<Unit>()
            .Where(u => affectedCells.Contains(u.GridPosition))
            .Where(u => TargetingRule.canTargetSelf || u != unit)
            .Distinct()
            .ToList();

        Debug.Log($"[AttackAction] Área aplicada | Células: {affectedCells.Count} | Units afetadas: {affectedUnits.Count}");
        return affectedUnits;
    }

    void OnSelectionCanceled()
    {
        state = ActionState.Finished;
        unit.LogCanConfirm(false);
    }

    void ClearSelection()
    {
        if (targetSelector != null)
        {
            targetSelector.OnTargetsConfirmed -= OnTargetsConfirmed;
            targetSelector.OnCanceled -= OnSelectionCanceled;
            Destroy(targetSelector);
        }

        targetSelector = null;
    }
}
