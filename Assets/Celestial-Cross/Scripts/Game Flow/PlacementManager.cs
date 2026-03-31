using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private GameObject playerUnitMoldPrefab;
    [SerializeField] private LayerMask tileLayer;
    [SerializeField] private UnitCatalog unitCatalog;

    [Header("UI")]
    [SerializeField] private ActionBarUI placementActionBar;

    private UnitData selectedUnitToPlace;
    private GameObject previewInstance;
    private List<Unit> placedUnits = new List<Unit>();

    public event System.Action OnPlacementEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void StartPlacementPhase()
    {
        Debug.Log("Starting Placement Phase...");
        placedUnits.Clear();
        SetupPlacementUI();
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (selectedUnitToPlace == null) return;

        HandlePreview();
        HandlePlacementInput();
    }

    private void SetupPlacementUI()
    {
        if (placementActionBar == null)
        {
            Debug.LogError("[PlacementManager] placementActionBar não configurado.");
            return;
        }

        if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null)
        {
            Debug.LogError("[PlacementManager] AccountManager/PlayerAccount não disponível.");
            return;
        }

        if (unitCatalog == null)
        {
            Debug.LogError("[PlacementManager] UnitCatalog não configurado. Necessário para resolver OwnedUnitIDs -> UnitData.");
            return;
        }

        var idsToShow = (GameFlowManager.Instance != null && GameFlowManager.Instance.SelectedUnitIDs != null && GameFlowManager.Instance.SelectedUnitIDs.Count > 0)
            ? GameFlowManager.Instance.SelectedUnitIDs
            : AccountManager.Instance.PlayerAccount.OwnedUnitIDs;

        var ownedUnits = idsToShow
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => unitCatalog.GetUnitData(id))
            .Where(u => u != null)
            .ToList();

        placementActionBar.ClearButtons();
        placementActionBar.GenerateButtonsForPlacement(ownedUnits, SelectUnitForPlacement);
    }

    private void SelectUnitForPlacement(UnitData unitData)
    {
        if (selectedUnitToPlace == unitData)
        {
            ClearSelection();
            return;
        }

        ClearPreview();
        selectedUnitToPlace = unitData;
        Debug.Log($"Selected '{unitData.displayName}' for placement.");
        CreatePreview();
    }

    private void HandlePreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileLayer))
        {
            GridTile tile = hit.collider.GetComponent<GridTile>();
            if (tile != null && IsValidPlacementTile(tile))
            {
                if (previewInstance != null)
                {
                    previewInstance.SetActive(true);
                    previewInstance.transform.position = tile.transform.position;
                }
            }
            else
            {
                if (previewInstance != null)
                {
                    previewInstance.SetActive(false);
                }
            }
        }
        else
        {
            if (previewInstance != null)
            {
                previewInstance.SetActive(false);
            }
        }
    }

    private void HandlePlacementInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileLayer))
            {
                GridTile tile = hit.collider.GetComponent<GridTile>();
                if (tile != null && IsValidPlacementTile(tile) && !tile.IsOccupied)
                {
                    PlaceUnit(selectedUnitToPlace, tile.GridPosition);
                    ClearSelection();
                }
            }
        }
    }

    private void CreatePreview()
    {
        if (selectedUnitToPlace == null || playerUnitMoldPrefab == null) return;

        previewInstance = Instantiate(playerUnitMoldPrefab);
        var spriteRenderer = previewInstance.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = selectedUnitToPlace.icon;
            var color = spriteRenderer.color;
            color.a = 0.5f; // Set transparency
            spriteRenderer.color = color;
        }
        previewInstance.SetActive(false); // Initially hidden
    }

    private void PlaceUnit(UnitData unitData, Vector2Int gridPosition)
    {
        var unit = GridMap.Instance.SpawnUnitAt(playerUnitMoldPrefab, gridPosition, Team.Player, unitData);
        if (unit != null)
        {
            placedUnits.Add(unit);
            Debug.Log($"Placed '{unitData.displayName}' at {gridPosition}.");
            // Update UI to show unit is placed (e.g., disable button)
            placementActionBar.SetButtonInteractable(unitData.UnitID, false);
        }
    }

    private bool IsValidPlacementTile(GridTile tile)
    {
        if (tile == null) return false;

        // This is a temporary solution, as we are comparing prefab instances.
        // A better approach would be for the GridTile to hold a reference to its TileDefinition.
        // For now, we will find the definition by comparing the name of the instantiated tile's prefab.
        string tileName = tile.gameObject.name.Replace("(Clone)", "").Trim();
        var definition = GridMap.Instance.tileDefinitions.Find(d => d.prefab.name == tileName);

        return definition != null && definition.isPlayerSpawnZone;
    }

    private void ClearPreview()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }
    }

    private void ClearSelection()
    {
        ClearPreview();
        selectedUnitToPlace = null;
    }

    public void EndPlacementPhase()
    {
        ClearSelection();
        gameObject.SetActive(false);
        GameFlowManager.Instance.PlayerFormation = placedUnits;
        Debug.Log("Ending Placement Phase...");
        OnPlacementEnded?.Invoke();
    }
}
