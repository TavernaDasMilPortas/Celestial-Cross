using System;
using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Data.Rewards;
using CelestialCross.Data.Loot;

namespace CelestialCross.Progression
{
    [Serializable]
    public class MilestoneReward
    {
        [Tooltip("Número de conclusões necessárias para ganhar esta recompensa")]
        public int RequiredCompletions;
        public List<RewardDefinition> Rewards = new List<RewardDefinition>();
    }

    [Serializable]
    public class NodeRewardConfig
    {
        [Header("Recompensas de Primeira Vez")]
        public List<RewardDefinition> FirstClearRewards = new List<RewardDefinition>();
        
        [Header("Recompensas de Repetição (cada vez que completa)")]
        public List<RewardDefinition> RepeatRewards = new List<RewardDefinition>();
        
        [Header("Loot Tables Procedurais")]
        [Tooltip("Tabelas de loot que rodam ao completar, idêntico ao sistema de Dungeons")]
        [SerializeReference]
        public List<BaseLootTable> LootTables = new List<BaseLootTable>();
        
        [Header("Recompensas por Milestone")]
        [Tooltip("Recompensas extras ao atingir X conclusões (ex: bonus na 5ª vez)")]
        public List<MilestoneReward> MilestoneRewards = new List<MilestoneReward>();
    }
}
