using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

namespace CelestialCross.System.UI
{
    public class MessageBubbleUI : MonoBehaviour
    {
        [Header("References")]
        public TMP_Text messageText;
        public Image iconImage;
        public Button skipButton;
        public RectTransform bubbleTransform;
        public CanvasGroup canvasGroup;

        private Action onComplete;
        private bool isSkipping = false;

        private void Awake()
        {
            if (skipButton != null)
                skipButton.onClick.AddListener(Skip);
        }

        public void Setup(string text, Sprite icon, Action onCompleteCallback)
        {
            this.onComplete = onCompleteCallback;
            isSkipping = false;

            if (messageText != null) messageText.text = text;
            
            if (iconImage != null)
            {
                if (icon != null)
                {
                    iconImage.sprite = icon;
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                }
            }

            // Animação de entrada (Persona style - pop and scale) ignorando timescale
            if (bubbleTransform != null)
            {
                bubbleTransform.localScale = new Vector3(0.1f, 0.1f, 1f);
                bubbleTransform.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-5f, 5f));
                
                bubbleTransform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
                bubbleTransform.DOLocalRotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
            }
        }

        public void Skip()
        {
            if (isSkipping) return;
            isSkipping = true;

            // Animação de saída ignorando timescale
            if (bubbleTransform != null)
            {
                bubbleTransform.DOScale(0.8f, 0.2f).SetEase(Ease.InBack).SetUpdate(true);
            }

            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() =>
                {
                    onComplete?.Invoke();
                    Destroy(gameObject);
                });
            }
            else
            {
                onComplete?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
