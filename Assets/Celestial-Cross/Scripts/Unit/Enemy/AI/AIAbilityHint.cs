using System;
using UnityEngine;

namespace Celestial_Cross.Scripts.Units.Enemy.AI
{
    [Serializable]
    public class AIAbilityHint
    {
        public enum AbilityCategory 
        { 
            Damage,     // Focus on dealing HP damage
            Heal,       // Focus on restoring HP to allies
            Buff,       // Focus on positive conditions
            Debuff,     // Focus on negative conditions
            Summon,     // Focus on adding entities to board
            Utility     // Other effects (displacement, mana restoration, etc.)
        }

        [Tooltip("The nature of this ability for AI scoring.")]
        public AbilityCategory category = AbilityCategory.Damage;

        [Tooltip("Base priority of this ability (0-100). Higher values means the AI is more likely to use it over regular attacks.")]
        [Range(0, 100)] public int basePriority = 50;

        [Tooltip("Cooldown in turns after use (AI only). 0 means no cooldown.")]
        public int cooldownTurns = 0;

        [Tooltip("Score bonus when the target has HP < 30%. Relevant for Damage (finishers) and Heal.")]
        public float lowHPTargetBonus = 20f;

        [Tooltip("Score bonus when applying a condition for the first time (target doesn't have it).")]
        public float freshApplicationBonus = 15f;

        [Tooltip("If true, the AI searches for allies (self/allies) instead of enemies.")]
        public bool targetsFriendlies = false;

        [Tooltip("Estimated numeric value for scaling (Damage amount, Heal power, etc.).")]
        public float estimatedValue = 10f;
    }
}
