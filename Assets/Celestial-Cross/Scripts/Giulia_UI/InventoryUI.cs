using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using CelestialCross.Giulia_UI;
using CelestialCross.Data.Pets;
using CelestialCross.System;


/// <summary>
/// (Fase 2) UI de inventÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡rio modular com 3 abas (Unidades, Pets, Artefatos).
/// Layout split-screen: Painel Superior (detalhes dinÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢micos) + Painel Inferior (grid/scroll).
/// Suporta troca por toque nas abas ou swipe horizontal.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Data Catalogs")]
    [Tooltip("NecessÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡rio para renderizar detalhes da Unidade (ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â­cone, stats, skills)")]
    public UnitCatalog unitCatalog;
    public PetCatalog petCatalog;
    public ArtifactSetCatalog artifactSetCatalog;
    public LevelingConfig levelingConfig;

    [Header("Abas")]
    [Tooltip("Arrastar as 3 InventoryTab (Unidades, Pets, Artefatos) na ordem")]
    public InventoryTab[] tabs;

    [Header("IntegraÃƒÆ’Ã‚Â¯Ãƒâ€šÃ‚Â¿Ãƒâ€šÃ‚Â½ÃƒÆ’Ã‚Â¯Ãƒâ€šÃ‚Â¿Ãƒâ€šÃ‚Â½o Aba de Itens")]
    public ItemsInventoryUI itemsInventoryPanel;

    [Header("ConteÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Âºdo Inferior (Grids)")]
    [Tooltip("Um RectTransform com GridLayoutGroup para cada aba, na mesma ordem das tabs")]
    public RectTransform[] gridContainers;

    [Header("Item Prefab (Opcional)")]
    [Tooltip("Prefab de cada item do grid (Button + Image/Text). Se vazio, o UI ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â© criado via cÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â³digo.")]
    public GameObject slotPrefab;

    [Header("Split Layout")]
    [Tooltip("Altura do painel superior em proporÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o (0..1). Ex: 0.45 = 45% superior.")]
    [Range(0.25f, 0.75f)]
    public float topPanelHeightNormalized = 0.45f;

    [Tooltip("Se true, ajusta anchors/ScrollRect e cria TopPanels automaticamente em runtime.")]
    

    [Header("Grid Config")]
    public int columns = 3;
    public Vector2 cellSize    = new Vector2(90f, 90f);
    public Vector2 cellSpacing = new Vector2(10f, 10f);

    [Header("Swipe")]
    [Tooltip("ReferÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Âªncia ao SwipeDetector (pode estar no mesmo GameObject)")]
    public SwipeDetector swipeDetector;

    private int currentTabIndex = 0;

    // --- Phase 3 & 4 State ---
    private bool isSelectingArtifact = false;
    private bool isSelectingPet = false;
    private CelestialCross.Artifacts.ArtifactType selectingForSlot;
    private string selectingForUnitId;
    
    // UI Refs for dynamic top panels
    public Image unitIconImage;
    public TextMeshProUGUI unitStatsText;
    public TextMeshProUGUI unitAbilitiesText;
    public RectTransform unitAbilitiesContainer;
    
    public RectTransform unitEquipContainer;
    public Button[] unitEquipButtons = new Button[7];
    public TextMeshProUGUI[] unitEquipTexts = new TextMeshProUGUI[7];

    public Button equipArtifactButton;
    public TextMeshProUGUI equipArtifactText;
    public Button cancelEquipButton;
    public Button unequipArtifactButton;
    [Header("Artifact Management UI")]
    public Button manageArtifactButton;
    public ArtifactUpgradeModal upgradeModal;

    [Header("Pet Management UI")]
    public Button managePetButton;
    public PetManageModal petManageModal;
    
    [Header("Leveling UI (Phase 1)")]
    public TMP_Text unitLevelText;
    public Image unitXPBar;
    public TMP_Text unitXPText;

    [Header("Constellation UI (Phase 2)")]
    public ConstellationModal constellationModal;
    public Image[] constellationStars = new Image[6];
    public Button constellationButton;
    public TMP_Text insigniaCountText;

    [Header("Unit Sub-Tabs Configuration")]
    public Button unitSubTabEquipButton;
    public Button unitSubTabConstellationButton;
    public Button unitSubTabSkillsButton;
    public GameObject unitSubPanelEquip;
    public GameObject unitSubPanelConstellation;
    public GameObject unitSubPanelSkills;
    public CelestialCross.UI.Skills.SkillTabUI skillTab;
    
    private CelestialCross.Artifacts.ArtifactInstanceData selectedArtifactToEquip;
    private RuntimePetData selectedPetInstance;
    private string selectedPetToEquipId;
    private string currentlySelectedTabItemId;
    private string originEquippedId;
    private string defaultStatsText;
    // -------------------------

    private readonly List<GameObject>[] spawnedItemsPerTab = new List<GameObject>[3]
    {
        new List<GameObject>(),
        new List<GameObject>(),
        new List<GameObject>()
    };

    public RectTransform[] topPanels;
    public TextMeshProUGUI[] topPanelTexts;
    public GameObject[] bottomScrollRoots;

    public RectTransform tabsBar;
    private const float TabsBarHeight = 80f;

    private enum InventoryKind
    {
        Units = 0,
        Pets = 1,
        Artifacts = 2
    }

    // =============================
    // LIFECYCLE
    // =============================

    void Start()
    {
        Debug.Log("[InventoryUI] Start chamado.");
        if (unitCatalog == null)
            Debug.LogWarning("InventoryUI: UnitCatalog não atribuído! A UI de Constelação e Detalhes pode não funcionar corretamente.");

        InitializeTabs();
        RegisterSwipe();

        if (manageArtifactButton != null)
            manageArtifactButton.onClick.AddListener(OnManageArtifactClicked);

        if (managePetButton != null)
            managePetButton.onClick.AddListener(OnManagePetClicked);

        WireUpFixedButtons();
        InitializeSubTabs();
        
        if (constellationButton != null)
            constellationButton.onClick.AddListener(OnConstellationUpgradeClicked);
        
        // Auto-criação do AccountManager para testes diretos no editor
        if (AccountManager.Instance == null)
        {
            Debug.Log("[InventoryUI] AccountManager não encontrado. Criando AccountManager dinâmico para depuração...");
            var go = new GameObject("AccountManager_AutoCreated");
            go.AddComponent<AccountManager>();
        }

        Debug.Log($"[InventoryUI] AccountManager.Instance: {AccountManager.Instance != null}, PlayerAccount: {(AccountManager.Instance != null ? AccountManager.Instance.PlayerAccount != null : false)}");

        if (AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
        {
            Debug.Log("[InventoryUI] Conta já disponível. Renderizando abas...");
            RefreshAllTabs();
            SwitchToTab(0);
        }
        else
        {
            Debug.Log("[InventoryUI] Conta não disponível ainda. Registrando listener do OnAccountReady...");
            AccountManager.OnAccountReady += HandleAccountReady;
        }
    }

    private void HandleAccountReady()
    {
        Debug.Log("[InventoryUI] Evento OnAccountReady recebido. Renderizando abas...");
        AccountManager.OnAccountReady -= HandleAccountReady;
        RefreshAllTabs();
        SwitchToTab(0);
    }

    public void OnConstellationUpgradeClicked()
    {
        Debug.Log($"[InventoryUI] Botão de Constelação clicado para unidade: {selectingForUnitId}");
        if (string.IsNullOrEmpty(selectingForUnitId))
        {
            Debug.LogWarning("[InventoryUI] Nenhuma unidade selecionada!");
            return;
        }
        
        if (constellationModal != null)
        {
            constellationModal.Open(selectingForUnitId);
        }
        else
        {
            Debug.LogError("[InventoryUI] Referência do ConstellationModal está nula no Inspector!");
        }
    }

    private void WireUpFixedButtons()
    {
        if (equipArtifactButton != null)
            equipArtifactButton.onClick.AddListener(ConfirmEquip);
            
        if (cancelEquipButton != null)
            cancelEquipButton.onClick.AddListener(CancelEquipMode);
            
        if (unequipArtifactButton != null)
            unequipArtifactButton.onClick.AddListener(ConfirmUnequip);

        if (unitEquipButtons != null && unitEquipButtons.Length == 7)
        {
            var slotTypes = (CelestialCross.Artifacts.ArtifactType[])System.Enum.GetValues(typeof(CelestialCross.Artifacts.ArtifactType));
            for (int i = 0; i < 7; i++)
            {
                if (unitEquipButtons[i] == null) continue;
                
                int sIdx = i;
                bool isPetSlot = sIdx == 6;
                var sType = isPetSlot ? default : slotTypes[UnityEngine.Mathf.Min(sIdx, slotTypes.Length - 1)];

                unitEquipButtons[i].onClick.AddListener(() => {
                    if (isPetSlot) OnUnitPetSlotClicked();
                    else OnUnitEquipSlotClicked(sType);
                });
            }
        }
    }

    private void InitializeSubTabs()
    {
        if (unitSubTabEquipButton != null) unitSubTabEquipButton.onClick.AddListener(() => SwitchUnitSubTab(0));
        if (unitSubTabConstellationButton != null) unitSubTabConstellationButton.onClick.AddListener(() => SwitchUnitSubTab(1));
        if (unitSubTabSkillsButton != null) unitSubTabSkillsButton.onClick.AddListener(() => SwitchUnitSubTab(2));
        
        SwitchUnitSubTab(0); // Equipamento ativo por padrão
    }

    public void SwitchUnitSubTab(int subTabIndex)
    {
        if (unitSubPanelEquip != null) unitSubPanelEquip.SetActive(subTabIndex == 0);
        if (unitSubPanelConstellation != null) unitSubPanelConstellation.SetActive(subTabIndex == 1);
        if (unitSubPanelSkills != null) unitSubPanelSkills.SetActive(subTabIndex == 2);

        UpdateSubTabButtonVisual(unitSubTabEquipButton, subTabIndex == 0);
        UpdateSubTabButtonVisual(unitSubTabConstellationButton, subTabIndex == 1);
        UpdateSubTabButtonVisual(unitSubTabSkillsButton, subTabIndex == 2);
    }

    private void UpdateSubTabButtonVisual(Button btn, bool isActive)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = isActive ? new Color(0.4f, 0.35f, 0.45f, 1f) : new Color(0.2f, 0.18f, 0.22f, 1f);
        }
        var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.color = isActive ? Color.white : Color.gray;
        }
    }

    private void OnEnable()
    {
        // When the panel is reopened, refresh lists from Account.
        if (Application.isPlaying)
        {
            if (AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
                RefreshAllTabs();
        }
    }

    void OnDestroy()
    {
        AccountManager.OnAccountReady -= HandleAccountReady;
        UnregisterSwipe();

        if (tabs != null)
        {
            foreach (var tab in tabs)
            {
                if (tab != null)
                    tab.OnTabClicked -= SwitchToTab;
            }
        }
    }

    // =============================
    // INICIALIZAÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â¡ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢O
    // =============================

    
        void InitializeTabs()
    {
        if (tabs == null) return;
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] == null) continue;
            tabs[i].tabIndex = i;
            tabs[i].OnTabClicked += SwitchToTab;
        }
        if (tabs.Length > 0 && tabs[0] != null) tabs[0].SetTitle("Unidades");
        if (tabs.Length > 1 && tabs[1] != null) tabs[1].SetTitle("Pets");
        if (tabs.Length > 2 && tabs[2] != null) tabs[2].SetTitle("Artefatos");
        if (tabs.Length > 3 && tabs[3] != null) tabs[3].SetTitle("Itens");
    }

    void RegisterSwipe()
    {
        if (swipeDetector == null) return;
        swipeDetector.OnSwipeLeft  += OnSwipeLeft;
        swipeDetector.OnSwipeRight += OnSwipeRight;
    }

    void UnregisterSwipe()
    {
        if (swipeDetector == null) return;
        swipeDetector.OnSwipeLeft  -= OnSwipeLeft;
        swipeDetector.OnSwipeRight -= OnSwipeRight;
    }

    // =============================
    // TROCA DE ABAS
    // =============================

    public void SwitchToTab(int index)
    {
        if (itemsInventoryPanel != null) itemsInventoryPanel.gameObject.SetActive(index == 3);
        if (tabs == null || gridContainers == null) return;
        if (index < 0 || index >= tabs.Length) return;

        // Automatically cancel any equip mode selection if we are navigating away from the target tab
        if (isSelectingArtifact && index != 2)
            CancelEquipMode();
        else if (isSelectingPet && index != 1)
            CancelEquipMode();

        if (index != 0 && skillTab != null)
        {
            if (skillTab.selectionModal != null) skillTab.selectionModal.gameObject.SetActive(false);
            if (skillTab.branchModal != null) skillTab.branchModal.gameObject.SetActive(false);
        }

        currentTabIndex = index;

        // Reset visibility of action buttons when switching
        if (manageArtifactButton != null) manageArtifactButton.gameObject.SetActive(false);
        if (managePetButton != null) managePetButton.gameObject.SetActive(false);
        if (equipArtifactButton != null) equipArtifactButton.gameObject.SetActive(false);
        if (cancelEquipButton != null) cancelEquipButton.gameObject.SetActive(false);
        if (unequipArtifactButton != null) unequipArtifactButton.gameObject.SetActive(false);
        
        selectedPetToEquipId = null;
        selectedArtifactToEquip = null;
        selectedPetInstance = null;

        // Ativar/desativar top panel + bottom area
        if (topPanels != null)
        {
            for (int i = 0; i < topPanels.Length; i++)
            {
                if (topPanels[i] != null)
                    topPanels[i].gameObject.SetActive(i == index && index != 3);
            }
        }

        if (bottomScrollRoots != null)
        {
            for (int i = 0; i < bottomScrollRoots.Length; i++)
            {
                if (bottomScrollRoots[i] != null)
                    bottomScrollRoots[i].SetActive(i == index && index != 3);
            }
        }
        else
        {
            // Fallback for old scene wiring: show/hide raw containers
            for (int i = 0; i < gridContainers.Length; i++)
            {
                if (gridContainers[i] != null)
                    gridContainers[i].gameObject.SetActive(i == index && index != 3);
            }
        }

        // Atualizar visual das abas
        for (int i = 0; i < tabs.Length; i++)
            tabs[i].SetActive(i == index);

        if (index == 3 && itemsInventoryPanel != null) itemsInventoryPanel.RefreshGrid(); else EnsureDefaultSelection(index);
    }

    // =============================
    // SWIPE HANDLERS
    // =============================

    void OnSwipeLeft()
    {
        // Swipe para esquerda ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â ÃƒÂ¢Ã¢â€šÂ¬Ã¢â€žÂ¢ prÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â³xima aba
        int next = currentTabIndex + 1;
        if (next < tabs.Length)
            SwitchToTab(next);
    }

    void OnSwipeRight()
    {
        // Swipe para direita ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â ÃƒÂ¢Ã¢â€šÂ¬Ã¢â€žÂ¢ aba anterior
        int prev = currentTabIndex - 1;
        if (prev >= 0)
            SwitchToTab(prev);
    }

    // =============================
    // SPLIT-SCREEN LAYOUT
    // =============================

    
    private void RefreshAllTabs()
    {
        Debug.Log($"[InventoryUI] RefreshAllTabs chamado. gridContainers nulo? {gridContainers == null}");
        if (gridContainers == null) return;
        Debug.Log($"[InventoryUI] gridContainers.Length: {gridContainers.Length}");

        for (int i = 0; i < gridContainers.Length; i++)
            PopulateTab(i);
    }

private void PopulateTab(int tabIndex)
    {
        Debug.Log($"[InventoryUI] PopulateTab({tabIndex}) chamado. gridContainers.Length: {(gridContainers != null ? gridContainers.Length : 0)}");
        if (gridContainers == null || tabIndex < 0 || tabIndex >= gridContainers.Length) return;
        var container = gridContainers[tabIndex];
        Debug.Log($"[InventoryUI] Container para tab {tabIndex}: {(container != null ? container.name : "NULO")}");
        if (container == null) return;

        ClearSpawned(tabIndex);

        var account = AccountManager.Instance != null ? AccountManager.Instance.PlayerAccount : null;
        Debug.Log($"[InventoryUI] PopulateTab - AccountManager.Instance: {AccountManager.Instance != null}, account nulo? {account == null}");

        if (account == null)
        {
            Debug.Log("[InventoryUI] Account nulo. Renderizando placeholders de debug...");
            // No account available: show placeholders.
            for (int i = 0; i < 12; i++)
                SpawnItem(tabIndex, container, "", $"Item {i + 1}", () => SetDetails(tabIndex, $"Item {i + 1}"));
            return;
        }

        account.EnsureInitialized();
        Debug.Log($"[InventoryUI] Unidades: {account.OwnedUnitIDs?.Count}, Pets: {account.OwnedRuntimePets?.Count}, Artefatos: {account.OwnedArtifacts?.Count}");

        switch ((InventoryKind)Mathf.Clamp(tabIndex, 0, 2))
        {
            case InventoryKind.Units:
                if (account.OwnedUnitIDs != null)
                {
                    Debug.Log($"[InventoryUI] Populando Unidades. Quantidade de IDs de unidade: {account.OwnedUnitIDs.Count}");
                    for (int i = 0; i < account.OwnedUnitIDs.Count; i++)
                    {
                        string id = account.OwnedUnitIDs[i];
                        if (string.IsNullOrWhiteSpace(id)) continue;
                        
                        Sprite icon = null;
                        string label = "Unidade";
                        if (unitCatalog != null) {
                            var data = unitCatalog.GetUnitData(id);
                            if (data != null) 
                            {
                                icon = data.icon;
                                label = data.displayName;
                            }
                        }
                        
                        SpawnItem(tabIndex, container, id, label, () => ShowUnitDetails(id, account), icon);
                    }
                }
                break;

            case InventoryKind.Pets:
                if (account.OwnedRuntimePets != null)
                {
                    Debug.Log($"[InventoryUI] Populando Pets. Quantidade de Pets: {account.OwnedRuntimePets.Count}");
                    for (int i = 0; i < account.OwnedRuntimePets.Count; i++)
                    {
                        var pet = account.OwnedRuntimePets[i];
                        if (pet == null) continue;
                        string id = pet.UUID;
                        
                        if (isSelectingPet) { if (id == originEquippedId) { /* keep it */ } else if (account.IsPetEquipped(id)) continue; }

                        Sprite icon = null;
                        string speciesName = pet.DisplayName;
                        if (petCatalog != null)
                        {
                            var speciesData = petCatalog.GetPetSpecies(pet.SpeciesID);
                            if (speciesData != null)
                            {
                                icon = speciesData.Icon;
                                speciesName = speciesData.SpeciesName;
                            }
                        }

                        string label = $"{speciesName}\n<size=10>{pet.RarityStars}* Lvl:{pet.CurrentLevel}</size>";
                        
                        SpawnItem(tabIndex, container, id, label, () => OnPetClicked(id), icon);
                    }
                }
                break;

            case InventoryKind.Artifacts:
                if (account.OwnedArtifacts != null)
                {
                    Debug.Log($"[InventoryUI] Populando Artefatos. Quantidade: {account.OwnedArtifacts.Count}");
                    for (int i = 0; i < account.OwnedArtifacts.Count; i++)
                    {
                        var a = account.OwnedArtifacts[i];
                        if (a == null) continue;

                        // Phase 4 filter
                        if (isSelectingArtifact) { if (a.slot != selectingForSlot) continue; if (a.idGUID == originEquippedId) { /* keep it */ } else if (account.IsArtifactEquipped(a.idGUID)) continue; }

                        Sprite icon = null;
                        if (artifactSetCatalog != null)
                        {
                            var set = artifactSetCatalog.GetSetById(a.artifactSetId);
                            if (set != null) icon = set.GetIconForSlot(a.slot);
                        }
                        string label = $"{a.slot}\\n{a.rarity} {a.GetStarsAsIntClamped()}* +{a.currentLevel}\\n<size=10>{UIStatFormatter.FormatStat(a.mainStat)}</size>";
                        SpawnItem(tabIndex, container, a.idGUID, label, () => OnArtifactClicked(a), icon);
                    }
                }
                break;
        }

        // Empty-state messaging
        Debug.Log($"[InventoryUI] spawnedItemsPerTab[{tabIndex}].Count = {spawnedItemsPerTab[tabIndex].Count}");
        if (spawnedItemsPerTab[tabIndex].Count == 0)
        {
            string emptyLabel = tabIndex == 0 ? "(sem unidades)" : tabIndex == 1 ? "(sem pets)" : "(sem artefatos)";
            SpawnItem(tabIndex, container, "", emptyLabel, () => SetDetails(tabIndex, emptyLabel));
        }
    }

    private void ClearSpawned(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= spawnedItemsPerTab.Length) return;
        var list = spawnedItemsPerTab[tabIndex];
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null)
                Destroy(list[i]);
        }
        list.Clear();
    }

    private void SpawnItem(int tabIndex, RectTransform parent, string itemId, string label, Action onClick, Sprite icon = null)
    {
        Debug.Log($"[InventoryUI] SpawnItem - tabIndex: {tabIndex}, label: {label}, icon? {icon != null}");
        bool isSelected = (!string.IsNullOrEmpty(itemId) && itemId == currentlySelectedTabItemId);
        bool isEquippedTarget = false;
        
        if (isSelectingPet && tabIndex == 1) isEquippedTarget = (!string.IsNullOrEmpty(itemId) && itemId == originEquippedId);
        if (isSelectingArtifact && tabIndex == 2) isEquippedTarget = (!string.IsNullOrEmpty(itemId) && itemId == originEquippedId);

        if (parent == null) return;

        GameObject go;
        if (slotPrefab != null)
        {
            go = Instantiate(slotPrefab, parent);
            go.SetActive(true); // Forçar ativação já que o prefab modelo pode estar inativo

            var iconImg = go.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImg != null)
            {
                if (icon != null)
                {
                    iconImg.gameObject.SetActive(true);
                    iconImg.sprite = icon;
                }
                else
                {
                    iconImg.gameObject.SetActive(false);
                }
            }

            var labelTxt = go.transform.Find("Label")?.GetComponent<TextMeshProUGUI>() ?? go.GetComponentInChildren<TextMeshProUGUI>();
            if (labelTxt != null)
            {
                labelTxt.text = label;
                if (icon != null && iconImg != null)
                {
                    labelTxt.gameObject.SetActive(false); // Esconder texto se houver ícone
                }
                else
                {
                    labelTxt.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            go = BuildDefaultGridItem(parent, label, icon);
        }

        if (go == null) return;
        spawnedItemsPerTab[tabIndex].Add(go);

        var button = go.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(() => { onClick(); RefreshAllTabs(); }); // Atualiza a borda quando clica
            }
        }

        var outline = go.GetComponent<UnityEngine.UI.Outline>();
        if (outline == null) outline = go.AddComponent<UnityEngine.UI.Outline>();
        
        if (isSelected) 
        {
            outline.enabled = true;
            outline.effectColor = isEquippedTarget ? new UnityEngine.Color(0.2f, 1f, 0.2f, 1f) : new UnityEngine.Color(1f, 0.9f, 0.2f, 1f); // Verde para equipado, Amarelo para seleÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o normal
            outline.effectDistance = new UnityEngine.Vector2(3, 3);
        }
        else if (isEquippedTarget)
        {
            outline.enabled = true;
            outline.effectColor = new UnityEngine.Color(0.2f, 1f, 0.2f, 0.5f); // Verde fraco para jÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡ equipado mas nÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o selecionado
            outline.effectDistance = new UnityEngine.Vector2(2, 2);
        }
        else
        {
            outline.enabled = false;
        }
    }

    private static GameObject BuildDefaultGridItem(RectTransform parent, string label, Sprite icon)
    {
        var root = new GameObject("GridItem", typeof(RectTransform), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);

        var img = root.GetComponent<Image>();
        img.color = new Color(0.25f, 0.23f, 0.2f, 0.85f);

        if (icon != null)
        {
            var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(root.transform, false);
            var iRt = (RectTransform)iconGO.transform;
            iRt.anchorMin = new Vector2(0, 0);
            iRt.anchorMax = new Vector2(1, 1);
            iRt.offsetMin = new Vector2(4, 4);
            iRt.offsetMax = new Vector2(-4, -4);
            var iconImg = iconGO.GetComponent<Image>();
            iconImg.sprite = icon;
            iconImg.preserveAspect = true;
        }

        var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(root.transform, false);
        var rt = (RectTransform)textGO.transform;
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(8, 8);
        rt.offsetMax = new Vector2(-8, -8);

        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = icon != null ? 16 : 22; // Smaller text if has icon
        tmp.alignment = icon != null ? TextAlignmentOptions.Bottom : TextAlignmentOptions.Center; // Bottom if has icon
        tmp.enableWordWrapping = true;
        tmp.color = Color.white;
        
        // Se tiver icone, poderiamos esconder o texto (temporariamente vamos mostrar embaixo com fonte menor)
        if (icon != null)
        {
            // Hide text if icon exists
            textGO.SetActive(false);
        }

        return root;
    }

    private void EnsureDefaultSelection(int tabIndex)
    {
        if (topPanelTexts == null || tabIndex < 0 || tabIndex >= topPanelTexts.Length) return;

        // If there are spawned items, click the first one to populate details.
        var list = spawnedItemsPerTab[tabIndex];
        if (list == null || list.Count == 0)
            return;

        var button = list[0] != null ? list[0].GetComponent<Button>() : null;
        if (button != null)
            button.onClick?.Invoke();
        else
            topPanelTexts[tabIndex].text = "(selecione um item abaixo)";
    }

    private void SetDetails(int tabIndex, string text)
    {
        if (topPanelTexts == null || tabIndex < 0 || tabIndex >= topPanelTexts.Length) return;
        if (topPanelTexts[tabIndex] != null)
            topPanelTexts[tabIndex].text = string.IsNullOrWhiteSpace(text) ? "" : text;
    }

    private void ProcessStatData(CelestialCross.Artifacts.StatModifierData stat, ref float hF, ref float hP, ref float aF, ref float aP, ref float dF, ref float dP, ref float spdF, ref float crF, ref float eaf, ref float cdF, ref float erf)
    {
        if (stat == null) return;
        switch (stat.statType)
        {
            case CelestialCross.Artifacts.StatType.HealthFlat: hF += stat.value; break;
            case CelestialCross.Artifacts.StatType.HealthPercent:  hP += stat.value; break;
            case CelestialCross.Artifacts.StatType.AttackFlat: aF += stat.value; break;
            case CelestialCross.Artifacts.StatType.AttackPercent:  aP += stat.value; break;
            case CelestialCross.Artifacts.StatType.DefenseFlat:dF += stat.value; break;
            case CelestialCross.Artifacts.StatType.DefensePercent: dP += stat.value; break;
            case CelestialCross.Artifacts.StatType.Speed:  spdF += stat.value; break;
            case CelestialCross.Artifacts.StatType.CriticalRate: crF += stat.value; break;
            case CelestialCross.Artifacts.StatType.CriticalDamage: cdF += stat.value; break;
            case CelestialCross.Artifacts.StatType.EffectHitRate: eaf += stat.value; break;
            case CelestialCross.Artifacts.StatType.EffectResistance: erf += stat.value; break;
        }
    }

    private void ProcessStatData(CelestialCross.Artifacts.StatModifier stat, ref float hF, ref float hP, ref float aF, ref float aP, ref float dF, ref float dP, ref float spdF, ref float crF, ref float eaf, ref float cdF, ref float erf)
    {
        switch (stat.statType)
        {
            case CelestialCross.Artifacts.StatType.HealthFlat: hF += stat.value; break;
            case CelestialCross.Artifacts.StatType.HealthPercent:  hP += stat.value; break;
            case CelestialCross.Artifacts.StatType.AttackFlat: aF += stat.value; break;
            case CelestialCross.Artifacts.StatType.AttackPercent:  aP += stat.value; break;
            case CelestialCross.Artifacts.StatType.DefenseFlat:dF += stat.value; break;
            case CelestialCross.Artifacts.StatType.DefensePercent: dP += stat.value; break;
            case CelestialCross.Artifacts.StatType.Speed:  spdF += stat.value; break;
            case CelestialCross.Artifacts.StatType.CriticalRate: crF += stat.value; break;
            case CelestialCross.Artifacts.StatType.CriticalDamage: cdF += stat.value; break;
            case CelestialCross.Artifacts.StatType.EffectHitRate: eaf += stat.value; break;
            case CelestialCross.Artifacts.StatType.EffectResistance: erf += stat.value; break;
        }
    }

    private void SpawnAbilityButton(RectTransform parent, Celestial_Cross.Scripts.Abilities.AbilityBlueprint ability, bool isPet)
    {
        var btnGO = new GameObject("AbilityBtn", typeof(RectTransform), typeof(Image), typeof(UnityEngine.EventSystems.EventTrigger));
        btnGO.transform.SetParent(parent, false);
        var img = btnGO.GetComponent<Image>();
        img.color = isPet ? new Color(0.8f, 0.4f, 0.1f, 1f) : new Color(0.2f, 0.5f, 0.8f, 1f); // Orange for pet, blue for unit
        
        var rt = (RectTransform)btnGO.transform;
        rt.sizeDelta = new Vector2(100, 48); // width 100
        
        var txtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(btnGO.transform, false);
        var trt = (RectTransform)txtGO.transform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var txt = txtGO.GetComponent<TextMeshProUGUI>();
        txt.fontSize = 12;
        txt.alignment = TextAlignmentOptions.Center;
        txt.text = ability.abilityName;
        txt.enableWordWrapping = true;
        txt.raycastTarget = false;

        // Tooltip logic
        var trigger = btnGO.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        var ptrDown = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown };
        string desc = string.IsNullOrEmpty(ability.abilityDescription) ? "Sem descrição" : ability.abilityDescription;
        ptrDown.callback.AddListener((e) => {
            if (unitStatsText != null) unitStatsText.text = $"<b>{ability.abilityName}</b>\n<size=14>{desc}</size>";
        });
        trigger.triggers.Add(ptrDown);

        var ptrUp = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp };
        ptrUp.callback.AddListener((e) => {
            if (unitStatsText != null) unitStatsText.text = defaultStatsText;
        });
        var ptrExit = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        ptrExit.callback.AddListener((e) => {
            if (unitStatsText != null) unitStatsText.text = defaultStatsText;
        });
        trigger.triggers.Add(ptrUp);
        trigger.triggers.Add(ptrExit);
    }

    private void SpawnGraphButton(RectTransform parent, Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO graph)
    {
        var btnGO = new GameObject("GraphBtn", typeof(RectTransform), typeof(Image), typeof(UnityEngine.EventSystems.EventTrigger));
        btnGO.transform.SetParent(parent, false);
        var img = btnGO.GetComponent<Image>();
        img.color = new Color(0.5f, 0.2f, 0.8f, 1f); // Purple for graphs
        
        var rt = (RectTransform)btnGO.transform;
        rt.sizeDelta = new Vector2(100, 48);
        
        var txtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(btnGO.transform, false);
        var trt = (RectTransform)txtGO.transform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var txt = txtGO.GetComponent<TextMeshProUGUI>();
        txt.fontSize = 12;
        txt.alignment = TextAlignmentOptions.Center;
        txt.text = string.IsNullOrEmpty(graph.abilityName) ? graph.name : graph.abilityName;
        txt.enableWordWrapping = true;
        txt.raycastTarget = false;

        var trigger = btnGO.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        var ptrDown = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown };
        string desc = string.IsNullOrEmpty(graph.abilityDescription) ? "Habilidade de Grafo" : graph.abilityDescription;
        ptrDown.callback.AddListener((e) => {
            if (unitStatsText != null) unitStatsText.text = $"<b>{txt.text}</b>\n<size=14>{desc}</size>";
        });
        trigger.triggers.Add(ptrDown);

        var ptrUp = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp };
        ptrUp.callback.AddListener((e) => {
            if (unitStatsText != null) unitStatsText.text = defaultStatsText;
        });
        trigger.triggers.Add(ptrUp);
    }

    // --- Phase 3 & 4 Logics ---
    private void ShowUnitDetails(string unitId, Account account)
    {
        currentlySelectedTabItemId = unitId;
        selectingForUnitId = unitId;
        
        if (unitEquipContainer != null) unitEquipContainer.gameObject.SetActive(true);

        UnitData data = null;
        if (unitCatalog != null)
            data = unitCatalog.GetUnitData(unitId);

        var loadout = account.GetLoadoutForUnit(unitId);
        
        var runtimeData = account.GetOwnedUnitRuntimeData(unitId);
        if (runtimeData == null)
        {
            // Se não encontrar runtime data, cria um default
            runtimeData = new CelestialCross.Data.RuntimeUnitData(unitId, 1);
        }

        // Update Level UI
        if (unitLevelText != null) unitLevelText.text = $"Lv. {runtimeData.Level}";
        if (unitXPBar != null && levelingConfig != null)
        {
            int xpToNext = levelingConfig.GetXPForNextLevel(runtimeData.Level);
            unitXPBar.fillAmount = (float)runtimeData.CurrentXP / xpToNext;
            if (unitXPText != null) unitXPText.text = $"{runtimeData.CurrentXP} / {xpToNext}";
        }

        // Update Constellation UI
        if (constellationStars != null)
        {
            for (int i = 0; i < constellationStars.Length; i++)
            {
                if (constellationStars[i] != null)
                    constellationStars[i].color = i < runtimeData.ConstellationLevel ? Color.yellow : Color.gray;
            }
        }
        if (insigniaCountText != null)
        {
            string insigniaID = ConstellationService.GetInsigniaItemID(unitId);
            int count = account.GetItemCount(insigniaID);
            insigniaCountText.text = $"Insígnias: {count}";
        }

        if (data != null && unitStatsText != null && unitIconImage != null)
        {
            unitIconImage.sprite = data.icon;
            
            int refMaxLevel = (levelingConfig != null) ? levelingConfig.globalMaxLevel : 100;
            var baseStats = data.GetStatsAtLevel(runtimeData.Level, refMaxLevel);
            RuntimePetData equippedPet = null;
            CelestialCross.Data.Pets.PetSpeciesSO equippedPetSpecies = null;
            if (loadout != null && !string.IsNullOrEmpty(loadout.PetID))
            {
                equippedPet = account.GetPetByUUID(loadout.PetID);
                if (equippedPet != null && petCatalog != null)
                {
                    equippedPetSpecies = petCatalog.GetPetSpecies(equippedPet.SpeciesID);
                }
            }

            float hF = 0, hP = 0, aF = 0, aP = 0, dF = 0, dP = 0, spdF = 0, crF = 0, eaf = 0, cdF = 0, erf = 0;
            if (loadout != null)
            {
                var artifactIDs = loadout.GetEquippedArtifactIDs();
                var setCounts = new Dictionary<string, int>();

                foreach (var guid in artifactIDs)
                {
                    var arti = account.GetArtifactByGuid(guid);
                    if (arti != null)
                    {
                        ProcessStatData(arti.mainStat, ref hF, ref hP, ref aF, ref aP, ref dF, ref dP, ref spdF, ref crF, ref eaf, ref cdF, ref erf);
                        if (arti.subStats != null)
                        {
                            foreach (var sub in arti.subStats)
                            {
                                ProcessStatData(sub, ref hF, ref hP, ref aF, ref aP, ref dF, ref dP, ref spdF, ref crF, ref eaf, ref cdF, ref erf);
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(arti.artifactSetId))
                        {
                            if (!setCounts.ContainsKey(arti.artifactSetId)) setCounts[arti.artifactSetId] = 0;
                            setCounts[arti.artifactSetId]++;
                        }
                    }
                }

                if (artifactSetCatalog != null)
                {
                    foreach (var kvp in setCounts)
                    {
                        var set = artifactSetCatalog.GetSetById(kvp.Key);
                        if (set == null) continue;

                        foreach (var bonus in set.setBonuses)
                        {
                            if (kvp.Value >= bonus.piecesRequired && bonus.statBonuses != null)
                            {
                                foreach (var statMod in bonus.statBonuses)
                                {
                                    ProcessStatData(statMod, ref hF, ref hP, ref aF, ref aP, ref dF, ref dP, ref spdF, ref crF, ref eaf, ref cdF, ref erf);
                                }
                            }
                        }
                    }
                }
            }

            int finalHealth = (int)Mathf.Round(baseStats.health * (1f + (hP / 100f)) + hF);
            int finalAttack = (int)Mathf.Round(baseStats.attack * (1f + (aP / 100f)) + aF);
            int finalDefense = (int)Mathf.Round(baseStats.defense * (1f + (dP / 100f)) + dF);
            int finalSpeed = (int)Mathf.Round(baseStats.speed + spdF);
            int finalCrit = Mathf.Clamp((int)Mathf.Round(baseStats.criticalChance + crF), 0, 100);
            int finalCritDmg = Mathf.Max(50, (int)Mathf.Round(baseStats.criticalDamage + cdF)); // Base 50%
            int finalAcc = Mathf.Clamp((int)Mathf.Round(baseStats.effectAccuracy + eaf), 0, 100);
            int finalRes = Mathf.Clamp((int)Mathf.Round(baseStats.effectResistance + erf), 0, 100);
            
            if (equippedPet != null && equippedPetSpecies != null)
            {
                finalHealth += equippedPet.Health;
                finalAttack += equippedPet.Attack;
                finalDefense += equippedPet.Defense;
                finalSpeed += equippedPet.Speed;
                finalCrit = Mathf.Clamp(finalCrit + equippedPet.CriticalChance, 0, 100);
                finalCritDmg = Mathf.Max(50, finalCritDmg + equippedPet.CriticalDamage);
                finalAcc = Mathf.Clamp(finalAcc + equippedPet.EffectAccuracy, 0, 100);
                finalRes = Mathf.Clamp(finalRes + equippedPet.EffectResistance, 0, 100);
            }
            
            int roundedBaseHealth = Mathf.RoundToInt(baseStats.health);
            int roundedBaseAttack = Mathf.RoundToInt(baseStats.attack);
            int roundedBaseDefense = Mathf.RoundToInt(baseStats.defense);
            int roundedBaseSpeed = Mathf.RoundToInt(baseStats.speed);
            int roundedBaseCrit = Mathf.RoundToInt(baseStats.criticalChance);
            int roundedBaseCritDmg = Mathf.RoundToInt(baseStats.criticalDamage);
            int roundedBaseAcc = Mathf.RoundToInt(baseStats.effectAccuracy);
            int roundedBaseRes = Mathf.RoundToInt(baseStats.effectResistance);

            string healthText = finalHealth > roundedBaseHealth ? $"<color=#00ff00>{finalHealth}</color> <color=#aaaaaa>({roundedBaseHealth} +{finalHealth - roundedBaseHealth})</color>" : finalHealth.ToString();
            string attackText = finalAttack > roundedBaseAttack ? $"<color=#00ff00>{finalAttack}</color> <color=#aaaaaa>({roundedBaseAttack} +{finalAttack - roundedBaseAttack})</color>" : finalAttack.ToString();
            string defenseText = finalDefense > roundedBaseDefense ? $"<color=#00ff00>{finalDefense}</color> <color=#aaaaaa>({roundedBaseDefense} +{finalDefense - roundedBaseDefense})</color>" : finalDefense.ToString();
            string speedText = finalSpeed > roundedBaseSpeed ? $"<color=#00ff00>{finalSpeed}</color> <color=#aaaaaa>({roundedBaseSpeed} +{finalSpeed - roundedBaseSpeed})</color>" : finalSpeed.ToString();
            string critText = finalCrit > roundedBaseCrit ? $"<color=#00ff00>{finalCrit}%</color> <color=#aaaaaa>({roundedBaseCrit}% +{finalCrit - roundedBaseCrit}%)</color>" : $"{finalCrit}%";
            string cdText = finalCritDmg > roundedBaseCritDmg ? $"<color=#00ff00>{finalCritDmg}%</color> <color=#aaaaaa>({roundedBaseCritDmg}% +{finalCritDmg - roundedBaseCritDmg}%)</color>" : $"{finalCritDmg}%";
            string accText = finalAcc > roundedBaseAcc ? $"<color=#00ff00>{finalAcc}%</color> <color=#aaaaaa>({roundedBaseAcc}% +{finalAcc - roundedBaseAcc}%)</color>" : $"{finalAcc}%";
            string resText = finalRes > roundedBaseRes ? $"<color=#00ff00>{finalRes}%</color> <color=#aaaaaa>({roundedBaseRes}% +{finalRes - roundedBaseRes}%)</color>" : $"{finalRes}%";

            string abilitiesList = "Nenhuma habilidade";
            if (unitAbilitiesContainer != null)
            {
                // Clear old buttons
                foreach (RectTransform child in unitAbilitiesContainer) Destroy(child.gameObject);
                
                // As habilidades agora são mostradas via Grafos abaixo

                if (data.GetAbilityGraphs() != null)
                {
                    foreach (var graph in data.GetAbilityGraphs())
                    {
                        if (graph != null) SpawnGraphButton(unitAbilitiesContainer, graph);
                    }
                }
                if (equippedPetSpecies != null)
                {
                    if (equippedPetSpecies.PassiveSkills != null) foreach(var ab in equippedPetSpecies.PassiveSkills) if (ab != null) SpawnAbilityButton(unitAbilitiesContainer, ab, true);
                    if (equippedPetSpecies.ActiveSkills != null) foreach(var ab in equippedPetSpecies.ActiveSkills) if (ab != null) SpawnAbilityButton(unitAbilitiesContainer, ab, true);
                    if (equippedPetSpecies.AbilityGraphs != null) foreach(var graph in equippedPetSpecies.AbilityGraphs) if (graph != null) SpawnGraphButton(unitAbilitiesContainer, graph);
                }
                
                // Adiciona Passivas de Sets de Artefatos
                if (loadout != null && artifactSetCatalog != null)
                {
                    var setCounts = new Dictionary<string, int>();
                    var artifactIDs = loadout.GetEquippedArtifactIDs();
                    foreach (var guid in artifactIDs)
                    {
                        var arti = account.GetArtifactByGuid(guid);
                        if (arti != null && !string.IsNullOrEmpty(arti.artifactSetId))
                        {
                            if (!setCounts.ContainsKey(arti.artifactSetId)) setCounts[arti.artifactSetId] = 0;
                            setCounts[arti.artifactSetId]++;
                        }
                    }

                    foreach (var kvp in setCounts)
                    {
                        var set = artifactSetCatalog.GetSetById(kvp.Key);
                        if (set == null) continue;

                        foreach (var bonus in set.setBonuses)
                        {
                            if (kvp.Value >= bonus.piecesRequired)
                            {
                                if (bonus.passiveAbility != null) SpawnAbilityButton(unitAbilitiesContainer, bonus.passiveAbility, false);
                                if (bonus.passiveGraph != null) SpawnGraphButton(unitAbilitiesContainer, bonus.passiveGraph);
                            }
                        }
                    }
                }
            }

            defaultStatsText = $"<b><size=24>{data.displayName}</size></b>\n" +
                                 $"<size=18>" +
                                 $"<color=#ff8888>HP:</color> {healthText}   " +
                                 $"<color=#ff8888>ATK:</color> {attackText}   " +
                                 $"<color=#ff8888>DEF:</color> {defenseText}\n" +
                                 $"<color=#ff8888>SPD:</color> {speedText}   " +
                                 $"<color=#ff8888>CRIT:</color> {critText}   " +
                                 $"<color=#ff8888>CR.DMG:</color> {cdText}\n" +
                                 $"<color=#ff8888>ACC:</color> {accText}   " +
                                 $"<color=#ff8888>RES:</color> {resText}" +
                                 $"</size>";
            unitStatsText.text = defaultStatsText;
        }
        else if (unitStatsText != null)
        {
            unitStatsText.text = $"<b>Unidade Desconhecida</b>\n(UnitData não encontrado no Catálogo)";
            if (unitIconImage != null) unitIconImage.sprite = null;
        }

        CelestialCross.Artifacts.ArtifactType[] slotTypes =
            (CelestialCross.Artifacts.ArtifactType[])Enum.GetValues(typeof(CelestialCross.Artifacts.ArtifactType));
            
        for (int i = 0; i < 7; i++)
        {
            if (unitEquipTexts[i] == null) continue;
            
            if (i == 6)
            {
                // Pet Slot
                Sprite petIcon = null;
                if (loadout != null && !string.IsNullOrEmpty(loadout.PetID))
                {
                    var rp = AccountManager.Instance.PlayerAccount.GetPetByUUID(loadout.PetID);
                    if (rp != null) {
                        var pet = petCatalog?.GetPetSpecies(rp.SpeciesID);
                        if (pet != null) petIcon = pet.Icon;
                        unitEquipTexts[i].text = $"<b>Pet</b>\n<color=#ffb>{rp.DisplayName}</color>";
                    } else {
                        unitEquipTexts[i].text = $"<b>Pet</b>\n<color=#ffb>Desconhecido</color>";
                    }
                }
                else { unitEquipTexts[i].text = "Pet\n<color=#888>(vazio)</color>"; }

                Transform iconTr = unitEquipButtons[i].transform.Find("Icon");
                if (iconTr == null) {
                    var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                    iconGo.transform.SetParent(unitEquipButtons[i].transform, false);
                    var irt = (RectTransform)iconGo.transform;
                    irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
                    irt.offsetMin = new Vector2(4, 4); irt.offsetMax = new Vector2(-4, -4);
                    var img = iconGo.GetComponent<Image>();
                    img.preserveAspect = true;
                    iconTr = iconGo.transform;
                }
                if (petIcon != null) {
                    iconTr.gameObject.SetActive(true);
                    iconTr.GetComponent<Image>().sprite = petIcon;
                    unitEquipTexts[i].gameObject.SetActive(false);
                } else {
                    iconTr.gameObject.SetActive(false);
                    unitEquipTexts[i].gameObject.SetActive(true);
                }
                continue;
            }

            var sType = slotTypes[Mathf.Min(i, slotTypes.Length - 1)];

            string equippedGuid = null;
            if (loadout != null)
            {
                switch (sType)
                {
                    case CelestialCross.Artifacts.ArtifactType.Helmet: equippedGuid = loadout.HelmetID; break;
                    case CelestialCross.Artifacts.ArtifactType.Chestplate: equippedGuid = loadout.ChestplateID; break;
                    case CelestialCross.Artifacts.ArtifactType.Gloves: equippedGuid = loadout.GlovesID; break;
                    case CelestialCross.Artifacts.ArtifactType.Boots:  equippedGuid = loadout.BootsID; break;
                    case CelestialCross.Artifacts.ArtifactType.Necklace: equippedGuid = loadout.NecklaceID; break;
                    case CelestialCross.Artifacts.ArtifactType.Ring:   equippedGuid = loadout.RingID; break;
                }
            }

            Sprite artiIcon = null;
            if (!string.IsNullOrEmpty(equippedGuid))
            {
                var artifact = account.GetArtifactByGuid(equippedGuid);
                if (artifact != null)
                {
                    unitEquipTexts[i].text = $"<b>{sType}</b>\n<color=#ffb>{artifact.rarity} +{artifact.currentLevel}</color>";
                    if (artifactSetCatalog != null)
                    {
                        var set = artifactSetCatalog.GetSetById(artifact.artifactSetId);
                        if (set != null) artiIcon = set.GetIconForSlot(artifact.slot);
                    }
                }
                else { unitEquipTexts[i].text = $"{sType}\n<color=red>Miss</color>"; }
            }
            else { unitEquipTexts[i].text = $"{sType}\n<color=#888>(vazio)</color>"; }
            
            { Transform iconTr = unitEquipButtons[i].transform.Find("Icon");
            if (iconTr == null) {
                var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(unitEquipButtons[i].transform, false);
                var irt = (RectTransform)iconGo.transform;
                irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
                irt.offsetMin = new Vector2(10, 10); irt.offsetMax = new Vector2(-10, -10);
                var img = iconGo.GetComponent<Image>();
                img.preserveAspect = true;
                img.raycastTarget = false;
                iconTr = iconGo.transform;
            }
            if (artiIcon != null) {
                iconTr.gameObject.SetActive(true);
                iconTr.GetComponent<Image>().sprite = artiIcon;
                unitEquipTexts[i].gameObject.SetActive(false);
            } else {
                iconTr.gameObject.SetActive(false);
                unitEquipTexts[i].gameObject.SetActive(true);
            } }
        }

        if (skillTab != null)
        {
            skillTab.Setup(unitId);
        }
    }

    private void OnUnitEquipSlotClicked(CelestialCross.Artifacts.ArtifactType slot)
    {
        if (string.IsNullOrEmpty(selectingForUnitId)) return;
        var account = AccountManager.Instance?.PlayerAccount;
        if (account == null) return;
        
        string unitName = "Unidade";
        if (unitCatalog != null)
        {
            var d = unitCatalog.GetUnitData(selectingForUnitId);
            if (d != null) unitName = d.displayName;
        }
        
        var loadout = account.GetLoadoutForUnit(selectingForUnitId);
        bool hasEquip = false;
        if (loadout != null)
        {
            switch (slot)
            {
                case CelestialCross.Artifacts.ArtifactType.Helmet: originEquippedId = loadout.HelmetID; hasEquip = !string.IsNullOrEmpty(originEquippedId); break;
                case CelestialCross.Artifacts.ArtifactType.Chestplate: originEquippedId = loadout.ChestplateID; hasEquip = !string.IsNullOrEmpty(originEquippedId); break;
                case CelestialCross.Artifacts.ArtifactType.Gloves: originEquippedId = loadout.GlovesID; hasEquip = !string.IsNullOrEmpty(originEquippedId); break;
                case CelestialCross.Artifacts.ArtifactType.Boots:  originEquippedId = loadout.BootsID; hasEquip = !string.IsNullOrEmpty(originEquippedId); break;
                case CelestialCross.Artifacts.ArtifactType.Necklace: originEquippedId = loadout.NecklaceID; hasEquip = !string.IsNullOrEmpty(originEquippedId); break;
                case CelestialCross.Artifacts.ArtifactType.Ring:   originEquippedId = loadout.RingID; hasEquip = !string.IsNullOrEmpty(originEquippedId); break;
            }
        }

        isSelectingArtifact = true;
        isSelectingPet = false;
        selectingForSlot = slot;
        currentlySelectedTabItemId = originEquippedId;
        
        if (cancelEquipButton != null) 
        {
            if (topPanels != null && topPanels.Length > 2) cancelEquipButton.transform.SetParent(topPanels[2], false);
            cancelEquipButton.gameObject.SetActive(true);
        }
        if (equipArtifactButton != null) 
        {
            if (topPanels != null && topPanels.Length > 2) equipArtifactButton.transform.SetParent(topPanels[2], false);
            equipArtifactButton.gameObject.SetActive(false);
        }
        if (unequipArtifactButton != null)
        {
            if (topPanels != null && topPanels.Length > 2) unequipArtifactButton.transform.SetParent(topPanels[2], false);
            unequipArtifactButton.gameObject.SetActive(hasEquip);
        }
        SetDetails(2, $"Selecione um <b>{slot}</b> para <b>{unitName}</b>");

        SwitchToTab(2); // Vai para artefatos
        PopulateTab(2); // Refaz a lista mostrando apenas a categoria filtrada
    }

    private void OnUnitPetSlotClicked()
    {
        if (string.IsNullOrEmpty(selectingForUnitId)) return;
        var account = AccountManager.Instance?.PlayerAccount;
        if (account == null) return;
        
        string unitName = "Unidade";
        var loadout = account.GetLoadoutForUnit(selectingForUnitId);
        bool hasPet = loadout != null && !string.IsNullOrEmpty(loadout.PetID);
        originEquippedId = hasPet ? loadout.PetID : null;
        currentlySelectedTabItemId = originEquippedId;

        if (unitCatalog != null)
        {
            var d = unitCatalog.GetUnitData(selectingForUnitId);
            if (d != null) unitName = d.displayName;
        }

        isSelectingPet = true;
        isSelectingArtifact = false;
        
        if (cancelEquipButton != null) 
        {
            if (topPanels != null && topPanels.Length > 1) cancelEquipButton.transform.SetParent(topPanels[1], false);
            cancelEquipButton.gameObject.SetActive(true);
        }
        if (equipArtifactButton != null) 
        {
            if (topPanels != null && topPanels.Length > 1) equipArtifactButton.transform.SetParent(topPanels[1], false);
            equipArtifactButton.gameObject.SetActive(false);
        }
        if (unequipArtifactButton != null)
        {
            if (topPanels != null && topPanels.Length > 1) unequipArtifactButton.transform.SetParent(topPanels[1], false);
            unequipArtifactButton.gameObject.SetActive(hasPet);
        }
        SetDetails(1, $"Selecione um Pet para <b>{unitName}</b>");

        SwitchToTab(1); // Vai para Pets
        PopulateTab(1); // Refaz a lista, dessa vez em modo seleÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o
    }

    private void OnPetClicked(string petId) {
        currentlySelectedTabItemId = petId;
        string details = "Pet desconhecido";
        
        var account = AccountManager.Instance.PlayerAccount;
        RuntimePetData data = account.GetPetByUUID(petId);

        if (data != null)
        {
            selectedPetInstance = data;
            string speciesName = data.DisplayName;
            CelestialCross.Data.Pets.PetSpeciesSO speciesData = null;
            if (petCatalog != null)
            {
                speciesData = petCatalog.GetPetSpecies(data.SpeciesID);
                if (speciesData != null) speciesName = speciesData.SpeciesName;
            }

            details = $"<b>{speciesName}</b>\n" +
                      $"Estrelas: {data.RarityStars}* | Nível: {data.CurrentLevel}\n\n" +
                      $"HP: +{data.Health}   ATK: +{data.Attack}   DEF: +{data.Defense}\n" +
                      $"SPD: +{data.Speed}   CRIT: +{data.CriticalChance}%\n" +
                      $"CR.DMG: +{data.CriticalDamage}%   ACC: +{data.EffectAccuracy}%   RES: +{data.EffectResistance}%\n\n";
            if (speciesData != null)
            {
                if (speciesData.PassiveSkills != null) foreach (var ab in speciesData.PassiveSkills) if (ab != null) { details += $"<color=#ffffaa>{ab.abilityName}</color>\n<size=16>{ab.abilityDescription}</size>\n"; }
                if (speciesData.ActiveSkills != null) foreach (var ab in speciesData.ActiveSkills) if (ab != null) { details += $"<color=#ffffaa>{ab.abilityName}</color>\n<size=16>{ab.abilityDescription}</size>\n"; }
                if (speciesData.AbilityGraphs != null) foreach (var graph in speciesData.AbilityGraphs) if (graph != null) { details += $"<color=#aaffaa>{(string.IsNullOrEmpty(graph.abilityName) ? graph.name : graph.abilityName)}</color>\n<size=16>{graph.abilityDescription}</size>\n"; }
            }
            else
            {
                details += "<color=#aaaaaa>(Pet sem habilidade equipada)</color>";
            }
        }

        SetDetails(1, details);
        
        if (isSelectingPet)
        {
            if (managePetButton != null) managePetButton.gameObject.SetActive(false);
            selectedPetToEquipId = petId;
            if (currentlySelectedTabItemId == originEquippedId && originEquippedId != null) 
            {
                if (equipArtifactButton != null) equipArtifactButton.gameObject.SetActive(false);
                if (unequipArtifactButton != null) unequipArtifactButton.gameObject.SetActive(true);
            }
            else 
            {
                if (equipArtifactButton != null) equipArtifactButton.gameObject.SetActive(true);
                if (unequipArtifactButton != null) unequipArtifactButton.gameObject.SetActive(false);
            }
        }
        else
        {
            if (managePetButton != null) managePetButton.gameObject.SetActive(true);
            if (equipArtifactButton != null) equipArtifactButton.gameObject.SetActive(false);
            if (unequipArtifactButton != null) unequipArtifactButton.gameObject.SetActive(false);
        }
    }

    private void CancelEquipMode()
    {
        isSelectingArtifact = false;
        isSelectingPet = false;
        selectingForUnitId = null;
        selectedArtifactToEquip = null;
        selectedPetToEquipId = null;
        originEquippedId = null;
        currentlySelectedTabItemId = null;
        selectedPetInstance = null;
        
        if (cancelEquipButton != null) cancelEquipButton.gameObject.SetActive(false);
        if (equipArtifactButton != null) equipArtifactButton.gameObject.SetActive(false);
        if (unequipArtifactButton != null) unequipArtifactButton.gameObject.SetActive(false);
        if (manageArtifactButton != null) manageArtifactButton.gameObject.SetActive(false);
        if (managePetButton != null) managePetButton.gameObject.SetActive(false);
        
        SwitchToTab(0); // Volta pra Unidades
        PopulateTab(1); // Atualiza Pets para todos
        PopulateTab(2); // Atualiza Artefatos para todos
    }

    private void OnArtifactClicked(CelestialCross.Artifacts.ArtifactInstanceData artifact) {
        if (artifact != null) currentlySelectedTabItemId = artifact.idGUID;
        SetDetails(2, FormatArtifactDetails(artifact));
        selectedArtifactToEquip = artifact;
        
        if (isSelectingArtifact)
        {
            if (manageArtifactButton != null) manageArtifactButton.gameObject.SetActive(false);

            if (currentlySelectedTabItemId == originEquippedId && originEquippedId != null) 
            {
                if (equipArtifactButton != null) equipArtifactButton.gameObject.SetActive(false);
                if (unequipArtifactButton != null) unequipArtifactButton.gameObject.SetActive(true);
            }
            else 
            {
                if (equipArtifactButton != null) equipArtifactButton.gameObject.SetActive(true);
                if (unequipArtifactButton != null) unequipArtifactButton.gameObject.SetActive(false);
            }
        }
        else
        {
            if (manageArtifactButton != null) manageArtifactButton.gameObject.SetActive(true);
            if (equipArtifactButton != null) equipArtifactButton.gameObject.SetActive(false);
            if (unequipArtifactButton != null) unequipArtifactButton.gameObject.SetActive(false);
        }
    }

    private void OnManageArtifactClicked()
    {
        if (selectedArtifactToEquip == null || upgradeModal == null) return;
        upgradeModal.Show(selectedArtifactToEquip, () =>
        {
            // Callback when artifact is upgraded or sold
            RefreshAllTabs();
            SetDetails(2, FormatArtifactDetails(selectedArtifactToEquip));
            if (manageArtifactButton != null) manageArtifactButton.gameObject.SetActive(false); // Hide until re-clicked
        });
    }

    private void OnManagePetClicked()
    {
        if (selectedPetInstance == null || petManageModal == null) return;
        petManageModal.Show(selectedPetInstance, () => 
        {
            RefreshAllTabs();
            SetDetails(1, "(selecione um item abaixo)");
            if (managePetButton != null) managePetButton.gameObject.SetActive(false);
        });
    }

    private void ConfirmUnequip()
    {
        var account = AccountManager.Instance?.PlayerAccount;
        if (account == null) return;
        
        var loadout = account.GetLoadoutForUnit(selectingForUnitId);
        if (loadout == null) return;

        if (isSelectingArtifact)
        {
            switch (selectingForSlot)
            {
                case CelestialCross.Artifacts.ArtifactType.Helmet: loadout.HelmetID = string.Empty; break;
                case CelestialCross.Artifacts.ArtifactType.Chestplate: loadout.ChestplateID = string.Empty; break;
                case CelestialCross.Artifacts.ArtifactType.Gloves: loadout.GlovesID = string.Empty; break;
                case CelestialCross.Artifacts.ArtifactType.Boots:  loadout.BootsID = string.Empty; break;
                case CelestialCross.Artifacts.ArtifactType.Necklace: loadout.NecklaceID = string.Empty; break;
                case CelestialCross.Artifacts.ArtifactType.Ring:   loadout.RingID = string.Empty; break;
            }
        }
        else if (isSelectingPet)
        {
            loadout.PetID = string.Empty;
        }

        AccountManager.Instance.SaveAccount();
        
        string unitToRefresh = selectingForUnitId;
        CancelEquipMode();
        ShowUnitDetails(unitToRefresh, account);
    }

    private void ConfirmEquip()
    {
        var account = AccountManager.Instance?.PlayerAccount;
        if (account == null) return;
        
        var loadout = account.GetLoadoutForUnit(selectingForUnitId);
        if (loadout == null) return;

        if (isSelectingArtifact && selectedArtifactToEquip != null)
        {
            account.UnequipArtifactFromAll(selectedArtifactToEquip.idGUID);

            switch (selectingForSlot)
            {
                case CelestialCross.Artifacts.ArtifactType.Helmet: loadout.HelmetID = selectedArtifactToEquip.idGUID; break;
                case CelestialCross.Artifacts.ArtifactType.Chestplate: loadout.ChestplateID = selectedArtifactToEquip.idGUID; break;
                case CelestialCross.Artifacts.ArtifactType.Gloves: loadout.GlovesID = selectedArtifactToEquip.idGUID; break;
                case CelestialCross.Artifacts.ArtifactType.Boots:  loadout.BootsID = selectedArtifactToEquip.idGUID; break;
                case CelestialCross.Artifacts.ArtifactType.Necklace: loadout.NecklaceID = selectedArtifactToEquip.idGUID; break;
                case CelestialCross.Artifacts.ArtifactType.Ring:   loadout.RingID = selectedArtifactToEquip.idGUID; break;
            }
        }
        else if (isSelectingPet && !string.IsNullOrEmpty(selectedPetToEquipId))
        {
            account.UnequipPetFromAll(selectedPetToEquipId);
            loadout.PetID = selectedPetToEquipId;
        }
        else
        {
            return;
        }
        
        // Opcional: Salvar ao equipar
        AccountManager.Instance.SaveAccount();
        
        string unitToRefresh = selectingForUnitId;
        CancelEquipMode(); // Reseta status e volta para as unidades
        ShowUnitDetails(unitToRefresh, account); // Da o refresh na tela de unidade
    }
    // ----------------------------

    private string FormatArtifactDetails(CelestialCross.Artifacts.ArtifactInstanceData a)
    {
        if (a == null) return "Artefato inválido.";
 
        string setLabel = string.IsNullOrWhiteSpace(a.artifactSetId) ? "<sem set>" : a.artifactSetId;
        string setBonusesDescription = "";
 
        if (artifactSetCatalog != null && !string.IsNullOrEmpty(a.artifactSetId))
        {
            var set = artifactSetCatalog.GetSetById(a.artifactSetId);
            if (set != null)
            {
                setLabel = set.setName;
                setBonusesDescription = $"\n\n<b>Efeitos do Conjunto ({set.setName}):</b>\n";
                foreach (var bonus in set.setBonuses)
                {
                    setBonusesDescription += $"- <color=#ffb>{bonus.piecesRequired} Peças:</color> ";
                    List<string> bonusesList = new List<string>();
                    if (bonus.statBonuses != null)
                    {
                        foreach (var stat in bonus.statBonuses)
                        {
                            bonusesList.Add(UIStatFormatter.FormatStat(stat));
                        }
                    }
                    if (bonus.passiveAbility != null)
                    {
                        bonusesList.Add($"Passiva: <color=#ffffaa>{bonus.passiveAbility.abilityName}</color>");
                    }
                    if (bonus.passiveGraph != null)
                    {
                        bonusesList.Add($"Passiva: <color=#aaffaa>{(string.IsNullOrEmpty(bonus.passiveGraph.abilityName) ? bonus.passiveGraph.name : bonus.passiveGraph.abilityName)}</color>");
                    }
 
                    if (bonusesList.Count > 0)
                        setBonusesDescription += string.Join(", ", bonusesList);
                    else
                        setBonusesDescription += "Nenhum";
                    setBonusesDescription += "\n";
                }
            }
        }
 
        string main = a.mainStat != null ? UIStatFormatter.FormatStat(a.mainStat) : "<mainStat null>";
 
        string sub = "(nenhum)";
        if (a.subStats != null && a.subStats.Count > 0)
        {
            sub = "";
            for (int i = 0; i < a.subStats.Count; i++)
            {
                var s = a.subStats[i];
                if (s == null) continue;
                sub += $"- {UIStatFormatter.FormatStat(s)}\n";
            }
            sub = sub.TrimEnd();
        }
 
        return
            $"Artefato\n" +
            $"Slot: {a.slot}\n" +
            $"Set: {setLabel}\n" +
            $"Raridade: {a.rarity}\n" +
            $"Estrelas: {a.GetStarsAsIntClamped()}*\n" +
            $"Nível: +{a.currentLevel}\n\n" +
            $"Main: {main}\n\n" +
            $"Substats:\n{sub}";
    }
}















