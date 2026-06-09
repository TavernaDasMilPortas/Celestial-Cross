using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Artifacts;

namespace CelestialCross.Scenes.Inventory
{
    public class ArtifactTabPanel : InventoryTabPanel
    {
        [Header("Artifact Detail UI")]
        public Image artifactIconImage;
        public TextMeshProUGUI artifactNameText;
        public TextMeshProUGUI artifactLevelText;
        public TextMeshProUGUI mainStatText;
        public TextMeshProUGUI subStatsText;
        public TextMeshProUGUI setBonusText;

        [Header("Actions")]
        public Button upgradeButton;
        public Button sellButton;
        public Button filterButton;

        private ArtifactInstanceData currentSelectedArtifact;
        private ArtifactFilterData activeFilter = null;

        private global::System.Collections.Generic.Dictionary<string, Image> artifactSlotImages = new global::System.Collections.Generic.Dictionary<string, Image>();
        private Color defaultSlotColor = new Color(0.2f, 0.2f, 0.28f, 1f);
        private Color selectedSlotColor = new Color(0.4f, 0.5f, 0.8f, 1f);

        protected override void Awake()
        {
            base.Awake();
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (sellButton != null) sellButton.onClick.AddListener(OnSellClicked);
            if (filterButton != null) filterButton.onClick.AddListener(OnFilterClicked);
        }

        private void Start()
        {
            if (InventorySceneController.Instance != null && InventorySceneController.Instance.artifactFilterModal != null)
            {
                InventorySceneController.Instance.artifactFilterModal.OnFilterApplied += HandleArtifactFilter;
            }
        }

        private void HandleArtifactFilter(ArtifactFilterData filter)
        {
            activeFilter = filter;
            Refresh();
        }

        protected override void OnShow()
        {
            base.OnShow();
            Refresh();
        }

        public override void Refresh()
        {
            if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null) return;
            if (InventorySceneController.Instance == null || InventorySceneController.Instance.artifactSetCatalog == null) return;
            
            var ownedArtifacts = AccountManager.Instance.PlayerAccount.OwnedArtifacts;
            if (ownedArtifacts == null) return;

            foreach (Transform child in gridContent.transform)
            {
                Destroy(child.gameObject);
            }

            artifactSlotImages.Clear();

            var prefab = InventorySceneController.Instance.slotPrefab;
            if (prefab == null) return;

            ArtifactInstanceData firstArtifact = null;
            ArtifactSet firstSet = null;

            foreach (var artifact in ownedArtifacts)
            {
                if (activeFilter != null) {
                    if (activeFilter.sets.Count > 0 && !activeFilter.sets.Contains(artifact.artifactSetId)) continue;
                    if (activeFilter.types.Count > 0 && !activeFilter.types.Contains(artifact.slot)) continue;
                    if (activeFilter.mainStat != "Qualquer" && artifact.mainStat.statType.ToString() != activeFilter.mainStat) continue;
                    
                    bool hasAllSubs = true;
                    foreach(var subFilter in activeFilter.subStats) {
                        bool found = false;
                        if (artifact.subStats != null) {
                            foreach(var sub in artifact.subStats) {
                                if (sub.statType.ToString() == subFilter) { found = true; break; }
                            }
                        }
                        if (!found) { hasAllSubs = false; break; }
                    }
                    if (!hasAllSubs) continue;
                }

                var setSO = InventorySceneController.Instance.artifactSetCatalog.GetSetById(artifact.artifactSetId);
                if (setSO == null) continue;

                var slotObj = Instantiate(prefab, gridContent.transform);
                slotObj.SetActive(true);

                var slotImg = slotObj.GetComponent<Image>();
                if (slotImg != null)
                {
                    artifactSlotImages[artifact.idGUID] = slotImg;
                    slotImg.color = defaultSlotColor;
                }

                var iconImg = slotObj.transform.Find("Icon")?.GetComponent<Image>();
                if (iconImg != null)
                {
                    iconImg.sprite = setSO.GetIconForSlot(artifact.slot);
                    iconImg.gameObject.SetActive(true);
                }

                var txt = slotObj.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
                if (txt != null) txt.text = $"Lv. {artifact.currentLevel}";

                var btn = slotObj.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(() => SelectArtifact(artifact, setSO));

                if (firstArtifact == null)
                {
                    firstArtifact = artifact;
                    firstSet = setSO;
                }
            }

            // Auto-selecionar o primeiro artefato se houver algum
            if (firstArtifact != null)
            {
                if (currentSelectedArtifact == null || !artifactSlotImages.ContainsKey(currentSelectedArtifact.idGUID))
                {
                    SelectArtifact(firstArtifact, firstSet);
                }
                else
                {
                    // Re-selecionar para atualizar highlight de cor
                    var currentSet = InventorySceneController.Instance.artifactSetCatalog.GetSetById(currentSelectedArtifact.artifactSetId);
                    if (currentSet != null)
                        SelectArtifact(currentSelectedArtifact, currentSet);
                }
            }
            else
            {
                // Limpa visual se vazio
                currentSelectedArtifact = null;
                if (artifactIconImage != null) artifactIconImage.sprite = null;
                if (artifactNameText != null) artifactNameText.text = "Selecione um Artefato";
                if (artifactLevelText != null) artifactLevelText.text = "Lv. —";
                if (mainStatText != null) mainStatText.text = "Stat Principal: —";
                if (subStatsText != null) subStatsText.text = "";
                if (setBonusText != null) setBonusText.text = "";
            }
        }

        public void SelectArtifact(ArtifactInstanceData artifactData, ArtifactSet setSO)
        {
            currentSelectedArtifact = artifactData;

            // Feedback visual de seleção no grid
            foreach (var kvp in artifactSlotImages)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.color = (kvp.Key == artifactData.idGUID) ? selectedSlotColor : defaultSlotColor;
                }
            }

            if (artifactIconImage != null) artifactIconImage.sprite = setSO.GetIconForSlot(artifactData.slot);
            if (artifactNameText != null) artifactNameText.text = $"{setSO.setName} ({artifactData.slot})";
            if (artifactLevelText != null) artifactLevelText.text = $"Lv. {artifactData.currentLevel}";
            
            if (mainStatText != null) mainStatText.text = $"{artifactData.mainStat.statType}: +{artifactData.mainStat.value}";
            
            if (subStatsText != null)
            {
                subStatsText.text = "";
                foreach (var sub in artifactData.subStats)
                {
                    subStatsText.text += $"{sub.statType}: +{sub.value}\n";
                }
            }

            if (setBonusText != null)
            {
                setBonusText.text = setSO.description;
            }
        }

        private void OnUpgradeClicked()
        {
            if (currentSelectedArtifact == null) return;
            if (InventorySceneController.Instance != null && InventorySceneController.Instance.upgradeSliderModal != null)
            {
                InventorySceneController.Instance.upgradeSliderModal.Show(currentSelectedArtifact, () => {
                    SelectArtifact(currentSelectedArtifact, InventorySceneController.Instance.artifactSetCatalog.GetSetById(currentSelectedArtifact.artifactSetId));
                    Refresh();
                });
            }
        }

        private void OnSellClicked()
        {
            if (currentSelectedArtifact == null) return;
            var account = AccountManager.Instance?.PlayerAccount;
            if (account != null)
            {
                if (CelestialCross.System.ArtifactEconomyService.TrySellArtifact(account, currentSelectedArtifact))
                {
                    currentSelectedArtifact = null;
                    Refresh();
                }
            }
        }

        private void OnFilterClicked()
        {
            if (InventorySceneController.Instance != null && InventorySceneController.Instance.artifactFilterModal != null)
            {
                InventorySceneController.Instance.artifactFilterModal.Show();
            }
        }
    }
}
