using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using CelestialCross.Artifacts;

namespace CelestialCross.Scenes.Unit
{
    public class ArtifactActionModal : MonoBehaviour
    {
        [Header("UI References")]
        public Image iconImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI statsText;
        
        [Header("Stars")]
        public Transform starsContainer;
        public GameObject starPrefab;

        [Header("Buttons")]
        public Button upgradeButton;
        public Button changeButton;
        public Button unequipButton;
        public Button closeButton;

        private ArtifactInstanceData currentArtifact;
        private ArtifactSet currentSet;
        private string currentUnitId;
        private Action onStateChanged;

        private void Awake()
        {
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (changeButton != null) changeButton.onClick.AddListener(OnChangeClicked);
            if (unequipButton != null) unequipButton.onClick.AddListener(OnUnequipClicked);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        public void Show(ArtifactInstanceData artifact, ArtifactSet set, string unitId, Action onStateChangedCallback)
        {
            currentArtifact = artifact;
            currentSet = set;
            currentUnitId = unitId;
            onStateChanged = onStateChangedCallback;

            RefreshUI();
            
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void RefreshUI()
        {
            if (currentArtifact == null) { Close(); return; }

            if (iconImage != null && currentSet != null)
            {
                iconImage.sprite = currentSet.GetIconForSlot(currentArtifact.slot);
                iconImage.gameObject.SetActive(true);
                iconImage.preserveAspect = true;
            }

            if (nameText != null)
            {
                string sName = currentSet != null && !string.IsNullOrEmpty(currentSet.setName) ? currentSet.setName : $"Artefato {currentArtifact.slot}";
                nameText.text = sName;
                nameText.color = GetRarityColor(currentArtifact.rarity);
            }

            if (levelText != null)
            {
                levelText.text = $"+{currentArtifact.currentLevel}";
            }

            if (statsText != null)
            {
                string main = currentArtifact.mainStat != null ? CelestialCross.Giulia_UI.UIStatFormatter.FormatStat(currentArtifact.mainStat) : "N/A";
                string info = $"<b>Main:</b> {main}\n\n<b>Substats:</b>\n";
                if (currentArtifact.subStats != null)
                {
                    foreach (var sub in currentArtifact.subStats)
                    {
                        info += $"- {CelestialCross.Giulia_UI.UIStatFormatter.FormatStat(sub)}\n";
                    }
                }
                statsText.text = info;
            }

            if (starsContainer != null && starPrefab != null)
            {
                foreach (Transform child in starsContainer) Destroy(child.gameObject);
                int starsCount = currentArtifact.GetStarsAsIntClamped();
                for (int i = 0; i < starsCount; i++)
                {
                    var star = Instantiate(starPrefab, starsContainer);
                    star.SetActive(true);
                }
            }

            if (iconImage != null)
            {
                var outline = iconImage.GetComponent<UnityEngine.UI.Outline>();
                if (outline != null)
                {
                    outline.effectColor = Color.white; // Reseta para branco ou a cor padrao
                }
            }
        }

        private Color GetRarityColor(ArtifactRarity rarity)
        {
            switch (rarity)
            {
                case ArtifactRarity.Common: return Color.white;
                case ArtifactRarity.Uncommon: return new Color(0.2f, 0.8f, 0.2f); // Uncommon
                case ArtifactRarity.Rare: return new Color(0.2f, 0.5f, 1f); // Rare
                case ArtifactRarity.Epic: return new Color(0.8f, 0f, 0.8f); // Epic
                case ArtifactRarity.Legendary: return new Color(1f, 0.6f, 0f); // Legendary
                default: return Color.white;
            }
        }

        private void OnUpgradeClicked()
        {
            if (UnitSceneController.Instance != null && UnitSceneController.Instance.equipmentDetailPanel != null)
            {
                var eqPanel = UnitSceneController.Instance.equipmentDetailPanel.GetComponent<UnitDetailPanel_Equipment>();
                if (eqPanel != null && eqPanel.upgradeModal != null)
                {
                    // Esconder o modal de ação
                    gameObject.SetActive(false);

                    // Abrir o upgrade modal (Nao permitir vender quando vier daqui)
                    eqPanel.upgradeModal.Show(currentArtifact, () => {
                        // Ao atualizar o artefato, damos refresh no UnitScene (background)
                        onStateChanged?.Invoke();
                    }, false, () => {
                        // Ao fechar o upgrade modal, volta para o action modal
                        gameObject.SetActive(true);
                        RefreshUI();
                    });
                }
                else
                {
                    Debug.LogWarning("[ArtifactActionModal] UpgradeModal não referenciado no UnitDetailPanel_Equipment.");
                }
            }
        }

        private void OnChangeClicked()
        {
            if (UnitSceneController.Instance != null && UnitSceneController.Instance.equipmentDetailPanel != null)
            {
                var eqPanel = UnitSceneController.Instance.equipmentDetailPanel.GetComponent<UnitDetailPanel_Equipment>();
                if (eqPanel != null && eqPanel.artifactSelectModal != null)
                {
                    // Esconder este modal
                    gameObject.SetActive(false);

                    // Abrir seleção com suporte ao botão voltar
                    eqPanel.artifactSelectModal.Show(currentUnitId, currentArtifact.slot, () => {
                        // OnComplete da seleção (equipou ou fechou normalmente)
                        onStateChanged?.Invoke();
                        // Não reabre este modal, pois o artefato foi trocado ou a ação concluída.
                    }, () => {
                        // OnBack (clicou no voltar)
                        gameObject.SetActive(true);
                    });
                }
            }
        }

        private void OnUnequipClicked()
        {
            var acc = AccountManager.Instance?.PlayerAccount;
            if (acc != null && !string.IsNullOrEmpty(currentUnitId))
            {
                var loadout = acc.GetLoadoutForUnit(currentUnitId);
                if (loadout != null)
                {
                    acc.UnequipArtifactFromAll(currentArtifact.idGUID);
                    AccountManager.Instance.SaveAccount();
                    onStateChanged?.Invoke();
                }
            }
            Close();
        }
    }
}
