using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Motor de decisão da IA. Avalia ações disponíveis, aplica scoring
/// baseado no AIBehaviorProfile e executa a melhor ação.
/// </summary>
public class AIBrain : MonoBehaviour
{
    EnemyUnit enemy;
    GridMap gridMap;

    void Awake()
    {
        enemy = GetComponent<EnemyUnit>();
    }

    void Start()
    {
        gridMap = GridMap.Instance;
    }

    // =============================
    // ENTRY POINT (chamado pelo TurnManager)
    // =============================

    /// <summary>
    /// Executa o turno da IA: avalia ações, escolhe a melhor, executa.
    /// </summary>
    public void ExecuteTurn()
    {
        Debug.Log($"[AIBrain] Iniciando turno de {enemy.DisplayName}");

        AIBehaviorProfile profile = enemy.BehaviorProfile;
        if (profile == null)
        {
            Debug.LogWarning($"[AIBrain] {enemy.DisplayName} sem AIBehaviorProfile. Passando turno.");
            TurnManager.Instance.EndTurn();
            return;
        }

        // 1) Contexto do estado atual
        float hpPercent = GetHpPercent();
        int aliveAllies = CountAliveAllies();
        bool isLastEnemy = aliveAllies == 0;

        // 2) Selecionar regra ativa
        AIBehaviorRule activeRule = profile.GetActiveRule(hpPercent, aliveAllies, isLastEnemy);

        BehaviorType behavior;
        AITargetPreference targetPref;
        float attackWeight, moveWeight;

        if (activeRule != null)
        {
            behavior = activeRule.behavior;
            targetPref = activeRule.targetPreference;
            attackWeight = activeRule.attackWeight;
            moveWeight = activeRule.moveWeight;
            Debug.Log($"[AIBrain] Regra ativa: {activeRule.ruleName} (priority {activeRule.priority})");
        }
        else
        {
            behavior = profile.fallbackBehavior;
            targetPref = profile.fallbackTargetPreference;
            attackWeight = 1f;
            moveWeight = 1f;
            Debug.Log($"[AIBrain] Nenhuma regra ativa. Usando fallback: {behavior}");
        }

        // 3) Coletar alvos possíveis (units do player)
        List<Unit> playerUnits = FindAlivePlayerUnits();

        if (playerUnits.Count == 0)
        {
            Debug.Log("[AIBrain] Sem alvos de player. Passando turno.");
            TurnManager.Instance.EndTurn();
            return;
        }

        // 4) Avaliar todas as ações
        List<AIActionScore> scores = EvaluateAllActions(
            playerUnits, behavior, targetPref,
            attackWeight, moveWeight, profile.randomnessFactor
        );

        if (scores.Count == 0)
        {
            Debug.Log("[AIBrain] Nenhuma ação viável. Passando turno.");
            TurnManager.Instance.EndTurn();
            return;
        }

        // 5) Executar a ação com maior score
        AIActionScore best = scores.OrderByDescending(s => s.score).First();
        ExecuteAction(best);
    }

    // =============================
    // AVALIAÇÃO DE AÇÕES
    // =============================

    List<AIActionScore> EvaluateAllActions(
        List<Unit> playerUnits,
        BehaviorType behavior,
        AITargetPreference targetPref,
        float attackWeight,
        float moveWeight,
        float randomness)
    {
        List<AIActionScore> scores = new();

        // Iterar sobre as ações da unit usando reflexão do pipeline existente
        var actionComponents = GetComponents<UnitActionBase>();

        for (int i = 0; i < actionComponents.Length; i++)
        {
            var action = actionComponents[i];

            if (action is AttackAction attackAction)
            {
                var attackScores = EvaluateAttack(
                    i, attackAction, playerUnits,
                    behavior, targetPref, attackWeight, randomness
                );
                scores.AddRange(attackScores);
            }
            else if (action is MoveAction moveAction)
            {
                var moveScores = EvaluateMove(
                    i, moveAction, playerUnits,
                    behavior, targetPref, moveWeight, randomness
                );
                scores.AddRange(moveScores);
            }
        }

        return scores;
    }

    // =============================
    // SCORING: ATTACK
    // =============================

    List<AIActionScore> EvaluateAttack(
        int actionIndex,
        AttackAction attackAction,
        List<Unit> playerUnits,
        BehaviorType behavior,
        AITargetPreference targetPref,
        float weight,
        float randomness)
    {
        List<AIActionScore> scores = new();
        int range = attackAction.Range;

        foreach (var target in playerUnits)
        {
            int dist = ManhattanDistance(enemy.GridPosition, target.GridPosition);

            // Só avalia se está no alcance
            if (dist > range)
                continue;

            float baseScore = CalculateTargetScore(target, targetPref, behavior);

            // Bônus por dano estimado
            float damageEstimate = Mathf.Max(1, enemy.Stats.attack + attackAction.Damage - target.Stats.defense);
            baseScore += damageEstimate * 0.5f;

            // Bônus se pode matar
            if (target.Health != null && damageEstimate >= target.Health.CurrentHealth)
                baseScore += 50f;

            // Aplicar peso e aleatoriedade
            float finalScore = ApplyWeightAndRandomness(baseScore, weight, randomness);

            scores.Add(new AIActionScore
            {
                actionIndex = actionIndex,
                target = target,
                moveTarget = target.GridPosition,
                score = finalScore
            });
        }

        return scores;
    }

    // =============================
    // SCORING: MOVE
    // =============================

    List<AIActionScore> EvaluateMove(
        int actionIndex,
        MoveAction moveAction,
        List<Unit> playerUnits,
        BehaviorType behavior,
        AITargetPreference targetPref,
        float weight,
        float randomness)
    {
        List<AIActionScore> scores = new();
        int range = moveAction.Range;

        // Encontrar tiles alcançáveis via BFS (replicando a lógica de MoveAction)
        HashSet<Vector2Int> reachable = GetReachableTiles(enemy.GridPosition, range);

        if (reachable.Count == 0)
            return scores;

        // Determinar o alvo preferido para guiar o movimento
        Unit preferredTarget = SelectPreferredTarget(playerUnits, targetPref);

        foreach (var tilePos in reachable)
        {
            float baseScore = 0f;

            if (preferredTarget != null)
            {
                int currentDist = ManhattanDistance(enemy.GridPosition, preferredTarget.GridPosition);
                int newDist = ManhattanDistance(tilePos, preferredTarget.GridPosition);

                switch (behavior)
                {
                    case BehaviorType.Aggressive:
                    case BehaviorType.Opportunist:
                    case BehaviorType.Balanced:
                        // Quanto mais perto do alvo, melhor
                        baseScore = (currentDist - newDist) * 10f;
                        break;

                    case BehaviorType.Defensive:
                        // Quanto mais longe, melhor
                        baseScore = (newDist - currentDist) * 10f;
                        break;

                    case BehaviorType.Support:
                        // Move em direção de aliados (futuro)
                        baseScore = 0f;
                        break;
                }
            }

            float finalScore = ApplyWeightAndRandomness(baseScore, weight, randomness);

            scores.Add(new AIActionScore
            {
                actionIndex = actionIndex,
                target = null,
                moveTarget = tilePos,
                score = finalScore
            });
        }

        return scores;
    }

    // =============================
    // TARGET PREFERENCE
    // =============================

    float CalculateTargetScore(Unit target, AITargetPreference pref, BehaviorType behavior)
    {
        float score = 0f;

        switch (pref)
        {
            case AITargetPreference.HighestHealth:
                score = target.Health != null ? target.Health.CurrentHealth : 0f;
                break;

            case AITargetPreference.LowestHealth:
                score = target.Health != null ? (target.MaxHealth - target.Health.CurrentHealth + 1) : 0f;
                break;

            case AITargetPreference.Closest:
                score = 100f - ManhattanDistance(enemy.GridPosition, target.GridPosition);
                break;

            case AITargetPreference.Farthest:
                score = ManhattanDistance(enemy.GridPosition, target.GridPosition);
                break;

            case AITargetPreference.HighestAttack:
                score = target.Stats.attack;
                break;

            case AITargetPreference.Random:
                score = Random.Range(0f, 100f);
                break;
        }

        return score;
    }

    Unit SelectPreferredTarget(List<Unit> playerUnits, AITargetPreference pref)
    {
        if (playerUnits.Count == 0)
            return null;

        return pref switch
        {
            AITargetPreference.HighestHealth =>
                playerUnits.OrderByDescending(u => u.Health != null ? u.Health.CurrentHealth : 0).First(),
            AITargetPreference.LowestHealth =>
                playerUnits.OrderBy(u => u.Health != null ? u.Health.CurrentHealth : int.MaxValue).First(),
            AITargetPreference.Closest =>
                playerUnits.OrderBy(u => ManhattanDistance(enemy.GridPosition, u.GridPosition)).First(),
            AITargetPreference.Farthest =>
                playerUnits.OrderByDescending(u => ManhattanDistance(enemy.GridPosition, u.GridPosition)).First(),
            AITargetPreference.HighestAttack =>
                playerUnits.OrderByDescending(u => u.Stats.attack).First(),
            _ => playerUnits[Random.Range(0, playerUnits.Count)]
        };
    }

    // =============================
    // EXECUÇÃO DA AÇÃO
    // =============================

    void ExecuteAction(AIActionScore best)
    {
        var actionComponents = GetComponents<UnitActionBase>();

        if (best.actionIndex < 0 || best.actionIndex >= actionComponents.Length)
        {
            Debug.LogError($"[AIBrain] actionIndex inválido: {best.actionIndex}");
            TurnManager.Instance.EndTurn();
            return;
        }

        var action = actionComponents[best.actionIndex];

        if (action is AttackAction attackAction && best.target != null)
        {
            Debug.Log($"[AIBrain] {enemy.DisplayName} ATACA {best.target.DisplayName} (score: {best.score:F1})");
            ExecuteAttackDirect(attackAction, best.target);
        }
        else if (action is MoveAction)
        {
            Debug.Log($"[AIBrain] {enemy.DisplayName} MOVE para {best.moveTarget} (score: {best.score:F1})");
            ExecuteMoveDirect(best.moveTarget);
        }
        else
        {
            Debug.LogWarning($"[AIBrain] Tipo de ação desconhecido no index {best.actionIndex}");
            TurnManager.Instance.EndTurn();
        }
    }

    /// <summary>
    /// Executa ataque diretamente sem input do jogador.
    /// Replica o fluxo de AttackAction.Resolve() de forma programática.
    /// </summary>
    void ExecuteAttackDirect(AttackAction attackAction, Unit target)
    {
        if (target.Health == null)
        {
            TurnManager.Instance.EndTurn();
            return;
        }

        int hits = enemy.GetAttacksAgainst(target);
        int totalDamage = 0;

        for (int i = 0; i < hits; i++)
        {
            AttackResult result = enemy.CalculateAttack(
                target,
                new DamageBonus { flat = attackAction.Damage, percent = 0f },
                new DamageReduction { flat = 0, percent = 0f }
            );

            totalDamage += result.damage;
            Debug.Log($"[AIBrain] Hit {i + 1}/{hits} | {enemy.DisplayName} -> {target.DisplayName} | Damage: {result.damage} | Critical: {result.isCritical}");
        }

        target.Health.TakeDamage(totalDamage);
        TurnManager.Instance.EndTurn();
    }

    /// <summary>
    /// Executa movimento diretamente, sem input do jogador.
    /// Replica a lógica de MoveAction.MoveUnit().
    /// </summary>
    void ExecuteMoveDirect(Vector2Int targetPos)
    {
        if (gridMap == null)
            gridMap = GridMap.Instance;

        GridTile currentTile = gridMap.GetTile(enemy.GridPosition);
        if (currentTile != null)
            currentTile.IsOccupied = false;

        GridTile destTile = gridMap.GetTile(targetPos);
        if (destTile == null)
        {
            Debug.LogError($"[AIBrain] Tile destino {targetPos} não existe!");
            TurnManager.Instance.EndTurn();
            return;
        }

        enemy.GridPosition = targetPos;
        enemy.transform.position = new Vector3(targetPos.x, 0f, targetPos.y);
        destTile.IsOccupied = true;

        Debug.Log($"[AIBrain] {enemy.DisplayName} moveu para {targetPos}");
        TurnManager.Instance.EndTurn();
    }

    // =============================
    // UTILITÁRIOS
    // =============================

    float GetHpPercent()
    {
        if (enemy.Health == null || enemy.MaxHealth <= 0)
            return 1f;

        return (float)enemy.Health.CurrentHealth / enemy.MaxHealth;
    }

    int CountAliveAllies()
    {
        int count = 0;

        foreach (var unit in FindObjectsOfType<EnemyUnit>())
        {
            if (unit == enemy)
                continue;

            if (unit.Health != null && unit.Health.CurrentHealth > 0)
                count++;
        }

        return count;
    }

    List<Unit> FindAlivePlayerUnits()
    {
        List<Unit> result = new();

        foreach (var unit in FindObjectsOfType<Unit>())
        {
            // Pula inimigos
            if (unit is EnemyUnit)
                continue;

            // Pula mortos
            if (unit.Health != null && unit.Health.CurrentHealth <= 0)
                continue;

            result.Add(unit);
        }

        return result;
    }

    HashSet<Vector2Int> GetReachableTiles(Vector2Int origin, int range)
    {
        HashSet<Vector2Int> reachable = new();
        Queue<(Vector2Int pos, int cost)> queue = new();
        HashSet<Vector2Int> visited = new();

        Vector2Int[] dirs = {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right
        };

        queue.Enqueue((origin, 0));
        visited.Add(origin);

        while (queue.Count > 0)
        {
            var (pos, cost) = queue.Dequeue();
            if (cost > range)
                continue;

            if (pos != origin)
                reachable.Add(pos);

            foreach (var d in dirs)
            {
                Vector2Int next = pos + d;
                if (visited.Contains(next))
                    continue;

                if (gridMap == null)
                    gridMap = GridMap.Instance;

                GridTile nextTile = gridMap.GetTile(next);
                if (nextTile == null || nextTile.IsOccupied)
                    continue;

                visited.Add(next);
                queue.Enqueue((next, cost + 1));
            }
        }

        return reachable;
    }

    int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    float ApplyWeightAndRandomness(float baseScore, float weight, float randomness)
    {
        float weighted = baseScore * weight;

        if (randomness > 0f)
        {
            float noise = Random.Range(-randomness, randomness) * Mathf.Abs(weighted);
            weighted += noise;
        }

        return weighted;
    }
}
