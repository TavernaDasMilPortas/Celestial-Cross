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

    [Header("Data")]
    [SerializeField] private LevelCatalog levelCatalog;

    [Header("UI")]
    [SerializeField] private Transform levelsContainer;
    [SerializeField] private Button levelButtonPrefab;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text energyText;

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
