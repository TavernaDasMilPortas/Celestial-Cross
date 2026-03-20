using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Combat;

/// <summary>
/// Uma ação versátil que executa uma lista de efeitos em sequência nos alvos selecionados.
/// </summary>
public class CompositeUnitAction : UnitActionBase
{
    private CompositeActionData compositeData;

    public void SetData(CompositeActionData data)
    {
        compositeData = data;
        ActionName = data.actionName;
        ActionIcon = data.actionIcon;
        ActionDescription = data.description;
        MarkConfigured();
    }

    protected override ActionContext CreateContext()
    {
        return new ActionContext(unit);
    }

    protected override void OnEnter()
    {
        // Usa o range definido no data. Por agora, fixo ou vindo do seletor.
        StartTargetSelection(5); 
    }

    protected override void OnUpdate() { }

    protected override void Resolve()
    {
        if (context.targets == null || compositeData == null) return;

        foreach (var target in context.targets)
        {
            CombatContext combatCtx = new CombatContext(unit, target, 0, this);
            
            foreach (var effect in compositeData.effects)
            {
                effect?.Execute(combatCtx);
            }
        }
    }

    protected override void OnCancel() { }
}
