using System.Collections.Generic;
using UnityEngine;

namespace CelestialCross.Progression
{
    [CreateAssetMenu(fileName = "ChapterCatalog", menuName = "Celestial Cross/Progression/Chapter Catalog")]
    public class ChapterCatalog : ScriptableObject
    {
        [Header("Main Story")]
        public List<ChapterData> mainStoryChapters = new List<ChapterData>();

        [Header("Diary Chapters")]
        public List<ChapterData> diaryChapters = new List<ChapterData>();

        /// <summary>
        /// Retorna todos os capítulos desbloqueados para um jogador.
        /// </summary>
        public List<ChapterData> GetUnlockedChapters(HashSet<string> completedNodes, List<string> ownedUnits)
        {
            List<ChapterData> unlocked = new List<ChapterData>();
            
            // Lógica para Main Story
            foreach (var chapter in mainStoryChapters)
            {
                if (!chapter.IsLocked(completedNodes, ownedUnits))
                    unlocked.Add(chapter);
            }

            // Lógica para Diários
            foreach (var chapter in diaryChapters)
            {
                if (!chapter.IsLocked(completedNodes, ownedUnits))
                    unlocked.Add(chapter);
            }

            return unlocked;
        }
    }
}