using UnityEngine;
using CelestialCross.Data.Dungeon;
using System.Threading.Tasks;

namespace CelestialCross.Data.Loot
{
    [global::System.Serializable]
    public abstract class BaseLootTable
    {
        [Tooltip("Respeita a chance percentual de este drop sequer acontecer? 100 = Garantido.")]
        [Range(0f, 100f)] public float BaseDropChance = 100f;

        public abstract Task GenerateLootAsync(RuntimeReward rewardData);
    }
}
