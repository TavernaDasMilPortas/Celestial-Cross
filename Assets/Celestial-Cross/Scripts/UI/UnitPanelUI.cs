using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Image petIconImage;
    [SerializeField] private Image hpFillImage;

    private Unit currentUnit;

    public void UpdatePanel(Unit unit)
    {
        if (unit == null)
            return;

        // Desinscreve da unidade antiga
        if (currentUnit != null && currentUnit.Health != null)
        {
            currentUnit.Health.OnHealthChanged -= HandleHealthChanged;
        }

        currentUnit = unit;

        if (nameText != null)
            nameText.text = currentUnit.DisplayName;
            
        if (speedText != null)
            speedText.text = $"Spd: {currentUnit.Speed}";

        if (currentUnit.Health != null)
        {
            currentUnit.Health.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(currentUnit.Health.CurrentHealth, currentUnit.Health.MaxHealth);
        }

        if (petIconImage != null)
        {
            if (currentUnit.EquippedPet != null && currentUnit.EquippedPet.icon != null)
            {
                petIconImage.sprite = currentUnit.EquippedPet.icon;
                petIconImage.gameObject.SetActive(true);
            }
            else
            {
                petIconImage.gameObject.SetActive(false);
            }
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (hpText != null)
            hpText.text = $"HP: {current} / {max}";
            
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = (float)current / max;
        }
    }

    private void OnDestroy()
    {
        if (currentUnit != null && currentUnit.Health != null)
        {
            currentUnit.Health.OnHealthChanged -= HandleHealthChanged;
        }
    }
}
