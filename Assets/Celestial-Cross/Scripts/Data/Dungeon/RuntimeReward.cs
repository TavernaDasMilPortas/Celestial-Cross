using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Artifacts;
using CelestialCross.Data.Pets;

namespace CelestialCross.Data.Dungeon
{
    [global::System.Serializable]
    public class RuntimeReward
    {
        public int Money;
        public int Energy;
        
        [Header("NOVO: Poeira Estelar")]
        public int Stardust;
        public int XP;

        public List<ArtifactInstanceData> GeneratedArtifacts = new List<ArtifactInstanceData>();
        public List<RuntimePetData> GeneratedPets = new List<RuntimePetData>();
        
        public RuntimeReward() { }
        
        public RuntimeReward(RewardPackage basePackage)
        {
            if (basePackage != null)
            {
                Money = basePackage.Money;
                Energy = basePackage.Energy;
                XP = basePackage.XP;
            }
        }
    }
}
