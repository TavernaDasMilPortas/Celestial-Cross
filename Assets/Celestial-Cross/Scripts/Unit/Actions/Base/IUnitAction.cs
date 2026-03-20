using UnityEngine;

public interface IUnitAction
{
    string ActionName { get; }
    Sprite ActionIcon { get; }
    string ActionDescription { get; }
    int Range { get; }
    event System.Action<ActionForecast> OnForecastUpdated;
    void EnterAction();
    void UpdateAction();
    void Confirm();
    void Cancel();
    string GetDetailStats();
}