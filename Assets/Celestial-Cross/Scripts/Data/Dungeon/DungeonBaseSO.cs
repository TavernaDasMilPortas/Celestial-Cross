using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Artifacts;
using CelestialCross.Data.Loot;

namespace CelestialCross.Data.Dungeon
{
    [global::System.Serializable]
    public class ArtifactDropMatrix
    {
        [Header("Chances de Raridade (%)")]
        public float commonChance = 50f;
        public float uncommonChance = 30f;
        public float rareChance = 15f;
        public float epicChance = 4f;
        public float legendaryChance = 1f;

        [Header("Chances de Estrelas (%)")]
        public float oneStarChance = 60f;
        public float twoStarChance = 30f;
        public float threeStarChance = 10f;
        public float fourStarChance = 0f;
        public float fiveStarChance = 0f;
    }

    [global::System.Serializable]
    public class DungeonLevelNode
    {
        public LevelData LevelRef;
        
        [Header("NOVO: Loot Procedural Flex�vel")]
        [Tooltip("Tabelas de Drop espec�ficas para ESTE ANDAR. Estas tabelas rodar�o ao completar esta fase.")]
        [SerializeReference]
        public List<BaseLootTable> SpecificLootTables = new List<BaseLootTable>();
    }

    [CreateAssetMenu(fileName = "NewDungeonBase", menuName = "Celestial Cross/Levels/Dungeon Base")]
    public class DungeonBaseSO : ScriptableObject
    {
        [Header("Dungeon Info")]
        public string DungeonName;
        [TextArea] public string Description;
        
        [Header("Gating")]
        [Tooltip("ID do Node da Hist\uFFFDria necess\uFFFDrio para desbloquear esta masmorra")]
        public string RequiredNodeID;

        [Header("Drop System (GENERICO)")]
        [Tooltip("Estas tabelas de loot globais rodam sempre que voc� vence QUALQUER fase desta masmorra.")]
        [SerializeReference]
        public List<BaseLootTable> GlobalLootTables = new List<BaseLootTable>();

        [Header("N�veis (Fases)")]
        public List<DungeonLevelNode> Levels;
    }
}
