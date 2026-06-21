using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using MoreMountains.Feedbacks;
using System.Collections.Generic;

namespace CelestialCross.Scenes.Unit
{
    public class UnitSceneController : MonoBehaviour
    {
        public static UnitSceneController Instance { get; private set; }

        [Header("Unit List UI")]
        public RectTransform unitListContainer;
        public GameObject unitListButtonPrefab;
        public UnityEngine.UI.Button backButton;

        [Header("Main Panels")]
        public UnitBannerUI bannerUI;
        public UnitMainPanel mainPanel;
        public RectTransform detailContainer;
        public RectTransform scrollCollectionContainer;
        
        [Header("Detail Panels")]
        public GameObject attributesDetailPanel;
        public GameObject petDetailPanel;
        public GameObject equipmentDetailPanel;
        public GameObject constellationDetailPanel;
        public GameObject abilitiesDetailPanel;

        [Header("Data References")]
        public UnitCatalog unitCatalog;
        public PetCatalog petCatalog;
        public ArtifactSetCatalog artifactSetCatalog;

        [Header("Juice - Panel Transition")]
        public CanvasGroup[] detailPanelCanvasGroups; 
        public Image modalOverlay;
        public CanvasGroup modalOverlayCanvasGroup;

        [Header("Juice - FEEL Feedbacks")]
        public MMF_Player panelTransitionFeedback;
        public MMF_Player modalOpenFeedback;
        public MMF_Player modalCloseFeedback;

        [Header("Juice - Transition Settings")]
        public float panelFadeDuration = 0.2f;
        public float panelSlideDelta = 30f;

        private UnitData currentLoadedUnit;
        private CelestialCross.Data.RuntimeUnitData currentLoadedRuntime;
        
        private int activeDetailPanelIndex = -1;
        private bool isTransitioning = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (backButton != null) backButton.onClick.AddListener(ReturnToHub);
        }

        private void Start()
        {
            if (AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
                InitializeUnitList();
            else
                AccountManager.OnAccountReady += InitializeUnitList;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            AccountManager.OnAccountReady -= InitializeUnitList;
        }

        private void InitializeUnitList()
        {
            if (unitCatalog == null) return;
            var account = AccountManager.Instance.PlayerAccount;
            if (account.OwnedUnits == null || account.OwnedUnits.Count == 0) return;

            // Limpar container
            if (unitListContainer != null)
            {
                foreach (Transform child in unitListContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            bool isFirst = true;
            Sequence seq = DOTween.Sequence();
            float delay = 0f;

            foreach (var runtimeData in account.OwnedUnits)
            {
                var unitSO = unitCatalog.GetUnitData(runtimeData.UnitID);
                if (unitSO == null) continue;

                if (unitListContainer != null && unitListButtonPrefab != null)
                {
                    var btnGO = Instantiate(unitListButtonPrefab, unitListContainer);
                    btnGO.SetActive(true);
                    
                    btnGO.transform.localScale = Vector3.zero;
                    seq.Insert(delay, btnGO.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
                    delay += 0.05f;

                    var img = btnGO.GetComponent<UnityEngine.UI.Image>();
                    if (img != null) img.sprite = unitSO.icon;

                    var btn = btnGO.GetComponent<UnityEngine.UI.Button>();
                    if (btn != null)
                    {
                        var rd = runtimeData; // capture
                        var so = unitSO;      // capture
                        btn.onClick.AddListener(() => LoadUnit(so, rd));
                    }
                }

                if (isFirst)
                {
                    LoadUnit(unitSO, runtimeData);
                    isFirst = false;
                }
            }
        }

        private void LoadUnit(UnitData unitSO, CelestialCross.Data.RuntimeUnitData runtimeData)
        {
            currentLoadedUnit = unitSO;
            currentLoadedRuntime = runtimeData;

            if (unitSO != null && mainPanel != null)
            {
                mainPanel.LoadUnit(unitSO, runtimeData);

                if (attributesDetailPanel != null)
                {
                    var attr = attributesDetailPanel.GetComponent<UnitDetailPanel_Attributes>();
                    if (attr != null) 
                    {
                        attr.artifactSetCatalog = artifactSetCatalog;
                        attr.Refresh(unitSO, runtimeData);
                    }
                }

                if (petDetailPanel != null)
                {
                    var pet = petDetailPanel.GetComponent<UnitDetailPanel_Pet>();
                    if (pet != null) pet.Refresh(unitSO, runtimeData, petCatalog);
                }

                if (equipmentDetailPanel != null)
                {
                    var eq = equipmentDetailPanel.GetComponent<UnitDetailPanel_Equipment>();
                    if (eq != null) eq.Refresh(unitSO, runtimeData, artifactSetCatalog);
                }

                if (constellationDetailPanel != null)
                {
                    var cons = constellationDetailPanel.GetComponent<UnitDetailPanel_Constellation>();
                    if (cons != null) cons.Refresh(unitSO, runtimeData);
                }

                if (abilitiesDetailPanel != null)
                {
                    var ab = abilitiesDetailPanel.GetComponent<UnitDetailPanel_Abilities>();
                    if (ab != null) 
                    {
                        ab.unitCatalog = unitCatalog;
                        ab.Refresh(unitSO, runtimeData, petCatalog);
                    }
                }
            }
        }

        public void ShowDetailPanel(int tabIndex, string tabName)
        {
            if (isTransitioning || activeDetailPanelIndex == tabIndex) return;
            
            if (bannerUI != null) bannerUI.SetBannerText(tabName);

            GameObject oldPanel = GetPanelByIndex(activeDetailPanelIndex);
            GameObject newPanel = GetPanelByIndex(tabIndex);

            if (oldPanel == null && newPanel != null)
            {
                // First time load
                newPanel.SetActive(true);
                RefreshPanelData(tabIndex);
                activeDetailPanelIndex = tabIndex;
                return;
            }

            if (oldPanel != null && newPanel != null)
            {
                isTransitioning = true;
                panelTransitionFeedback?.PlayFeedbacks();

                CanvasGroup oldCg = oldPanel.GetComponent<CanvasGroup>();
                CanvasGroup newCg = newPanel.GetComponent<CanvasGroup>();
                RectTransform oldRt = oldPanel.GetComponent<RectTransform>();
                RectTransform newRt = newPanel.GetComponent<RectTransform>();

                Sequence seq = DOTween.Sequence();
                
                if (oldCg != null && oldRt != null)
                {
                    oldCg.DOKill();
                    oldRt.DOKill();
                    seq.Join(oldCg.DOFade(0f, 0.15f));
                    seq.Join(oldRt.DOAnchorPosY(-panelSlideDelta, 0.15f).SetEase(Ease.InCubic));
                }

                seq.AppendCallback(() => {
                    oldPanel.SetActive(false);
                    if (oldRt != null) oldRt.anchoredPosition = new Vector2(oldRt.anchoredPosition.x, 0);
                    
                    newPanel.SetActive(true);
                    RefreshPanelData(tabIndex);

                    if (newCg != null && newRt != null)
                    {
                        newCg.DOKill();
                        newRt.DOKill();
                        newCg.alpha = 0f;
                        newRt.anchoredPosition = new Vector2(newRt.anchoredPosition.x, panelSlideDelta);
                        
                        newCg.DOFade(1f, panelFadeDuration);
                        newRt.DOAnchorPosY(0f, panelFadeDuration).SetEase(Ease.OutCubic);
                    }
                });

                seq.OnComplete(() => {
                    isTransitioning = false;
                    activeDetailPanelIndex = tabIndex;
                });
            }
        }

        private GameObject GetPanelByIndex(int index)
        {
            switch (index)
            {
                case 0: return attributesDetailPanel;
                case 1: return petDetailPanel;
                case 2: return equipmentDetailPanel;
                case 3: return constellationDetailPanel;
                case 4: return abilitiesDetailPanel;
                default: return null;
            }
        }

        private void RefreshPanelData(int index)
        {
            switch (index)
            {
                case 0: 
                    attributesDetailPanel.GetComponent<UnitDetailPanel_Attributes>()?.Refresh(currentLoadedUnit, currentLoadedRuntime);
                    break;
                case 1: 
                    petDetailPanel.GetComponent<UnitDetailPanel_Pet>()?.Refresh(currentLoadedUnit, currentLoadedRuntime, petCatalog);
                    break;
                case 2: 
                    equipmentDetailPanel.GetComponent<UnitDetailPanel_Equipment>()?.Refresh(currentLoadedUnit, currentLoadedRuntime, artifactSetCatalog);
                    break;
                case 3: 
                    constellationDetailPanel.GetComponent<UnitDetailPanel_Constellation>()?.Refresh(currentLoadedUnit, currentLoadedRuntime);
                    break;
                case 4: 
                    abilitiesDetailPanel.GetComponent<UnitDetailPanel_Abilities>()?.Refresh(currentLoadedUnit, currentLoadedRuntime, petCatalog);
                    break;
            }
        }

        public void ShowModalOverlay(float duration = 0.2f) 
        {
            if (modalOverlay == null || modalOverlayCanvasGroup == null) return;
            modalOverlay.gameObject.SetActive(true);
            modalOverlayCanvasGroup.DOKill();
            modalOverlayCanvasGroup.DOFade(0.7f, duration).SetUpdate(true);
            modalOpenFeedback?.PlayFeedbacks();
        }
        
        public void HideModalOverlay(float duration = 0.15f) 
        {
            if (modalOverlay == null || modalOverlayCanvasGroup == null) return;
            modalOverlayCanvasGroup.DOKill();
            modalOverlayCanvasGroup.DOFade(0f, duration).SetUpdate(true)
                .OnComplete(() => modalOverlay.gameObject.SetActive(false));
            modalCloseFeedback?.PlayFeedbacks();
        }

        public void ReturnToHub()
        {
            SceneManager.LoadScene("HubScene");
        }
    }
}
