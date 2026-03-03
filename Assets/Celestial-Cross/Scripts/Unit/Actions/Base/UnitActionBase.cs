using UnityEngine;

public abstract class UnitActionBase : MonoBehaviour, IUnitAction
{
    protected Unit unit;
    protected ActionState state = ActionState.Idle;
    protected ActionContext context;

    bool configured;

    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
    }

    // =========================
    // CONFIG
    // =========================

    public void MarkConfigured()
    {
        configured = true;
    }

    // =========================
    // IUnitAction (FINAL)
    // =========================

    public void EnterAction()
    {
        if (!configured || unit == null)
            return;

        context = CreateContext();
        state = ActionState.SelectingTargets;

        OnEnter();
    }

    public void UpdateAction()
    {
        if (state == ActionState.Idle || state == ActionState.Finished)
            return;

        OnUpdate();
    }

    public void Confirm()
    {
        if (state != ActionState.ReadyToConfirm)
            return;

        state = ActionState.Resolving;
        Resolve();
        state = ActionState.Finished;

        PlayerController.Instance.EndTurn();
    }

    public void Cancel()
    {
        OnCancel();
        state = ActionState.Finished;
    }

    // =========================
    // EXTENSÃO
    // =========================

    protected abstract ActionContext CreateContext();
    protected abstract void OnEnter();
    protected abstract void OnUpdate();
    protected abstract void Resolve();
    protected abstract void OnCancel();
}
