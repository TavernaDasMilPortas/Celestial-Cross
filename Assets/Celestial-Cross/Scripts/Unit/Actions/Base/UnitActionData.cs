using System;
using UnityEngine;

[Serializable]
public abstract class UnitActionData : IExecutableDefinitionData
{
    public string actionName;
    public Sprite actionIcon;
    [TextArea] public string description;

    public string DefinitionName => actionName;

    // Cada ação sabe qual runtime action cria
    public abstract Type GetRuntimeActionType();

    // Hook para passar dados para o runtime action
    public abstract void Configure(IUnitAction action);

    protected void ConfigureBase(UnitActionBase actionBase)
    {
        actionBase.ActionName = actionName;
        actionBase.ActionIcon = actionIcon;
        actionBase.ActionDescription = description;
    }
}
