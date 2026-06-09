using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelCatalog", menuName = "Celestial Cross/Catalogs/Level Catalog")]
public class LevelCatalog : ScriptableObject
{
    public List<LevelData> Levels = new List<LevelData>();
}
