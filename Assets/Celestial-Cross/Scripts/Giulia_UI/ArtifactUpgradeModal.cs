using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using CelestialCross.Artifacts;
using CelestialCross.System;

namespace CelestialCross.Giulia_UI
{
    public class ArtifactUpgradeModal : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI detailsText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeCostText;
        [SerializeField] private Button sellButton;
        [SerializeField] private TextMeshProUGUI sellPriceText;
        [SerializeField] private Button closeButton;

        [Header("Multiple Upgrade Additions")]
        [SerializeField] private Slider levelSlider;
        [SerializeField] private TextMeshProUGUI levelTargetText;

        private ArtifactInstanceData currentArtifact;
        private Action onStateChanged;
        private Action onClose;
        private bool canSellCurrent;

        private void Awake()
        {
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (sellButton != null) sellButton.onClick.AddListener(OnSellClicked);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (levelSlider != null) levelSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        public void Show(ArtifactInstanceData artifact, Action onStateChangedCallback, bool allowSell = true, Action onCloseCallback = null)
        {
            currentArtifact = artifact;
            onStateChanged = onStateChangedCallback;
            canSellCurrent = allowSell;
            onClose = onCloseCallback;
            RefreshUI();
            
            // Força o modal a ser desenhado por cima de tudo no parent
            transform.SetAsLastSibling();
            
            gameObject.SetActive(true);
        }

        public void Close()
        {
            Debug.Log("Close Btn clicked");
            gameObject.SetActive(false);
            onClose?.Invoke();
        }

        private void RefreshUI()
        {
            if (currentArtifact == null) { Close(); return; }

            var acc = AccountManager.Instance.PlayerAccount;
            
            titleText.text = $"Artefato {currentArtifact.slot} (+{currentArtifact.currentLevel})";
            
            string setLabel = string.IsNullOrWhiteSpace(currentArtifact.artifactSetId) ? "<sem set>" : currentArtifact.artifactSetId;
            string main = currentArtifact.mainStat != null ? UIStatFormatter.FormatStat(currentArtifact.mainStat) : "N/A";
            
            string baseInfo = $"Set: {setLabel}\nEstrelas: {currentArtifact.stars}? | Raridade: {currentArtifact.rarity}\n\n";
            string statsInfo = $"<b>MAIN STAT:</b> {main}\n\n<b>SUBSTATS:</b>\n";
            
            for (int i = 0; i < currentArtifact.subStats.Count; i++)
            {
                var s = currentArtifact.subStats[i];
                statsInfo += $"- {UIStatFormatter.FormatStat(s)}\n";
            }

            detailsText.text = baseInfo + statsInfo;

            // Upgrade State
            if (currentArtifact.currentLevel >= 15)
            {
                upgradeButton.interactable = false;
                upgradeCostText.text = "NÍVEL MAX (15)";
                if (levelSlider != null) levelSlider.gameObject.SetActive(false);
                if (levelTargetText != null) levelTargetText.gameObject.SetActive(false);
            }
            else
            {
                if (levelSlider != null)
                {
                    levelSlider.gameObject.SetActive(true);
                    levelSlider.minValue = currentArtifact.currentLevel;
                    levelSlider.maxValue = 15;
                    if (levelSlider.value < currentArtifact.currentLevel) levelSlider.value = currentArtifact.currentLevel;
                    if (levelSlider.value == currentArtifact.currentLevel) levelSlider.value = currentArtifact.currentLevel + 1;
                }
                if (levelTargetText != null) levelTargetText.gameObject.SetActive(true);
                UpdateCostUI();
            }

            // Sell State
            if (canSellCurrent && sellButton != null)
            {
                sellButton.gameObject.SetActive(true);
                int sellValue = ArtifactEconomyService.GetSellValue(currentArtifact);
                sellPriceText.text = $"VENDER\n(+{sellValue} moedas)";
            }
            else if (sellButton != null)
            {
                sellButton.gameObject.SetActive(false);
            }
        }

        private void OnSliderChanged(float val)
        {
            UpdateCostUI();
        }

        private void UpdateCostUI()
        {
            if (currentArtifact == null || currentArtifact.currentLevel >= 15) return;
            
            int target = levelSlider != null ? Mathf.RoundToInt(levelSlider.value) : currentArtifact.currentLevel + 1;
            if (target <= currentArtifact.currentLevel) target = currentArtifact.currentLevel + 1;
            
            if (levelTargetText != null) levelTargetText.text = $"-> Nível {target}";

            int levelsToUpgrade = target - currentArtifact.currentLevel;
            int totalCost = 0;
            for(int i = 0; i < levelsToUpgrade; i++)
            {
                totalCost += ArtifactEconomyService.GetUpgradeCost(currentArtifact.currentLevel + i, (int)currentArtifact.stars, currentArtifact.rarity);
            }

            var acc = AccountManager.Instance.PlayerAccount;
            upgradeButton.interactable = acc.Money >= totalCost && levelsToUpgrade > 0;
            upgradeCostText.text = $"UPGRADE\n(Custo: {totalCost} moedas) | Saldo: {acc.Money}";
        }

        private void OnUpgradeClicked()
        {
            var acc = AccountManager.Instance.PlayerAccount;
            int target = levelSlider != null ? Mathf.RoundToInt(levelSlider.value) : currentArtifact.currentLevel + 1;
            if (target <= currentArtifact.currentLevel) target = currentArtifact.currentLevel + 1;
            
            int levelsToUpgrade = target - currentArtifact.currentLevel;
            bool successAny = false;

            for(int i = 0; i < levelsToUpgrade; i++)
            {
                if (ArtifactEconomyService.TryUpgradeArtifact(acc, currentArtifact))
                {
                    successAny = true;
                }
                else
                {
                    Debug.Log($"Upgrade failed on step {i} (Insufficient funds). Level: {currentArtifact.currentLevel}, Funds: {acc.Money}");
                    break;
                }
            }

            if (successAny)
            {
                AccountManager.Instance.SaveAccount();
                RefreshUI();
                onStateChanged?.Invoke();
                
                if (currentArtifact.currentLevel >= 15)
                {
                    Close();
                }
            }
        }

        private void OnSellClicked()
        {
            Debug.Log("Sell Btn clicked");
            var acc = AccountManager.Instance.PlayerAccount;
            if (ArtifactEconomyService.TrySellArtifact(acc, currentArtifact))
            {
                Debug.Log("Sold successfully!");
                AccountManager.Instance.SaveAccount();
                onStateChanged?.Invoke();
                Close();
            }
        }
    }
}
