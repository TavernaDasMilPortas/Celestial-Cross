using System;

[Serializable]
public abstract class UnitActionData : IExecutableDefinitionData
{
    public string actionName;

    public string DefinitionName => actionName;

    // Cada ação sabe qual runtime action cria
    public abstract Type GetRuntimeActionType();

    // Hook para passar dados para o runtime action
    public abstract void Configure(IUnitAction action);
}
