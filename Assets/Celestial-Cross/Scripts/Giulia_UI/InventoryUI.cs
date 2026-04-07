using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// (Fase 2) UI de inventário modular com 3 abas (Unidades, Pets, Artefatos).
/// Layout split-screen: Painel Superior (detalhes dinâmicos) + Painel Inferior (grid/scroll).
/// Suporta troca por toque nas abas ou swipe horizontal.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Data Catalogs")]
    [Tooltip("Necessário para renderizar detalhes da Unidade (ícone, stats, skills)")]
    public UnitCatalog unitCatalog;
    public PetCatalog petCatalog;

    [Header("Abas")]
    [Tooltip("Arrastar as 3 InventoryTab (Unidades, Pets, Artefatos) na ordem")]
    public InventoryTab[] tabs;

    [Header("Conteúdo Inferior (Grids)")]
    [Tooltip("Um RectTransform com GridLayoutGroup para cada aba, na mesma ordem das tabs")]
    public RectTransform[] gridContainers;

    [Header("Item Prefab (Opcional)")]
    [Tooltip("Prefab de cada item do grid (Button + Image/Text). Se vazio, o UI é criado via código.")]
    public GameObject slotPrefab;

    [Header("Split Layout")]
    [Tooltip("Altura do painel superior em proporção (0..1). Ex: 0.45 = 45% superior.")]
    [Range(0.25f, 0.75f)]
    public float topPanelHeightNormalized = 0.45f;

    [Tooltip("Se true, ajusta anchors/ScrollRect e cria TopPanels automaticamente em runtime.")]
    public bool autoBuildSplitLayout = true;

    [Header("Grid Config")]
    public int columns = 3;
    public Vector2 cellSize    = new Vector2(90f, 90f);
    public Vector2 cellSpacing = new Vector2(10f, 10f);

    [Header("Swipe")]
    [Tooltip("Referência ao SwipeDetector (pode estar no mesmo GameObject)")]
    public SwipeDetector swipeDetector;

    private int currentTabIndex = 0;

    // --- Phase 3 & 4 State ---
    private bool isSelectingArtifact = false;
    private bool isSelectingPet = false;
    private CelestialCross.Artifacts.ArtifactType selectingForSlot;
    private string selectingForUnitId;
    
    // UI Refs for dynamic top panels
    private Image unitIconImage;
    private TextMeshProUGUI unitStatsText;
    private TextMeshProUGUI unitAbilitiesText;
    private RectTransform unitAbilitiesContainer;
    
    private RectTransform unitEquipContainer;
    private Button[] unitEquipButtons = new Button[7];
    private TextMeshProUGUI[] unitEquipTexts = new TextMeshProUGUI[7];

    private Button equipArtifactButton;
    private TextMeshProUGUI equipArtifactText;
    private Button cancelEquipButton;
    private CelestialCross.Artifacts.ArtifactInstanceData selectedArtifactToEquip;
    private string selectedPetToEquipId;
    private string defaultStatsText;
    // -------------------------

    private readonly List<GameObject>[] spawnedItemsPerTab = new List<GameObject>[3]
    {
        new List<GameObject>(),
        new List<GameObject>(),
        new List<GameObject>()
    };

    private RectTransform[] topPanels;
    private TextMeshProUGUI[] topPanelTexts;
    private GameObject[] bottomScrollRoots;

    private RectTransform tabsBar;
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
        InitializeTabs();
        RegisterSwipe();

        if (autoBuildSplitLayout)
            EnsureSplitLayout();

        ConfigureGrids();
        RefreshAllTabs();
        SwitchToTab(0);
    }

    private void OnEnable()
    {
        // When the panel is reopened, refresh lists from Account.
        if (Application.isPlaying)
            RefreshAllTabs();
    }

    void OnDestroy()
    {
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
    // INICIALIZAÇÃO
    // =============================

    private void ConfigureGrids()
    {
        if (gridContainers == null)
            return;

        for (int i = 0; i < gridContainers.Length; i++)
        {
            if (gridContainers[i] == null) continue;

            GridLayoutGroup grid = gridContainers[i].GetComponent<GridLayoutGroup>();
            if (grid == null)
                grid = gridContainers[i].gameObject.AddComponent<GridLayoutGroup>();

            grid.cellSize = cellSize;
            grid.spacing = cellSpacing;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Mathf.Max(1, columns);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(16, 16, 16, 16);

            // Helps ScrollRect content size.
            var fitter = gridContainers[i].GetComponent<ContentSizeFitter>();
            if (fitter == null)
                fitter = gridContainers[i].gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    void InitializeTabs()
    {
        if (tabs == null) return;

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] == null) continue;
            tabs[i].tabIndex = i;
            tabs[i].OnTabClicked += SwitchToTab;
        }

        // Rename tab titles to match the new plan.
        if (tabs.Length > 0 && tabs[0] != null) tabs[0].SetTitle("Unidades");
        if (tabs.Length > 1 && tabs[1] != null) tabs[1].SetTitle("Pets");
        if (tabs.Length > 2 && tabs[2] != null) tabs[2].SetTitle("Artefatos");
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
        if (tabs == null || gridContainers == null) return;
        if (index < 0 || index >= tabs.Length) return;

        currentTabIndex = index;

        // Ativar/desativar top panel + bottom area
        if (topPanels != null)
        {
            for (int i = 0; i < topPanels.Length; i++)
            {
                if (topPanels[i] != null)
                    topPanels[i].gameObject.SetActive(i == index);
            }
        }

        if (bottomScrollRoots != null)
        {
            for (int i = 0; i < bottomScrollRoots.Length; i++)
            {
                if (bottomScrollRoots[i] != null)
                    bottomScrollRoots[i].SetActive(i == index);
            }
        }
        else
        {
            // Fallback for old scene wiring: show/hide raw containers
            for (int i = 0; i < gridContainers.Length; i++)
            {
                if (gridContainers[i] != null)
                    gridContainers[i].gameObject.SetActive(i == index);
            }
        }

        // Atualizar visual das abas
        for (int i = 0; i < tabs.Length; i++)
            tabs[i].SetActive(i == index);

        EnsureDefaultSelection(index);
    }

    // =============================
    // SWIPE HANDLERS
    // =============================

    void OnSwipeLeft()
    {
        // Swipe para esquerda → próxima aba
        int next = currentTabIndex + 1;
        if (next < tabs.Length)
            SwitchToTab(next);
    }

    void OnSwipeRight()
    {
        // Swipe para direita → aba anterior
        int prev = currentTabIndex - 1;
        if (prev >= 0)
            SwitchToTab(prev);
    }

    // =============================
    // SPLIT-SCREEN LAYOUT
    // =============================

    private void EnsureSplitLayout()
    {
        // This script lives on InventoryPanel in RestScene.
        // The old scene has only Tabs + Grids; we create TopPanels and wrap grids in ScrollRects.
        int tabCount = tabs != null ? tabs.Length : 0;
        if (tabCount <= 0) return;

        EnsureTabsBar(tabCount);

        // 1) Top panels (one per tab)
        if (topPanels == null || topPanels.Length != tabCount)
        {
            topPanels = new RectTransform[tabCount];
            topPanelTexts = new TextMeshProUGUI[tabCount];

            for (int i = 0; i < tabCount; i++)
            {
                var panelGO = new GameObject($"TopPanel_{(InventoryKind)i}", typeof(RectTransform), typeof(Image));
                panelGO.transform.SetParent(transform, false);

                var rt = (RectTransform)panelGO.transform;
                rt.anchorMin = new Vector2(0, 1f - Mathf.Clamp01(topPanelHeightNormalized));
                rt.anchorMax = new Vector2(1, 1);
                rt.offsetMin = new Vector2(16, 16);
                rt.offsetMax = new Vector2(-16, -TabsBarHeight - 8);

                var img = panelGO.GetComponent<Image>();
                img.color = new Color(0, 0, 0, 0.25f);
                img.raycastTarget = false;

                var textGO = new GameObject("DetailsText", typeof(RectTransform), typeof(TextMeshProUGUI));
                textGO.transform.SetParent(panelGO.transform, false);

                var textRt = (RectTransform)textGO.transform;
                textRt.anchorMin = new Vector2(0, 0);
                textRt.anchorMax = new Vector2(1, 1);
                textRt.offsetMin = new Vector2(16, 16);
                textRt.offsetMax = new Vector2(-16, -16);

                var tmp = textGO.GetComponent<TextMeshProUGUI>();
                tmp.enableWordWrapping = true;
                tmp.fontSize = 28;
                tmp.color = Color.white;
                tmp.text = "(selecione um item abaixo)";
                tmp.raycastTarget = false;

                topPanels[i] = rt;
                topPanelTexts[i] = tmp;
            }

            // --- Phase 3 & 4 additions ---
            // Unit Top Panel (Index 0): Add a container for profile and 6 equipment slots
            if (topPanels.Length > 0)
            {
                var unitPanel = topPanels[0];

                if (topPanelTexts[0] != null)
                    topPanelTexts[0].gameObject.SetActive(false);

                // --- LEFT HALF: Profile (Icon + Stats/Abilities) ---
                var profileGO = new GameObject("ProfileContainer", typeof(RectTransform));
                profileGO.transform.SetParent(unitPanel, false);
                var profileRT = (RectTransform)profileGO.transform;
                profileRT.anchorMin = new Vector2(0, 0);
                profileRT.anchorMax = new Vector2(0.4f, 1);
                profileRT.offsetMin = new Vector2(16, 16);
                profileRT.offsetMax = new Vector2(0, -16);

                var iconGO = new GameObject("UnitIcon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(profileRT, false);
                var iconRT = (RectTransform)iconGO.transform;
                iconRT.anchorMin = new Vector2(0.1f, 0.45f);
                iconRT.anchorMax = new Vector2(0.9f, 1f);
                iconRT.offsetMin = Vector2.zero;
                iconRT.offsetMax = Vector2.zero;
                unitIconImage = iconGO.GetComponent<Image>();
                unitIconImage.preserveAspect = true;

                var statsGO = new GameObject("UnitStats", typeof(RectTransform), typeof(TextMeshProUGUI));
                statsGO.transform.SetParent(profileRT, false);
                var statsRT = (RectTransform)statsGO.transform;
                statsRT.anchorMin = new Vector2(0, 0f);
                statsRT.anchorMax = new Vector2(1, 0.45f);
                statsRT.offsetMin = new Vector2(0, 48); // 48px space for abilities
                statsRT.offsetMax = new Vector2(0, -8);
                unitStatsText = statsGO.GetComponent<TextMeshProUGUI>();
                unitStatsText.fontSize = 18; // Menor para caber tudo
                unitStatsText.enableWordWrapping = true;
                unitStatsText.color = Color.white;
                unitStatsText.alignment = TextAlignmentOptions.TopLeft;

                var abilitiesGO = new GameObject("UnitAbilities", typeof(RectTransform), typeof(UnityEngine.UI.HorizontalLayoutGroup));
                abilitiesGO.transform.SetParent(profileRT, false);
                unitAbilitiesContainer = (RectTransform)abilitiesGO.transform;
                unitAbilitiesContainer.anchorMin = new Vector2(0, 0);
                unitAbilitiesContainer.anchorMax = new Vector2(1, 0);
                unitAbilitiesContainer.offsetMin = new Vector2(0, 0);
                unitAbilitiesContainer.offsetMax = new Vector2(0, 48);
                var hLG = abilitiesGO.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                hLG.childControlWidth = false; hLG.childForceExpandWidth = false;
                hLG.childControlHeight = true; hLG.childForceExpandHeight = true;
                hLG.spacing = 8;
                hLG.childAlignment = TextAnchor.MiddleLeft;

                // --- RIGHT HALF: Equip slots ---
                var equipGO = new GameObject("EquipContainer", typeof(RectTransform), typeof(GridLayoutGroup));
                equipGO.transform.SetParent(unitPanel, false);
                unitEquipContainer = (RectTransform)equipGO.transform;
                unitEquipContainer.anchorMin = new Vector2(0.4f, 0);
                unitEquipContainer.anchorMax = new Vector2(1, 1);
                unitEquipContainer.offsetMin = new Vector2(16, 16);
                unitEquipContainer.offsetMax = new Vector2(-16, -16);
                
                var grid = equipGO.GetComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(80, 80);
                grid.spacing = new Vector2(10, 10);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 2; // 2 cols x 3 rows = 6 slots
                grid.childAlignment = TextAnchor.MiddleCenter;

                CelestialCross.Artifacts.ArtifactType[] slotTypes = 
                    (CelestialCross.Artifacts.ArtifactType[])Enum.GetValues(typeof(CelestialCross.Artifacts.ArtifactType));
                
                for (int slotIdx = 0; slotIdx < 7; slotIdx++)
                {
                    int sIdx = slotIdx;
                    bool isPetSlot = sIdx == 6;
                    var sType = isPetSlot ? default : slotTypes[Mathf.Min(slotIdx, slotTypes.Length - 1)];
                    
                    string slotName = isPetSlot ? "Pet" : sType.ToString();
                    var bGO = new GameObject($"SlotBtn_{slotName}", typeof(RectTransform), typeof(Image), typeof(Button));
                    bGO.transform.SetParent(unitEquipContainer, false);
                    bGO.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);
                    var btn = bGO.GetComponent<Button>();
                    
                    var tGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    tGO.transform.SetParent(bGO.transform, false);
                    var tRT = (RectTransform)tGO.transform;
                    tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
                    tRT.offsetMin = tRT.offsetMax = Vector2.zero;
                    
                    var tTMP = tGO.GetComponent<TextMeshProUGUI>();
                    tTMP.fontSize = 15;
                    tTMP.alignment = TextAlignmentOptions.Center;
                    tTMP.enableWordWrapping = true;
                    tTMP.text = slotName + "\n<color=#888>(Vazio)</color>";
                    tTMP.raycastTarget = false;
                    
                    unitEquipButtons[slotIdx] = btn;
                    unitEquipTexts[slotIdx] = tTMP;
                    
                    if (isPetSlot)
                        btn.onClick.AddListener(() => OnUnitPetSlotClicked());
                    else
                        btn.onClick.AddListener(() => OnUnitEquipSlotClicked(sType));
                }
            }

            // Artifact Top Panel (Index 2): Add Equip/Cancel buttons
            if (topPanels.Length > 2)
            {
                var artPanel = topPanels[2];
                // Cancel
                var cGO = new GameObject("CancelEquipBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                cGO.transform.SetParent(artPanel, false);
                var cRT = (RectTransform)cGO.transform;
                cRT.anchorMin = new Vector2(1, 1); cRT.anchorMax = new Vector2(1, 1);
                cRT.pivot = new Vector2(1, 1);
                cRT.anchoredPosition = new Vector2(-16, -16);
                cRT.sizeDelta = new Vector2(100, 40);
                cGO.GetComponent<Image>().color = Color.red * 0.7f;
                var cText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                cText.transform.SetParent(cGO.transform, false);
                var cTMP = cText.GetComponent<TextMeshProUGUI>();
                cTMP.rectTransform.anchorMin = Vector2.zero; cTMP.rectTransform.anchorMax = Vector2.one;
                cTMP.rectTransform.offsetMin = cTMP.rectTransform.offsetMax = Vector2.zero;
                cTMP.alignment = TextAlignmentOptions.Center;
                cTMP.fontSize = 20; cTMP.text = "Voltar";
                cancelEquipButton = cGO.GetComponent<Button>();
                cancelEquipButton.gameObject.SetActive(false);
                cancelEquipButton.onClick.AddListener(CancelEquipMode);

                // Equip
                var eGO = new GameObject("EquipBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                eGO.transform.SetParent(artPanel, false);
                var eRT = (RectTransform)eGO.transform;
                eRT.anchorMin = new Vector2(1, 0); eRT.anchorMax = new Vector2(1, 0);
                eRT.pivot = new Vector2(1, 0);
                eRT.anchoredPosition = new Vector2(-16, 16);
                eRT.sizeDelta = new Vector2(120, 50);
                eGO.GetComponent<Image>().color = Color.green * 0.7f;
                var eText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                eText.transform.SetParent(eGO.transform, false);
                var eTMP = eText.GetComponent<TextMeshProUGUI>();
                eTMP.rectTransform.anchorMin = Vector2.zero; eTMP.rectTransform.anchorMax = Vector2.one;
                eTMP.rectTransform.offsetMin = eTMP.rectTransform.offsetMax = Vector2.zero;
                eTMP.alignment = TextAlignmentOptions.Center;
                eTMP.fontSize = 24; eTMP.text = "Equipar";
                equipArtifactButton = eGO.GetComponent<Button>();
                equipArtifactText = eTMP;
                equipArtifactButton.gameObject.SetActive(false);
                equipArtifactButton.onClick.AddListener(ConfirmEquip);
            }
            // -----------------------------
        }

        // 2) Bottom scroll wrappers
        if (gridContainers == null || gridContainers.Length == 0)
            return;

        bottomScrollRoots = new GameObject[gridContainers.Length];

        for (int i = 0; i < gridContainers.Length; i++)
        {
            var content = gridContainers[i];
            if (content == null) continue;

            // If already inside a ScrollRect, reuse it.
            var existingScroll = content.GetComponentInParent<ScrollRect>();
            if (existingScroll != null && existingScroll.content == content)
            {
                bottomScrollRoots[i] = existingScroll.gameObject;
                continue;
            }

            // Create scroll root
            var scrollGO = new GameObject($"BottomScroll_{(InventoryKind)i}", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
            scrollGO.transform.SetParent(transform, false);
            var scrollRT = (RectTransform)scrollGO.transform;
            scrollRT.anchorMin = new Vector2(0, 0);
            scrollRT.anchorMax = new Vector2(1, 1f - Mathf.Clamp01(topPanelHeightNormalized));
            scrollRT.offsetMin = new Vector2(16, 16);
            scrollRT.offsetMax = new Vector2(-16, -16);

            var scrollImage = scrollGO.GetComponent<Image>();
            scrollImage.color = new Color(0, 0, 0, 0.15f);

            var mask = scrollGO.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            // Viewport
            var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGO.transform.SetParent(scrollGO.transform, false);
            var viewportRT = (RectTransform)viewportGO.transform;
            viewportRT.anchorMin = new Vector2(0, 0);
            viewportRT.anchorMax = new Vector2(1, 1);
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;
            var viewportMask = viewportGO.GetComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            var viewportImage = viewportGO.GetComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0.02f);
            viewportImage.raycastTarget = false;

            // Reparent content under viewport
            content.SetParent(viewportRT, false);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.offsetMin = new Vector2(0, 0);
            content.offsetMax = new Vector2(0, 0);

            var scroll = scrollGO.GetComponent<ScrollRect>();
            scroll.viewport = viewportRT;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;

            bottomScrollRoots[i] = scrollGO;
        }

        // Ensure the tabs are always on top of the dynamic content.
        if (tabsBar != null)
            tabsBar.SetAsLastSibling();
    }

    private void EnsureTabsBar(int tabCount)
    {
        if (tabsBar == null)
        {
            var existing = transform.Find("TabsBar") as RectTransform;
            if (existing != null)
            {
                tabsBar = existing;
            }
            else
            {
                var barGO = new GameObject("TabsBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                barGO.transform.SetParent(transform, false);
                tabsBar = (RectTransform)barGO.transform;

                tabsBar.anchorMin = new Vector2(0, 1);
                tabsBar.anchorMax = new Vector2(1, 1);
                tabsBar.pivot = new Vector2(0.5f, 1);
                tabsBar.anchoredPosition = Vector2.zero;
                tabsBar.sizeDelta = new Vector2(0, TabsBarHeight);

                var layout = barGO.GetComponent<HorizontalLayoutGroup>();
                layout.padding = new RectOffset(16, 16, 12, 12);
                layout.spacing = 12f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
            }
        }

        // Reparent tabs into the bar (keeps current buttons/images intact).
        for (int i = 0; i < tabCount; i++)
        {
            if (tabs[i] == null) continue;

            var tabRT = tabs[i].GetComponent<RectTransform>();
            if (tabRT == null) continue;

            if (tabRT.parent != tabsBar)
                tabRT.SetParent(tabsBar, false);

            tabRT.anchorMin = new Vector2(0.5f, 0.5f);
            tabRT.anchorMax = new Vector2(0.5f, 0.5f);
            tabRT.pivot = new Vector2(0.5f, 0.5f);
            tabRT.anchoredPosition = Vector2.zero;
        }

        tabsBar.SetAsLastSibling();
    }

    // =============================
    // DATA POPULATION
    // =============================

    private void RefreshAllTabs()
    {
        if (gridContainers == null) return;

        for (int i = 0; i < gridContainers.Length; i++)
            PopulateTab(i);
    }

    private void PopulateTab(int tabIndex)
    {
        if (gridContainers == null || tabIndex < 0 || tabIndex >= gridContainers.Length) return;
        var container = gridContainers[tabIndex];
        if (container == null) return;

        ClearSpawned(tabIndex);

        var account = AccountManager.Instance != null ? AccountManager.Instance.PlayerAccount : null;

        if (account == null)
        {
            // No account available: show placeholders.
            for (int i = 0; i < 12; i++)
                SpawnItem(tabIndex, container, $"Item {i + 1}", () => SetDetails(tabIndex, $"Item {i + 1}"));
            return;
        }

        account.EnsureInitialized();

        switch ((InventoryKind)Mathf.Clamp(tabIndex, 0, 2))
        {
            case InventoryKind.Units:
                if (account.OwnedUnitIDs != null)
                {
                    for (int i = 0; i < account.OwnedUnitIDs.Count; i++)
                    {
                        string id = account.OwnedUnitIDs[i];
                        if (string.IsNullOrWhiteSpace(id)) continue;
                        
                        Sprite icon = null;
                        if (unitCatalog != null) {
                            var data = unitCatalog.GetUnitData(id);
                            if (data != null) icon = data.icon;
                        }
                        
                        SpawnItem(tabIndex, container, id, () => ShowUnitDetails(id, account), icon);
                    }
                }
                break;

            case InventoryKind.Pets:
                if (account.OwnedPetIDs != null)
                {
                    for (int i = 0; i < account.OwnedPetIDs.Count; i++)
                    {
                        string id = account.OwnedPetIDs[i];
                        if (string.IsNullOrWhiteSpace(id)) continue;
                        
                        Sprite icon = null;
                        if (petCatalog != null) {
                            var data = petCatalog.GetPetData(id);
                            if (data != null) icon = data.icon;
                        }
                        
                        SpawnItem(tabIndex, container, id, () => OnPetClicked(id), icon);
                    }
                }
                break;

            case InventoryKind.Artifacts:
                if (account.OwnedArtifacts != null)
                {
                    for (int i = 0; i < account.OwnedArtifacts.Count; i++)
                    {
                        var a = account.OwnedArtifacts[i];
                        if (a == null) continue;

                        // Phase 4 filter
                        if (isSelectingArtifact && a.slot != selectingForSlot)
                            continue;

                        string label = $"{a.slot}\n{a.rarity} {a.GetStarsAsIntClamped()}* +{a.currentLevel}";
                        SpawnItem(tabIndex, container, label, () => OnArtifactClicked(a));
                    }
                }
                break;
        }

        // Empty-state messaging
        if (spawnedItemsPerTab[tabIndex].Count == 0)
        {
            string emptyLabel = tabIndex == 0 ? "(sem unidades)" : tabIndex == 1 ? "(sem pets)" : "(sem artefatos)";
            SpawnItem(tabIndex, container, emptyLabel, () => SetDetails(tabIndex, emptyLabel));
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

    private void SpawnItem(int tabIndex, RectTransform parent, string label, Action onClick, Sprite icon = null)
    {
        if (parent == null) return;

        GameObject go;
        if (slotPrefab != null)
        {
            go = Instantiate(slotPrefab, parent);
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
                button.onClick.AddListener(() => onClick());
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
            // Opcional: esconder a label inteira
            // textGO.SetActive(false);
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

    private void ProcessStatData(CelestialCross.Artifacts.StatModifierData stat, ref float hF, ref float hP, ref float aF, ref float aP, ref float dF, ref float dP, ref float spdF, ref float crF, ref float eaf)
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
            case CelestialCross.Artifacts.StatType.EffectHitRate: eaf += stat.value; break;
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

        // Tooltip logic: press and hold overrides unitStatsText temporarly or a dedicated panel
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

    // --- Phase 3 & 4 Logics ---
    private void ShowUnitDetails(string unitId, Account account)
    {
        selectingForUnitId = unitId;
        
        if (unitEquipContainer != null) unitEquipContainer.gameObject.SetActive(true);

        UnitData data = null;
        if (unitCatalog != null)
            data = unitCatalog.GetUnitData(unitId);

        var loadout = account.GetLoadoutForUnit(unitId);
        
        if (data != null && unitStatsText != null && unitIconImage != null)
        {
            unitIconImage.sprite = data.icon;
            
            var baseStats = data.baseStats;
            PetData equippedPet = null;
            if (loadout != null && !string.IsNullOrEmpty(loadout.PetID) && petCatalog != null)
            {
                equippedPet = petCatalog.GetPetData(loadout.PetID);
                if (equippedPet != null)
                {
                    baseStats += equippedPet.baseStats;
                }
            }

            float hF = 0, hP = 0, aF = 0, aP = 0, dF = 0, dP = 0, spdF = 0, crF = 0, eaf = 0;
            if (loadout != null)
            {
                var artifactIDs = loadout.GetEquippedArtifactIDs();
                foreach (var guid in artifactIDs)
                {
                    var arti = account.GetArtifactByGuid(guid);
                    if (arti != null)
                    {
                        ProcessStatData(arti.mainStat, ref hF, ref hP, ref aF, ref aP, ref dF, ref dP, ref spdF, ref crF, ref eaf);
                        if (arti.subStats != null)
                        {
                            foreach (var sub in arti.subStats)
                            {
                                ProcessStatData(sub, ref hF, ref hP, ref aF, ref aP, ref dF, ref dP, ref spdF, ref crF, ref eaf);
                            }
                        }
                    }
                }
            }

            int finalHealth = (int)(Mathf.Round(baseStats.health + hF) * (1f + (hP / 100f)));
            int finalAttack = (int)(Mathf.Round(baseStats.attack + aF) * (1f + (aP / 100f)));
            int finalDefense = (int)(Mathf.Round(baseStats.defense + dF) * (1f + (dP / 100f)));
            int finalSpeed = (int)Mathf.Round(baseStats.speed + spdF);
            int finalCrit = Mathf.Clamp((int)Mathf.Round(baseStats.criticalChance + crF), 0, 100);
            int finalAcc = Mathf.Clamp((int)Mathf.Round(baseStats.effectAccuracy + eaf), 0, 100);

            string abilitiesList = "Nenhuma habilidade";
            if (unitAbilitiesContainer != null)
            {
                // Clear old buttons
                foreach (RectTransform child in unitAbilitiesContainer) Destroy(child.gameObject);
                
                if (data.GetAbilities() != null)
                {
                    foreach (var ab in data.GetAbilities())
                    {
                        if (ab != null) SpawnAbilityButton(unitAbilitiesContainer, ab, false);
                    }
                }
                
                if (equippedPet != null && equippedPet.ability != null)
                {
                    SpawnAbilityButton(unitAbilitiesContainer, equippedPet.ability, true);
                }
            }

            defaultStatsText = $"<b><size=24>{data.displayName}</size></b>\n" +
                                 $"<size=18>" +
                                 $"<color=#ff8888>HP:</color> {finalHealth}   " +
                                 $"<color=#ff8888>ATK:</color> {finalAttack}   " +
                                 $"<color=#ff8888>DEF:</color> {finalDefense}\n" +
                                 $"<color=#ff8888>SPD:</color> {finalSpeed}   " +
                                 $"<color=#ff8888>CRIT:</color> {finalCrit}%   " +
                                 $"<color=#ff8888>ACC:</color> {finalAcc}%" +
                                 $"</size>";
            unitStatsText.text = defaultStatsText;
        }
        else if (unitStatsText != null)
        {
            unitStatsText.text = $"<b>{unitId}</b>\n(UnitData não encontrado - configure o UnitCatalog)";
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
                if (loadout != null && !string.IsNullOrEmpty(loadout.PetID))
                {
                    var pet = petCatalog?.GetPetData(loadout.PetID);
                    if (pet != null)
                        unitEquipTexts[i].text = $"<b>Pet</b>\n<color=#ffb>{pet.displayName}</color>";
                    else
                        unitEquipTexts[i].text = $"<b>Pet</b>\n<color=#ffb>{loadout.PetID}</color>";
                }
                else
                {
                    unitEquipTexts[i].text = "Pet\n<color=#888>(vazio)</color>";
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

            if (string.IsNullOrEmpty(equippedGuid))
            {
                unitEquipTexts[i].text = $"{sType}\n<color=#888>(vazio)</color>";
            }
            else
            {
                var artifact = account.GetArtifactByGuid(equippedGuid);
                if (artifact != null)
                    unitEquipTexts[i].text = $"<b>{sType}</b>\n<color=#ffb>{artifact.rarity} +{artifact.currentLevel}</color>";
                else
                    unitEquipTexts[i].text = $"{sType}\n<color=red>Miss</color>";
            }
        }
    }

    private void OnUnitEquipSlotClicked(CelestialCross.Artifacts.ArtifactType slot)
    {
        if (string.IsNullOrEmpty(selectingForUnitId)) return;
        
        isSelectingArtifact = true;
        isSelectingPet = false;
        selectingForSlot = slot;
        
        if (cancelEquipButton != null) cancelEquipButton.gameObject.SetActive(true);
        if (equipArtifactButton != null) equipArtifactButton.gameObject.SetActive(false);
        SetDetails(2, $"Selecione um <b>{slot}</b> para <b>{selectingForUnitId}</b>");

        SwitchToTab(2); // Vai para artefatos
        PopulateTab(2); // Refaz a lista mostrando apenas a categoria filtrada
    }

    private void OnUnitPetSlotClicked()
    {
        if (string.IsNullOrEmpty(selectingForUnitId)) return;
        
        isSelectingPet = true;
        isSelectingArtifact = false;
        
        if (cancelEquipButton != null) cancelEquipButton.gameObject.SetActive(true);
        if (equipArtifactButton != null) equipArtifactButton.gameObject.SetActive(false);
        SetDetails(1, $"Selecione um Pet para <b>{selectingForUnitId}</b>");

        SwitchToTab(1); // Vai para Pets
        PopulateTab(1); // Refaz a lista, dessa vez em modo seleção
    }

    private void OnPetClicked(string petId)
    {
        SetDetails(1, $"Pet selecionado:\n{petId}");
        
        if (isSelectingPet)
        {
            selectedPetToEquipId = petId;
            if (equipArtifactButton != null) equipArtifactButton.gameObject.SetActive(true);
        }
    }

    private void CancelEquipMode()
    {
        isSelectingArtifact = false;
        isSelectingPet = false;
        selectingForUnitId = null;
        selectedArtifactToEquip = null;
        selectedPetToEquipId = null;
        
        if (cancelEquipButton != null) cancelEquipButton.gameObject.SetActive(false);
        if (equipArtifactButton != null) equipArtifactButton.gameObject.SetActive(false);
        
        SwitchToTab(0); // Volta pra Unidades
        PopulateTab(1); // Atualiza Pets para todos
        PopulateTab(2); // Atualiza Artefatos para todos
    }

    private void OnArtifactClicked(CelestialCross.Artifacts.ArtifactInstanceData artifact)
    {
        SetDetails(2, FormatArtifactDetails(artifact));
        
        if (isSelectingArtifact)
        {
            selectedArtifactToEquip = artifact;
            if (equipArtifactButton != null) equipArtifactButton.gameObject.SetActive(true);
        }
    }

    private void ConfirmEquip()
    {
        var account = AccountManager.Instance?.PlayerAccount;
        if (account == null) return;
        
        var loadout = account.GetLoadoutForUnit(selectingForUnitId);
        if (loadout == null) return;

        if (isSelectingArtifact && selectedArtifactToEquip != null)
        {
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

    private static string FormatArtifactDetails(CelestialCross.Artifacts.ArtifactInstanceData a)
    {
        if (a == null) return "Artefato inválido.";

        string setLabel = string.IsNullOrWhiteSpace(a.artifactSetId) ? "<sem set>" : a.artifactSetId;
        string main = a.mainStat != null ? $"{a.mainStat.statType} +{a.mainStat.value:F0}" : "<mainStat null>";

        string sub = "(nenhum)";
        if (a.subStats != null && a.subStats.Count > 0)
        {
            sub = "";
            for (int i = 0; i < a.subStats.Count; i++)
            {
                var s = a.subStats[i];
                if (s == null) continue;
                sub += $"- {s.statType} +{s.value:F0}\n";
            }
            sub = sub.TrimEnd();
        }

        return
            $"Artefato\n" +
            $"GUID: {a.idGUID}\n" +
            $"Slot: {a.slot}\n" +
            $"Set: {setLabel}\n" +
            $"Raridade: {a.rarity}\n" +
            $"Estrelas: {a.GetStarsAsIntClamped()}*\n" +
            $"Nível: +{a.currentLevel}\n\n" +
            $"Main: {main}\n\n" +
            $"Substats:\n{sub}";
    }
}
