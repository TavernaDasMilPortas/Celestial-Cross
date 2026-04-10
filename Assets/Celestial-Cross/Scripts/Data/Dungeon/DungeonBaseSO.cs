using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Artifacts;

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
        public ArtifactDropMatrix DropMatrix;
        
        [Tooltip("Quantidade de artefatos sorteados ao concluir.")]
        public int ArtifactsToDrop = 1;
    }

    [CreateAssetMenu(fileName = "NewDungeonBase", menuName = "RPG/Dungeon/Dungeon Base")]
    public class DungeonBaseSO : ScriptableObject
    {
        [Header("Dungeon Info")]
        public string DungeonName;
        [TextArea] public string Description;

        [Header("Loot Pool Global da Dungeon")]
        [Tooltip("Quais conjuntos de artefatos podem dropar em qualquer fase desta masmorra?")]
        public List<ArtifactSet> AllowedArtifactSets;

        [Header("Níveis (Fases)")]
        public List<DungeonLevelNode> Levels;
    }
}
