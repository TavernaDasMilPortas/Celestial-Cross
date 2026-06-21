using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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
            
            transform.SetAsLastSibling();
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.zero;
                rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }

        public void Hide()
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
                    gameObject.SetActive(false);
                });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
