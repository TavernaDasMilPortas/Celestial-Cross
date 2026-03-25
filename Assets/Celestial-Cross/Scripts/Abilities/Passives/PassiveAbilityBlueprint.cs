using UnityEngine;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities;

namespace Celestial_Cross.Scripts.Abilities
{
    /// <summary>
    /// A ScriptableObject that represents a passive ability.
    /// </summary>
    [CreateAssetMenu(fileName = "New Passive Ability", menuName = "Celestial-Cross/Abilities/Passive Ability")]
    public class PassiveAbilityBlueprint : AbilityBlueprint
    {
        /// <summary>
        /// The list of passive effects that this ability has.
        /// </summary>
        [SerializeReference]
        public List<PassiveEffect> passiveEffects = new List<PassiveEffect>();
    }
}
