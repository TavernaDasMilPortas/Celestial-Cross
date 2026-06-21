using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using CelestialCross.Artifacts;
using DG.Tweening;

namespace CelestialCross.Scenes.Unit
{
    public class ArtifactMiniInfoModal : MonoBehaviour
    {
        [Header("UI")]
        public Image iconImage;
        public TextMeshProUGUI statsText;
        public Button alterButton;
        public Button closeButton;

        private Action onAlterCallback;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (alterButton != null) alterButton.onClick.AddListener(OnAlterClicked);
        }

        public void Show(ArtifactInstanceData artifactData, Action onAlter)
        {
            onAlterCallback = onAlter;
            
            if (UnitSceneController.Instance != null) UnitSceneController.Instance.ShowModalOverlay();
            
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOKill();
                rect.localScale = Vector3.zero;
                rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            // Popula visual baseado no artifactData
            if (statsText != null)
            {
                statsText.text = $"{artifactData.mainStat.statType}: +{artifactData.mainStat.value}";
            }
        }

        public void Hide()
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null && gameObject.activeSelf)
            {
                rect.DOKill();
                rect.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
                    gameObject.SetActive(false);
                    if (UnitSceneController.Instance != null) UnitSceneController.Instance.HideModalOverlay();
                });
            }
            else
            {
                gameObject.SetActive(false);
                if (UnitSceneController.Instance != null) UnitSceneController.Instance.HideModalOverlay();
            }
        }

        private void OnAlterClicked()
        {
            onAlterCallback?.Invoke();
        }
    }
}
