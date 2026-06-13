using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Celestial_Cross.Scripts.Units.Enemy;

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

        // Aguarda a Câmera terminar seu setup inicial (Zoom/Enquadramento do mapa)
        // para garantir que ela não roube o controle da câmera durante a animação do primeiro inimigo.
        if (CameraController.Instance != null)
        {
            float maxWait = Time.realtimeSinceStartup + 2f;
            while (!CameraController.Instance.IsSetupComplete && Time.realtimeSinceStartup < maxWait)
            {
                yield return null;
            }
            if (!CameraController.Instance.IsSetupComplete)
            {
                Debug.LogWarning("[BattleLevelBuilder] CameraController não terminou o setup após 2s. Prosseguindo...");
            }
        }
        
        // Aguarda mais um frame para garantir estabilidade do engine
        yield return null;

        // Se a lista retornada por SpawnEnemies estiver vazia, tenta coletar todas as EnemyUnits da cena (fallback robusto)
        if (enemies == null || enemies.Count == 0)
        {
            enemies = Object.FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None).ToList();
        }

        // Fase de foco da câmera em cada inimigo antes do posicionamento (Highlight dos Inimigos)
        if (enableEnemyFocusPhase && CameraController.Instance != null)
        {
            if (enemies != null && enemies.Count > 0)
            {
                Debug.Log($"[BattleLevelBuilder] Iniciando fase de foco/highlight nos inimigos. Total={enemies.Count}");
                
                // Coloca a câmera em modo livre para mover programaticamente
                CameraController.Instance.EnableFreeCamera(true);
                
                foreach (var enemy in enemies)
                {
                    if (enemy == null) continue;
                    
                    Debug.Log($"[BattleLevelBuilder] Focando e destacando inimigo: {enemy.DisplayName} em {enemy.transform.position}");
                    CameraController.Instance.TargetProjectedPoint = enemy.transform.position;
                    CameraController.Instance.TargetZoom = enemyFocusZoom;
                    
                // Se for o primeiro inimigo, pula direto para ele para não perder o início da animação viajando pelo mapa
                bool isFirstEnemy = (enemy == enemies[0]);
                
                if (isFirstEnemy)
                {
                    CameraController.Instance.SnapToTarget();
                }
                else
                {
                    // Anima suavemente para os próximos inimigos usando DOTween (ex: 0.6s de viagem)
                    CameraController.Instance.AnimateToTarget(0.6f);
                }
                
                // Ativa o outline visual para destacar o inimigo
                var outline = enemy.GetComponent<UnitOutlineController>();
                if (outline == null) outline = enemy.GetComponentInChildren<UnitOutlineController>();
                outline?.SetSelected(true);
                    
                    // Espera pelo tempo determinado ou até que o jogador arraste a tela
                    // Usa Time.time ao invés de Time.deltaTime para evitar que o lag de instanciação do mapa 
                    // (que faz o deltaTime ser > 1.5s no primeiro frame) pule instantaneamente o primeiro inimigo.
                    float startTime = Time.time;
                    while (Time.time - startTime < enemyFocusDuration)
                    {
                        if (CameraController.Instance.IsDragging)
                        {
                            Debug.Log("[BattleLevelBuilder] Foco de câmera cancelado pelo arrasto do jogador.");
                            break;
                        }
                        yield return null;
                    }
                    
                    // Desativa o outline após focar nele
                    outline?.SetSelected(false);
                    
                    if (CameraController.Instance.IsDragging)
                        break;
                }
                
                // Redefine a câmera para o enquadramento inicial
                CameraController.Instance.ResetToInitialFraming();
                yield return new WaitForSeconds(0.8f);
            }
            else
            {
                Debug.LogWarning("[BattleLevelBuilder] Fase de foco pulada: nenhuma unidade EnemyUnit foi instanciada.");
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
