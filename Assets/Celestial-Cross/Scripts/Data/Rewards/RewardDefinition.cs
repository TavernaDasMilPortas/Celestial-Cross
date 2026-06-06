using System;
using UnityEngine;
using CelestialCross.Data.Pets;
using CelestialCross.Artifacts;
using CelestialCross.Data.Loot;
using Sirenix.OdinInspector;

namespace CelestialCross.Data.Rewards
{
    [Serializable]
    public class RewardDefinition
    {
        public RewardType Type;
        
        [ShowIf("@this.Type != RewardType.Unit && this.Type != RewardType.Pet && this.Type != RewardType.Artifact && this.Type != RewardType.LootTable")]
        public int Amount;
        
        [ShowIf("Type", RewardType.Item)]
        public string ReferenceID;           // ID do item, etc.
        
        [ShowIf("Type", RewardType.Unit)]
        public UnitData UnitRef;             // Referência direta ao SO (para Unit)
        
        [ShowIf("Type", RewardType.Pet)]
        [Header("Pet Options")]
        public CelestialCross.Data.Pets.PetSpeciesSO PetRef;      // Referência direta ao SO (para Pet)
        
        [ShowIf("Type", RewardType.Artifact)]
        [Header("Artifact Options")]
        public CelestialCross.Artifacts.ArtifactSet ArtifactSetRef;    // Referência direta ao SO (para Artifact)
        
        [ShowIf("Type", RewardType.LootTable)]
        [SerializeReference]
        public BaseLootTable LootTableRef;   // Referência ao loot table (para LootTable)
    }
}
