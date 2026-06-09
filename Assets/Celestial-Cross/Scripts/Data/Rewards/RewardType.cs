namespace CelestialCross.Data.Rewards
{
    public enum RewardType
    {
        Money,
        Energy, 
        Stardust,
        StarMaps,
        XP,
        Unit,
        Pet,
        Artifact,
        Item,        // Inclui convites (convite_leidell, convite_generico, etc.)
        LootTable    // Referência a uma BaseLootTable para geração procedural
    }
}
