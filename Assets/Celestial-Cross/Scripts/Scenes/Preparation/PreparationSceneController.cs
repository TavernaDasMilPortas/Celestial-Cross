using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CelestialCross.System;
using CelestialCross.Audio;
using CelestialCross.Preparation;
using System.Collections;

public class PreparationSceneController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UnitCatalog unitCatalog;

    [Header("UI - Owned Units")]
    [SerializeField] private Transform ownedUnitsContainer;
    [SerializeField] private PreparationUnitButtonUI ownedUnitButtonPrefab;

    [Header("UI - Selection")]
    [SerializeField] private Transform selectedUnitsContainer;
    [SerializeField] private PreparationUnitButtonUI selectedUnitButtonPrefab;
    [SerializeField] private TextMeshProUGUI selectedCountText;
    [SerializeField] private Button startBattleButton;

    [Header("Constraints")]
    [SerializeField] private int maxUnitsToBring = 3;

    private readonly HashSet<string> selectedUnitIds = new HashSet<string>();
    private readonly Dictionary<string, PreparationUnitButtonUI> ownedButtonsMap = new Dictionary<string, PreparationUnitButtonUI>();
    private readonly Dictionary<string, PreparationUnitButtonUI> selectedInstancesMap = new Dictionary<string, PreparationUnitButtonUI>();

    void Start()
    {
        BuildOwnedUnitButtons();

        // Add fixed slots
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.FixedSlots != null)
        {
            foreach (var fixedSlot in GameFlowManager.Instance.FixedSlots)
            {
                if (fixedSlot != null && fixedSlot.UnitRef != null)
                {
                    string fId = fixedSlot.UnitRef.UnitID;
                    if (!selectedUnitIds.Contains(fId))
                    {
                        SelectUnit(fId);
                    }
                }
            }
        }

        if (startBattleButton != null)
        {
            startBattleButton.onClick.AddListener(StartBattle);
        }

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

        float delayStep = 0.05f;
        float currentDelay = 0f;

        foreach (var unitId in AccountManager.Instance.PlayerAccount.OwnedUnitIDs)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                continue;

            var data = unitCatalog.GetUnitData(unitId);

            PreparationUnitButtonUI btn = Instantiate(ownedUnitButtonPrefab, ownedUnitsContainer);
            btn.gameObject.SetActive(true);
            btn.Setup(data, unitId, false);
            btn.OnUnitClicked = ToggleSelectUnit;
            btn.PlayPopInAnimation(currentDelay);
            
            ownedButtonsMap[unitId] = btn;
            currentDelay += delayStep;
        }
    }

    void ToggleSelectUnit(string unitId)
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.FixedSlots != null)
        {
            if (GameFlowManager.Instance.FixedSlots.Exists(s => s.UnitRef != null && s.UnitRef.UnitID == unitId && s.IsLocked))
            {
                Debug.Log($"[PreparationScene] Unidade {unitId} é obrigatória e não pode ser removida.");
                if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(SoundKey.ButtonClickRejection01);
                return;
            }
        }

        if (selectedUnitIds.Contains(unitId))
        {
            DeselectUnit(unitId);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(SoundKey.ButtonClick02);
            return;
        }

        if (selectedUnitIds.Count >= maxUnitsToBring)
        {
            Debug.Log($"[PreparationScene] Limite de units atingido ({maxUnitsToBring}).");
            if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(SoundKey.ButtonClickRejection01);
            return;
        }

        SelectUnit(unitId);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(SoundKey.Selection01);
    }

    void SelectUnit(string unitId)
    {
        selectedUnitIds.Add(unitId);
        
        if (ownedButtonsMap.TryGetValue(unitId, out PreparationUnitButtonUI ownedBtn))
            ownedBtn.SetSelected(true);

        // Adiciona à UI de Selecionados
        if (selectedUnitsContainer != null && selectedUnitButtonPrefab != null)
        {
            var data = unitCatalog.GetUnitData(unitId);
            PreparationUnitButtonUI selBtn = Instantiate(selectedUnitButtonPrefab, selectedUnitsContainer);
            selBtn.gameObject.SetActive(true);
            selBtn.Setup(data, unitId, true);
            selBtn.OnUnitClicked = ToggleSelectUnit; // Clicar na selecionada desseleciona
            selBtn.PlayPopInAnimation(0f);
            
            selectedInstancesMap[unitId] = selBtn;
        }

        RefreshSelectedCount();
    }

    void DeselectUnit(string unitId)
    {
        selectedUnitIds.Remove(unitId);

        if (ownedButtonsMap.TryGetValue(unitId, out PreparationUnitButtonUI ownedBtn))
            ownedBtn.SetSelected(false);

        if (selectedInstancesMap.TryGetValue(unitId, out PreparationUnitButtonUI selObj))
        {
            if (selObj != null) Destroy(selObj.gameObject);
            selectedInstancesMap.Remove(unitId);
        }

        RefreshSelectedCount();
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

        if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(SoundKey.GameStateChange01);

        var level = GameFlowManager.Instance.SelectedLevel;
        if (level != null && !string.IsNullOrEmpty(level.SceneName))
        {
            Debug.Log($"[PreparationScene] Iniciando combate! Carregando cena: {level.SceneName}");
            GameFlowManager.Instance.SelectedUnitIDs = selectedUnitIds.ToList();
            GameFlowManager.Instance.PlayerFormation.Clear();

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadSceneWithFlash(level.SceneName);
            }
            else
            {
                // Fallback se não configurou o manager antes
                GameObject transitionObj = new GameObject("TempTransitionManager");
                SceneTransitionManager tempManager = transitionObj.AddComponent<SceneTransitionManager>();
                tempManager.LoadSceneWithFlash(level.SceneName);
            }
        }
        else
        {
            Debug.LogError("[PreparationScene] Falha ao iniciar combate: Level ou SceneName inválido.");
        }
    }
}
