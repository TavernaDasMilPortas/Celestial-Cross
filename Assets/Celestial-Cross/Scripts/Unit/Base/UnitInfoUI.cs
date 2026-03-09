using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitInfoUI : MonoBehaviour
{
    public static UnitInfoUI Instance;

    [Header("UI Refs")]
    public TextMeshProUGUI nameText;
    public Image healthFill;
    public TextMeshProUGUI healthText;

    private Unit targetUnit;
    private Health targetHealth;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (targetUnit == null)
            return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(
            targetUnit.transform.position + Vector3.up * 1.5f
        );

        transform.position = screenPos;
    }

    // =============================
    // PUBLIC API
    // =============================

    public void Show(Unit unit)
    {
        targetUnit = unit;
        targetHealth = unit.Health;

        if (targetHealth == null)
        {
            Debug.LogWarning("Unit sem Health.");
            return;
        }

        nameText.text = unit.DisplayName;

        targetHealth.OnHealthChanged += UpdateHealth;
        UpdateHealth(targetHealth.CurrentHealth, targetHealth.MaxHealth);

        gameObject.SetActive(true);
    }

    public void Hide(Unit unit)
    {
        if (targetHealth != null)
            targetHealth.OnHealthChanged -= UpdateHealth;

        targetUnit = null;
        targetHealth = null;
        gameObject.SetActive(false);
    }

    // =============================
    // INTERNAL
    // =============================

    void UpdateHealth(int current, int max)
    {
        healthFill.fillAmount = (float)current / max;
        healthText.text = $"{current} / {max}";
    }
}
