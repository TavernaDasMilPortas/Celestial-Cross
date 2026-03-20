using UnityEngine;
using System.Collections.Generic;

namespace CelestialCross.Combat
{
    [CreateAssetMenu(fileName = "New Weaver Condition", menuName = "Combat/Weaver Condition")]
    public class WeaverConditionData : ScriptableObject
    {
        public string displayName;
        public int duration = 3;
        public Sprite icon;

        [Header("Efeitos")]
        [SerializeReference] public List<AbilityEffectBase> onApplyEffects = new();
        [SerializeReference] public List<AbilityEffectBase> tickEffects = new();
        [SerializeReference] public List<AbilityEffectBase> onExpireEffects = new();

        public void ExecuteTick(CombatContext context)
        {
            foreach (var effect in tickEffects)
                effect?.Execute(context);
        }
    }
}
