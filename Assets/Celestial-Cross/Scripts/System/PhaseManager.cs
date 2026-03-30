using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhaseManager : MonoBehaviour
{
    public static PhaseManager Instance { get; private set; }

    [SerializeField] private RewardPackage victoryRewards;

    [Header("Flow")]
    [SerializeField] private bool autoReturnToHub = false;
    [SerializeField] private string hubSceneName = "HubScene";
    [SerializeField] private float returnDelaySeconds = 1.0f;

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
        if (winningTeam == Team.Player)
        {
            Debug.Log("Fase concluída! Vitória do Jogador!");
            GrantRewards();
        }
        else
        {
            Debug.Log("Fase concluída! Derrota.");
        }

        OnPhaseEnded?.Invoke(winningTeam);

        if (autoReturnToHub && !string.IsNullOrWhiteSpace(hubSceneName))
        {
            Invoke(nameof(ReturnToHub), Mathf.Max(0f, returnDelaySeconds));
        }
        // Aqui você pode adicionar a lógica para carregar a próxima cena, mostrar uma tela de vitória/derrota, etc.
        // Time.timeScale = 0f; // Pausa o jogo, por exemplo.
    }

    void ReturnToHub()
    {
        SceneManager.LoadScene(hubSceneName);
    }

    private void GrantRewards()
    {
        RewardPackage rewardToGrant = null;

        if (GameFlowManager.Instance != null && GameFlowManager.Instance.SelectedLevel != null)
            rewardToGrant = GameFlowManager.Instance.SelectedLevel.VictoryRewards;

        if (rewardToGrant == null)
            rewardToGrant = victoryRewards;

        if (rewardToGrant != null)
        {
            Debug.Log($"Recompensa concedida: {rewardToGrant.Money} de dinheiro, {rewardToGrant.Energy} de energia.");

            if (AccountManager.Instance != null)
            {
                AccountManager.Instance.ApplyRewards(rewardToGrant);
            }
        }
        else
        {
            Debug.Log("Nenhuma recompensa configurada para esta fase.");
        }
    }
}
