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

    // Stars (1..6) define scaling and stat value ranges.
    // Explicit values keep compatibility with prior int-based serialization.
    public enum ArtifactStars
    {
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6
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
