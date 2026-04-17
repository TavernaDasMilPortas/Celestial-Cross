using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Celestial Cross/Levels/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string LevelName;
    public string SceneName; // Nome da cena a ser carregada

    [Header("Grid")]
    public PhaseMap PhaseMap;

    [Header("Waves")]
    [Tooltip("Se preenchido, a Wave 0 é usada como spawn inicial. (Suporte a múltiplas waves/ondas.)")]
    public List<EnemyWave> Waves = new();

    [Header("Enemies (Legacy)")]
    [Tooltip("Legacy: use apenas se Waves estiver vazio. Mantido para não quebrar assets antigos.")]
    public List<EnemySpawnInfo> Enemies;

    [Header("Rewards")]
    public RewardPackage VictoryRewards;
}

[System.Serializable]
public class EnemyWave
{
    public string WaveName;
    public List<EnemySpawnInfo> Enemies = new();
}

[System.Serializable]
public struct EnemySpawnInfo
{
    public UnitData UnitData;
    public Vector2Int GridPosition;
    
    [Tooltip("Se preenchido, substitui o perfil de IA padrão desta unidade (útil para chefes ou fases específicas).")]
    public AIBehaviorProfile OverrideBehaviorProfile;

    [Tooltip("Opcional: Define um Padrão de Fases / Gatilhos (AIPatternData). Suplanta o BehaviorProfile normal se possuir fases.")]
    public AIPatternData OverridePatternData;
}
