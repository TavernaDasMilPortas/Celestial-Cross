using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Artifacts;
using CelestialCross.Data.Dungeon;

namespace CelestialCross.Data.Loot
{
    [global::System.Serializable]
    public class ArtifactLootTable : BaseLootTable
    {
        [Header("Global Loot Pool da Masmorra ou Região")]
        public List<ArtifactSet> AllowedArtifactSets = new List<ArtifactSet>();

        [Header("Tabela de Chance Base")]
        public ArtifactDropMatrix DropMatrix = new ArtifactDropMatrix();
        
        [Tooltip("Quantidade a ser gerada por padrão")]
        public int NumberOfDrops = 1;

        public override void GenerateLoot(RuntimeReward rewardData)
        {
            if (AllowedArtifactSets == null || AllowedArtifactSets.Count == 0) return;
            if (DropMatrix == null) DropMatrix = new ArtifactDropMatrix();

            for (int i = 0; i < NumberOfDrops; i++)
            {
                float rand = Random.Range(0f, 100f);
                if (rand > BaseDropChance) continue;

                var artifact = CelestialCross.System.ArtifactLootService.GenerateSingleFromMatrix(AllowedArtifactSets, DropMatrix);
                if (artifact != null)
                {
                    if (rewardData.GeneratedArtifacts == null) rewardData.GeneratedArtifacts = new List<ArtifactInstanceData>();
                    rewardData.GeneratedArtifacts.Add(artifact);
                }
            }
        }
    }
}
