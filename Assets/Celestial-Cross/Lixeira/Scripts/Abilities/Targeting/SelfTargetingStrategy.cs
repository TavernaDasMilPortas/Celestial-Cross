using System;
using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Targeting
{
    [Serializable]
    [System.Obsolete("Use AutoTargetingStrategy com 'Self' ou 'MainTarget'.")]
    public class SelfTargetingStrategy : TargetingStrategyData
    {
        public SelfTargetingStrategy()
        {
            RequiresManualSelection = false;
        }

        public override List<Unit> GetTargets(CombatContext context)
        {
            // O alvo é o próprio conjurador
            return new List<Unit> { context.source };
        }
    }
}
