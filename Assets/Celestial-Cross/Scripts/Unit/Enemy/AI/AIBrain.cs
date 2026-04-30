using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Combat.Execution;

/// <summary>
/// Motor de decisão da IA. Avalia ações disponíveis, aplica scoring
/// baseado no AIBehaviorProfile e executa a melhor ação.
/// </summary>
public class AIBrain : MonoBehaviour
{
    EnemyUnit enemy;
    GridMap gridMap;
    
    public Unit MyUnit { get; private set; }
    private Celestial_Cross.Scripts.Units.Enemy.AI.AICooldownTracker cooldownTracker = new();

    void Awake()
    {
        enemy = GetComponent<EnemyUnit>();
        MyUnit = enemy;
    }

    void Start()
    {
        gridMap = GridMap.Instance;
    }

    void OnEnable()
    {
        TurnManager.OnTurnEnded += HandleTurnEnded;
    }

    void OnDisable()
    {
        TurnManager.OnTurnEnded -= HandleTurnEnded;
    }

    void HandleTurnEnded()
    {
        // Só ticka se for o turno DESTA unidade que acabou
        if (TurnManager.Instance.CurrentUnit == MyUnit)
        {
            cooldownTracker.TickCooldowns();
        }
    }

    // =============================
    // ENTRY POINT (chamado pelo TurnManager)
    // =============================

    public void ExecuteTurn()
    {
        Debug.Log($"[AIBrain] Iniciando turno de {enemy.DisplayName}");

        CheckPatternTriggers();

        AIBehaviorProfile profile = enemy.BehaviorProfile;
        if (profile == null)
        {
            TurnManager.Instance.EndTurn();
            return;
        }

        float hpPercent = GetHpPercent();
        int aliveAllies = CountAliveAllies();
        bool isLastEnemy = aliveAllies == 0;

        AIBehaviorRule activeRule = profile.GetActiveRule(hpPercent, aliveAllies, isLastEnemy);
        
        // Context variables for evaluation
        BehaviorType behavior = activeRule?.behavior ?? profile.fallbackBehavior;
        AITargetPreference targetPref = activeRule?.targetPreference ?? profile.fallbackTargetPreference;
        float attackWeight = activeRule?.attackWeight ?? 1f;
        float moveWeight = activeRule?.moveWeight ?? 1f;
        UnitRole prefRole = activeRule?.preferredRole ?? profile.fallbackPreferredRole;
        UnitClass prefClass = activeRule?.preferredClass ?? profile.fallbackPreferredClass;
        float randomness = profile.randomnessFactor;

        List<Unit> playerUnits = FindAlivePlayerUnits();
        if (playerUnits.Count == 0)
        {
            TurnManager.Instance.EndTurn();
            return;
        }

        // 1) PLANEJAMENTO DE TURNO (Move + Action)
        Celestial_Cross.Scripts.Units.Enemy.AI.AITurnPlan bestPlan = PlanTurn(
            playerUnits, behavior, targetPref, 
            attackWeight, moveWeight, randomness, 
            prefRole, prefClass, activeRule
        );

        if (!bestPlan.IsValid)
        {
            Debug.Log("[AIBrain] Nenhuma ação viável. Passando turno.");
            TurnManager.Instance.EndTurn();
            return;
        }

        // 2) EXECUÇÃO DO PLANO
        StartCoroutine(ExecutePlanRoutine(bestPlan));
    }

    private Celestial_Cross.Scripts.Units.Enemy.AI.AITurnPlan PlanTurn(
        List<Unit> playerUnits, BehaviorType behavior, AITargetPreference targetPref,
        float attackWeight, float moveWeight, float randomness,
        UnitRole prefRole, UnitClass prefClass, AIBehaviorRule rule)
    {
        Celestial_Cross.Scripts.Units.Enemy.AI.AITurnPlan bestPlan = default;
        
        // Avaliar ações da posição ATUAL (sem mover)
        var staticScores = EvaluateAllActions(
            playerUnits, behavior, targetPref, 
            attackWeight, moveWeight, randomness, 
            prefRole, prefClass, rule
        );

        foreach (var score in staticScores)
        {
            if (score.score > bestPlan.combinedScore)
            {
                bestPlan.actionStep = score;
                bestPlan.hasAction = true;
                bestPlan.combinedScore = score.score;
            }
        }

        // Avaliar possíveis movimentos + ações de lá
        if (!enemy.hasMovedThisTurn)
        {
            var moveActions = enemy.Actions.OfType<MoveAction>().ToList();
            if (moveActions.Count > 0)
            {
                var moveAction = moveActions[0];
                var reachable = GetReachableTiles(enemy.GridPosition, moveAction.Range);
                
                Vector2Int originalPos = enemy.GridPosition;

                foreach (var tile in reachable)
                {
                    // Simular posição para avaliar ações
                    enemy.GridPosition = tile;
                    var hypotheticalScores = EvaluateAllActions(
                        playerUnits, behavior, targetPref, 
                        attackWeight, moveWeight, randomness, 
                        prefRole, prefClass, rule
                    );

                    foreach (var actionScore in hypotheticalScores)
                    {
                        // Aqui o wrapper de ação não sabe que pularia o move, 
                        // precisamos filtrar para não pegar o MoveAction novamente como ação hipotética se já estamos movendo.
                        if (enemy.Actions[actionScore.actionIndex] is MoveAction) continue;

                        float moveBonus = 0f; // Poderíamos adicionar bônus tático por tile aqui
                        float combined = actionScore.score + moveBonus;

                        if (combined > bestPlan.combinedScore)
                        {
                            // Encontrar o índice manualmente ou castar para List se possível
                            int moveActionIndex = -1;
                            for (int j = 0; j < enemy.Actions.Count; j++) {
                                if (enemy.Actions[j] == moveAction) {
                                    moveActionIndex = j;
                                    break;
                                }
                            }

                            bestPlan.moveStep = new AIActionScore(moveActionIndex, 10) { moveTarget = tile };
                            bestPlan.actionStep = actionScore;
                            bestPlan.hasMove = true;
                            bestPlan.hasAction = true;
                            bestPlan.combinedScore = combined;
                        }
                    }
                }
                // Restaurar posição original após simulação
                enemy.GridPosition = originalPos;
            }
        }

        return bestPlan;
    }

    private System.Collections.IEnumerator ExecutePlanRoutine(Celestial_Cross.Scripts.Units.Enemy.AI.AITurnPlan plan)
    {
        if (plan.hasMove)
        {
            Debug.Log($"[AIBrain] Executando Movimento para {plan.moveStep.moveTarget}");
            ExecuteMoveDirect(plan.moveStep.moveTarget);
            yield return new WaitForSeconds(0.6f);
        }

        if (plan.hasAction)
        {
            var action = enemy.Actions[plan.actionStep.actionIndex];
            Debug.Log($"[AIBrain] Executando Ação: {action.ActionName} em {plan.actionStep.target?.DisplayName ?? "Ponto"}");
            
            // Registrar cooldown
            Celestial_Cross.Scripts.Units.Enemy.AI.AIAbilityHint hint = GetHintForAction(action);
            if (hint != null) cooldownTracker.UseAbility(action.ActionName, hint.cooldownTurns);

            if (action is AttackAction atk && plan.actionStep.target != null)
                ExecuteAttackDirect(atk, plan.actionStep.target);
            else
                ExecuteAbilityWrapperDirect(action, plan.actionStep.target);
        }
        else
        {
            TurnManager.Instance.EndTurn();
        }
    }

    // =============================
    // AVALIAÇÃO DE AÇÕES
    // =============================

    List<AIActionScore> EvaluateAllActions(
        List<Unit> playerUnits,
        BehaviorType behavior,
        AITargetPreference targetPreference,
        float attackWeight,
        float moveWeight,
        float randomness,
        UnitRole prefRole,
        UnitClass prefClass,
        AIBehaviorRule activeRule)
    {
        List<AIActionScore> scores = new();
        List<Unit> allyUnits = FindAliveAllies(); // For heals/buffs

        // Iterar sobre as ações da unit (incluindo wrappers de Graph e Blueprint)
        for (int i = 0; i < enemy.Actions.Count; i++)
        {
            var action = enemy.Actions[i];

            // Pular ações passivas ou em cooldown
            if (cooldownTracker.IsOnCooldown(action.ActionName)) continue;

            Celestial_Cross.Scripts.Units.Enemy.AI.AIAbilityHint hint = GetHintForAction(action);

            if (action is AttackAction attackAction)
            {
                scores.AddRange(EvaluateAttack(
                    i, attackAction, playerUnits,
                    behavior, targetPreference, attackWeight, randomness,
                    prefRole, prefClass
                ));
            }
            else if (action is MoveAction moveAction)
            {
                if (enemy.hasMovedThisTurn) continue;
                scores.AddRange(EvaluateMove(
                    i, moveAction, playerUnits,
                    behavior, targetPreference, moveWeight, randomness,
                    prefRole, prefClass
                ));
            }
            else if (hint != null) // Graph ou Blueprint com Hint
            {
                scores.AddRange(EvaluateAbility(
                    i, action, hint, playerUnits, allyUnits,
                    behavior, activeRule, randomness, prefRole, prefClass
                ));
            }
        }

        return scores;
    }

    Celestial_Cross.Scripts.Units.Enemy.AI.AIAbilityHint GetHintForAction(IUnitAction action)
    {
        if (action is Celestial_Cross.Scripts.Units.GraphActionWrapper gw)
            return gw.Graph.aiHint;
        if (action is Celestial_Cross.Scripts.Units.BlueprintActionWrapper bw)
            return bw.Blueprint.aiHint;
        
        return null; 
    }

    List<AIActionScore> EvaluateAbility(
        int actionIndex,
        IUnitAction action,
        Celestial_Cross.Scripts.Units.Enemy.AI.AIAbilityHint hint,
        List<Unit> playerUnits,
        List<Unit> allyUnits,
        BehaviorType behavior,
        AIBehaviorRule rule,
        float randomness,
        UnitRole prefRole,
        UnitClass prefClass)
    {
        List<AIActionScore> scores = new();
        List<Unit> potentialTargets = hint.targetsFriendlies ? allyUnits : playerUnits;

        foreach (var target in potentialTargets)
        {
            int dist = ChebyshevDistance(enemy.GridPosition, target.GridPosition);
            if (dist > action.Range) continue;

            float baseScore = hint.basePriority;
            float categoryMultiplier = 1f;

            switch (hint.category)
            {
                case Celestial_Cross.Scripts.Units.Enemy.AI.AIAbilityHint.AbilityCategory.Damage:
                    categoryMultiplier = rule != null ? rule.damageWeight : 1f;
                    baseScore += hint.estimatedValue * 0.5f;
                    if (target.Health != null && target.Health.CurrentHealth <= hint.estimatedValue) 
                        baseScore += 50f;
                    float targetHpPct = (float)target.Health.CurrentHealth / target.MaxHealth;
                    if (targetHpPct < 0.3f) baseScore += hint.lowHPTargetBonus;
                    break;

                case Celestial_Cross.Scripts.Units.Enemy.AI.AIAbilityHint.AbilityCategory.Heal:
                    categoryMultiplier = rule != null ? rule.healWeight : 1f;
                    float missingHP = target.MaxHealth - target.Health.CurrentHealth;
                    if (missingHP <= 0) baseScore = -100f; // Don't heal full health
                    else baseScore += missingHP * 0.3f;
                    if (missingHP > target.MaxHealth * 0.7f) baseScore += hint.lowHPTargetBonus;
                    break;

                case Celestial_Cross.Scripts.Units.Enemy.AI.AIAbilityHint.AbilityCategory.Buff:
                    categoryMultiplier = rule != null ? rule.buffWeight : 1f;
                    baseScore += hint.freshApplicationBonus; // Simple heuristic
                    break;

                case Celestial_Cross.Scripts.Units.Enemy.AI.AIAbilityHint.AbilityCategory.Debuff:
                    categoryMultiplier = rule != null ? rule.debuffWeight : 1f;
                    baseScore += hint.freshApplicationBonus;
                    break;
            }

            float weight = rule != null ? rule.abilityWeight : 1f;
            float finalScore = ApplyWeightAndRandomness(baseScore * categoryMultiplier, weight, randomness);

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
    // SCORING: ATTACK
    // =============================

    List<AIActionScore> EvaluateAttack(
        int actionIndex,
        AttackAction attackAction,
        List<Unit> playerUnits,
        BehaviorType behavior,
        AITargetPreference targetPref,
        float weight,
        float randomness,
        UnitRole prefRole,
        UnitClass prefClass)
    {
        List<AIActionScore> scores = new();
        int range = attackAction.Range;

        foreach (var target in playerUnits)
        {
            int dist = ChebyshevDistance(enemy.GridPosition, target.GridPosition);

            // Só avalia se está no alcance
            if (dist > range)
                continue;

            float baseScore = CalculateTargetScore(target, targetPref, behavior, prefRole, prefClass);

            // Bônus por dano estimado
            float damageEstimate = Mathf.Max(1, Mathf.FloorToInt(enemy.Stats.attack * attackAction.DamageMultiplier) - target.Stats.defense);
            baseScore += damageEstimate * 0.5f;

            // Bônus se pode matar
            if (target.Health != null && damageEstimate >= target.Health.CurrentHealth)
                baseScore += 50f;

            // Bônus DESTRUIDOR para Habilidades "Ultimate" do Chefe nesta fase
            if (attackAction.IsAbsolutePriority)
            {
                baseScore += 1000000f; // Bypassa todo o peso normal, AI VAI focar nisto
                Debug.Log($"[AIBrain/Priority] Prioridade Absoluta Ativada para {attackAction.ActionName} em {target.DisplayName}!");
            }

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
        float randomness,
        UnitRole prefRole,
        UnitClass prefClass)
    {
        List<AIActionScore> scores = new();
        int range = moveAction.Range;

        // Encontrar tiles alcançáveis via BFS (replicando a lógica de MoveAction)
        HashSet<Vector2Int> reachable = GetReachableTiles(enemy.GridPosition, range);

        if (reachable.Count == 0)
            return scores;

        // Determinar o alvo preferido para guiar o movimento
        Unit preferredTarget = SelectPreferredTarget(playerUnits, targetPref, prefRole, prefClass);

        foreach (var tilePos in reachable)
        {
            float baseScore = 0f;

            if (preferredTarget != null)
            {
                int currentDist = ChebyshevDistance(enemy.GridPosition, preferredTarget.GridPosition);
                int newDist = ChebyshevDistance(tilePos, preferredTarget.GridPosition);

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

    float CalculateTargetScore(Unit target, AITargetPreference pref, BehaviorType behavior, UnitRole prefRole, UnitClass prefClass)
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
                score = 100f - ChebyshevDistance(enemy.GridPosition, target.GridPosition);
                break;

            case AITargetPreference.Farthest:
                score = ChebyshevDistance(enemy.GridPosition, target.GridPosition);
                break;

            case AITargetPreference.HighestAttack:
                score = target.Stats.attack;
                break;

            case AITargetPreference.Random:
                score = Random.Range(0f, 100f);
                break;
                
            case AITargetPreference.PrioritizeRole:
                score = (target.unitData != null && target.unitData.role == prefRole) ? 100f : 0f;
                score += 50f - ChebyshevDistance(enemy.GridPosition, target.GridPosition); // tie-breaker distance
                break;
                
            case AITargetPreference.PrioritizeClass:
                score = (target.unitData != null && target.unitData.unitClass == prefClass) ? 100f : 0f;
                score += 50f - ChebyshevDistance(enemy.GridPosition, target.GridPosition); // tie-breaker distance
                break;
        }

        return score;
    }

    Unit SelectPreferredTarget(List<Unit> playerUnits, AITargetPreference pref, UnitRole prefRole, UnitClass prefClass)
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
                playerUnits.OrderBy(u => ChebyshevDistance(enemy.GridPosition, u.GridPosition)).First(),
            AITargetPreference.Farthest =>
                playerUnits.OrderByDescending(u => ChebyshevDistance(enemy.GridPosition, u.GridPosition)).First(),
            AITargetPreference.HighestAttack =>
                playerUnits.OrderByDescending(u => u.Stats.attack).First(),
            AITargetPreference.PrioritizeRole =>
                playerUnits.OrderByDescending(u => u.unitData != null && u.unitData.role == prefRole ? 1 : 0)
                           .ThenBy(u => ChebyshevDistance(enemy.GridPosition, u.GridPosition)).First(),
            AITargetPreference.PrioritizeClass =>
                playerUnits.OrderByDescending(u => u.unitData != null && u.unitData.unitClass == prefClass ? 1 : 0)
                           .ThenBy(u => ChebyshevDistance(enemy.GridPosition, u.GridPosition)).First(),
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
        float multiplier = attackAction != null ? attackAction.DamageMultiplier : 1.0f;

        for (int i = 0; i < hits; i++)
        {
            var ctx = new CombatContext(enemy, target, Mathf.FloorToInt(enemy.Stats.attack * multiplier));
            Celestial_Cross.Scripts.Combat.Execution.DamageProcessor.ProcessAndApplyDamage(ctx, applyDefense: true);
        }
        TurnManager.Instance.EndTurn();
    }

    void ExecuteAbilityWrapperDirect(IUnitAction action, Unit target)
    {
        // Se a ação tiver um alvo, definimos no wrapper antes de entrar
        if (target != null) action.Target = target.GridPosition;

        // O wrapper já sabe como se executar via AbilityExecutor
        action.EnterAction();
    }

    /// <summary>
    /// Executa o movimento diretamente, sem input do jogador.
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
            return;
        }

        enemy.GridPosition = targetPos;
        enemy.transform.position = new Vector3(targetPos.x, 0f, targetPos.y);
        destTile.IsOccupied = true;
        enemy.hasMovedThisTurn = true;

        Debug.Log($"[AIBrain] {enemy.DisplayName} moveu para {targetPos}");
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

        foreach (var unit in Object.FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None))
        {
            if (unit == enemy)
                continue;

            if (unit.Health != null && unit.Health.CurrentHealth > 0)
                count++;
        }

        return count;
    }

    List<Unit> FindAliveAllies()
    {
        List<Unit> result = new();
        foreach (var unit in Object.FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            if (unit.Team == enemy.Team && unit != enemy && unit.Health != null && unit.Health.CurrentHealth > 0)
                result.Add(unit);
        }
        return result;
    }

    List<Unit> FindAlivePlayerUnits()
    {
        List<Unit> result = new();

        foreach (var unit in Object.FindObjectsByType<Unit>(FindObjectsSortMode.None))
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

    int ChebyshevDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
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

    // =============================
    // PADRÕES DE CHEFE / TRANSIÇÕES
    // =============================
    void CheckPatternTriggers()
    {
        if (enemy.PatternData == null) return;
        if (enemy.PatternData.phases == null || enemy.PatternData.phases.Count == 0) return;

        float hpPercent = GetHpPercent();

        // Passa por todas as fases não ativadas, da maior prioridade (menor HP) pra cima, 
        // ou dependendo da ordem configurada no ScriptableObject.
        foreach (var phase in enemy.PatternData.phases)
        {
            if (phase.hasTriggered) continue;

            if (hpPercent <= phase.triggerHpBelowPercent)
            {
                Debug.Log($"[AIBrain] {enemy.DisplayName} alcançou < {phase.triggerHpBelowPercent}% de HP. Mudando para Fase Mágica: {phase.phaseName}!");
                phase.hasTriggered = true;
                
                if (phase.newBehaviorProfile != null)
                {
                    enemy.SetBehaviorProfile(phase.newBehaviorProfile);
                }
                
                // Opção: Pode tocar uma animação ou efeito de "Power Up" aqui no futuro
                break; // Apenas trigamos uma fase por verificação
            }
        }
    }
}

