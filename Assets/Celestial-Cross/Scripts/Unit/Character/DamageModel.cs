using UnityEngine;

[System.Serializable]
public struct DamageBonus
{
    public int flat;
    public float percent;

    public static DamageBonus Combine(DamageBonus a, DamageBonus b)
    {
        return new DamageBonus
        {
            flat = a.flat + b.flat,
            percent = a.percent + b.percent,
        };
    }

    public int Evaluate(int baseAttack)
    {
        return flat + Mathf.RoundToInt(baseAttack * Mathf.Max(0f, percent));
    }
}

[System.Serializable]
public struct DamageReduction
{
    public int flat;
    public float percent;

    public static DamageReduction Combine(DamageReduction a, DamageReduction b)
    {
        return new DamageReduction
        {
            flat = a.flat + b.flat,
            percent = a.percent + b.percent,
        };
    }

    public int Evaluate(int incomingDamage)
    {
        return flat + Mathf.RoundToInt(incomingDamage * Mathf.Max(0f, percent));
    }
}

public readonly struct AttackResult
{
    public readonly int damage;
    public readonly bool isCritical;

    public AttackResult(int damage, bool isCritical)
    {
        this.damage = Mathf.Max(1, damage);
        this.isCritical = isCritical;
    }
}

public static class DamageModel
{
    public static AttackResult ResolveHit(
        CombatStats attacker,
        CombatStats defender,
        DamageBonus bonus,
        DamageReduction reduction
    )
    {
        int raw = attacker.attack + bonus.Evaluate(attacker.attack) - defender.defense;
        int mitigated = raw - reduction.Evaluate(Mathf.Max(1, raw));
        bool isCritical = Random.Range(0, 100) < Mathf.Clamp(attacker.criticalChance, 0, 100);

        if (isCritical)
            mitigated *= 2;

        return new AttackResult(Mathf.Max(1, mitigated), isCritical);
    }

    public static int GetAttackCountBySpeed(CombatStats attacker, CombatStats defender)
    {
        return attacker.speed >= defender.speed + 10 ? 2 : 1;
    }
}
