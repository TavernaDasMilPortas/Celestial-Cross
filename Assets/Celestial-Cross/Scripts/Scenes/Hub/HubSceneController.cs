using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HubSceneController : MonoBehaviour
{
    [Header("Flow")]
    [SerializeField] private string preparationSceneName = "PreparationScene";
    [SerializeField] private string restSceneName = "RestScene";

    [Header("Data")]
    [SerializeField] private LevelCatalog levelCatalog;
    [SerializeField] private CelestialCross.Data.DungeonCatalog dungeonCatalog;

    [Header("UI")]
    [SerializeField] private Transform levelsContainer;
    [SerializeField] private Button levelButtonPrefab;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text energyText;

    void Start()
    {
        RefreshAccountUI();
        BuildLevelButtons();
        EnsureInventoryButton();
    }

    private void EnsureInventoryButton()
    {
        if (levelsContainer == null) return;
        var parentCanvas = levelsContainer.GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;

        var go = new GameObject("Btn_GoInventory", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parentCanvas.transform, false);
        
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -100); // Top Right corner below potential safe area
        rt.sizeDelta = new Vector2(180, 60);

        go.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.8f, 1f);
        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(GoToInventoryScene);

        var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TMP_Text));
        txtGo.transform.SetParent(go.transform, false);
        var txtRt = (RectTransform)txtGo.transform;
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;

        var tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "Inventário";
        tmp.color = Color.white;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    public void GoToInventoryScene()
    {
        if (!string.IsNullOrEmpty(restSceneName))
            SceneManager.LoadScene(restSceneName);
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

    void BuildLevelButtons()
    {
        if (levelsContainer == null || levelButtonPrefab == null)
        {
            Debug.LogWarning("[HubSceneController] UI não configurada (levelsContainer / levelButtonPrefab). ");
            return;
        }

        foreach (Transform child in levelsContainer)
            Destroy(child.gameObject);

        List<LevelData> levels = levelCatalog != null ? levelCatalog.Levels : null;
        if (levels == null || levels.Count == 0)
        {
            Debug.LogWarning("[HubSceneController] Nenhum LevelData configurado no LevelCatalog.");
            return;
        }

        foreach (var level in levels)
        {
            if (level == null) continue;

            Button btn = Instantiate(levelButtonPrefab, levelsContainer);
            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = string.IsNullOrWhiteSpace(level.LevelName) ? level.name : level.LevelName;

            btn.onClick.AddListener(() => SelectLevelAndGo(level));
        }
    }

    void SelectLevelAndGo(LevelData level)
    {
        if (GameFlowManager.Instance == null)
        {
            Debug.LogError("[HubSceneController] GameFlowManager não encontrado na cena.");
            return;
        }

        GameFlowManager.Instance.SelectedLevel = level;

        // Resolve qual Dungeon e Node pertencem a essa fase para poder dropar os artefatos certos depois
        if (dungeonCatalog != null)
        {
            if (dungeonCatalog.TryFindDungeonForLevel(level, out var d, out var node))
            {
                GameFlowManager.Instance.SelectedDungeon = d;
                GameFlowManager.Instance.SelectedDungeonNode = node;
            }
            else
            {
                GameFlowManager.Instance.SelectedDungeon = null;
                GameFlowManager.Instance.SelectedDungeonNode = null;
                Debug.LogWarning($"[HubSceneController] Nenhuma Dungeon no catálogo contém o {level.name}. Esta fase não vai ter drop procedural de Artefatos.");
            }
        }
        else
        {
            Debug.LogWarning("[HubSceneController] dungeonCatalog não referenciado no Inspector.");
        }

        // A seleção acontece na PreparationScene.
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
