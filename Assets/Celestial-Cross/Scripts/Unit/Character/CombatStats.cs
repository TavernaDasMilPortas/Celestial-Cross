using UnityEngine;

[System.Serializable]
public struct CombatStats
{
    [Min(1)] public int health;
    public int attack;
    public int defense;
    public int speed;
    [Range(0, 100)] public int criticalChance;
    [Range(0, 100)] public int effectAccuracy;

    public CombatStats(int health, int attack, int defense, int speed, int criticalChance, int effectAccuracy)
    {
        this.health = Mathf.Max(1, health);
        this.attack = attack;
        this.defense = defense;
        this.speed = speed;
        this.criticalChance = Mathf.Clamp(criticalChance, 0, 100);
        this.effectAccuracy = Mathf.Clamp(effectAccuracy, 0, 100);
    }

    public static CombatStats operator +(CombatStats a, CombatStats b)
    {
        return new CombatStats(
            a.health + b.health,
            a.attack + b.attack,
            a.defense + b.defense,
            a.speed + b.speed,
            a.criticalChance + b.criticalChance,
            a.effectAccuracy + b.effectAccuracy
        );
    }

    public static CombatStats Zero => new CombatStats(1, 0, 0, 0, 0, 0);
}
