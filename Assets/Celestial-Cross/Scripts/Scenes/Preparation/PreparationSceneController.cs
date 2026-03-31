using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PreparationSceneController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UnitCatalog unitCatalog;

    [Header("UI - Owned Units")]
    [SerializeField] private Transform ownedUnitsContainer;
    [SerializeField] private Button ownedUnitButtonPrefab;

    [Header("UI - Selection")]
    [SerializeField] private Text selectedCountText;
    [SerializeField] private Button startBattleButton;

    [Header("Constraints")]
    [SerializeField] private int maxUnitsToBring = 3;

    private readonly HashSet<string> selectedUnitIds = new HashSet<string>();

    void Start()
    {
        BuildOwnedUnitButtons();

        if (startBattleButton != null)
            startBattleButton.onClick.AddListener(StartBattle);

        RefreshSelectedCount();
    }

    void BuildOwnedUnitButtons()
    {
        if (ownedUnitsContainer == null || ownedUnitButtonPrefab == null)
        {
            Debug.LogWarning("[PreparationScene] UI não configurada (ownedUnitsContainer / ownedUnitButtonPrefab). ");
            return;
        }

        foreach (Transform child in ownedUnitsContainer)
            Destroy(child.gameObject);

        if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null)
        {
            Debug.LogError("[PreparationScene] AccountManager/PlayerAccount não encontrado.");
            return;
        }

        if (unitCatalog == null)
        {
            Debug.LogError("[PreparationScene] UnitCatalog não configurado.");
            return;
        }

        foreach (var unitId in AccountManager.Instance.PlayerAccount.OwnedUnitIDs)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                continue;

            var data = unitCatalog.GetUnitData(unitId);

            Button btn = Instantiate(ownedUnitButtonPrefab, ownedUnitsContainer);
            Text label = btn.GetComponentInChildren<Text>();
            if (label != null)
                label.text = data != null && !string.IsNullOrWhiteSpace(data.displayName) ? data.displayName : unitId;

            btn.onClick.AddListener(() => ToggleSelectUnit(unitId, btn));
        }
    }

    void ToggleSelectUnit(string unitId, Button btn)
    {
        if (selectedUnitIds.Contains(unitId))
        {
            selectedUnitIds.Remove(unitId);
            SetButtonSelectedVisual(btn, false);
            RefreshSelectedCount();
            return;
        }

        if (selectedUnitIds.Count >= maxUnitsToBring)
        {
            Debug.Log($"[PreparationScene] Limite de units atingido ({maxUnitsToBring}).");
            return;
        }

        selectedUnitIds.Add(unitId);
        SetButtonSelectedVisual(btn, true);
        RefreshSelectedCount();
    }

    void SetButtonSelectedVisual(Button btn, bool selected)
    {
        if (btn == null) return;

        // Minimal visual feedback: change button alpha.
        var colors = btn.colors;
        colors.normalColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, selected ? 0.6f : 1f);
        btn.colors = colors;
    }

    void RefreshSelectedCount()
    {
        if (selectedCountText != null)
            selectedCountText.text = $"Selecionadas: {selectedUnitIds.Count}/{maxUnitsToBring}";

        if (startBattleButton != null)
            startBattleButton.interactable = selectedUnitIds.Count > 0;
    }

    void StartBattle()
    {
        if (GameFlowManager.Instance == null || GameFlowManager.Instance.SelectedLevel == null)
        {
            Debug.LogError("[PreparationScene] GameFlowManager/SelectedLevel não configurado.");
            return;
        }

        var level = GameFlowManager.Instance.SelectedLevel;
        if (string.IsNullOrWhiteSpace(level.SceneName))
        {
            Debug.LogError($"[PreparationScene] LevelData '{level.name}' sem SceneName.");
            return;
        }

        GameFlowManager.Instance.SelectedUnitIDs = selectedUnitIds.ToList();
        GameFlowManager.Instance.PlayerFormation.Clear();

        SceneManager.LoadScene(level.SceneName);
    }
}
