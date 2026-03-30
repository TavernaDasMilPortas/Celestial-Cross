using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelCatalog", menuName = "RPG/Level Catalog")]
public class LevelCatalog : ScriptableObject
{
    public List<LevelData> Levels = new List<LevelData>();
}
