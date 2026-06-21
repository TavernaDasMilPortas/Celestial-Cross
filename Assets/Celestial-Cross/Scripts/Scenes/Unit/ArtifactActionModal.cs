using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using CelestialCross.Artifacts;
using DG.Tweening;
using CelestialCross.Audio;

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
        private bool isFromGacha = false;

        private void Awake()
        {
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (changeButton != null) changeButton.onClick.AddListener(OnChangeClicked);
            if (unequipButton != null) unequipButton.onClick.AddListener(OnUnequipClicked);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        public void Show(ArtifactInstanceData artifact, ArtifactSet set, string unitId, Action onStateChangedCallback)
        {
            isFromGacha = false;
            currentArtifact = artifact;
            currentSet = set;
            currentUnitId = unitId;
            onStateChanged = onStateChangedCallback;

            if (changeButton != null) 
            {
                changeButton.gameObject.SetActive(true);
                if (changeButton.transform.parent != null && changeButton.transform.parent != transform)
                {
                    changeButton.transform.parent.gameObject.SetActive(true);
                }
            }
            var unequipText = unequipButton != null ? unequipButton.GetComponentInChildren<TextMeshProUGUI>() : null;
            if (unequipText != null) unequipText.text = "Desequipar";

            RefreshUI();
            
            DoPopIn();
        }

        private Action<bool> onStateChangedFromGacha;

        public void ShowFromGacha(ArtifactInstanceData artifact, ArtifactSet set, Action<bool> onStateChangedCallback)
        {
            isFromGacha = true;
            currentArtifact = artifact;
            currentSet = set;
            currentUnitId = "";
            onStateChangedFromGacha = onStateChangedCallback;

            if (changeButton != null) 
            {
                changeButton.gameObject.SetActive(false);
                if (changeButton.transform.parent != null && changeButton.transform.parent != transform)
                {
                    changeButton.transform.parent.gameObject.SetActive(false);
                }
            }
            
            var unequipText = unequipButton != null ? unequipButton.GetComponentInChildren<TextMeshProUGUI>() : null;
            if (unequipText != null) unequipText.text = "Vender";

            RefreshUI();
            
            DoPopIn();
        }

        private void DoPopIn()
        {
            if (UnitSceneController.Instance != null) UnitSceneController.Instance.ShowModalOverlay();
            
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOKill();
                rect.localScale = Vector3.zero;
                Sequence seq = DOTween.Sequence();
                seq.SetUpdate(true);
                seq.Append(rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack));

                // Stagger buttons
                Button[] buttons = { upgradeButton, changeButton, unequipButton };
                float delay = 0.1f;
                foreach (var btn in buttons)
                {
                    if (btn != null && btn.gameObject.activeInHierarchy)
                    {
                        btn.transform.DOKill();
                        btn.transform.localScale = Vector3.zero;
                        seq.Insert(delay, btn.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
                        delay += 0.05f;
                    }
                }
            }
        }

        public void Close()
        {
            if (isFromGacha && onStateChangedFromGacha != null)
            {
                var cb = onStateChangedFromGacha;
                onStateChangedFromGacha = null;
                cb.Invoke(false);
            }

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
                
                if (isFromGacha)
                {
                    int sellVal = CelestialCross.System.ArtifactEconomyService.GetSellValue(currentArtifact);
                    info += $"\n\n<color=#FFD700><b>Venda:</b> {sellVal} Fragmentos Estelares</color>";
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
            if (isFromGacha)
            {
                if (UnitSceneController.Instance != null && UnitSceneController.Instance.equipmentDetailPanel != null)
                {
                    var eqPanel = UnitSceneController.Instance.equipmentDetailPanel.GetComponent<UnitDetailPanel_Equipment>();
                    if (eqPanel != null && eqPanel.upgradeModal != null)
                    {
                        gameObject.SetActive(false);
                        eqPanel.upgradeModal.Show(currentArtifact, () => {
                            // Apenas upou, nada a notificar ao Gacha ainda
                        }, false, () => {
                            gameObject.SetActive(true);
                            RefreshUI();
                        });
                    }
                }
                else
                {
                    var upgModal = FindObjectOfType<CelestialCross.Giulia_UI.ArtifactUpgradeModal>(true);
                    if (upgModal != null)
                    {
                        gameObject.SetActive(false);
                        upgModal.Show(currentArtifact, () => {
                            // Apenas upou, nada a notificar ao Gacha ainda
                        }, false, () => {
                            gameObject.SetActive(true);
                            RefreshUI();
                        });
                    }
                    else
                    {
                        Debug.LogWarning("[ArtifactActionModal] UpgradeModal não encontrado no cenário atual.");
                    }
                }
            }
            else
            {
                if (UnitSceneController.Instance != null && UnitSceneController.Instance.equipmentDetailPanel != null)
                {
                    var eqPanel = UnitSceneController.Instance.equipmentDetailPanel.GetComponent<UnitDetailPanel_Equipment>();
                    if (eqPanel != null && eqPanel.upgradeModal != null)
                    {
                        gameObject.SetActive(false);
                        eqPanel.upgradeModal.Show(currentArtifact, () => {
                            onStateChanged?.Invoke();
                        }, false, () => {
                            gameObject.SetActive(true);
                            RefreshUI();
                        });
                    }
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
                    gameObject.SetActive(false);

                    eqPanel.artifactSelectModal.Show(currentUnitId, currentArtifact.slot, () => {
                        if (isFromGacha) onStateChangedFromGacha?.Invoke(false);
                        else onStateChanged?.Invoke();
                    }, () => {
                        gameObject.SetActive(true);
                    });
                }
            }
        }

        private void OnUnequipClicked()
        {
            var acc = AccountManager.Instance?.PlayerAccount;
            if (acc != null)
            {
                if (isFromGacha)
                {
                    CelestialCross.System.ArtifactEconomyService.TrySellArtifact(acc, currentArtifact);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(SoundKey.ItemSold01);
                    onStateChangedFromGacha?.Invoke(true);
                    Close();
                    return;
                }

                if (!string.IsNullOrEmpty(currentUnitId))
                {
                    var loadout = acc.GetLoadoutForUnit(currentUnitId);
                    if (loadout != null)
                    {
                        acc.UnequipArtifactFromAll(currentArtifact.idGUID);
                        AccountManager.Instance.SaveAccount();
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(SoundKey.ItemUnequip01);
                        onStateChanged?.Invoke();
                    }
                }
            }
            Close();
        }
    }
}
