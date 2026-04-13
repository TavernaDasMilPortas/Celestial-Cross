using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Data.Dungeon;

namespace CelestialCross.Data
{
    [CreateAssetMenu(fileName = "DungeonCatalog", menuName = "RPG/Dungeon/Dungeon Catalog")]
    public class DungeonCatalog : ScriptableObject
    {
        public List<DungeonBaseSO> Dungeons = new List<DungeonBaseSO>();

        public bool TryFindDungeonForLevel(LevelData levelData, out DungeonBaseSO foundDungeon, out DungeonLevelNode foundNode)
        {
            foundDungeon = null;
            foundNode = null;

            if (levelData == null) return false;

            foreach (var dungeon in Dungeons)
            {
                if (dungeon == null || dungeon.Levels == null) continue;

                foreach (var node in dungeon.Levels)
                {
                    if (node != null && node.LevelRef == levelData)
                    {
                        foundDungeon = dungeon;
                        foundNode = node;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
