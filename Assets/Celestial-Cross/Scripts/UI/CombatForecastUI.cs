using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatForecastUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI critText;
    [SerializeField] private TextMeshProUGUI hitCountText;
    [SerializeField] private TextMeshProUGUI targetNameText;

    private IUnitAction currentAction;

    public void SetAction(IUnitAction action)
    {
        if (currentAction != null)
            currentAction.OnForecastUpdated -= HandleForecastUpdated;

        currentAction = action;

        if (currentAction != null)
            currentAction.OnForecastUpdated += HandleForecastUpdated;
            
        Hide();
    }

    private void HandleForecastUpdated(ActionForecast forecast)
    {
        if (forecast.Target == null)
        {
            Hide();
            return;
        }

        Show();

        if (targetNameText != null)
            targetNameText.text = forecast.Target.DisplayName;

        if (damageText != null)
            damageText.text = $"Damage: {forecast.Damage}";

        if (critText != null)
            critText.text = $"Crit: {forecast.CriticalChance}%";

        if (hitCountText != null)
            hitCountText.text = forecast.AttackCount > 1 ? $"x{forecast.AttackCount} Hits" : "";
    }

    public void Show() => panel?.SetActive(true);
    public void Hide() => panel?.SetActive(false);

    private void OnDestroy()
    {
        if (currentAction != null)
            currentAction.OnForecastUpdated -= HandleForecastUpdated;
    }
}
