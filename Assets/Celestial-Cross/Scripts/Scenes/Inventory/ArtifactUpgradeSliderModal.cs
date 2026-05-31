using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Artifacts;

namespace CelestialCross.Scenes.Inventory
{
    public class ArtifactUpgradeSliderModal : MonoBehaviour
    {
        [Header("UI References")]
        public Slider levelSlider;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI costText;
        public Button confirmButton;
        public Button closeButton;

        private ArtifactInstanceData currentArtifact;
        private int currentLevel;
        private int maxLevel = 15;
        private global::System.Action onComplete;

        private void Awake()
        {
            if (levelSlider != null)
            {
                levelSlider.onValueChanged.AddListener(OnSliderValueChanged);
            }
            if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmUpgrade);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }

        public void Show(ArtifactInstanceData artifact, global::System.Action onCompleteCallback = null)
        {
            gameObject.SetActive(true);
            currentArtifact = artifact;
            currentLevel = artifact.currentLevel;
            onComplete = onCompleteCallback;

            if (levelSlider != null)
            {
                levelSlider.minValue = currentLevel;
                levelSlider.maxValue = maxLevel;
                levelSlider.value = currentLevel;
            }
            
            UpdateUI(currentLevel);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnSliderValueChanged(float value)
        {
            int targetLevel = Mathf.RoundToInt(value);
            UpdateUI(targetLevel);
        }

        private void UpdateUI(int targetLevel)
        {
            if (levelText != null)
                levelText.text = $"Level: {currentLevel} -> {targetLevel}";

            int levelsToUpgrade = targetLevel - currentLevel;
            int totalCost = 0;
            
            for(int i = 0; i < levelsToUpgrade; i++)
            {
                totalCost += CelestialCross.System.ArtifactEconomyService.GetUpgradeCost(currentLevel + i, (int)currentArtifact.stars, currentArtifact.rarity);
            }
            
            if (costText != null)
                costText.text = $"Custo: {totalCost}";

            var account = global::AccountManager.Instance?.PlayerAccount;
            bool canAfford = account != null && account.Money >= totalCost;

            if (confirmButton != null)
                confirmButton.interactable = levelsToUpgrade > 0 && canAfford;
        }

        private void ConfirmUpgrade()
        {
            if (currentArtifact == null) return;
            var account = global::AccountManager.Instance?.PlayerAccount;
            if (account == null) return;

            int targetLevel = Mathf.RoundToInt(levelSlider.value);
            int levelsToUpgrade = targetLevel - currentLevel;

            for(int i=0; i<levelsToUpgrade; i++)
            {
                if (!CelestialCross.System.ArtifactEconomyService.TryUpgradeArtifact(account, currentArtifact))
                {
                    break; // Parar se faltar dinheiro no meio
                }
            }

            Debug.Log($"Artefato upado para o nível {currentArtifact.currentLevel}");
            onComplete?.Invoke();
            Hide();
        }
    }
}
