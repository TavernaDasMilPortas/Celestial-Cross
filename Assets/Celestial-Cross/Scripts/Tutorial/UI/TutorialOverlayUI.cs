using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace CelestialCross.Tutorial
{
    public class TutorialOverlayUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private CanvasGroup rootCanvasGroup;
        [SerializeField] private Image dimPanel;
        [SerializeField] private RectTransform spotlightTransform;

        [Header("Instructions")]
        [SerializeField] private RectTransform bannerPanel;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private Image instructionIcon;

        [Header("Interactions")]
        [SerializeField] private Button fullscreenButton;
        [SerializeField] private RectTransform arrowIndicator;

        private Material spotlightMaterial;

        private void Awake()
        {
            if (dimPanel != null)
            {
                // Cria uma instância única do material para este overlay
                spotlightMaterial = Instantiate(dimPanel.material);
                dimPanel.material = spotlightMaterial;
            }

            if (fullscreenButton != null)
            {
                fullscreenButton.onClick.AddListener(() => TutorialManager.Instance?.OnOverlayClicked());
            }

            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (rootCanvasGroup != null)
            {
                rootCanvasGroup.alpha = 1f;
                rootCanvasGroup.blocksRaycasts = true;
            }
        }

        public void Hide()
        {
            if (rootCanvasGroup != null)
            {
                rootCanvasGroup.alpha = 0f;
                rootCanvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
        }

        public void SetInstruction(string text, Sprite icon, TutorialBannerPosition position)
        {
            if (instructionText != null) instructionText.text = text;
            if (instructionIcon != null)
            {
                instructionIcon.sprite = icon;
                instructionIcon.gameObject.SetActive(icon != null);
            }

            // Ajusta posição do banner (Top, Center, Bottom)
            if (bannerPanel != null)
            {
                switch (position)
                {
                    case TutorialBannerPosition.Top:
                        bannerPanel.anchorMin = new Vector2(0.5f, 1f);
                        bannerPanel.anchorMax = new Vector2(0.5f, 1f);
                        bannerPanel.anchoredPosition = new Vector2(0, -200);
                        break;
                    case TutorialBannerPosition.Center:
                        bannerPanel.anchorMin = new Vector2(0.5f, 0.5f);
                        bannerPanel.anchorMax = new Vector2(0.5f, 0.5f);
                        bannerPanel.anchoredPosition = Vector2.zero;
                        break;
                    case TutorialBannerPosition.Bottom:
                        bannerPanel.anchorMin = new Vector2(0.5f, 0f);
                        bannerPanel.anchorMax = new Vector2(0.5f, 0f);
                        bannerPanel.anchoredPosition = new Vector2(0, 200);
                        break;
                }
            }
        }

        public void SetSpotlight(Vector2 screenPos, Vector2 size, bool active)
        {
            if (spotlightMaterial == null) return;

            if (!active)
            {
                spotlightMaterial.SetVector("_HoleSize", Vector4.zero);
                if (arrowIndicator != null) arrowIndicator.gameObject.SetActive(false);
                return;
            }

            // Inverte Y pois o Screen Space do Shader costuma ser de baixo para cima
            // mas o ScreenPoint do Unity também. No entanto, se houver problemas, ajustamos aqui.
            spotlightMaterial.SetVector("_HoleCenter", new Vector4(screenPos.x, screenPos.y, 0, 0));
            spotlightMaterial.SetVector("_HoleSize", new Vector4(size.x, size.y, 0, 0));

            if (arrowIndicator != null)
            {
                arrowIndicator.gameObject.SetActive(true);
                arrowIndicator.position = screenPos + new Vector2(0, size.y * 0.5f + 50f);
            }
        }
    }
}
