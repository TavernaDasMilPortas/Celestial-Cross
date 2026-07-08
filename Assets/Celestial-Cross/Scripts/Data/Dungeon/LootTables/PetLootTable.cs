using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CelestialCross.Data.Pets;
using CelestialCross.Cloud;

namespace CelestialCross.Data.Dungeon
{
    [global::System.Serializable]
    public class PetDropMatrix
    {
        [Header("Chances de Estrelas (%)")]
        public float oneStarChance = 60f;
        public float twoStarChance = 30f;
        public float threeStarChance = 10f;
        public float fourStarChance = 0f;
        public float fiveStarChance = 0f;
    }

    [global::System.Serializable]
    public class PetDropEntry
    {
        public PetSpeciesSO Species;
        [Tooltip("Peso de chance de drop desta espécie em relação às outras da mesma lista")]
        public float Weight = 10f;
    }

    [global::System.Serializable]
    public class PetLootTable : CelestialCross.Data.Loot.BaseLootTable
    {
        [Header("Global Loot Pool da Masmorra ou Região (Pets Possíveis)")]
        public List<PetDropEntry> AllowedPets = new List<PetDropEntry>();

        [Header("Tabela de Chance Base")]
        public PetDropMatrix DropMatrix = new PetDropMatrix();

        [Header("Configurações Gerais")]
        [Tooltip("Quantidade a ser gerada por padrão")]
        public int NumberOfRolls = 1;

        public override async Task GenerateLootAsync(RuntimeReward rewardData)
        {
            if (rewardData.GeneratedPets == null)
            {
                rewardData.GeneratedPets = new List<RuntimePetData>();
            }

            if (AllowedPets == null || AllowedPets.Count == 0) return;

            for (int i = 0; i < NumberOfRolls; i++)
            {
                float rand = Random.Range(0f, 100f);
                if (rand > BaseDropChance) continue;

                var selectedPetSpecies = GetRandomPetByWeight();
                
                if (selectedPetSpecies != null)
                {
                    int stars = RollStars();
                    var newPet = await CloudPetGenerator.GeneratePetAsync(selectedPetSpecies, stars);

                    if (newPet != null)
                    {
                        rewardData.GeneratedPets.Add(newPet);
                    }
                }
            }
        }

        private PetSpeciesSO GetRandomPetByWeight()
        {
            float totalWeight = 0f;
            foreach (var entry in AllowedPets)
            {
                if (entry.Species != null)
                    totalWeight += entry.Weight;
            }

            if (totalWeight <= 0f) return null;

            float r = Random.Range(0f, totalWeight);
            foreach (var entry in AllowedPets)
            {
                if (entry.Species != null)
                {
                    if (r <= entry.Weight) return entry.Species;
                    r -= entry.Weight;
                }
            }

            return null;
        }

        private int RollStars()
        {
            float r = Random.Range(0f, 100f);
            
            if (DropMatrix == null) return 1;

            if (r <= DropMatrix.fiveStarChance && DropMatrix.fiveStarChance > 0) return 5;
            r -= DropMatrix.fiveStarChance;

            if (r <= DropMatrix.fourStarChance && DropMatrix.fourStarChance > 0) return 4;
            r -= DropMatrix.fourStarChance;

            if (r <= DropMatrix.threeStarChance && DropMatrix.threeStarChance > 0) return 3;
            r -= DropMatrix.threeStarChance;

            if (r <= DropMatrix.twoStarChance && DropMatrix.twoStarChance > 0) return 2;
            
            return 1;
        }
    }
}
