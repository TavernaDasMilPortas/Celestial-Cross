using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DG.Tweening;

namespace CelestialCross.Scenes.Inventory
{
    [global::System.Serializable]
    public class TabVisualHierarchy
    {
        public InventoryTabPanel tabPanel;
        public RectTransform rootComponent;
        public RectTransform middleComponent;
        [Tooltip("Arraste apenas o botão da tab para ser animado")]
        public RectTransform tabButtonRect;
    }

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
        public RectTransform activeTabContainer;
        public List<TabVisualHierarchy> tabVisualHierarchies = new List<TabVisualHierarchy>();

        // Lista antiga mantida escondida para não quebrar referências locais por enquanto
        [HideInInspector]
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

            // Sincronizando referências caso algo procure pela lista antiga
            tabPanels.Clear();
            foreach (var tabInfo in tabVisualHierarchies)
            {
                if (tabInfo.tabPanel != null)
                    tabPanels.Add(tabInfo.tabPanel);
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
            Debug.Log($"[InventorySceneController] InitializeTabs chamado. tabVisualHierarchies count: {tabVisualHierarchies.Count}");
            
            float animationDelay = 0f;

            foreach (var tabInfo in tabVisualHierarchies)
            {
                if (tabInfo == null || tabInfo.tabPanel == null)
                {
                    Debug.LogError("[InventorySceneController] Encontrado painel de aba NULO na lista tabVisualHierarchies!");
                    continue;
                }
                
                Debug.Log($"[InventorySceneController] Escondendo aba: {tabInfo.tabPanel.name}");
                tabInfo.tabPanel.Hide();

                // Reparenting inicial: garantir que todos estão no seu root mantendo a posição mundial
                if (tabInfo.middleComponent != null && tabInfo.rootComponent != null)
                {
                    tabInfo.middleComponent.SetParent(tabInfo.rootComponent, true);
                }

                // Animação de pop-in inicial EXCLUSIVA para os botões
                if (tabInfo.tabButtonRect != null)
                {
                    tabInfo.tabButtonRect.localScale = Vector3.zero;
                    tabInfo.tabButtonRect.DOScale(Vector3.one, 0.4f)
                        .SetEase(Ease.OutBack)
                        .SetDelay(animationDelay);
                        
                    animationDelay += 0.1f; // cascata
                }
            }

            if (tabVisualHierarchies.Count > 0)
            {
                if (tabVisualHierarchies[0] != null && tabVisualHierarchies[0].tabPanel != null)
                {
                    Debug.Log($"[InventorySceneController] Selecionando aba inicial: {tabVisualHierarchies[0].tabPanel.name}");
                    SelectTab(tabVisualHierarchies[0].tabPanel);
                }
                else
                {
                    Debug.LogError("[InventorySceneController] A aba inicial (índice 0) é nula!");
                }
            }
            else
            {
                Debug.LogWarning("[InventorySceneController] Lista tabVisualHierarchies vazia! Nenhuma aba foi selecionada.");
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

            if (CelestialCross.Audio.AudioManager.Instance != null)
                CelestialCross.Audio.AudioManager.Instance.PlayUI(CelestialCross.Audio.SoundKey.DropdownMenu01);

            // Lidar com a aba que estava ativa antes
            if (currentTab != null)
            {
                currentTab.Hide();
                var previousTabInfo = tabVisualHierarchies.Find(t => t.tabPanel == currentTab);
                if (previousTabInfo != null && previousTabInfo.middleComponent != null && previousTabInfo.rootComponent != null)
                {
                    previousTabInfo.middleComponent.SetParent(previousTabInfo.rootComponent, true);
                }
            }
            
            currentTab = tabToSelect;
            
            // Lidar com a nova aba ativa
            if (currentTab != null)
            {
                currentTab.Show();
                var newTabInfo = tabVisualHierarchies.Find(t => t.tabPanel == currentTab);
                if (newTabInfo != null)
                {
                    // Reparentar para o Active Tab Container mantendo a posição original na tela
                    if (newTabInfo.middleComponent != null && activeTabContainer != null)
                    {
                        newTabInfo.middleComponent.SetParent(activeTabContainer, true);
                    }

                    // Animação de pulso no botão
                    if (newTabInfo.tabButtonRect != null)
                    {
                        newTabInfo.tabButtonRect.DOKill(true); // Cancela qualquer tween anterior no botão
                        newTabInfo.tabButtonRect.localScale = Vector3.one; // Reseta para garantir
                        newTabInfo.tabButtonRect.localEulerAngles = Vector3.zero; // Reseta para garantir
                        
                        // O DOPunchScale vai animar e ao final retorna ao scale original, garantimos o Vector3.one no OnComplete também
                        newTabInfo.tabButtonRect.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.3f, 5, 0.5f)
                            .OnComplete(() => newTabInfo.tabButtonRect.localScale = Vector3.one);
                        newTabInfo.tabButtonRect.DOPunchRotation(new Vector3(0, 0, -5f), 0.3f, 5, 0.5f)
                            .OnComplete(() => newTabInfo.tabButtonRect.localEulerAngles = Vector3.zero);
                    }
                }
            }
        }

        public void ReturnToHub()
        {
            // O nome da cena base será configurado no GameFlowManager, mas para segurança chamamos pelo nome.
            CelestialCross.System.SceneTransitionManager.LoadScene("HubScene");
        }
    }
}
