using UnityEngine;
using System.Collections.Generic;

public class AttackAction : UnitActionBase
{
    public int Range { get; set; }
    public int Damage { get; set; }

    TargetSelector targetSelector;

    protected override ActionContext CreateContext()
    {
        return new ActionContext(unit);
    }

    protected override void OnEnter()
    {
        Debug.Log(
            $"[AttackAction] {unit.DisplayName} | Range {Range} | Flat Bonus {Damage}"
        );

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

                Debug.Log(
                    $"[AttackAction] Hit {i + 1}/{hits} | {unit.DisplayName} -> {target.DisplayName} | Damage: {result.damage} | Critical: {result.isCritical}"
                );
            }

            target.Health.TakeDamage(totalDamage);
        }

        ClearSelection();
    }

    protected override void OnCancel()
    {
        ClearSelection();
    }

    // =========================
    // TARGET SELECTION
    // =========================

    void StartTargetSelection()
    {
        targetSelector = gameObject.AddComponent<TargetSelector>();

        targetSelector.Begin(
            sourceUnit: unit,
            selectionRange: Range,
            multipleTargets: false,
            canTargetSelf: false
        );

        targetSelector.OnTargetsConfirmed += OnTargetsConfirmed;
        targetSelector.OnCanceled += OnSelectionCanceled;

        state = ActionState.SelectingTargets;
    }

    void OnTargetsConfirmed(List<Unit> targets)
    {
        context.targets = targets;
        state = ActionState.ReadyToConfirm;

        Debug.Log(
            $"[AttackAction] Alvo confirmado: {targets[0].DisplayName}"
        );

        unit.LogCanConfirm(true);
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
