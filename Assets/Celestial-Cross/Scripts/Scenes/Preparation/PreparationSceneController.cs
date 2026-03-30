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

    [Header("UI - Formation")]
    [SerializeField] private List<FormationSlotUI> formationSlots = new List<FormationSlotUI>();
    [SerializeField] private Button startBattleButton;

    [Header("Constraints")]
    [SerializeField] private int maxUnitsToBring = 3;

    string selectedUnitIdForPlacement;
    readonly HashSet<string> selectedUnitIds = new HashSet<string>();

    void Start()
    {
        WireSlots();
        BuildOwnedUnitButtons();

        if (startBattleButton != null)
            startBattleButton.onClick.AddListener(StartBattle);
    }

    void WireSlots()
    {
        foreach (var slot in formationSlots)
        {
            if (slot == null) continue;
            slot.OnClicked += OnFormationSlotClicked;
        }
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
            Debug.LogError("[PreparationScene] AccountManager não encontrado.");
            return;
        }

        foreach (var unitId in AccountManager.Instance.PlayerAccount.OwnedUnitIDs)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                continue;

            Button btn = Instantiate(ownedUnitButtonPrefab, ownedUnitsContainer);
            Text label = btn.GetComponentInChildren<Text>();

            var data = unitCatalog != null ? unitCatalog.GetUnitData(unitId) : null;
            if (label != null)
                label.text = data != null && !string.IsNullOrWhiteSpace(data.displayName) ? data.displayName : unitId;

            btn.onClick.AddListener(() => SelectUnitForPlacement(unitId));
        }
    }

    void SelectUnitForPlacement(string unitId)
    {
        selectedUnitIdForPlacement = unitId;
        Debug.Log($"[PreparationScene] Selecionado para posicionar: {unitId}");
    }

    void OnFormationSlotClicked(FormationSlotUI slot)
    {
        if (slot == null) return;

        if (GameFlowManager.Instance == null)
        {
            Debug.LogError("[PreparationScene] GameFlowManager não encontrado.");
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedUnitIdForPlacement))
        {
            Debug.Log("[PreparationScene] Selecione uma unit antes de posicionar.");
            return;
        }

        if (!selectedUnitIds.Contains(selectedUnitIdForPlacement) && selectedUnitIds.Count >= maxUnitsToBring)
        {
            Debug.Log($"[PreparationScene] Limite atingido: {maxUnitsToBring} units.");
            return;
        }

        var flow = GameFlowManager.Instance;
        if (flow.UnitInitialPositions == null)
            flow.UnitInitialPositions = new Dictionary<string, Vector2Int>();

        // Se a unit selecionada já estava em algum slot, limpa o slot antigo
        if (flow.UnitInitialPositions.TryGetValue(selectedUnitIdForPlacement, out var oldPos))
        {
            var oldSlot = formationSlots.FirstOrDefault(s => s != null && s.GridPos == oldPos);
            if (oldSlot != null)
                oldSlot.SetIcon(null);
        }

        // Se outro unit já estiver ocupando esse slot, remove mapeamento + seleção
        string previousUnit = flow.UnitInitialPositions.FirstOrDefault(kv => kv.Value == slot.GridPos).Key;
        if (!string.IsNullOrWhiteSpace(previousUnit) && previousUnit != selectedUnitIdForPlacement)
        {
            flow.UnitInitialPositions.Remove(previousUnit);
            selectedUnitIds.Remove(previousUnit);
        }

        selectedUnitIds.Add(selectedUnitIdForPlacement);
        flow.UnitInitialPositions[selectedUnitIdForPlacement] = slot.GridPos;

        var data = unitCatalog != null ? unitCatalog.GetUnitData(selectedUnitIdForPlacement) : null;
        slot.SetIcon(data != null ? data.icon : null);

        Debug.Log($"[PreparationScene] Posicionado {selectedUnitIdForPlacement} em {slot.GridPos}");
    }

    void StartBattle()
    {
        if (GameFlowManager.Instance == null || GameFlowManager.Instance.SelectedLevel == null)
        {
            Debug.LogError("[PreparationScene] Level não selecionado.");
            return;
        }

        // selectedUnitIds vem dos slots. Garante consistência.
        GameFlowManager.Instance.SelectedUnitIDs = selectedUnitIds.ToList();

        string sceneName = GameFlowManager.Instance.SelectedLevel.SceneName;
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[PreparationScene] SelectedLevel.SceneName vazio.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
