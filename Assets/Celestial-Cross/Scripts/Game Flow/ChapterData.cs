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

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Gera um ID para o capítulo se estiver vazio
            if (string.IsNullOrEmpty(ChapterID))
            {
                ChapterID = global::System.Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(this);
            }

            // Gera IDs únicos para todos os nós dentro do capítulo se estiverem vazios
            if (Nodes != null)
            {
                bool isDirty = false;
                foreach (var node in Nodes)
                {
                    if (node != null && string.IsNullOrEmpty(node.NodeID))
                    {
                        node.NodeID = global::System.Guid.NewGuid().ToString();
                        isDirty = true;
                    }
                }
                if (isDirty)
                {
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }
#endif
    }
}