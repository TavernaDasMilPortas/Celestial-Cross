using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using CelestialCross.Artifacts;

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
            gameObject.SetActive(true);
            
            // Popula visual baseado no artifactData
            if (statsText != null)
            {
                statsText.text = $"{artifactData.mainStat.statType}: +{artifactData.mainStat.value}";
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnAlterClicked()
        {
            onAlterCallback?.Invoke();
        }
    }
}
