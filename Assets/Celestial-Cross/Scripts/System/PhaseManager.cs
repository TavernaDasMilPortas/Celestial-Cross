using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CelestialCross.System;

public class PhaseManager : MonoBehaviour
{
    public static PhaseManager Instance { get; private set; }

    [SerializeField] private List<CelestialCross.Data.Rewards.RewardDefinition> fallbackVictoryRewards = new List<CelestialCross.Data.Rewards.RewardDefinition>();

    [Header("Flow")]
    [SerializeField] private string hubSceneName = "HubScene";
    [SerializeField] private float returnDelaySeconds = 2.0f;

    private List<Unit> playerUnits = new List<Unit>();
    private List<Unit> enemyUnits = new List<Unit>();

    public System.Action<Team> OnPhaseEnded;

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

    public void RegisterUnit(Unit unit)
    {
        if (unit.Team == Team.Player)
        {
            if (!playerUnits.Contains(unit))
                playerUnits.Add(unit);
            Debug.Log($"[PhaseManager] Player Unit Registrada: {unit.DisplayName}. Total Player Units: {playerUnits.Count}");
        }
        else if (unit.Team == Team.Enemy)
        {
            if (!enemyUnits.Contains(unit))
                enemyUnits.Add(unit);
            Debug.Log($"[PhaseManager] Enemy Unit Registrada: {unit.DisplayName}. Total Enemy Units: {enemyUnits.Count}");
        }
    }

    public void UnregisterUnit(Unit unit)
    {
        if (unit.Team == Team.Player)
        {
            playerUnits.Remove(unit);
            Debug.Log($"[PhaseManager] Player Unit Desregistrada: {unit.DisplayName}. Restantes: {playerUnits.Count}");
        }
        else if (unit.Team == Team.Enemy)
        {
            enemyUnits.Remove(unit);
            Debug.Log($"[PhaseManager] Enemy Unit Desregistrada: {unit.DisplayName}. Restantes: {enemyUnits.Count}");
        }
        CheckForGameEnd();
    }

    public bool IsUnitRegistered(Unit unit)
    {
        if (unit.Team == Team.Player) return playerUnits.Contains(unit);
        if (unit.Team == Team.Enemy) return enemyUnits.Contains(unit);
        return false;
    }

    public void CheckForGameEnd()
    {
        playerUnits.RemoveAll(u => u == null || u.Health == null || u.Health.CurrentHealth <= 0 || !u.gameObject.activeInHierarchy);
        enemyUnits.RemoveAll(u => u == null || u.Health == null || u.Health.CurrentHealth <= 0 || !u.gameObject.activeInHierarchy);

        Debug.Log($"[PhaseManager] Checking game end. Player units count: {playerUnits.Count}, Enemy units count: {enemyUnits.Count}");
        for (int i = 0; i < playerUnits.Count; i++)
        {
            if (playerUnits[i] != null)
                Debug.Log($" - Player Unit [{i}]: {playerUnits[i].DisplayName} | HP: {playerUnits[i].Health?.CurrentHealth} | Active: {playerUnits[i].gameObject.activeInHierarchy}");
        }
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            if (enemyUnits[i] != null)
                Debug.Log($" - Enemy Unit [{i}]: {enemyUnits[i].DisplayName} | HP: {enemyUnits[i].Health?.CurrentHealth} | Active: {enemyUnits[i].gameObject.activeInHierarchy}");
        }

        if (playerUnits.Count == 0)
        {
            EndPhase(Team.Enemy); // Inimigos venceram
        }
        else if (enemyUnits.Count == 0)
        {
            EndPhase(Team.Player); // Jogador venceu
        }
    }

    private bool isPhaseEnded = false;

    private void EndPhase(Team winningTeam)
    {
        if (isPhaseEnded) return;
        isPhaseEnded = true;

        CelestialCross.Data.Dungeon.RuntimeReward finalReward = null;

        if (winningTeam == Team.Player)
        {
            Debug.Log("Fase concluída! Vitória do Jogador!");
            finalReward = GrantRewards();

            // Gravar conclusão do StoryNode após pegar os rewards (para garantir que pegamos o FirstClear antes do +1)
            if (GameFlowManager.Instance != null && GameFlowManager.Instance.SelectedStoryNode != null)
            {
                CelestialCross.System.ProgressionService.Instance?.RecordNodeCompletion(GameFlowManager.Instance.SelectedStoryNode);
            }
        }
        else
        {
            Debug.Log("Fase concluída! Derrota.");
        }

        OnPhaseEnded?.Invoke(winningTeam);

        bool isVictory = winningTeam == Team.Player;
        
        Dictionary<string, XPGainResult> xpResults = null;
        if (isVictory && finalReward != null && CelestialCross.Giulia_UI.VictoryRewardUI.Instance != null && CelestialCross.Giulia_UI.VictoryRewardUI.Instance.levelingConfig != null)
        {
            xpResults = XPDistributor.DistributeXP(finalReward.XP, playerUnits, CelestialCross.Giulia_UI.VictoryRewardUI.Instance.levelingConfig);
        }

        if (finalReward != null)
        {
            // Mostra a UI procedimental e só volta pro Hub depois do clique
            CelestialCross.Giulia_UI.VictoryRewardUI.ShowVictoryUIWithXP(finalReward, xpResults, () => 
            {
                if (!string.IsNullOrWhiteSpace(hubSceneName))
                    ReturnToHub();
            }, isVictory);
        }
        else
        {
            // Derrota ou sem erro/loot especial: Mostra ui de derrota vazia
            CelestialCross.Giulia_UI.VictoryRewardUI.ShowVictoryUIWithXP(null, null, () => 
            {
                if (!string.IsNullOrWhiteSpace(hubSceneName))
                    ReturnToHub();
            }, isVictory);
        }
    }

    void ReturnToHub()
    {
        SceneManager.LoadScene(hubSceneName);
    }

    public CelestialCross.Data.Dungeon.RuntimeReward GrantRewards()
    {
        Debug.Log("[PhaseManager] GrantRewards INICIADO!");

        List<CelestialCross.Data.Rewards.RewardDefinition> baseRewards = new List<CelestialCross.Data.Rewards.RewardDefinition>();

        bool usedNodeRewards = false;

        if (GameFlowManager.Instance != null && GameFlowManager.Instance.SelectedStoryNode != null)
        {
            Debug.Log($"[PhaseManager] Gerando rewards para StoryNode: {GameFlowManager.Instance.SelectedStoryNode.NodeID}");
            var nodeRewards = CelestialCross.System.ProgressionService.Instance?.GetRewardsForNode(GameFlowManager.Instance.SelectedStoryNode);
            if (nodeRewards != null && nodeRewards.Count > 0)
            {
                baseRewards.AddRange(nodeRewards);
                usedNodeRewards = true;
            }
        }
        
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.SelectedLevel != null)
        {
            Debug.Log($"[PhaseManager] Gerando rewards para LevelData: {GameFlowManager.Instance.SelectedLevel.name}");
            if (GameFlowManager.Instance.SelectedLevel.FirstClearRewards != null && GameFlowManager.Instance.SelectedLevel.FirstClearRewards.Count > 0)
            {
                baseRewards.AddRange(GameFlowManager.Instance.SelectedLevel.FirstClearRewards);
                usedNodeRewards = true;
            }
        }

        // Se a fase não retornou NENHUM reward específico (ou se quisermos garantir que o fallback atue como base),
        // no passado, se a lista estivesse vazia, caíamos no fallback. 
        // Agora, como as LootTables garantem que a lista nunca seja vazia, precisamos checar se existem rewards do tipo Money/XP/etc.
        bool hasBaseEconomy = baseRewards.Exists(r => r.Type == CelestialCross.Data.Rewards.RewardType.Money || r.Type == CelestialCross.Data.Rewards.RewardType.Energy || r.Type == CelestialCross.Data.Rewards.RewardType.XP);
        if (!hasBaseEconomy && fallbackVictoryRewards != null)
        {
            Debug.Log("[PhaseManager] Fase não possui economia base explícita. Adicionando FallbackVictoryRewards.");
            baseRewards.AddRange(fallbackVictoryRewards);
        }

        Debug.Log($"[PhaseManager] Criando RuntimeReward com {baseRewards.Count} base rewards...");
        for (int i = 0; i < baseRewards.Count; i++)
        {
            if (baseRewards[i] != null)
            {
                Debug.Log($"[PhaseManager] Item [{i}]: Type={baseRewards[i].Type}, Amount={baseRewards[i].Amount}");
            }
            else
            {
                Debug.Log($"[PhaseManager] Item [{i}]: NULL");
            }
        }
        var rewardToGrant = CelestialCross.System.RewardService.CreateRuntimeReward(baseRewards);

        // --- GERAÇÃO DE LOOT PROCEDURAL E DINÂMICO ---
        // NOTA: As LootTables do StoryNode.Rewards.LootTables já são processadas via
        // GetRewardsForNode() → RewardService.CreateRuntimeReward(), então NÃO devem ser
        // processadas aqui novamente para evitar drops duplicados.
        if (GameFlowManager.Instance != null)
        {
            // 1. Processar Drop Tables Globais do Dungeon (Apenas se for uma run de Masmorra!)
            if (GameFlowManager.Instance.SelectedDungeon != null && GameFlowManager.Instance.SelectedDungeonNode != null)
            {
                var dungeon = GameFlowManager.Instance.SelectedDungeon;
                if (dungeon.GlobalLootTables != null)
                {
                    foreach (var table in dungeon.GlobalLootTables)
                    {
                        if (table != null) table.GenerateLoot(rewardToGrant);
                    }
                }
            }

            // 2. Processar Drop Tables Específicos deste Andar do Dungeon
            if (GameFlowManager.Instance.SelectedDungeonNode != null)
            {
                var node = GameFlowManager.Instance.SelectedDungeonNode;
                if (node.SpecificLootTables != null)
                {
                    foreach (var table in node.SpecificLootTables)
                    {
                        if (table != null) table.GenerateLoot(rewardToGrant);
                    }
                }
            }

            Debug.Log($"[PhaseManager] Recompensa procedural gerada! Artefatos: {rewardToGrant.GeneratedArtifacts.Count}, Pets: {rewardToGrant.GeneratedPets.Count}, Defs: {rewardToGrant.SourceDefinitions.Count}");
        }

        if (rewardToGrant != null)
        {
            Debug.Log($"Recompensa base adquirida: {rewardToGrant.Money} de dinheiro, {rewardToGrant.Energy} de energia.");

            if (AccountManager.Instance != null)
            {
                AccountManager.Instance.ApplyRewards(rewardToGrant);
                Debug.Log($"Novos valores da conta -> Dinheiro: {AccountManager.Instance.PlayerAccount.Money} | Energia: {AccountManager.Instance.PlayerAccount.Energy}");
            }
        }
        else
        {
            Debug.Log("Nenhuma recompensa configurada para esta fase.");
        }

        return rewardToGrant;
    }
}
