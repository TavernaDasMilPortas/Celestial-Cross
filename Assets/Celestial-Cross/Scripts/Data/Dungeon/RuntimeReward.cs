using System.Collections.Generic;
using CelestialCross.Artifacts;

namespace CelestialCross.Data.Dungeon
{
    /// <summary>
    /// Objeto que transporta as recompensas de fim de fase dinamicamente, 
    /// em vez de referenciar diretamente o SO de Reward.
    /// </summary>
    public class RuntimeReward
    {
        public int Money;
        public int Energy;
        
        // Artefatos gerados aleatoriamente baseados na matriz da Dungeon
        public List<ArtifactInstanceData> GeneratedArtifacts = new List<ArtifactInstanceData>();
        
        public RuntimeReward() { }
        
        public RuntimeReward(RewardPackage basePackage)
        {
            if (basePackage != null)
            {
                Money = basePackage.Money;
                Energy = basePackage.Energy;
            }
        }
    }
}
