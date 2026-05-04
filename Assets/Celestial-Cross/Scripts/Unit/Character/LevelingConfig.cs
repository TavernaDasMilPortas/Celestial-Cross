using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Celestial Cross/Config/Leveling Config")]
public class LevelingConfig : ScriptableObject
{
    public static LevelingConfig Instance { get; private set; }

    private void OnEnable()
    {
        Instance = this;
    }
    [Header("XP Curve Settings")]
    public int baseXPForLevel2 = 100;     // XP base para sair do level 1 para o 2
    public float xpGrowthFactor = 1.15f;  // Multiplicador exponencial por nível (ex: 1.15 = +15% a cada nível)
    
    [Header("Level Limits by Stars")]
    [Tooltip("Define o nível máximo permitido baseado na quantidade de estrelas da unidade (Índice 0 = 1 estrela, etc)")]
    public int[] maxLevelsByStars = new int[6] { 20, 30, 40, 50, 60, 70 };

    [Header("Global Settings")]
    public int globalMaxLevel = 100;      // Teto absoluto de nível no jogo
    public int defaultMinLevel = 1;       // Nível inicial de uma unidade nova
    public int initialXP = 0;             // XP inicial de uma unidade nova

    /// <summary>
    /// Calcula quanto de XP é necessário para passar do nível atual para o próximo.
    /// </summary>
    public int GetXPForNextLevel(int currentLevel)
    {
        if (currentLevel < 1) return baseXPForLevel2;
        if (currentLevel >= globalMaxLevel) return 0;

        // Fórmula: Base * (Factor ^ (Level - 1))
        return Mathf.RoundToInt(baseXPForLevel2 * Mathf.Pow(xpGrowthFactor, currentLevel - 1));
    }

    /// <summary>
    /// Calcula o XP acumulado necessário para atingir um nível específico.
    /// </summary>
    public int GetTotalXPForLevel(int targetLevel)
    {
        int total = 0;
        for (int i = 1; i < targetLevel; i++)
        {
            total += GetXPForNextLevel(i);
        }
        return total;
    }

    /// <summary>
    /// Retorna o nível máximo baseado nas estrelas da unidade.
    /// </summary>
    public int GetMaxLevelForStars(int stars)
    {
        int index = Mathf.Clamp(stars - 1, 0, maxLevelsByStars.Length - 1);
        int starMax = maxLevelsByStars[index];
        return Mathf.Min(starMax, globalMaxLevel);
    }
}
