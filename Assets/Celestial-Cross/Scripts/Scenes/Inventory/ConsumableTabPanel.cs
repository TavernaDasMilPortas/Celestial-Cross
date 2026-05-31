using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Scenes.Inventory
{
    public class ConsumableTabPanel : InventoryTabPanel
    {
        [Header("Consumable Detail UI")]
        public Image consumableIconImage;
        public TextMeshProUGUI consumableNameText;
        public TextMeshProUGUI consumableDescriptionText;

        public void SelectConsumable(Sprite icon, string name, string description)
        {
            if (consumableIconImage != null) consumableIconImage.sprite = icon;
            if (consumableNameText != null) consumableNameText.text = name;
            if (consumableDescriptionText != null) consumableDescriptionText.text = description;
        }
    }
}
