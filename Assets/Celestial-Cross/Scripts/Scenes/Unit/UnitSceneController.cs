using UnityEngine;
using UnityEngine.SceneManagement;

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

        private UnitData currentLoadedUnit;
        private CelestialCross.Data.RuntimeUnitData currentLoadedRuntime;

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

            foreach (var runtimeData in account.OwnedUnits)
            {
                var unitSO = unitCatalog.GetUnitData(runtimeData.UnitID);
                if (unitSO == null) continue;

                if (unitListContainer != null && unitListButtonPrefab != null)
                {
                    var btnGO = Instantiate(unitListButtonPrefab, unitListContainer);
                    btnGO.SetActive(true);

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
            if (bannerUI != null) bannerUI.SetBannerText(tabName);
            
            // Esconder tudo
            if (attributesDetailPanel) attributesDetailPanel.SetActive(false);
            if (petDetailPanel) petDetailPanel.SetActive(false);
            if (equipmentDetailPanel) equipmentDetailPanel.SetActive(false);
            if (constellationDetailPanel) constellationDetailPanel.SetActive(false);
            if (abilitiesDetailPanel) abilitiesDetailPanel.SetActive(false);

            // Mostrar ativado e Forçar Refresh para consertar o bug do BetterUI limpando as imagens
            switch (tabIndex)
            {
                case 0: 
                    if (attributesDetailPanel) 
                    { 
                        attributesDetailPanel.SetActive(true); 
                        var attr = attributesDetailPanel.GetComponent<UnitDetailPanel_Attributes>();
                        if (attr != null) 
                        {
                            attr.artifactSetCatalog = artifactSetCatalog;
                            attr.Refresh(currentLoadedUnit, currentLoadedRuntime);
                        }
                    } 
                    break;
                case 1: 
                    if (petDetailPanel) 
                    { 
                        petDetailPanel.SetActive(true); 
                        petDetailPanel.GetComponent<UnitDetailPanel_Pet>()?.Refresh(currentLoadedUnit, currentLoadedRuntime, petCatalog);
                    } 
                    break;
                case 2: 
                    if (equipmentDetailPanel) 
                    { 
                        equipmentDetailPanel.SetActive(true); 
                        equipmentDetailPanel.GetComponent<UnitDetailPanel_Equipment>()?.Refresh(currentLoadedUnit, currentLoadedRuntime, artifactSetCatalog);
                    } 
                    break;
                case 3: 
                    if (constellationDetailPanel) 
                    { 
                        constellationDetailPanel.SetActive(true); 
                        constellationDetailPanel.GetComponent<UnitDetailPanel_Constellation>()?.Refresh(currentLoadedUnit, currentLoadedRuntime);
                    } 
                    break;
                case 4: 
                    if (abilitiesDetailPanel) 
                    { 
                        abilitiesDetailPanel.SetActive(true); 
                        var ab = abilitiesDetailPanel.GetComponent<UnitDetailPanel_Abilities>();
                        if (ab != null) { ab.unitCatalog = unitCatalog; ab.Refresh(currentLoadedUnit, currentLoadedRuntime, petCatalog); }
                    } 
                    break;
            }
        }

        public void ReturnToHub()
        {
            SceneManager.LoadScene("HubScene");
        }
    }
}
