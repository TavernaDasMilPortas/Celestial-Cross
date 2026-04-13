using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        if (winningTeam == Team.Player && finalReward != null)
        {
            // Mostra a UI procedimental e só volta pro Hub depois do clique
            CelestialCross.Giulia_UI.VictoryRewardUI.ShowVictoryUI(finalReward, () => 
            {
                if (!string.IsNullOrWhiteSpace(hubSceneName))
                    ReturnToHub();
            });
        }
        else if (!string.IsNullOrWhiteSpace(hubSceneName))
        {
            // Derrota: Volta para o hub com atraso
            Invoke(nameof(ReturnToHub), Mathf.Max(0.5f, returnDelaySeconds));
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

        // --- INTEGRAÇÃO DO DROP PROCEDURAL DE ARTEFATOS ---
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.SelectedDungeon != null && GameFlowManager.Instance.SelectedDungeonNode != null)
        {
            var generatedLoot = CelestialCross.System.ArtifactLootService.GenerateLoot(
                GameFlowManager.Instance.SelectedDungeon, 
                GameFlowManager.Instance.SelectedDungeonNode
            );

            if (generatedLoot != null && generatedLoot.Count > 0)
            {
                rewardToGrant.GeneratedArtifacts.AddRange(generatedLoot);
                Debug.Log($"[PhaseManager] {generatedLoot.Count} Artefatos gerados proceduralmente!");
            }
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
