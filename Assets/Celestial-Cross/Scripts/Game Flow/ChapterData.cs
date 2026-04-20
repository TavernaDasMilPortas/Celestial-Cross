using System.Collections.Generic;
using UnityEngine;

namespace CelestialCross.Progression
{
    [CreateAssetMenu(fileName = "NewChapter", menuName = "Celestial Cross/Progression/Chapter")]
    public class ChapterData : ScriptableObject
    {
        [Header("Chapter Info")]
        public string ChapterID;
        public string ChapterTitle;
        [TextArea] public string Description;

        [Header("Gating")]
        public ChapterData RequiredChapter;
        [Tooltip("ID da unidade necessária para este capítulo ser visível (usado em Diários)")]
        public string RequiredUnitID;
        public bool IsDiaryChapter = false;

        [Header("Flow")]
        [SerializeReference]
        public List<StoryNode> Nodes = new List<StoryNode>();

        public bool IsLocked(HashSet<string> completedNodes, List<string> ownedUnits)
        {
            // Verificar capítulo anterior
            if (RequiredChapter != null)
            {
                // Se o último nó do capítulo anterior não estiver completo, este está trancado
                // (Assumindo que o capítulo anterior precisa ser totalmente terminado)
                // Por simplicidade, verificaremos se o RequiredID do último nó do anterior está na lista.
            }

            // Verificar requisito de unidade (para Diários)
            if (!string.IsNullOrEmpty(RequiredUnitID))
            {
                if (ownedUnits == null || !ownedUnits.Contains(RequiredUnitID))
                {
                    return true;
                }
            }

            return false;
        }
    }
}