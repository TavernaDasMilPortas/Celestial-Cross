using System;
using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Progression;
using CelestialCross.Data.Dungeon;
using CelestialCross.Data.Rewards;

namespace CelestialCross.System
{
    public class ProgressionService : MonoBehaviour
    {
        public static ProgressionService Instance { get; private set; }

        public event Action<string> OnNodeCompleted;
        public event Action<ChapterData> OnChapterCompleted;
        public event Action<ChapterData> OnChapterUnlocked;
        public event Action<string> OnProgressionError;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool IsNodeCompleted(string nodeID)
        {
            if (string.IsNullOrEmpty(nodeID)) return false;
            var account = AccountManager.Instance?.PlayerAccount;
            return account != null && account.CompletedNodeIDs.Contains(nodeID);
        }

        public void CompleteNode(string nodeID)
        {
            if (string.IsNullOrEmpty(nodeID)) return;
            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return;

            if (!account.CompletedNodeIDs.Contains(nodeID))
            {
                account.CompletedNodeIDs.Add(nodeID);
            }

            if (!account.NodeCompletionCounts.ContainsKey(nodeID))
            {
                account.NodeCompletionCounts[nodeID] = 0;
            }
            // Increment logic. If it was reset by time, this might not reflect correctly unless we also reset the counter.
            // Since we only reset visually/logically, let's actually reset the counter in the Account here before incrementing if needed.
            // Wait, we don't have the StoryNode here, just the ID. It's safer to just increment. 
            // We'll trust that the counter was validated before starting.
            // Actually, if we just increment, we must clear the counter if a reset happened BEFORE incrementing.
            // But we don't have the node's ResetPolicy here.
            // For now, let's just increment and set time. We can do reset logic inside GetCompletionCount if we pass the node.
            
            // For now, we will handle the "reset" by checking it dynamically, but we really should reset the counter if it expired.
            // Let's just increment for now. The TryStartNode/GetCompletionCount will handle dynamic resets.
            // Actually, a simpler way: just expose `RecordNodeCompletion(StoryNode node)`
            account.CompletedNodeIDs.Add(nodeID); // HashSet behavior
            account.NodeCompletionCounts[nodeID] = account.NodeCompletionCounts.GetValueOrDefault(nodeID, 0) + 1;
            account.NodeLastCompletionTime[nodeID] = DateTime.UtcNow.ToString("o");

            AccountManager.Instance.SaveAccount();
            OnNodeCompleted?.Invoke(nodeID);
        }

        public void RecordNodeCompletion(StoryNode node)
        {
            if (node == null) return;
            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return;

            string nodeID = node.NodeID;
            
            // Check if it should be reset before incrementing
            int currentCount = GetCompletionCount(node);
            
            if (!account.CompletedNodeIDs.Contains(nodeID))
                account.CompletedNodeIDs.Add(nodeID);

            account.NodeCompletionCounts[nodeID] = currentCount + 1;
            account.NodeLastCompletionTime[nodeID] = DateTime.UtcNow.ToString("o");

            AccountManager.Instance.SaveAccount();
            OnNodeCompleted?.Invoke(nodeID);
        }

        public int GetCompletionCount(StoryNode node)
        {
            if (node == null) return 0;
            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return 0;

            string nodeID = node.NodeID;
            if (!account.NodeCompletionCounts.TryGetValue(nodeID, out int count))
                return 0;

            if (node.ResetPolicy.ResetType == CompletionResetType.Never)
                return count;

            if (!account.NodeLastCompletionTime.TryGetValue(nodeID, out string lastTimeStr))
                return count;

            if (!DateTime.TryParse(lastTimeStr, out DateTime lastTime))
                return count;

            // Check if reset occurred
            if (HasResetOccurred(lastTime, node.ResetPolicy))
            {
                return 0; // It resets!
            }

            return count;
        }

        private bool HasResetOccurred(DateTime lastCompletionTime, CompletionResetPolicy policy)
        {
            DateTime now = DateTime.UtcNow;
            if (now <= lastCompletionTime) return false;

            if (policy.ResetType == CompletionResetType.Custom)
            {
                return (now - lastCompletionTime).TotalHours >= policy.CustomResetIntervalHours;
            }

            if (policy.ResetType == CompletionResetType.Daily)
            {
                // Next reset is the first time after lastCompletionTime that hour == ResetHourUTC
                DateTime nextReset = lastCompletionTime.Date.AddHours(policy.ResetHourUTC);
                if (nextReset <= lastCompletionTime) nextReset = nextReset.AddDays(1);
                return now >= nextReset;
            }

            if (policy.ResetType == CompletionResetType.Weekly)
            {
                DateTime nextReset = lastCompletionTime.Date.AddHours(policy.WeeklyResetHourUTC);
                while (nextReset.DayOfWeek != policy.ResetDayOfWeek || nextReset <= lastCompletionTime)
                {
                    nextReset = nextReset.AddDays(1);
                }
                return now >= nextReset;
            }

            if (policy.ResetType == CompletionResetType.Monthly)
            {
                DateTime nextReset = new DateTime(lastCompletionTime.Year, lastCompletionTime.Month, policy.ResetDayOfMonth, policy.MonthlyResetHourUTC, 0, 0, DateTimeKind.Utc);
                if (nextReset <= lastCompletionTime) nextReset = nextReset.AddMonths(1);
                return now >= nextReset;
            }

            return false;
        }

        public bool HasReachedMaxCompletions(StoryNode node)
        {
            if (node == null || node.MaxCompletions == -1) return false;
            return GetCompletionCount(node) >= node.MaxCompletions;
        }

        public bool CanAffordItemCosts(StoryNode node)
        {
            if (node == null || node.EntryCost == null || node.EntryCost.ItemCosts == null) return true;
            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return false;

            foreach (var cost in node.EntryCost.ItemCosts)
            {
                if (account.GetItemCount(cost.ItemID) < cost.Amount)
                    return false;
            }
            return true;
        }

        public void ConsumeItemCosts(StoryNode node)
        {
            if (node == null || node.EntryCost == null || node.EntryCost.ItemCosts == null) return;
            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return;

            foreach (var cost in node.EntryCost.ItemCosts)
            {
                account.RemoveItem(cost.ItemID, cost.Amount);
            }
        }

        public bool IsChapterUnlocked(ChapterData chapter)
        {
            if (chapter == null) return false;

            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return false;

            if (chapter.IsDiaryChapter && !string.IsNullOrEmpty(chapter.RequiredUnitID))
            {
                bool ownsUnit = account.OwnedUnitIDs.Contains(chapter.RequiredUnitID) || 
                                account.OwnedUnits.Exists(u => u.UnitID == chapter.RequiredUnitID);
                if (!ownsUnit) return false;
            }

            if (chapter.RequiredChapter != null)
            {
                if (chapter.RequiredChapter.Nodes != null && chapter.RequiredChapter.Nodes.Count > 0)
                {
                    var lastNode = chapter.RequiredChapter.Nodes[chapter.RequiredChapter.Nodes.Count - 1];
                    if (!IsNodeCompleted(lastNode.NodeID))
                        return false;
                }
            }

            return true;
        }

        public bool IsRuinFloorUnlocked(DungeonBaseSO dungeon, int floorIndex)
        {
            if (dungeon == null) return false;
            
            if (!string.IsNullOrEmpty(dungeon.RequiredNodeID) && !IsNodeCompleted(dungeon.RequiredNodeID))
                return false;

            if (floorIndex <= 0) return true;

            if (dungeon.Levels != null && floorIndex < dungeon.Levels.Count)
            {
                var prevLevel = dungeon.Levels[floorIndex - 1].LevelRef;
                if (prevLevel != null)
                {
                    return IsNodeCompleted(prevLevel.name);
                }
            }
            return false;
        }

        public bool IsDiaryUnlocked(string diaryNodeID)
        {
            if (string.IsNullOrEmpty(diaryNodeID)) return false;
            var account = AccountManager.Instance?.PlayerAccount;
            if (account == null) return false;

            return account.CompletedNodeIDs.Contains(diaryNodeID) || account.UnlockedDiaryNodeIDs.Contains(diaryNodeID);
        }

        public bool CanAffordEnergy(int cost)
        {
            if (cost <= 0) return true;
            return EnergyService.Instance != null && EnergyService.Instance.GetCurrentEnergy() >= cost;
        }

        public bool TryStartNode(StoryNode node)
        {
            if (node == null) return false;

            if (node.Requirement != null)
            {
                if (node.Requirement.RequiresPreviousNode && !string.IsNullOrEmpty(node.Requirement.PreviousNodeID))
                {
                    if (!IsNodeCompleted(node.Requirement.PreviousNodeID))
                    {
                        OnProgressionError?.Invoke("Complete o nó anterior primeiro.");
                        return false;
                    }
                }

                if (node.Requirement.RequiresInvite && !IsDiaryUnlocked(node.NodeID))
                {
                    OnProgressionError?.Invoke("Você precisa desbloquear este diário com convites.");
                    return false;
                }
            }

            if (HasReachedMaxCompletions(node))
            {
                OnProgressionError?.Invoke("Limite de conclusões atingido.");
                return false;
            }

            if (!CanAffordItemCosts(node))
            {
                OnProgressionError?.Invoke("Itens insuficientes.");
                return false;
            }

            if (node.EntryCost != null && node.EntryCost.EnergyCost > 0)
            {
                if (!CanAffordEnergy(node.EntryCost.EnergyCost))
                {
                    OnProgressionError?.Invoke("Energia Insuficiente.");
                    return false;
                }
                
                if (EnergyService.Instance != null)
                {
                    if (!EnergyService.Instance.TryConsumeEnergy(node.EntryCost.EnergyCost))
                    {
                        OnProgressionError?.Invoke("Erro ao consumir energia.");
                        return false;
                    }
                }
            }

            ConsumeItemCosts(node);

            node.Execute();
            return true;
        }

        public List<RewardDefinition> GetRewardsForNode(StoryNode node)
        {
            if (node == null || node.Rewards == null) return new List<RewardDefinition>();

            var rewards = new List<RewardDefinition>();

            // Base rewards
            if (IsNodeCompleted(node.NodeID))
            {
                if (node.Rewards.RepeatRewards != null)
                    rewards.AddRange(node.Rewards.RepeatRewards);
            }
            else
            {
                if (node.Rewards.FirstClearRewards != null)
                    rewards.AddRange(node.Rewards.FirstClearRewards);
            }

            // Milestone rewards
            int nextCompletions = GetCompletionCount(node) + 1;
            if (node.Rewards.MilestoneRewards != null)
            {
                foreach (var milestone in node.Rewards.MilestoneRewards)
                {
                    if (milestone.RequiredCompletions == nextCompletions && milestone.Rewards != null)
                    {
                        rewards.AddRange(milestone.Rewards);
                    }
                }
            }

            return rewards;
        }
    }
}
