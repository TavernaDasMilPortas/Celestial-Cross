using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private GameObject playerUnitMoldPrefab;
    [SerializeField] private LayerMask tileLayer;
    [SerializeField] private UnitCatalog unitCatalog;
    [SerializeField] private PetCatalog petCatalog;

    [Header("UI")]
    [SerializeField] private ActionBarUI placementActionBar;
    [SerializeField] private SwipeDetector swipeDetector;

    private UnitData selectedUnitToPlace;
    private Dictionary<UnitData, Unit> placedUnitsDict = new Dictionary<UnitData, Unit>();
    private HashSet<UnitData> confirmedUnits = new HashSet<UnitData>();
    private List<UnitData> allOwnedUnits = new List<UnitData>();
    private int totalUnitsToPlace;

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

        if (swipeDetector == null)
            swipeDetector = FindFirstObjectByType<SwipeDetector>();
        
#if UNITY_EDITOR
        if (petCatalog == null)
        {
            petCatalog = UnityEditor.AssetDatabase.LoadAssetAtPath<PetCatalog>("Assets/Celestial-Cross/Prefabs/PetCatalog.asset");
        }
#endif
    }

    public void StartPlacementPhase()
    {
        Debug.Log("Starting Placement Phase...");
        placedUnitsDict.Clear();
        confirmedUnits.Clear();
        SetupPlacementUI();
        HighlightSpawnZones();
        gameObject.SetActive(true);
    }

    private void HighlightSpawnZones()
    {
        var spawnTiles = GridMap.Instance.GetAllTiles().Where(t => t.IsPlayerSpawnZone).Select(t => t.GridPosition).ToList();
        GridMap.Instance.HighlightArea(spawnTiles);

        Debug.Log($"[PlacementManager] Spawn zones destacadas. Total={spawnTiles.Count}. A câmera não será enquadrada nesses tiles.");
    }

    private void Update()
    {
        if (selectedUnitToPlace == null) return;
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

        totalUnitsToPlace = ownedUnits.Count;
        allOwnedUnits = ownedUnits;

        placementActionBar.ClearButtons();
        placementActionBar.GenerateButtonsForPlacement(ownedUnits, SelectUnitForPlacement);
        
        // 1- Pré-seleciona a primeira unidade do array automaticamente
        if (ownedUnits.Count > 0)
        {
            SelectUnitForPlacement(ownedUnits[0]);
        }
    }

    private void SelectUnitForPlacement(UnitData unitData)
    {
        // Se a unidade já foi confirmada, retiramos ela do mapa e o controle passa para ela
        if (confirmedUnits.Contains(unitData))
        {
            confirmedUnits.Remove(unitData);
            placementActionBar.SetButtonConfirmedVisual(unitData.UnitID, false);

            if (placedUnitsDict.TryGetValue(unitData, out Unit unit))
            {
                if (unit != null)
                {
                    var tile = GridMap.Instance.GetTile(unit.GridPosition);
                    if (tile != null)
                    {
                        tile.SetConfirmed(false);
                        tile.IsOccupied = false;
                        tile.OccupyingUnit = null;
                    }
                    Destroy(unit.gameObject);
                }
                placedUnitsDict.Remove(unitData);
            }
        }

        selectedUnitToPlace = unitData;
        placementActionBar.SetButtonSelected(unitData.UnitID);
        Debug.Log($"Selected '{unitData.displayName}' for placement.");
    }

    private Vector2 pointerDownPos;
    private bool pointerDownValid;

    // Increase threshold to prevent small jitters during drag from counting as clicks
    private float dragThreshold = 40f;

    private void HandlePlacementInput()
    {
        if (swipeDetector != null && (swipeDetector.IsSwipeInProgress || swipeDetector.WasSwipeConsumed))
        {
            Debug.Log("[PlacementManager] Input de placement ignorado porque um swipe está em andamento ou acabou de ser consumido.");
            return;
        }

        bool isClick = false;

        // Mouse click detection (with threshold for drag)
        if (Input.GetMouseButtonDown(0))
        {
            pointerDownValid = RenderTextureInputManager.Instance == null || RenderTextureInputManager.Instance.IsScreenPointOverExclusiveRenderTarget(Input.mousePosition);
            pointerDownPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (pointerDownValid && Vector2.Distance(pointerDownPos, Input.mousePosition) < dragThreshold)
            {
                isClick = true;
            }

            pointerDownValid = false;
        }

        // Touch click detection
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                pointerDownValid = RenderTextureInputManager.Instance == null || RenderTextureInputManager.Instance.IsScreenPointOverExclusiveRenderTarget(touch.position);
                pointerDownPos = touch.position;
                isClick = false; // Reset mouse click if touch is active to prevent double trigger
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (pointerDownValid && Vector2.Distance(pointerDownPos, touch.position) < dragThreshold)
                {
                    isClick = true;
                }

                pointerDownValid = false;
            }
        }

        // If the user was dragging the camera, we should ignore the click
        if (CameraController.Instance != null && CameraController.Instance.cameraMode == CameraController.CameraMode.Free)
        {
            // Extra safety: you can't click to place if you dragged enough to trigger free mode
            // unless distance is very tight. Let's just rely on the distance limit.
        }

        if (!isClick) return;

        Vector2Int gridPos = GridMap.Instance.GetMouseGridPosition();
        if (gridPos.x != -1 && gridPos.y != -1)
        {
            GridTile tile = GridMap.Instance.GetTile(gridPos);
            if (tile != null && IsValidPlacementTile(tile))
            {
                HandleTileClick(tile);
            }
        }
    }

    private void HandleTileClick(GridTile tile)
    {
        Unit existingUnit = placedUnitsDict.ContainsKey(selectedUnitToPlace) ? placedUnitsDict[selectedUnitToPlace] : null;

        // Se clicarmos no mesmo tile onde a unidade selecionada DÁ ESTÁ, isso é um Double Tap/Confirmação
        if (existingUnit != null && existingUnit.GridPosition == tile.GridPosition)
        {
            ConfirmUnit(selectedUnitToPlace);
            return;
        }

        // Se o tile já estiver ocupado por OUTRA unidade, ignoramos (não substitui ou empilha)
        if (tile.IsOccupied && tile.OccupyingUnit != existingUnit)
        {
            Debug.Log("Tile já ocupado por outra unidade.");
            return;
        }

        // Cria a unidade provisória ou movimenta a já existente controlada agora
        PlaceOrMoveUnit(selectedUnitToPlace, tile.GridPosition);
    }

    private void PlaceOrMoveUnit(UnitData unitData, Vector2Int newPos)
    {
        if (placedUnitsDict.TryGetValue(unitData, out Unit existingUnit))
        {
            // Move a unidade existente
            var oldTile = GridMap.Instance.GetTile(existingUnit.GridPosition);
            if (oldTile != null)
            {
                oldTile.IsOccupied = false;
                oldTile.OccupyingUnit = null;
            }

            existingUnit.GridPosition = newPos;
            existingUnit.transform.position = GridMap.Instance.GridToWorld(newPos);
            
            var newTile = GridMap.Instance.GetTile(newPos);
            if (newTile != null)
            {
                newTile.IsOccupied = true;
                newTile.OccupyingUnit = existingUnit;
            }
        }
        else
        {
            // Recupera o loadout e o pet (se houver) dessa unidade
            CelestialCross.Data.Pets.PetSpeciesSO petSpecies = null;
            CelestialCross.Data.Pets.RuntimePetData runtimePetData = null;
            if (petCatalog != null && AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
            {
                var loadout = AccountManager.Instance.PlayerAccount.GetLoadoutForUnit(unitData.UnitID);
                if (loadout != null && !string.IsNullOrEmpty(loadout.PetID))
                {
                    runtimePetData = AccountManager.Instance.PlayerAccount.GetPetByUUID(loadout.PetID);
                    if (runtimePetData != null)
                    {
                        petSpecies = petCatalog.GetPetSpecies(runtimePetData.SpeciesID);
                    }
                    else
                    {
                        petSpecies = petCatalog.GetPetSpecies(loadout.PetID);
                    }
                }
            }

            // Spawna a nova unidade e atrela ela
            var unit = GridMap.Instance.SpawnUnitAt(playerUnitMoldPrefab, newPos, Team.Player, unitData, runtimePetData, petSpecies);
            if (unit != null)
            {
                unit.runtimePetData = runtimePetData;
                
                // --- ATRIBUIÇÃO DE RUNTIME DATA (Essencial para XP e Nível) ---
                if (AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
                {
                    unit.runtimeUnitData = AccountManager.Instance.PlayerAccount.GetOwnedUnitRuntimeData(unitData.UnitID);
                    if (unit.runtimeUnitData == null)
                    {
                        // Fallback: Se não existe no save (ex: debug), cria um temporário
                        unit.runtimeUnitData = new CelestialCross.Data.RuntimeUnitData(unitData.UnitID, 1);
                    }
                }

                placedUnitsDict[unitData] = unit;
            }
        }
    }

    private void ConfirmUnit(UnitData unitData)
    {
        confirmedUnits.Add(unitData);
        placementActionBar.SetButtonConfirmedVisual(unitData.UnitID, true);

        // Deixa o tile sob o personagem confirmado verde
        if (placedUnitsDict.TryGetValue(unitData, out Unit unit))
        {
            if (unit != null)
            {
                var tile = GridMap.Instance.GetTile(unit.GridPosition);
                if (tile != null)
                {
                    tile.SetConfirmed(true);
                }
            }
        }
        
        Debug.Log($"Unit '{unitData.displayName}' placement confirmed.");
        
        CelestialCross.Tutorial.TutorialManager.Instance?.NotifyUnitPlaced(unitData);

        // Termina a fase se a unidade foi confirmada E todas as outras já estão posicionadas (no mapa)
        if (placedUnitsDict.Count >= totalUnitsToPlace)
        {
            EndPlacementPhase();
            return;
        }

        // Tenta passar o controle para a próxima unidade que ainda NÃO foi colocada no mapa
        var nextUnit = allOwnedUnits.FirstOrDefault(u => !placedUnitsDict.ContainsKey(u));
        // Se todas já estão no mapa, pega a próxima não confirmada
        if (nextUnit == null)
        {
            nextUnit = allOwnedUnits.FirstOrDefault(u => !confirmedUnits.Contains(u));
        }

        if (nextUnit != null)
        {
            SelectUnitForPlacement(nextUnit);
        }
        else
        {
            selectedUnitToPlace = null;
        }
    }

    private bool IsValidPlacementTile(GridTile tile)
    {
        if (tile == null) return false;

        return tile.IsPlayerSpawnZone;
    }

    private void ClearSelection()
    {
        selectedUnitToPlace = null;
    }

    public void EndPlacementPhase()
    {
        StartCoroutine(CoEndPlacementPhase());
    }

    private IEnumerator CoEndPlacementPhase()
    {
        ClearSelection();

        // Faz com que todos os highlights fiquem verdes
        if (GridMap.Instance != null)
        {
            GridMap.Instance.forceAllHighlightsGreen = true;
            GridMap.Instance.RefreshDynamicHighlights();
        }

        yield return new WaitForSeconds(0.3f);

        // Reseta todos os tiles confirmados (cor verde)
        if (GridMap.Instance != null)
        {
            GridMap.Instance.forceAllHighlightsGreen = false;
            foreach (var tile in GridMap.Instance.GetAllTiles())
            {
                if (tile != null)
                {
                    tile.SetConfirmed(false);
                }
            }
            GridMap.Instance.ResetAllTileVisuals();
        }

        gameObject.SetActive(false);
        GameFlowManager.Instance.PlayerFormation = placedUnitsDict.Values.ToList();
        Debug.Log("Ending Placement Phase...");
        OnPlacementEnded?.Invoke();
    }
}



