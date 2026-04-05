using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [SerializeField] private Transform selectedUnitsContainer;
    [SerializeField] private Button selectedUnitButtonPrefab;
    [SerializeField] private TextMeshProUGUI selectedCountText;
    [SerializeField] private Button startBattleButton;

    [Header("Constraints")]
    [SerializeField] private int maxUnitsToBring = 3;

    private readonly HashSet<string> selectedUnitIds = new HashSet<string>();
    private readonly Dictionary<string, Button> ownedButtonsMap = new Dictionary<string, Button>();
    private readonly Dictionary<string, GameObject> selectedInstancesMap = new Dictionary<string, GameObject>();

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

        ownedButtonsMap.Clear();
        selectedInstancesMap.Clear();

        if (selectedUnitsContainer != null)
        {
            foreach (Transform child in selectedUnitsContainer)
                Destroy(child.gameObject);
        }

        foreach (var unitId in AccountManager.Instance.PlayerAccount.OwnedUnitIDs)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                continue;

            var data = unitCatalog.GetUnitData(unitId);

            Button btn = Instantiate(ownedUnitButtonPrefab, ownedUnitsContainer);
            SetupButtonVisuals(btn, data, unitId);

            btn.onClick.AddListener(() => ToggleSelectUnit(unitId));
            ownedButtonsMap[unitId] = btn;
        }
    }

    void SetupButtonVisuals(Button btn, UnitData data, string unitId)
    {
        // Define o texto (suporta tanto Text do Unity UI básico quanto TMPro se existir no projeto no futuro)
        var textLabels = btn.GetComponentsInChildren<Text>(true);
        foreach (var label in textLabels)
            label.text = data != null && !string.IsNullOrWhiteSpace(data.displayName) ? data.displayName : unitId;

        var tmproLabels = btn.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in tmproLabels)
            tmp.text = data != null && !string.IsNullOrWhiteSpace(data.displayName) ? data.displayName : unitId;

        // Define a imagem da unidade (procura imgs que sejam filhas, mas que não estejam no próprio gameObject do Button - para não sobrescrever o background do botão em si)
        if (data != null && data.icon != null)
        {
            var images = btn.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject != btn.gameObject)
                {
                    img.sprite = data.icon;
                    img.color = Color.white; // Reseta transparente
                }
            }
        }
    }

    void ToggleSelectUnit(string unitId)
    {
        if (selectedUnitIds.Contains(unitId))
        {
            DeselectUnit(unitId);
            return;
        }

        if (selectedUnitIds.Count >= maxUnitsToBring)
        {
            Debug.Log($"[PreparationScene] Limite de units atingido ({maxUnitsToBring}).");
            return;
        }

        SelectUnit(unitId);
    }

    void SelectUnit(string unitId)
    {
        selectedUnitIds.Add(unitId);
        
        if (ownedButtonsMap.TryGetValue(unitId, out Button ownedBtn))
            SetButtonSelectedVisual(ownedBtn, true);

        // Adiciona à UI de Selecionados
        if (selectedUnitsContainer != null && selectedUnitButtonPrefab != null)
        {
            var data = unitCatalog.GetUnitData(unitId);
            Button selBtn = Instantiate(selectedUnitButtonPrefab, selectedUnitsContainer);
            SetupButtonVisuals(selBtn, data, unitId);

            selBtn.onClick.AddListener(() => ToggleSelectUnit(unitId)); // Clicar na selecionada desseleciona
            selectedInstancesMap[unitId] = selBtn.gameObject;
        }

        RefreshSelectedCount();
    }

    void DeselectUnit(string unitId)
    {
        selectedUnitIds.Remove(unitId);

        if (ownedButtonsMap.TryGetValue(unitId, out Button ownedBtn))
            SetButtonSelectedVisual(ownedBtn, false);

        if (selectedInstancesMap.TryGetValue(unitId, out GameObject selObj))
        {
            if (selObj != null) Destroy(selObj);
            selectedInstancesMap.Remove(unitId);
        }

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
