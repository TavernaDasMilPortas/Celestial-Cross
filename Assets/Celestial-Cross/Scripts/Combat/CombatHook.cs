using UnityEngine;

namespace CelestialCross.Combat
{
    public enum CombatHook
    {
        OnManualCast,           // Executado quando o jogador/IA seleciona e usa a habilidade ativamente no turno
        
        OnRoundStart,
        OnRoundEnd,
        OnTurnStart,
        OnTurnEnd,
        OnBeforeAction,         
        OnAfterAction,          
        OnBeforeTakeDamage,
        OnAfterTakeDamage,
        OnBeforeDealDamage,
        OnAfterDealDamage,
        OnBeforeTakeHeal,       
        OnAfterTakeHeal,
        OnBeforeDealHeal,       
        OnAfterDealHeal,
        OnBeforeApplyCondition, 
        OnAfterApplyCondition,  
        OnBeforeRemoveCondition,
        OnAfterRemoveCondition,
        OnDeath,
        OnKill,
        OnMoveStart,
        OnMoveEnd
    }

    public class CombatContext
    {
        public Unit source;      
        public Unit target;      
        public global::System.Collections.Generic.List<Unit> targets;
        public int amount;       
        public bool isCritical;
        public IUnitAction action; 

        public int abilityLevel = 1;
        public global::System.Collections.Generic.Dictionary<string, float> Variables;
        public global::System.Collections.Generic.Dictionary<string, int> loopCounters;
        public global::System.Collections.Generic.List<Celestial_Cross.Scripts.Abilities.Conditions.AbilityConditionData> conditionPool;

        public CombatContext(Unit source, Unit target = null, int amount = 0, IUnitAction action = null)
        {
            this.source = source;
            this.target = target;
            this.targets = new global::System.Collections.Generic.List<Unit>();
            if (target != null) this.targets.Add(target);
            this.amount = amount;
            this.action = action;
            this.Variables = new global::System.Collections.Generic.Dictionary<string, float>();
            this.loopCounters = new global::System.Collections.Generic.Dictionary<string, int>();
            this.conditionPool = new global::System.Collections.Generic.List<Celestial_Cross.Scripts.Abilities.Conditions.AbilityConditionData>();
        }
    }
}
