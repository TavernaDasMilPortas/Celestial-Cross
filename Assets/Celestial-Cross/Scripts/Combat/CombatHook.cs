using UnityEngine;

namespace CelestialCross.Combat
{
    /// <summary>
    /// Todos os pontos de intercepção possíveis no pipeline de combate.
    /// </summary>
    public enum CombatHook
    {
        // Nível de Rodada (Round)
        OnRoundStart,
        OnRoundEnd,

        // Nível de Turno (Turn)
        OnTurnStart,
        OnTurnEnd,

        // Nível de Ação (Action)
        OnBeforeAction,         // Antes de iniciar animação/efeitos da ação
        OnAfterAction,          // Após a ação ser totalmente resolvida

        // Nível de Dano (Damage)
        OnBeforeTakeDamage,
        OnAfterTakeDamage,
        OnBeforeDealDamage,
        OnAfterDealDamage,
        
        // Nível de Cura (Heal)
        OnBeforeTakeHeal,       // Recebendo cura
        OnAfterTakeHeal,
        OnBeforeDealHeal,       // Causando cura (ex: aumentar cura feita)
        OnAfterDealHeal,

        // Nível de Condição (Status Effects)
        OnBeforeApplyCondition, // Chance de resistir
        OnAfterApplyCondition,  // Reações a buffs/debuffs
        OnBeforeRemoveCondition,
        OnAfterRemoveCondition,

        // Ciclo de Vida
        OnDeath,
        OnKill,
        
        // Movimentação
        OnMoveStart,
        OnMoveEnd
    }

    /// <summary>
    /// Dados necessários para que o efeito saiba quem, onde e quanto.
    /// </summary>
    public class CombatContext
    {
        public Unit source;      // Quem disparou o evento
        public Unit target;      // Quem está sofrendo o evento (se houver)
        public int amount;       // Valor de dano/cura (se houver)
        public bool isCritical;
        public IUnitAction action; // Ação que originou o evento

        public CombatContext(Unit source, Unit target = null, int amount = 0, IUnitAction action = null)
        {
            this.source = source;
            this.target = target;
            this.amount = amount;
            this.action = action;
        }
    }
}
