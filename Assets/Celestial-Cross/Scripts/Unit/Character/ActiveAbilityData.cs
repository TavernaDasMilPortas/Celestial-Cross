using System;
using UnityEngine;

[Serializable]
public class ActiveAbilityData : IExecutableDefinitionData
{
    public string actionName = "Active Ability";

    [SerializeReference]
    public UnitActionData actionDefinition;

    public int level = 1;

    public string DefinitionName => actionName;

    public Type GetRuntimeActionType()
    {
        return actionDefinition != null ? actionDefinition.GetRuntimeActionType() : null;
    }

    public void Configure(IUnitAction action)
    {
        if (actionDefinition == null)
        {
            Debug.LogError("[ActiveAbilityData] actionDefinition não configurada.");
            return;
        }

        action.Level = level;
        actionDefinition.Configure(action);
    }
}
