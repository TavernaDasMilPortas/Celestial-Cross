using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.UI.Skills
{
    public class PassiveDetailModal : MonoBehaviour
    {
        public GameObject modalRoot;
        public Image iconImage;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descText;
        public Button closeButton;

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        public void Open(Sprite icon, string name, string durationText, string description)
        {
            if (modalRoot != null) modalRoot.SetActive(true);
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(icon != null);
            }
            if (titleText != null)
            {
                titleText.text = $"<b>{name}</b>\n<size=80%>{durationText}</size>";
            }
            if (descText != null)
            {
                descText.text = description;
            }
        }

        public void Close()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
        }
    }
}
