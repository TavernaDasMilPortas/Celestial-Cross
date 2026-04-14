using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Data.Pets;

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
        public PetDropMatrix DropMatrix;

        [Header("Configurações Gerais")]
        [Tooltip("Quantidade a ser gerada por padrão")]
        public int NumberOfRolls = 1;

        public override void GenerateLoot(RuntimeReward rewardData)
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
                    float starMultiplier = 1f + (stars - 1) * 0.2f;

                    int finalHp = Mathf.RoundToInt(Random.Range(selectedPetSpecies.MinBaseHealth, selectedPetSpecies.MaxBaseHealth) * starMultiplier);
                    int finalAtk = Mathf.RoundToInt(Random.Range(selectedPetSpecies.MinBaseAttack, selectedPetSpecies.MaxBaseAttack) * starMultiplier);
                    int finalDef = Mathf.RoundToInt(Random.Range(selectedPetSpecies.MinBaseDefense, selectedPetSpecies.MaxBaseDefense) * starMultiplier);
                    int finalSpd = Mathf.RoundToInt(Random.Range(selectedPetSpecies.MinBaseSpeed, selectedPetSpecies.MaxBaseSpeed) * starMultiplier);
                    int finalCrit = Mathf.Clamp(Mathf.RoundToInt(Random.Range(selectedPetSpecies.MinBaseCriticalChance, selectedPetSpecies.MaxBaseCriticalChance) * starMultiplier), 0, 100);
                    int finalAcc = Mathf.Clamp(Mathf.RoundToInt(Random.Range(selectedPetSpecies.MinBaseEffectAccuracy, selectedPetSpecies.MaxBaseEffectAccuracy) * starMultiplier), 0, 100);

                    var newPet = new RuntimePetData(selectedPetSpecies.id, selectedPetSpecies.SpeciesName, stars,
                        finalHp, finalAtk, finalDef, finalSpd, finalCrit, finalAcc
                    );

                    rewardData.GeneratedPets.Add(newPet);
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
