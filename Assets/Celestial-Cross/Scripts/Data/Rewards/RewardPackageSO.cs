using System.Collections.Generic;
using UnityEngine;

namespace CelestialCross.Data.Rewards
{
    [CreateAssetMenu(fileName = "NewRewardPackageSO", menuName = "Celestial Cross/Rewards/Reward Package SO")]
    public class RewardPackageSO : ScriptableObject
    {
        public List<RewardDefinition> Rewards = new List<RewardDefinition>();
    }
}
