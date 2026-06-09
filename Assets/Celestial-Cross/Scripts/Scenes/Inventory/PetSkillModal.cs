using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Scenes.Inventory
{
    public class PetSkillModal : MonoBehaviour
    {
        [Header("UI References")]
        public Image skillIconImage;
        public TextMeshProUGUI skillNameText;
        public TextMeshProUGUI skillDescriptionText;
        public Button closeButton;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
        }

        public void Show(string name, Sprite icon, string description)
        {
            if (skillNameText != null) skillNameText.text = name;
            
            if (skillIconImage != null)
            {
                skillIconImage.sprite = icon;
                skillIconImage.gameObject.SetActive(icon != null);
            }
            
            if (skillDescriptionText != null) skillDescriptionText.text = description;

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
