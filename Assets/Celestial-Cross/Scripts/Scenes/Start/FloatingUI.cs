using UnityEngine;
using DG.Tweening;

namespace CelestialCross.UI
{
    public class FloatingUI : MonoBehaviour
    {
        [Header("Floating Settings")]
        [SerializeField] private float floatDistance = 20f;
        [SerializeField] private float floatDuration = 2f;
        [SerializeField] private Ease floatEase = Ease.InOutSine;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (rectTransform != null)
            {
                // Move up and down infinitely
                rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + floatDistance, floatDuration)
                    .SetEase(floatEase)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                // Fallback for non-UI elements if attached to normal transforms
                transform.DOMoveY(transform.position.y + floatDistance * 0.01f, floatDuration)
                    .SetEase(floatEase)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        private void OnDestroy()
        {
            if (rectTransform != null) rectTransform.DOKill();
            transform.DOKill();
        }
    }
}
