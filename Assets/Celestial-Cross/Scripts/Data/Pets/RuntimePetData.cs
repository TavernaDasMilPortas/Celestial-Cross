namespace CelestialCross.Data.Pets
{
    [global::System.Serializable]
    public class RuntimePetData
    {
        public string UUID; // O ID único que diferencia os múltiplos Poring
        public string SpeciesID; // Referência ao PetSpeciesSO.id
        public string SpeciesName; // Nome legível salvo no momento do drop

        public int RarityStars; 
        public int CurrentLevel;
        
        // Status Finais Aleatorizados (Inteiros p evitar quebrados na UI/DB)
        public int Health;
        public int Attack;
        public int Defense;
        public int Speed;
        public int CriticalChance;
        public int EffectAccuracy;

        public string DisplayName => string.IsNullOrWhiteSpace(SpeciesName) ? $"Espécie {SpeciesID}" : SpeciesName;

        public RuntimePetData() {}

        public RuntimePetData(string speciesID, string speciesName, int stars, int hp, int atk, int def, int spd, int crit, int acc)
        {
            UUID = global::System.Guid.NewGuid().ToString();
            SpeciesID = speciesID;
            SpeciesName = string.IsNullOrWhiteSpace(speciesName) ? speciesID : speciesName;
            RarityStars = stars;
            CurrentLevel = 1;
            Health = hp;
            Attack = atk;
            Defense = def;
            Speed = spd;
            CriticalChance = crit;
            EffectAccuracy = acc;
        }
    }
}
