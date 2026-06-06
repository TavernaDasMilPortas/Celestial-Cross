using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Progression;

namespace CelestialCross.Scenes.Hub
{
    [CreateAssetMenu(fileName = "NewHubCategory", menuName = "Celestial Cross/Hub/Hub Category")]
    public class HubCategorySO : ScriptableObject
    {
        public string CategoryName;
        public HubCategoryType CategoryType = HubCategoryType.Story;
        [Tooltip("Used if CategoryType is Incursion or future custom types.")]
        public string CustomTag;
        public Sprite Icon;
        [TextArea] public string Description;

        [Tooltip("List of chapters or ruins that belong to this category.")]
        public List<ChapterData> Chapters = new List<ChapterData>();

        // Returns (completedNodes, totalNodes) across all chapters
        public (int, int) GetProgress(global::Account account)
        {
            if (account == null || Chapters == null) return (0, 0);

            int total = 0;
            int completed = 0;

            var completedNodes = new HashSet<string>(account.CompletedNodeIDs ?? new List<string>());

            foreach (var chapter in Chapters)
            {
                if (chapter == null || chapter.Nodes == null) continue;
                total += chapter.Nodes.Count;
                foreach (var node in chapter.Nodes)
                {
                    if (node != null && completedNodes.Contains(node.NodeID))
                    {
                        completed++;
                    }
                }
            }

            return (completed, total);
        }
    }
}
