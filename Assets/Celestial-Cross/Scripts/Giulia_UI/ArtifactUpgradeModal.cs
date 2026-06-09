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

        private ArtifactInstanceData currentArtifact;
        private Action onStateChanged;

        private void Awake()
        {
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (sellButton != null) sellButton.onClick.AddListener(OnSellClicked);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        public void Show(ArtifactInstanceData artifact, Action onStateChangedCallback)
        {
            currentArtifact = artifact;
            onStateChanged = onStateChangedCallback;
            RefreshUI();
            
            // Força o modal a ser desenhado por cima de tudo no parent
            transform.SetAsLastSibling();
            
            gameObject.SetActive(true);
        }

        public void Close()
        {
            Debug.Log("Close Btn clicked");
            gameObject.SetActive(false);
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
                upgradeCostText.text = "N�VEL MAX (15)";
            }
            else
            {
                int cost = ArtifactEconomyService.GetUpgradeCost(currentArtifact.currentLevel, (int)currentArtifact.stars, currentArtifact.rarity);
                upgradeButton.interactable = acc.Money >= cost;
                upgradeCostText.text = $"UPGRADE\n(Custo: {cost} moedas) | Saldo: {acc.Money}";
            }

            // Sell State
            int sellValue = ArtifactEconomyService.GetSellValue(currentArtifact);
            sellPriceText.text = $"VENDER\n(+{sellValue} moedas)";
        }

        private void OnUpgradeClicked()
        {
            Debug.Log("Upgrade Btn clicked");
            var acc = AccountManager.Instance.PlayerAccount;
            if (ArtifactEconomyService.TryUpgradeArtifact(acc, currentArtifact))
            {
                Debug.Log($"Upgraded successfully! New level: {currentArtifact.currentLevel}");
                AccountManager.Instance.SaveAccount();
                RefreshUI();
                onStateChanged?.Invoke();
            }
            else
            {
                Debug.Log($"Upgrade failed (Level cap or insufficient funds). Level: {currentArtifact.currentLevel}, Funds: {acc.Money}");
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
