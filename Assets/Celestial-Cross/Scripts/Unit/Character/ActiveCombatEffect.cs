using UnityEngine;

[System.Serializable]
public class ActiveCombatEffect
{
    public string effectName;

    [Header("Sempre ativo")]
    public DamageBonus persistentDamageBonus;
    public DamageReduction persistentDamageReduction;

    [Header("Ao iniciar combate")]
    public DamageBonus combatStartDamageBonus;
    public DamageReduction combatStartDamageReduction;

    [Header("Ao receber ataque")]
    public DamageReduction onReceiveAttackReduction;

    bool combatStarted;

    public void TriggerCombatStart()
    {
        combatStarted = true;
    }

    public DamageBonus GetOutgoingDamageBonus()
    {
        DamageBonus total = persistentDamageBonus;

        if (combatStarted)
            total = DamageBonus.Combine(total, combatStartDamageBonus);

        return total;
    }

    public DamageReduction GetIncomingReduction(bool isReceivingAttack)
    {
        DamageReduction total = persistentDamageReduction;

        if (combatStarted)
            total = DamageReduction.Combine(total, combatStartDamageReduction);

        if (isReceivingAttack)
            total = DamageReduction.Combine(total, onReceiveAttackReduction);

        return total;
    }
}
