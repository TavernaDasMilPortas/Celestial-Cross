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
        public int amount;       
        public bool isCritical;
        public IUnitAction action; 

        public System.Collections.Generic.Dictionary<string, float> Variables;

        public CombatContext(Unit source, Unit target = null, int amount = 0, IUnitAction action = null)
        {
            this.source = source;
            this.target = target;
            this.amount = amount;
            this.action = action;
            this.Variables = new System.Collections.Generic.Dictionary<string, float>();
        }
    }
}
