using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class HubCategory
{
    public string categoryName;
    [Tooltip("Deixe preenchido para modos com várias masmorras.")]
    public CelestialCross.Data.DungeonCatalog dungeonCatalog;
    [Tooltip("Deixe preenchido para modos de história diretos (sem masmorras).")]
    public LevelCatalog levelCatalog;
    [Tooltip("Deixe preenchido para o Catálogo de Diários.")]
    public CelestialCross.Dialogue.Data.DiaryCatalog diaryCatalog;
    [Tooltip("Deixe preenchido para o novo Sistema de Capítulos (História/Diários).")]
    public CelestialCross.Progression.ChapterCatalog chapterCatalog;
}

public class HubSceneController : MonoBehaviour
{
    [Header("Flow")]
    [SerializeField] private string preparationSceneName = "PreparationScene";
    [SerializeField] private string restSceneName = "RestScene";
    [SerializeField] private string shopSceneName = "ShopScene";
    [SerializeField] private string dialogueSceneName = "DialogueScene";

    [Header("Categories Config")]
    [SerializeField] private List<HubCategory> hubCategories = new List<HubCategory>();

    [Header("Top Bar UI")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text energyText;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject dungeonsPanel;
    [SerializeField] private GameObject levelsPanel;
    [SerializeField] private GameObject diaryPanel;

    [Header("Titles")]
    [SerializeField] private TMP_Text dungeonsPanelTitle;
    [SerializeField] private TMP_Text levelsPanelTitle;
    [SerializeField] private TMP_Text diaryPanelTitle;

    [Header("Containers")]
    [SerializeField] private Transform mainCategoriesContainer;
    [SerializeField] private Transform dungeonsContainer;
    [SerializeField] private Transform levelsContainer;
    [SerializeField] private Transform diaryContainer;
    
    [Header("Buttons & Prefabs")]
    [SerializeField] private Button genericButtonPrefab;
    [SerializeField] private Button btnGoInventory;
    [SerializeField] private Button btnGoShop;  // NOVO
    [SerializeField] private Button btnBackFromDungeons;
    [SerializeField] private Button btnBackFromLevels;
    [SerializeField] private Button btnBackFromDiary;

    private bool levelsCameFromDungeon = false;

    void Start()
    {
        RefreshAccountUI();

        if (btnGoInventory != null) btnGoInventory.onClick.AddListener(GoToInventoryScene);
        if (btnGoShop != null) btnGoShop.onClick.AddListener(GoToShopScene);
        if (btnBackFromDungeons != null) btnBackFromDungeons.onClick.AddListener(ShowMainPanel);
        if (btnBackFromLevels != null) btnBackFromLevels.onClick.AddListener(OnBackFromLevels);
        if (btnBackFromDiary != null) btnBackFromDiary.onClick.AddListener(ShowMainPanel);

        BuildCategoryButtons();
        ShowMainPanel();
    }

    public void GoToShopScene()
    {
        if (!string.IsNullOrEmpty(shopSceneName))
            SceneManager.LoadScene(shopSceneName);
    }

    public void RefreshAccountUI()
    {
        if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null)
            return;

        if (moneyText != null)
            moneyText.text = $"Dinheiro: {AccountManager.Instance.PlayerAccount.Money}";

        if (energyText != null)
            energyText.text = $"Energia: {AccountManager.Instance.PlayerAccount.Energy}";
    }

    public void GoToInventoryScene()
    {
        if (!string.IsNullOrEmpty(restSceneName))
            SceneManager.LoadScene(restSceneName);
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        foreach (Transform child in container)
            Destroy(child.gameObject);
    }

    private void BuildCategoryButtons()
    {
        ClearContainer(mainCategoriesContainer);

        if (hubCategories == null) return;

        foreach (var cat in hubCategories)
        {
            Button btn = Instantiate(genericButtonPrefab, mainCategoriesContainer);
            btn.gameObject.SetActive(true);

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = cat.categoryName;

            btn.onClick.AddListener(() => OnCategoryClicked(cat));
        }
    }

    private void OnCategoryClicked(HubCategory category)
    {
        if (category.dungeonCatalog != null)
        {
            if (dungeonsPanelTitle != null) dungeonsPanelTitle.text = category.categoryName;
            BuildDungeonButtons(category.dungeonCatalog);
            ShowDungeonsPanel();
        }
        else if (category.levelCatalog != null)
        {
            if (levelsPanelTitle != null) levelsPanelTitle.text = category.categoryName;
            levelsCameFromDungeon = false;
            BuildLevelButtonsForCatalog(category.levelCatalog);
            ShowLevelsPanel();
        }
        else if (category.diaryCatalog != null)
        {
            if (diaryPanelTitle != null) diaryPanelTitle.text = category.categoryName;
            BuildDiaryButtons(category.diaryCatalog);
            ShowDiaryPanel();
        }
        else if (category.chapterCatalog != null)
        {
            if (diaryPanelTitle != null) diaryPanelTitle.text = category.categoryName;
            BuildChapterButtons(category.chapterCatalog);
            ShowDiaryPanel(); // Usamos o painel de diário por enquanto, ou um específico no futuro
        }
        else
        {
            Debug.LogWarning($"[HubSceneController] Categoria '{category.categoryName}' não tem catálogo configurado.");
        }
    }

    private void BuildChapterButtons(CelestialCross.Progression.ChapterCatalog catalog)
    {
        ClearContainer(diaryContainer);
        if (catalog == null) return;

        // Pegar dados de progresso e unidades para filtrar
        var account = AccountManager.Instance?.PlayerAccount;
        HashSet<string> completedNodes = new HashSet<string>(account?.CompletedNodeIDs ?? new List<string>());
        List<string> ownedUnits = account?.OwnedUnitIDs ?? new List<string>();

        // Usar o método de filtro do catálogo
        var availableChapters = catalog.GetUnlockedChapters(completedNodes, ownedUnits);

        foreach (var chapter in availableChapters)
        {
            Button btn = Instantiate(genericButtonPrefab, diaryContainer);
            btn.gameObject.SetActive(true);

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = chapter.ChapterTitle;

            // Ao clicar no capítulo, abrimos a lista de nós (Nodes) dele
            // Aqui poderíamos ter uma tela específica, mas para simplificar, 
            // vamos carregar o primeiro nó não concluído do capítulo.
            btn.onClick.AddListener(() => {
                Debug.Log($"Abrindo Capítulo: {chapter.ChapterTitle}");
                // Lógica personalizada de navegação de capítulo...
            });
        }
    }

    public void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (dungeonsPanel != null) dungeonsPanel.SetActive(false);
        if (levelsPanel != null) levelsPanel.SetActive(false);
        if (diaryPanel != null) diaryPanel.SetActive(false);
    }

    public void ShowDungeonsPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (dungeonsPanel != null) dungeonsPanel.SetActive(true);
        if (levelsPanel != null) levelsPanel.SetActive(false);
        if (diaryPanel != null) diaryPanel.SetActive(false);
    }

    public void ShowLevelsPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (dungeonsPanel != null) dungeonsPanel.SetActive(false);
        if (levelsPanel != null) levelsPanel.SetActive(true);
        if (diaryPanel != null) diaryPanel.SetActive(false);
    }

    public void ShowDiaryPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (dungeonsPanel != null) dungeonsPanel.SetActive(false);
        if (levelsPanel != null) levelsPanel.SetActive(false);
        if (diaryPanel != null) diaryPanel.SetActive(true);
    }

    private void OnBackFromLevels()
    {
        if (levelsCameFromDungeon)
            ShowDungeonsPanel();
        else
            ShowMainPanel();
    }

    private void BuildDungeonButtons(CelestialCross.Data.DungeonCatalog catalog)
    {
        ClearContainer(dungeonsContainer);

        if (catalog == null || catalog.Dungeons == null) return;

        foreach (var dungeon in catalog.Dungeons)
        {
            if (dungeon == null) continue;

            Button btn = Instantiate(genericButtonPrefab, dungeonsContainer);
            btn.gameObject.SetActive(true);

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = string.IsNullOrWhiteSpace(dungeon.DungeonName) ? dungeon.name : dungeon.DungeonName;

            btn.onClick.AddListener(() => 
            {
                if (levelsPanelTitle != null) levelsPanelTitle.text = label.text;
                levelsCameFromDungeon = true;
                BuildLevelButtonsForDungeon(dungeon);
                ShowLevelsPanel();
            });
        }
    }

    private void BuildLevelButtonsForDungeon(CelestialCross.Data.Dungeon.DungeonBaseSO dungeon)
    {
        ClearContainer(levelsContainer);

        if (dungeon == null || dungeon.Levels == null) return;

        foreach (var node in dungeon.Levels)
        {
            if (node == null || node.LevelRef == null) continue;

            Button btn = Instantiate(genericButtonPrefab, levelsContainer);
            btn.gameObject.SetActive(true);
            
            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = string.IsNullOrWhiteSpace(node.LevelRef.LevelName) ? node.LevelRef.name : node.LevelRef.LevelName;

            btn.onClick.AddListener(() => SelectLevelAndGo(node.LevelRef, dungeon, node));
        }
    }

    private void BuildLevelButtonsForCatalog(LevelCatalog catalog)
    {
        ClearContainer(levelsContainer);

        if (catalog == null || catalog.Levels == null) return;

        foreach (var level in catalog.Levels)
        {
            if (level == null) continue;

            Button btn = Instantiate(genericButtonPrefab, levelsContainer);
            btn.gameObject.SetActive(true);
            
            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = string.IsNullOrWhiteSpace(level.LevelName) ? level.name : level.LevelName;

            btn.onClick.AddListener(() => SelectLevelAndGo(level, null, null));
        }
    }

    private void BuildDiaryButtons(CelestialCross.Dialogue.Data.DiaryCatalog catalog)
    {
        ClearContainer(diaryContainer);
        if (catalog == null || catalog.entries == null) return;

        foreach (var entry in catalog.entries)
        {
            if (entry == null || entry.dialogueGraph == null) continue;

            Button btn = Instantiate(genericButtonPrefab, diaryContainer);
            btn.gameObject.SetActive(true);

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = string.IsNullOrWhiteSpace(entry.diaryName) ? entry.dialogueGraph.name : entry.diaryName;

            btn.onClick.AddListener(() => StartDiary(entry));
        }
    }

    private void StartDiary(CelestialCross.Dialogue.Data.DiaryEntry entry)
    {
        if (entry == null || entry.dialogueGraph == null) return;

        CelestialCross.Dialogue.Manager.DialogueManager.Instance?.StartDialogue(entry.dialogueGraph);
        CelestialCross.Dialogue.Manager.DialogueManager.NextGraphToLoad = entry.dialogueGraph;

        if (string.IsNullOrWhiteSpace(dialogueSceneName))
        {
            Debug.LogError("[HubSceneController] dialogueSceneName vazio.");
            return;
        }

        SceneManager.LoadScene(dialogueSceneName);
    }

    private void SelectLevelAndGo(LevelData level, CelestialCross.Data.Dungeon.DungeonBaseSO dungeon, CelestialCross.Data.Dungeon.DungeonLevelNode node)
    {
        if (GameFlowManager.Instance == null)
        {
            Debug.LogError("[HubSceneController] GameFlowManager não encontrado na cena.");
            return;
        }

        GameFlowManager.Instance.SelectedLevel = level;
        GameFlowManager.Instance.SelectedDungeon = dungeon;
        GameFlowManager.Instance.SelectedDungeonNode = node;

        GameFlowManager.Instance.SelectedUnitIDs.Clear();
        GameFlowManager.Instance.PlayerFormation.Clear();

        if (string.IsNullOrWhiteSpace(preparationSceneName))
        {
            Debug.LogError("[HubSceneController] preparationSceneName vazio.");
            return;
        }

        SceneManager.LoadScene(preparationSceneName);
    }
}