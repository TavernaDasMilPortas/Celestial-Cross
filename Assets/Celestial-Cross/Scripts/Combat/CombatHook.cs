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

    /// <summary>
    /// Tipos de seleção de alvos para efeitos do Weaver System.
    /// </summary>
    public enum EffectTargetType
    {
        Default,            // Mantém o alvo do contexto (ex: quem sofreu o dano)
        Self,               // A própria unidade (source)
        AllEnemies,         // Todos os inimigos do dono da passiva
        AllAllies,          // Todos os aliados do dono da passiva
        EnemyMostHP,        // Inimigo com mais HP
        EnemyLeastHP,       // Inimigo com menos HP
        AllyMostHP,         // Aliado com mais HP
        AllyLeastHP,        // Aliado com menos HP
        NearestEnemy,       // Inimigo mais próximo
        FarthestEnemy,      // Inimigo mais distante
        NearestAlly,        // Aliado mais próximo
        FarthestAlly        // Aliado mais distante
    }
}
