using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Combat;

[CreateAssetMenu(fileName = "New Composite Action", menuName = "Units/Actions/Composite Action")]
public class CompositeActionData : UnitActionData
{
    [SerializeReference]
    public List<AbilityEffectBase> effects = new();

    public override System.Type GetRuntimeActionType()
    {
        return typeof(CompositeUnitAction);
    }

    public override void Configure(IUnitAction action)
    {
        if (action is CompositeUnitAction compositeAction)
        {
            compositeAction.SetData(this);
        }
    }
}
