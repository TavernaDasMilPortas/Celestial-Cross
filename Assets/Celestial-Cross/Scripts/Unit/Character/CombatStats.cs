using UnityEngine;

[System.Serializable]
public struct CombatStats
{
    [Min(1)] public int health;
    public int attack;
    public int defense;
    public int speed;
    [Range(0, 100)] public int criticalChance;
    public int criticalDamage;        // Base: 50 (= 150% do dano em crit)
    [Range(0, 100)] public int effectAccuracy;
    [Range(0, 100)] public int effectResistance;  // Base: 0

    public CombatStats(int health, int attack, int defense, int speed, int criticalChance, int effectAccuracy, int criticalDamage = 50, int effectResistance = 0)
    {
        this.health = Mathf.Max(1, health);
        this.attack = attack;
        this.defense = defense;
        this.speed = speed;
        this.criticalChance = Mathf.Clamp(criticalChance, 0, 100);
        this.criticalDamage = criticalDamage;
        this.effectAccuracy = Mathf.Clamp(effectAccuracy, 0, 100);
        this.effectResistance = Mathf.Max(0, effectResistance);
    }

    public static CombatStats operator +(CombatStats a, CombatStats b)
    {
        return new CombatStats(
            a.health + b.health,
            a.attack + b.attack,
            a.defense + b.defense,
            a.speed + b.speed,
            a.criticalChance + b.criticalChance,
            a.effectAccuracy + b.effectAccuracy,
            a.criticalDamage + b.criticalDamage,
            a.effectResistance + b.effectResistance
        );
    }

    public static CombatStats Zero => new CombatStats(1, 0, 0, 0, 0, 0, 0, 0);
}
