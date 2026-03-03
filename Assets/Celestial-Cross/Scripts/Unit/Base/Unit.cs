using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(UnitHoverDetector))]
[RequireComponent(typeof(UnitOutlineController))]
public abstract class Unit : MonoBehaviour
{
    [Header("Base Data")]
    [SerializeField] private UnitData unitData;

    [Header("Runtime")]
    public Vector2Int GridPosition;

    // =========================
    // PROPERTIES
    // =========================

    public UnitData Data => unitData;

    public string DisplayName =>
        unitData != null ? unitData.displayName : name;

    public int Speed =>
        unitData != null ? unitData.speed : 0;

    public int MaxHealth =>
        unitData != null ? unitData.maxHealth : 0;

    public Health Health { get; private set; }

    // =========================
    // ACTIONS
    // =========================

    protected List<IUnitAction> actions = new();
    protected IUnitAction currentAction;

    // =========================
    // UNITY
    // =========================

    protected virtual void Awake()
    {
        Health = GetComponent<Health>();

        if (Health != null && unitData != null)
            Health.SetMaxHealth(unitData.maxHealth);

        SetupActionsFromData();
    }

    // =========================
    // ACTION SETUP
    // =========================

    void SetupActionsFromData()
    {
        if (unitData == null)
        {
            Debug.LogError($"[Unit] {name} não possui UnitData.");
            return;
        }

        actions.Clear();

        foreach (var action in GetComponents<IUnitAction>())
            Destroy(action as Component);

        Debug.Log($"[Unit] Criando ações para {DisplayName}");

        foreach (var actionData in unitData.actions)
        {
            if (actionData == null)
                continue;

            System.Type actionType = actionData.GetRuntimeActionType();
            if (actionType == null)
            {
                Debug.LogError("[Unit] ActionData sem RuntimeActionType.");
                continue;
            }

            var action = gameObject.AddComponent(actionType) as IUnitAction;
            if (action == null)
            {
                Debug.LogError($"[Unit] Falha ao criar {actionType.Name}");
                continue;
            }

            actionData.Configure(action);
            actions.Add(action);

            Debug.Log($"[Unit] Action adicionada: {actionType.Name}");
        }
    }

    // =========================
    // ACTION FLOW
    // =========================

    public void SelectAction(int index)
    {
        if (index < 0 || index >= actions.Count)
            return;

        currentAction?.Cancel();
        currentAction = actions[index];
        currentAction.EnterAction();
    }

    public void UpdateAction()
    {
        currentAction?.UpdateAction();
    }

    public void ConfirmAction()
    {
        currentAction?.Confirm();
    }

    public void CancelAction()
    {
        currentAction?.Cancel();
    }

    // =========================
    // FEEDBACK
    // =========================

    public void LogCanConfirm(bool canConfirm)
    {
        Debug.Log(
            canConfirm
                ? $"[{DisplayName}] Pronto para confirmar (ENTER)"
                : $"[{DisplayName}] Selecione alvo"
        );
    }
}
