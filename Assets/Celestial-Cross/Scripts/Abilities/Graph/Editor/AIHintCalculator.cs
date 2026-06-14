using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;
using Celestial_Cross.Scripts.Units.Enemy.AI;

namespace Celestial_Cross.Scripts.Abilities.Graph.Editor
{
    public static class AIHintCalculator
    {
        public static void Recalculate(AbilityGraphSO graphSO)
        {
            if (graphSO == null) return;
            if (graphSO.aiHint == null)
                graphSO.aiHint = new AIAbilityHint();

            if (graphSO.aiHint.isLocked) return;

            var nodes = graphSO.NodeData;
            if (nodes == null || nodes.Count == 0) return;

            // 1. Extração dos Nós Relevantes
            bool hasDamage = false;
            bool hasHeal = false;
            bool hasBuff = false;
            bool hasSummon = false;

            float damageTotalScaling = 0f;
            float healTotalScaling = 0f;
            float buffTotalValue = 0f;
            int maxCooldown = 0;
            int maxTargets = 1;
            bool targetsAlly = false;
            bool targetsEnemy = false;
            bool hasArea = false;
            int areaCellCount = 1;

            foreach (var node in nodes)
            {
                if (string.IsNullOrEmpty(node.JsonData)) continue;

                if (node.NodeType == "DamageEffectNode" || node.NodeTitle.Contains("Damage"))
                {
                    hasDamage = true;
                    var dmgData = JsonUtility.FromJson<DamageNodeData>(node.JsonData);
                    if (dmgData != null && dmgData.scalings != null)
                    {
                        foreach (var sc in dmgData.scalings) damageTotalScaling += sc.percentage;
                    }
                }
                else if (node.NodeType == "HealEffectNode" || node.NodeTitle.Contains("Heal"))
                {
                    hasHeal = true;
                    var healData = JsonUtility.FromJson<HealNodeData>(node.JsonData);
                    if (healData != null && healData.scalings != null)
                    {
                        foreach (var sc in healData.scalings) healTotalScaling += sc.percentage;
                    }
                }
                else if (node.NodeType == "StatModifierEffectNode" || node.NodeTitle.Contains("Buff"))
                {
                    hasBuff = true;
                    var modData = JsonUtility.FromJson<StatModifierNodeData>(node.JsonData);
                    if (modData != null && modData.stats != null)
                    {
                        foreach (var st in modData.stats) buffTotalValue += Mathf.Abs(st.value);
                    }
                }
                else if (node.NodeType == "SummonNode" || node.NodeTitle.Contains("Summon"))
                {
                    hasSummon = true;
                }
                else if (node.NodeType == "LimitPerTurnNode")
                {
                    var limitData = JsonUtility.FromJson<LimitPerTurnNodeData>(node.JsonData);
                    if (limitData != null && limitData.maxExecutionsPerTurn <= 1)
                        maxCooldown = Mathf.Max(maxCooldown, 1);
                }
                else if (node.NodeType == "CostNode")
                {
                    var costData = JsonUtility.FromJson<CostNodeData>(node.JsonData);
                    if (costData != null && costData.manaCost > 0)
                    {
                        int heuristcCd = Mathf.Max(1, costData.manaCost / 20);
                        maxCooldown = Mathf.Max(maxCooldown, heuristcCd);
                    }
                }
                else if (node.NodeType == "TargetNode")
                {
                    var tgtData = JsonUtility.FromJson<TargetNodeData>(node.JsonData);
                    if (tgtData != null)
                    {
                        if (tgtData.multipleTargets && tgtData.maxTargets > 1)
                            maxTargets = Mathf.Max(maxTargets, tgtData.maxTargets);

                        if (tgtData.factionType == GraphFactionType.Ally)
                            targetsAlly = true;
                        else if (tgtData.factionType == GraphFactionType.Enemy)
                            targetsEnemy = true;
                        else if (tgtData.factionType == GraphFactionType.Any)
                        {
                            targetsAlly = true;
                            targetsEnemy = true;
                        }

                        // Calcular Células da Área
                        AreaPatternData pattern = null;
                        if (!string.IsNullOrEmpty(tgtData.patternReferenceId))
                            pattern = graphSO.GetAsset<AreaPatternData>(tgtData.patternReferenceId);
                        if (pattern == null) pattern = node.areaPattern;

                        if (pattern != null)
                        {
                            hasArea = true;
                            int cells = 0;
                            foreach (var row in pattern.Rows)
                            {
                                foreach (var cell in row.cells)
                                    if (cell) cells++;
                            }
                            areaCellCount = Mathf.Max(areaCellCount, cells);
                        }
                    }
                }
            }

            // 2. Classificar Categoria (Heal > Damage > Buff/Debuff > Summon > Utility)
            var category = AIAbilityHint.AbilityCategory.Utility;
            int basePrio = 30;
            float estimatedVal = 10f;
            bool isFriendly = false;

            if (hasHeal)
            {
                category = AIAbilityHint.AbilityCategory.Heal;
                basePrio = 80;
                estimatedVal = healTotalScaling > 0 ? healTotalScaling : 20f;
                isFriendly = true;
            }
            else if (hasDamage)
            {
                category = AIAbilityHint.AbilityCategory.Damage;
                basePrio = 50;
                estimatedVal = damageTotalScaling > 0 ? damageTotalScaling : 20f;
                isFriendly = false;
            }
            else if (hasBuff)
            {
                category = AIAbilityHint.AbilityCategory.Buff; // Pode ser Debuff, mas vamos agrupar em Buff para pontuação
                basePrio = 60;
                estimatedVal = buffTotalValue > 0 ? buffTotalValue : 15f;
                isFriendly = targetsAlly && !targetsEnemy; // Se alvo for Any, assume inimigo
            }
            else if (hasSummon)
            {
                category = AIAbilityHint.AbilityCategory.Summon;
                basePrio = 70;
                estimatedVal = 30f;
                isFriendly = false; // Invocações focam células vazias ou em volta do boss
            }

            // Override via TargetNode Faction
            if (targetsAlly && !targetsEnemy) isFriendly = true;
            else if (targetsEnemy && !targetsAlly) isFriendly = false;

            // 3. Ajuste de Prioridade por Área e Alcance e MultiTarget
            int range = graphSO.displayRange > 0 ? graphSO.displayRange : 1;
            
            if (hasArea && areaCellCount > 1)
            {
                basePrio += (areaCellCount - 1) * 2;
            }
            
            if (maxTargets > 1)
            {
                basePrio += (maxTargets - 1) * 3;
            }

            if (range >= 4)
            {
                basePrio += 5; // Long range bonus
            }

            basePrio = Mathf.Clamp(basePrio, 0, 100);

            // 4. Bônus Contextuais
            float lowHpBonus = 0f;
            float freshBonus = 0f;

            if (category == AIAbilityHint.AbilityCategory.Damage)
                lowHpBonus = estimatedVal * 0.4f;
            else if (category == AIAbilityHint.AbilityCategory.Heal)
                lowHpBonus = estimatedVal * 0.6f;

            if (category == AIAbilityHint.AbilityCategory.Buff)
                freshBonus = 15f; // Valor fixo razoável

            // 5. Atribuir Valores
            graphSO.aiHint.category = category;
            graphSO.aiHint.basePriority = basePrio;
            graphSO.aiHint.targetsFriendlies = isFriendly;
            graphSO.aiHint.cooldownTurns = maxCooldown;
            graphSO.aiHint.estimatedValue = estimatedVal;
            graphSO.aiHint.lowHPTargetBonus = lowHpBonus;
            graphSO.aiHint.freshApplicationBonus = freshBonus;
            
            // maxTargets currently isn't natively in AIAbilityHint, 
            // the AI will read it from the action's wrapper directly, 
            // but we ensure it exists in the calculations above.
        }
    }
}
