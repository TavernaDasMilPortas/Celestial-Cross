using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CelestialCross.System;

public class PhaseManager : MonoBehaviour
{
    public static PhaseManager Instance { get; private set; }

    [SerializeField] private RewardPackage victoryRewards;

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
        }
        else if (unit.Team == Team.Enemy)
        {
            if (!enemyUnits.Contains(unit))
                enemyUnits.Add(unit);
        }
    }

    public void UnregisterUnit(Unit unit)
    {
        if (unit.Team == Team.Player)
        {
            playerUnits.Remove(unit);
        }
        else if (unit.Team == Team.Enemy)
        {
            enemyUnits.Remove(unit);
        }
        CheckForGameEnd();
    }

    private void CheckForGameEnd()
    {
        if (playerUnits.Count == 0)
        {
            EndPhase(Team.Enemy); // Inimigos venceram
        }
        else if (enemyUnits.Count == 0)
        {
            EndPhase(Team.Player); // Jogador venceu
        }
    }

    private void EndPhase(Team winningTeam)
    {
        CelestialCross.Data.Dungeon.RuntimeReward finalReward = null;

        if (winningTeam == Team.Player)
        {
            Debug.Log("Fase concluída! Vitória do Jogador!");
            finalReward = GrantRewards();
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
        else if (!string.IsNullOrWhiteSpace(hubSceneName))
        {
            // Derrota ou sem erro/loot especial: Mostra ui de derrota vazia
            CelestialCross.Giulia_UI.VictoryRewardUI.ShowVictoryUIWithXP(null, null, () => 
            {
                ReturnToHub();
            }, isVictory);
        }
    }

    void ReturnToHub()
    {
        SceneManager.LoadScene(hubSceneName);
    }

    private CelestialCross.Data.Dungeon.RuntimeReward GrantRewards()
    {
        RewardPackage baseReward = null;

        if (GameFlowManager.Instance != null && GameFlowManager.Instance.SelectedLevel != null)
            baseReward = GameFlowManager.Instance.SelectedLevel.VictoryRewards;

        if (baseReward == null)
            baseReward = victoryRewards;

        var rewardToGrant = new CelestialCross.Data.Dungeon.RuntimeReward(baseReward);

        // --- GERAÇÃO DE LOOT PROCEDURAL E DINÂMICO ---
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.SelectedDungeon != null && GameFlowManager.Instance.SelectedDungeonNode != null)
        {
            var dungeon = GameFlowManager.Instance.SelectedDungeon;
            var node = GameFlowManager.Instance.SelectedDungeonNode;

            // 1. Processar Drop Tables Globais (Nova Arquitetura)
            if (dungeon.GlobalLootTables != null)
            {
                foreach (var table in dungeon.GlobalLootTables)
                {
                    if (table != null) table.GenerateLoot(rewardToGrant);
                }
            }

            // 2. Processar Drop Tables Específicos deste Andar (Nova Arquitetura)
            if (node.SpecificLootTables != null)
            {
                foreach (var table in node.SpecificLootTables)
                {
                    if (table != null) table.GenerateLoot(rewardToGrant);
                }
            }

            Debug.Log($"[PhaseManager] Recompensa gerada! Artefatos totais: {rewardToGrant.GeneratedArtifacts.Count}");
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
