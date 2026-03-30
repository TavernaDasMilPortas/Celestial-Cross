using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HubSceneController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private LevelCatalog levelCatalog;

    [Header("UI")]
    [SerializeField] private Transform levelsContainer;
    [SerializeField] private Button levelButtonPrefab;
    [SerializeField] private Text moneyText;
    [SerializeField] private Text energyText;

    [Header("Flow")]
    [SerializeField] private string preparationSceneName = "PreparationScene";

    void Start()
    {
        RefreshAccountUI();
        BuildLevelButtons();
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
            Text label = btn.GetComponentInChildren<Text>();
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
        GameFlowManager.Instance.SelectedUnitIDs = new List<string>();
        GameFlowManager.Instance.UnitInitialPositions = new Dictionary<string, Vector2Int>();

        if (string.IsNullOrWhiteSpace(preparationSceneName))
        {
            Debug.LogError("[HubSceneController] preparationSceneName vazio.");
            return;
        }

        SceneManager.LoadScene(preparationSceneName);
    }
}
