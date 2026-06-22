using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using CelestialCross.Audio;
using CelestialCross.System;

namespace CelestialCross.Preparation
{
    public class PreparationUnitButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        public Button button;
        public Image iconImage;
        public TextMeshProUGUI nameText;
        public GameObject selectionOutline;
        public CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float animationDuration = 0.2f;

        private string unitId;
        private bool isSelected;
        public Action<string> OnUnitClicked;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
            }
        }

        public void Setup(UnitData data, string id, bool initiallySelected = false)
        {
            unitId = id;
            isSelected = initiallySelected;

            if (nameText != null)
            {
                nameText.text = (data != null && !string.IsNullOrWhiteSpace(data.displayName)) ? data.displayName : id;
            }

            if (iconImage != null && data != null && data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.color = Color.white;
            }

            UpdateVisualState(false);
        }

        public void SetSelected(bool selected, bool animate = true)
        {
            if (isSelected == selected) return;
            
            isSelected = selected;
            UpdateVisualState(animate);

            if (animate)
            {
                // Play a small bounce animation
                transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 1f);
            }
        }

        private void UpdateVisualState(bool animate)
        {
            if (selectionOutline != null)
            {
                selectionOutline.SetActive(isSelected);
            }

            float targetAlpha = isSelected ? 1f : 0.8f;
            if (canvasGroup != null)
            {
                if (animate)
                    canvasGroup.DOFade(targetAlpha, animationDuration);
                else
                    canvasGroup.alpha = targetAlpha;
            }
        }

        private void HandleClick()
        {
            // The controller will decide the success/failure sound
            OnUnitClicked?.Invoke(unitId);
        }

        public void PlayPopInAnimation(float delay)
        {
            transform.localScale = Vector3.zero;
            if (canvasGroup != null) canvasGroup.alpha = 0f;

            Sequence seq = DOTween.Sequence();
            seq.SetDelay(delay);
            if (canvasGroup != null) seq.Join(canvasGroup.DOFade(1f, animationDuration));
            seq.Join(transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && button.interactable)
            {
                transform.DOScale(Vector3.one * hoverScale, animationDuration).SetEase(Ease.OutQuad);
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayUI(SoundKey.Navigation01);
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (button != null && button.interactable)
            {
                transform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutQuad);
            }
        }

        private void OnDestroy()
        {
            transform.DOKill();
            if (canvasGroup != null) canvasGroup.DOKill();
        }
    }
}
