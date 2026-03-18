using UnityEngine;

public interface IUnitAction
{
    string ActionName { get; }
    Sprite ActionIcon { get; }
    event System.Action<ActionForecast> OnForecastUpdated;
    void EnterAction();
    void UpdateAction();
    void Confirm();
    void Cancel();
}