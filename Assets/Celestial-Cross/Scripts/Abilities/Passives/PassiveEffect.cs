using CelestialCross.Combat;
using System;

namespace Celestial_Cross.Scripts.Abilities
{
    /// <summary>
    /// A base class for all passive effects (Inline Version).
    /// </summary>
    [Serializable]
    public abstract class PassiveEffect
    {
        /// <summary>
        /// The combat hook that this effect listens to.
        /// </summary>
        public CombatHook triggerHook;

        /// <summary>
        /// Executes the effect.
        /// </summary>
        /// <param name="context">The combat context.</param>
        public abstract void Execute(CombatContext context);
    }
}
