using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Cross/Config/Leveling Config")]
public class LevelingConfig : ScriptableObject
{
    public int baseXPPerLevel = 100;     // XP para level 2
    public float xpGrowthFactor = 1.15f; // Multiplicador por nível
    
    public int GetXPForLevel(int level) =>
        Mathf.RoundToInt(baseXPPerLevel * Mathf.Pow(xpGrowthFactor, level - 1));
}
