using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace CelestialCross.Scenes.Inventory
{
    public class InventorySceneController : MonoBehaviour
    {
        public static InventorySceneController Instance { get; private set; }

        [Header("Containers")]
        public RectTransform topPanelContainer;
        public RectTransform gridContainer;
        public RectTransform tabBarContainer;
        
        [Header("Modals")]
        public RectTransform modalContainer;
        public PetFilterModal petFilterModal;
        public ArtifactFilterModal artifactFilterModal;
        public ArtifactUpgradeSliderModal upgradeSliderModal;

        [Header("Tabs Config")]
        public List<InventoryTabPanel> tabPanels = new List<InventoryTabPanel>();

        [Header("Data References")]
        public GameObject slotPrefab;
        public PetCatalog petCatalog;
        public ArtifactSetCatalog artifactSetCatalog;

        private InventoryTabPanel currentTab;

        private void Awake()
        {
            Debug.Log($"[InventorySceneController] Awake chamado. Instance: {(Instance != null ? Instance.name : "null")}, this: {name}");
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Debug.LogWarning($"[InventorySceneController] Outra instância ativa detectada no Awake! Destruindo este Canvas ({name}) para evitar duplicatas. Existente: {Instance.name}");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Debug.Log($"[InventorySceneController] Start chamado. AccountManager.Instance: {(AccountManager.Instance != null ? "exists" : "null")}, PlayerAccount: {(AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null ? "loaded" : "null")}");
            if (AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
            {
                Debug.Log("[InventorySceneController] AccountManager pronto. Inicializando abas...");
                InitializeTabs();
            }
            else
            {
                Debug.Log("[InventorySceneController] AccountManager não pronto ou nulo. Aguardando OnAccountReady...");
                AccountManager.OnAccountReady += InitializeTabs;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            AccountManager.OnAccountReady -= InitializeTabs;
        }

        public void InitializeTabs()
        {
            Debug.Log($"[InventorySceneController] InitializeTabs chamado. tabPanels count: {tabPanels.Count}");
            foreach (var tab in tabPanels)
            {
                if (tab == null)
                {
                    Debug.LogError("[InventorySceneController] Encontrado painel de aba NULO na lista tabPanels!");
                    continue;
                }
                Debug.Log($"[InventorySceneController] Escondendo aba: {tab.name}");
                tab.Hide();
            }

            if (tabPanels.Count > 0)
            {
                if (tabPanels[0] != null)
                {
                    Debug.Log($"[InventorySceneController] Selecionando aba inicial: {tabPanels[0].name}");
                    SelectTab(tabPanels[0]);
                }
                else
                {
                    Debug.LogError("[InventorySceneController] A aba inicial (índice 0) é nula!");
                }
            }
            else
            {
                Debug.LogWarning("[InventorySceneController] Lista tabPanels vazia! Nenhuma aba foi selecionada.");
            }
        }

        public void SelectTab(InventoryTabPanel tabToSelect)
        {
            if (tabToSelect == null)
            {
                Debug.LogError("[InventorySceneController] Tentou selecionar uma aba nula!");
                return;
            }
            Debug.Log($"[InventorySceneController] SelectTab: {tabToSelect.name}. Aba atual: {(currentTab != null ? currentTab.name : "null")}");
            if (currentTab == tabToSelect) return;

            if (currentTab != null)
            {
                currentTab.Hide();
            }
            
            currentTab = tabToSelect;
            
            if (currentTab != null)
            {
                currentTab.Show();
            }
        }

        public void ReturnToHub()
        {
            // O nome da cena base será configurado no GameFlowManager, mas para segurança chamamos pelo nome.
            SceneManager.LoadScene("HubScene");
        }
    }
}
