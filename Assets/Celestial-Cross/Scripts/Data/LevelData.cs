using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "RPG/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string LevelName;
    public string SceneName; // Nome da cena a ser carregada

    [Header("Enemies")]
    public List<EnemySpawnInfo> Enemies;

    [Header("Rewards")]
    public RewardPackage VictoryRewards;
}

[System.Serializable]
public struct EnemySpawnInfo
{
    public UnitData UnitData;
    public Vector2Int GridPosition;
}
