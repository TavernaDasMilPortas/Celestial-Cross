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
            $"[AttackAction] {unit.DisplayName} | Range {Range} | Damage {Damage}"
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

            Debug.Log(
                $"[AttackAction] {unit.DisplayName} causa {Damage} de dano em {target.DisplayName}"
            );

            target.Health.TakeDamage(Damage);
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
