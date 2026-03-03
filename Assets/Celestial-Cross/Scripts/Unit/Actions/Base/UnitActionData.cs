using System;

[System.Serializable]
public abstract class UnitActionData
{
    public string actionName;

    /// Cada ação sabe qual runtime action cria
    public abstract System.Type GetRuntimeActionType();

    /// Hook para passar dados para o runtime action
    public abstract void Configure(IUnitAction action);
}