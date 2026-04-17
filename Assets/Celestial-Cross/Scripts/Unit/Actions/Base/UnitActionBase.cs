using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum UnitActionCategory
{
    Attack,       // Ataque
    Movement,     // Deslocamento
    Ability,      // Habilidade
    Spell         // Magia
}

public abstract class UnitActionBase : MonoBehaviour, IUnitAction
{
    protected Unit unit;
    protected ActionState state = ActionState.Idle;
    protected ActionContext context;
    protected TargetSelector targetSelector;

    public string ActionName { get; set; }
    public UnitActionCategory ActionCategory { get; set; } = UnitActionCategory.Ability; // Categoria da ação
    
    [Tooltip("Se verdadeiro, esta ação pula as regras de peso padrão e tem prioridade máxima (Ex: Especial de Chefe).")]
    public bool IsAbsolutePriority { get; set; }

    public Sprite ActionIcon { get; set; }
    public string ActionDescription { get; set; }
    public virtual int Range { get; set; }
    public Vector2Int Target { get; set; }
    public virtual AreaPatternData GetAreaPattern() => null;
    public virtual string GetDetailStats() => "";
    public event System.Action<ActionForecast> OnForecastUpdated;

    protected void InvokeForecastUpdated(ActionForecast forecast)
    {
        OnForecastUpdated?.Invoke(forecast);
    }

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

    protected void StartTargetSelection(int range, TargetingRuleData rule = null)
    {
        // Se já existir um seletor (por exemplo, após um swap), limpamos o anterior
        if (targetSelector != null) Destroy(targetSelector);

        targetSelector = gameObject.AddComponent<TargetSelector>();
        targetSelector.OnTargetsConfirmed += OnTargetsConfirmed;
        targetSelector.OnCanceled += OnSelectionCanceled;
        targetSelector.Begin(unit, range, rule);
    }

    protected virtual void OnTargetsConfirmed(List<Unit> targets)
    {
        context.targets = targets;
        state = ActionState.ReadyToConfirm;
        unit.LogCanConfirm(true);
        PerformFinalExecution();
    }

    protected virtual void OnSelectionCanceled()
    {
        state = ActionState.Finished;
        unit.LogCanConfirm(false);
    }

    protected void PerformFinalExecution()
    {
        if (state != ActionState.ReadyToConfirm) return;
        StartCoroutine(ExecuteRoutine());
    }

    private IEnumerator ExecuteRoutine()
    {
        state = ActionState.Resolving;

        // Limpamos o Overlay chamativo (Amarelo) e pegamos a área final
        HashSet<Vector2Int> finalArea = new HashSet<Vector2Int>();
        if (targetSelector != null)
        {
            finalArea = targetSelector.GetFinalTargetArea();
            targetSelector.ClearAllHighlights(); 
        }

        // Feedback Visual: Darken os Tiles finais
        foreach (var pos in finalArea)
        {
            GridMap.Instance?.GetTile(pos)?.Darken();
        }

        yield return new WaitForSeconds(0.5f);

        Execute();

        if (targetSelector != null)
        {
            Destroy(targetSelector);
        }

        GridMap.Instance?.ResetAllTileVisuals();
    }

    public void Confirm()
    {
        PerformFinalExecution();
    }

    public void Execute()
    {
        Resolve();
        state = ActionState.Finished;

        // Dispara o hook OnAfterAction no PassiveManager da unidade
        var passiveManager = unit.GetComponent<PassiveManager>();
        if (passiveManager != null)
        {
            var combatContext = new CelestialCross.Combat.CombatContext(unit, unit, 0, this);
            passiveManager.TriggerHook(CelestialCross.Combat.CombatHook.OnAfterAction, combatContext);
        }

        OnActionFinished();
    }

    protected virtual void OnActionFinished()
    {
        CameraController.Instance?.ResetFocus();

        if (unit is EnemyUnit)
            TurnManager.Instance.EndTurn();
        else
            PlayerController.Instance.EndTurn();
    }

    public void Cancel()
    {
        OnCancel();
        
        if (targetSelector != null)
        {
            targetSelector.ClearAllHighlights();
            Destroy(targetSelector);
        }

        GridMap.Instance?.ResetAllTileVisuals();
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
