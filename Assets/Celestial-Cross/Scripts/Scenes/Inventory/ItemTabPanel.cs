using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Scenes.Inventory
{
    public class ItemTabPanel : InventoryTabPanel
    {
        [Header("Item Detail UI")]
        public Image itemIconImage;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI itemDescriptionText;

        public void SelectItem(Sprite icon, string name, string description)
        {
            if (itemIconImage != null) itemIconImage.sprite = icon;
            if (itemNameText != null) itemNameText.text = name;
            if (itemDescriptionText != null) itemDescriptionText.text = description;
        }
    }
}
