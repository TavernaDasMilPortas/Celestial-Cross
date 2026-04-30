using System.Collections.Generic;
using UnityEngine;

namespace Celestial_Cross.Scripts.Units.Enemy.AI
{
    public class AICooldownTracker
    {
        // Dictionary mapping action unique identifier (ActionName) to remaining turns
        private Dictionary<string, int> cooldowns = new();

        public void UseAbility(string actionName, int cooldownTurns)
        {
            if (cooldownTurns <= 0) return;
            cooldowns[actionName] = cooldownTurns;
        }

        public bool IsOnCooldown(string actionName)
        {
            return cooldowns.ContainsKey(actionName) && cooldowns[actionName] > 0;
        }

        /// <summary>
        /// Ticks all cooldowns. Should be called at the end of the unit's turn.
        /// </summary>
        public void TickCooldowns()
        {
            List<string> keys = new List<string>(cooldowns.Keys);
            foreach (var key in keys)
            {
                if (cooldowns[key] > 0)
                {
                    cooldowns[key]--;
                    if (cooldowns[key] <= 0)
                    {
                        cooldowns.Remove(key);
                    }
                }
            }
        }

        public void Clear()
        {
            cooldowns.Clear();
        }
    }
}
