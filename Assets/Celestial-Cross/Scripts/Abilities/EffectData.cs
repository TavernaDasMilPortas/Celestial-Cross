using System;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities
{
    [Serializable]
    public abstract class EffectData
    {
        public abstract void Execute(CombatContext context);
    }
}
