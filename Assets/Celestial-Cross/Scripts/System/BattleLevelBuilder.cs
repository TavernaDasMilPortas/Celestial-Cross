using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Celestial_Cross.Scripts.Units.Enemy;
using CelestialCross.UI;

public class BattleLevelBuilder : MonoBehaviour
{
    [Header("Molds")]
    [SerializeField] private GameObject playerUnitMold;
    [SerializeField] private GameObject enemyUnitMold;

    [Header("Dependencies")]
    [SerializeField] private PlacementManager placementManager;

    [Header("Spawn")]
    [SerializeField] private bool clearExistingUnits = true;

    [Header("Combat")]
    [Tooltip("Se true, chama CombatInitializer.StartCombat() após spawnar as units")]
    [SerializeField] private bool autoStartCombatAfterBuild = true;
 
    [Header("Enemy Focus Phase")]
    [SerializeField] private bool enableEnemyFocusPhase = true;
    [SerializeField] private float enemyFocusDuration = 1.5f;
    [SerializeField] private float enemyFocusZoom = 4.5f;

    void Start()
    {
        StartCoroutine(BuildRoutine());
    }

    public IEnumerator BuildRoutine()
    {
        var flow = GameFlowManager.Instance;
        if (flow == null || flow.SelectedLevel == null)
        {
            Debug.LogWarning("[BattleLevelBuilder] GameFlowManager/SelectedLevel não configurado.");
            yield break;
        }

        var grid = GridMap.Instance;
        if (grid == null)
        {
            Debug.LogError("[BattleLevelBuilder] GridMap.Instance não encontrado na cena.");
            yield break;
        }

        // Aplica o PhaseMap definido no LevelData e gera o grid
        var phaseMap = flow.SelectedLevel.PhaseMap;
        if (phaseMap != null)
        {
            Debug.Log($"[BattleLevelBuilder] Iniciando build. Level='{flow.SelectedLevel.name}', PhaseMap={phaseMap.width}x{phaseMap.height}, grid atual='{grid.name}'.");
            grid.phaseMap = phaseMap;
            grid.Generate();
            Debug.Log($"[BattleLevelBuilder] Grid.Generate() finalizado. Tiles lógicos disponíveis no GridMap: {grid.GetAllTiles().Count()}.");
        }
        else
        {
            Debug.LogError($"[BattleLevelBuilder] LevelData '{flow.SelectedLevel.name}' sem PhaseMap. A geração do grid falhará.");
            yield break;
        }

        // Aguarda um frame para garantir que o grid foi totalmente construído
        yield return null;

        Debug.Log($"[BattleLevelBuilder] Pós-frame de build. GridMap.Instance={(GridMap.Instance != null ? GridMap.Instance.name : "null")}, PhaseMap={grid.phaseMap?.name ?? "null"}, tiles={grid.GetAllTiles().Count()}.");

        if (clearExistingUnits)
        {
            // A lógica de limpar unidades precisa ser revista, pois o grid é recriado.
            // Por enquanto, vamos assumir que a recriação do grid já limpa o cenário.
        }

        // Spawns dos inimigos
        List<EnemyUnit> enemies = SpawnEnemies(flow, grid);

        // Aguarda a Câmera terminar seu setup inicial
        if (CameraController.Instance != null)
        {
            float maxWait = Time.realtimeSinceStartup + 2f;
            while (!CameraController.Instance.IsSetupComplete && Time.realtimeSinceStartup < maxWait)
            {
                yield return null;
            }
        }
        
        // Fase 1: Animação de Intro da Fase (IntroModalUI)
        if (IntroModalUI.Instance != null)
        {
            yield return IntroModalUI.Instance.PlayIntroSequence();
        }

        // Se a lista retornada por SpawnEnemies estiver vazia, tenta coletar todas as EnemyUnits da cena (fallback robusto)
        if (enemies == null || enemies.Count == 0)
        {
            enemies = Object.FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None).ToList();
        }

        // Fase de foco da câmera em cada inimigo antes do posicionamento
        if (enableEnemyFocusPhase && CameraController.Instance != null)
        {
            if (enemies != null && enemies.Count > 0)
            {
                Debug.Log($"[BattleLevelBuilder] Iniciando fase de foco/highlight nos inimigos. Total={enemies.Count}");
                
                // Mostrar botão de Skip
                if (EnemyFocusSkipUI.Instance != null)
                {
                    EnemyFocusSkipUI.Instance.Show();
                }

                CameraController.Instance.EnableFreeCamera(true);
                
                foreach (var enemy in enemies)
                {
                    if (enemy == null) continue;
                    
                    if (EnemyFocusSkipUI.Instance != null && EnemyFocusSkipUI.Instance.IsSkipRequested)
                    {
                        Debug.Log("[BattleLevelBuilder] Foco de inimigos pulado pelo jogador.");
                        break;
                    }

                    CameraController.Instance.TargetProjectedPoint = enemy.transform.position;
                    // Aplica um leve zoom out em relação ao foco original, por exemplo, multiplicando ou usando o valor base
                    CameraController.Instance.TargetZoom = enemyFocusZoom * 1.2f; 
                    
                    bool isFirstEnemy = (enemy == enemies[0]);
                    
                    if (isFirstEnemy)
                    {
                        // Se for o primeiro, faz um pan um pouco mais rápido mas ainda suave
                        CameraController.Instance.AnimateToTarget(0.8f);
                    }
                    else
                    {
                        CameraController.Instance.AnimateToTarget(0.6f);
                    }
                    
                    var outline = enemy.GetComponent<UnitOutlineController>();
                    if (outline == null) outline = enemy.GetComponentInChildren<UnitOutlineController>();
                    outline?.SetSelected(true);
                    
                    float startTime = Time.time;
                    while (Time.time - startTime < enemyFocusDuration)
                    {
                        if (EnemyFocusSkipUI.Instance != null && EnemyFocusSkipUI.Instance.IsSkipRequested)
                            break;

                        if (CameraController.Instance.IsDragging)
                        {
                            break;
                        }
                        yield return null;
                    }
                    
                    outline?.SetSelected(false);
                    
                    if (EnemyFocusSkipUI.Instance != null && EnemyFocusSkipUI.Instance.IsSkipRequested)
                        break;

                    if (CameraController.Instance.IsDragging)
                        break;
                }
                
                // Esconder botão de Skip
                if (EnemyFocusSkipUI.Instance != null)
                {
                    EnemyFocusSkipUI.Instance.Hide();
                }

                CameraController.Instance.ResetToInitialFraming();
                yield return new WaitForSeconds(0.8f);
            }
        }

        // Inicia a fase de posicionamento do jogador
        if (placementManager != null)
        {
            Debug.Log($"[BattleLevelBuilder] Iniciando PlacementPhase com {grid.GetAllTiles().Count()} tiles e {flow.SelectedLevel?.PhaseMap?.unitSpawns?.Count ?? 0} spawns configurados.");
            placementManager.OnPlacementEnded += HandlePlacementEnded;
            placementManager.StartPlacementPhase();
        }
        else
        {
            Debug.LogError("[BattleLevelBuilder] PlacementManager não está configurado!");
            // Se não há placement, podemos considerar iniciar o combate aqui se for o caso
            if (autoStartCombatAfterBuild)
            {
                StartCombat();
            }
        }
    }

    private void HandlePlacementEnded()
    {
        // Remove o listener para não ser chamado múltiplas vezes
        if (placementManager != null)
        {
            placementManager.OnPlacementEnded -= HandlePlacementEnded;
        }

        Debug.Log("[BattleLevelBuilder] Fase de posicionamento concluída.");

        if (autoStartCombatAfterBuild)
        {
            StartCombat();
        }
    }

    private void StartCombat()
    {
        if (IntroModalUI.Instance != null)
        {
            IntroModalUI.Instance.HideIntroImmediate();
        }

        // Animação para diminuir a Game Render View ao entrar em modo combate
        if (GameRenderTween.Instance != null)
        {
            GameRenderTween.Instance.SetCombatMode();
        }

        var initializer = FindFirstObjectByType<CombatInitializer>();
        if (initializer != null)
        {
            Debug.Log("[BattleLevelBuilder] Iniciando o combate.");
            initializer.StartCombat();
        }
        else
        {
            Debug.LogWarning("[BattleLevelBuilder] CombatInitializer não encontrado para autoStart.");
        }
    }

    private List<EnemyUnit> SpawnEnemies(GameFlowManager flow, GridMap grid)
    {
        List<EnemyUnit> spawnedEnemiesList = new List<EnemyUnit>();
        var level = flow.SelectedLevel;
        List<EnemySpawnInfo> enemySpawns = null;

        if (level.Waves != null && level.Waves.Count > 0 && level.Waves[0] != null && level.Waves[0].Enemies != null && level.Waves[0].Enemies.Count > 0)
            enemySpawns = level.Waves[0].Enemies;
        else
            enemySpawns = level.Enemies;

        int spawnsCount = enemySpawns != null ? enemySpawns.Count : 0;
        Debug.Log($"[BattleLevelBuilder] SpawnEnemies: Total de spawns configurados = {spawnsCount}");

        if (enemySpawns != null)
        {
            foreach (var enemy in enemySpawns)
            {
                if (enemy.UnitData == null)
                    continue;

                try
                {
                    Unit spawnedUnit = grid.SpawnUnitAt(enemyUnitMold, enemy.GridPosition, Team.Enemy, enemy.UnitData);
                    
                    if (spawnedUnit != null)
                    {
                        EnemyUnit eUnit = spawnedUnit as EnemyUnit;
                        if (eUnit != null)
                        {
                            spawnedEnemiesList.Add(eUnit);
                            if (enemy.OverrideBehaviorTree != null)
                            {
                                eUnit.SetBehaviorTree(enemy.OverrideBehaviorTree);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[BattleLevelBuilder] Unidade spawnada em {enemy.GridPosition} não é do tipo EnemyUnit (tipo: {spawnedUnit.GetType()})");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[BattleLevelBuilder] Erro fatal ao spawnar inimigo {enemy.UnitData.name} em {enemy.GridPosition}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
        
        Debug.Log($"[BattleLevelBuilder] SpawnEnemies concluído. Total instanciado = {spawnedEnemiesList.Count}");
        return spawnedEnemiesList;
    }

    static void ClearUnits(GridMap grid)
    {
        var units = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (var u in units)
        {
            if (u == null) continue;
            Destroy(u.gameObject);
        }
    }

    static void ClearOccupancy(GridMap grid)
    {
        foreach (var tile in grid.GetAllTiles())
        {
            if (tile == null) continue;
            tile.IsOccupied = false;
            tile.OccupyingUnit = null;
        }
    }

    // Spawn movido para GridMap.SpawnUnitAt
}
