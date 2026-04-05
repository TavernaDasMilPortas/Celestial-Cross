using UnityEngine;

namespace CelestialCross.Artifacts
{
    public enum ArtifactType
    {
        Helmet,     // Slot 1
        Chestplate, // Slot 2
        Gloves,     // Slot 3
        Boots,      // Slot 4
        Necklace,   // Slot 5
        Ring        // Slot 6
    }

    public enum ArtifactRarity
    {
        Common,     // Geralmente 0 a 1 substat inicial
        Uncommon,   // 1 a 2 substats iniciais
        Rare,       // 2 a 3 substats iniciais
        Epic,       // 3 substats iniciais
        Legendary   // 4 substats garantidos iniciais
    }

    public enum StatType
    {
        HealthFlat,
        HealthPercent,
        AttackFlat,
        AttackPercent,
        DefenseFlat,
        DefensePercent,
        Speed,
        CriticalRate,
        CriticalDamage,
        EffectResistance,
        EffectHitRate
    }
}
