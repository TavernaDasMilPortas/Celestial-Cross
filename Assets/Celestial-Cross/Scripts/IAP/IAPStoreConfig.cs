using System.Collections.Generic;
using UnityEngine;

namespace CelestialCross.IAP
{
    [CreateAssetMenu(fileName = "IAPStoreConfig", menuName = "Celestial Cross/IAP/Store Config")]
    public class IAPStoreConfig : ScriptableObject
    {
        public List<IAPProductSO> Products = new List<IAPProductSO>();
    }

    [global::System.Serializable]
    public enum IAPType { Consumable, NonConsumable, Subscription }

    [CreateAssetMenu(fileName = "NewIAPProduct", menuName = "Celestial Cross/IAP/Product")]
    public class IAPProductSO : ScriptableObject
    {
        public string ProductID; // ID da loja (ex: com.game.crystals_100)
        public string DisplayName;
        public string Description;
        public IAPType ProductType = IAPType.Consumable;
        public float Price; // Apenas para exibição local no Mock
        
        [Header("Rewards")]
        public int MoneyReward;
        public int StarMapsReward;
        public int StardustReward;
    }
}
