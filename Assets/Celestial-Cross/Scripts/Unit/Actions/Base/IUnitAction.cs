using UnityEngine;

public interface IUnitAction
{
    string ActionName { get; }
    Sprite ActionIcon { get; }
    string ActionDescription { get; }
    int Range { get; }
    int Level { get; set; }
    Vector2Int Target { get; set; }
    AreaPatternData GetAreaPattern();
    event System.Action<ActionForecast> OnForecastUpdated;
    void EnterAction();
    void UpdateAction();
    void Confirm();
    void Cancel();
    string GetDetailStats();
}