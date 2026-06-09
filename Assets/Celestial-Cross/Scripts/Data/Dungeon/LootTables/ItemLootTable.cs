using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Data.Dungeon;
using CelestialCross.Data.Rewards;

namespace CelestialCross.Data.Loot
{
    [global::System.Serializable]
    public class ItemDropEntry
    {
        public string ItemID;
        public int MinAmount = 1;
        public int MaxAmount = 1;

        [Tooltip("Peso de chance de drop deste item em relação aos outros da mesma lista")]
        public float Weight = 10f;
    }

    [global::System.Serializable]
    public class ItemLootTable : BaseLootTable
    {
        [Header("Pool de Itens")]
        public List<ItemDropEntry> AllowedItems = new List<ItemDropEntry>();

        [Header("Configurações")]
        [Tooltip("Quantidade de vezes que a tabela vai rodar para gerar itens")]
        public int NumberOfRolls = 1;

        public override void GenerateLoot(RuntimeReward rewardData)
        {
            if (AllowedItems == null || AllowedItems.Count == 0) return;

            if (rewardData.SourceDefinitions == null)
            {
                rewardData.SourceDefinitions = new List<RewardDefinition>();
            }

            for (int i = 0; i < NumberOfRolls; i++)
            {
                float rand = Random.Range(0f, 100f);
                if (rand > BaseDropChance) continue;

                var selectedItem = GetRandomItemByWeight();
                if (selectedItem != null && !string.IsNullOrEmpty(selectedItem.ItemID))
                {
                    int amount = Random.Range(selectedItem.MinAmount, selectedItem.MaxAmount + 1);

                    var newRewardDef = new RewardDefinition
                    {
                        Type = RewardType.Item,
                        ReferenceID = selectedItem.ItemID,
                        Amount = amount
                    };

                    rewardData.SourceDefinitions.Add(newRewardDef);
                }
            }
        }

        private ItemDropEntry GetRandomItemByWeight()
        {
            float totalWeight = 0f;
            foreach (var entry in AllowedItems)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.ItemID))
                    totalWeight += entry.Weight;
            }

            if (totalWeight <= 0f) return null;

            float r = Random.Range(0f, totalWeight);
            foreach (var entry in AllowedItems)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.ItemID))
                {
                    if (r <= entry.Weight) return entry;
                    r -= entry.Weight;
                }
            }

            return null;
        }
    }
}
