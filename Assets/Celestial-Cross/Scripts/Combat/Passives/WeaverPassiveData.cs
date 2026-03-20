using UnityEngine;
using System.Collections.Generic;

namespace CelestialCross.Combat
{
    [CreateAssetMenu(fileName = "New Weaver Passive", menuName = "Combat/Weaver Passive")]
    public class WeaverPassiveData : ScriptableObject
    {
        public string displayName;
        public CombatHook trigger;
        
        [SerializeReference]
        public List<AbilityEffectBase> effects = new();

        public void Execute(CombatContext context)
        {
            Debug.Log($"[PassiveAbilityData] Disparando: {displayName}");
            foreach (var effect in effects)
            {
                effect?.Execute(context);
            }
        }
    }
}
