using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using CelestialCross.Giulia_UI;
using CelestialCross.Data.Pets;

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
        InitializeTabs();
        RegisterSwipe();

        if (manageArtifactButton != null)
            manageArtifactButton.onClick.AddListener(OnManageArtifactClicked);

        if (managePetButton != null)
            managePetButton.onClick.AddListener(OnManagePetClicked);

        WireUpFixedButtons();
        
        RefreshAllTabs();
        SwitchToTab(0);
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
                SpawnItem(tabIndex, container, "", $"Item {i + 1}", () => SetDetails(tabIndex, $"Item {i + 1}"));
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
        bool isSelected = (!string.IsNullOrEmpty(itemId) && itemId == currentlySelectedTabItemId);
        bool isEquippedTarget = false;
        
        if (isSelectingPet && tabIndex == 1) isEquippedTarget = (!string.IsNullOrEmpty(itemId) && itemId == originEquippedId);
        if (isSelectingArtifact && tabIndex == 2) isEquippedTarget = (!string.IsNullOrEmpty(itemId) && itemId == originEquippedId);

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
        string desc = string.IsNullOrEmpty(ability.abilityDescription) ? "Sem descriÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o" : ability.abilityDescription;
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
        currentlySelectedTabItemId = unitId;
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
            RuntimePetData equippedPet = null;
            CelestialCross.Data.Pets.PetSpeciesSO equippedPetSpecies = null;
            if (loadout != null && !string.IsNullOrEmpty(loadout.PetID))
            {
                equippedPet = account.GetPetByUUID(loadout.PetID);
                if (equippedPet != null && petCatalog != null)
                {
                    equippedPetSpecies = petCatalog.GetPetSpecies(equippedPet.SpeciesID);
                    if (equippedPetSpecies != null)
                    {
                        // Adding simple pet stats to base stats manually 
                        baseStats.health += equippedPet.Health;
                        baseStats.attack += equippedPet.Attack;
                        baseStats.defense += equippedPet.Defense;
                        baseStats.speed += equippedPet.Speed;
                        baseStats.criticalChance = Mathf.Clamp(baseStats.criticalChance + equippedPet.CriticalChance, 0, 100);
                        baseStats.effectAccuracy = Mathf.Clamp(baseStats.effectAccuracy + equippedPet.EffectAccuracy, 0, 100);
                    }
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

            int finalHealth = (int)Mathf.Round(data.baseStats.health * (1f + (hP / 100f)) + hF);
            int finalAttack = (int)Mathf.Round(data.baseStats.attack * (1f + (aP / 100f)) + aF);
            int finalDefense = (int)Mathf.Round(data.baseStats.defense * (1f + (dP / 100f)) + dF);
            
            if (equippedPet != null && equippedPetSpecies != null)
            {
                finalHealth += equippedPet.Health;
                finalAttack += equippedPet.Attack;
                finalDefense += equippedPet.Defense;
            }
            
            int finalSpeed = (int)Mathf.Round(baseStats.speed + spdF);
            int finalCrit = Mathf.Clamp((int)Mathf.Round(baseStats.criticalChance + crF), 0, 100);
            int finalAcc = Mathf.Clamp((int)Mathf.Round(baseStats.effectAccuracy + eaf), 0, 100);

            string healthText = finalHealth > data.baseStats.health ? $"<color=#00ff00>{finalHealth}</color> <color=#aaaaaa>({data.baseStats.health} +{finalHealth - data.baseStats.health})</color>" : finalHealth.ToString();
            string attackText = finalAttack > data.baseStats.attack ? $"<color=#00ff00>{finalAttack}</color> <color=#aaaaaa>({data.baseStats.attack} +{finalAttack - data.baseStats.attack})</color>" : finalAttack.ToString();
            string defenseText = finalDefense > data.baseStats.defense ? $"<color=#00ff00>{finalDefense}</color> <color=#aaaaaa>({data.baseStats.defense} +{finalDefense - data.baseStats.defense})</color>" : finalDefense.ToString();
            string speedText = finalSpeed > data.baseStats.speed ? $"<color=#00ff00>{finalSpeed}</color> <color=#aaaaaa>({data.baseStats.speed} +{finalSpeed - data.baseStats.speed})</color>" : finalSpeed.ToString();
            string critText = finalCrit > data.baseStats.criticalChance ? $"<color=#00ff00>{finalCrit}%</color> <color=#aaaaaa>({data.baseStats.criticalChance}% +{finalCrit - data.baseStats.criticalChance}%)</color>" : $"{finalCrit}%";
            string accText = finalAcc > data.baseStats.effectAccuracy ? $"<color=#00ff00>{finalAcc}%</color> <color=#aaaaaa>({data.baseStats.effectAccuracy}% +{finalAcc - data.baseStats.effectAccuracy}%)</color>" : $"{finalAcc}%";

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
                if (equippedPetSpecies != null)
                {
                    if (equippedPetSpecies.PassiveSkills != null) foreach(var ab in equippedPetSpecies.PassiveSkills) if (ab != null) SpawnAbilityButton(unitAbilitiesContainer, ab, true);
                    if (equippedPetSpecies.ActiveSkills != null) foreach(var ab in equippedPetSpecies.ActiveSkills) if (ab != null) SpawnAbilityButton(unitAbilitiesContainer, ab, true);
                }
            }

            defaultStatsText = $"<b><size=24>{data.displayName}</size></b>\n" +
                                 $"<size=18>" +
                                 $"<color=#ff8888>HP:</color> {healthText}   " +
                                 $"<color=#ff8888>ATK:</color> {attackText}   " +
                                 $"<color=#ff8888>DEF:</color> {defenseText}\n" +
                                 $"<color=#ff8888>SPD:</color> {speedText}   " +
                                 $"<color=#ff8888>CRIT:</color> {critText}   " +
                                 $"<color=#ff8888>ACC:</color> {accText}" +
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
                      $"SPD: +{data.Speed}   CRIT: +{data.CriticalChance}%   ACC: +{data.EffectAccuracy}%\n\n";
            if (speciesData != null)
            {
                if (speciesData.PassiveSkills != null) foreach (var ab in speciesData.PassiveSkills) if (ab != null) { details += $"<color=#ffffaa>{ab.abilityName}</color>\n<size=16>{ab.abilityDescription}</size>\n"; }
                if (speciesData.ActiveSkills != null) foreach (var ab in speciesData.ActiveSkills) if (ab != null) { details += $"<color=#ffffaa>{ab.abilityName}</color>\n<size=16>{ab.abilityDescription}</size>\n"; }
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

    private static string FormatArtifactDetails(CelestialCross.Artifacts.ArtifactInstanceData a)
    {
        if (a == null) return "Artefato invÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡lido.";

        string setLabel = string.IsNullOrWhiteSpace(a.artifactSetId) ? "<sem set>" : a.artifactSetId;
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















