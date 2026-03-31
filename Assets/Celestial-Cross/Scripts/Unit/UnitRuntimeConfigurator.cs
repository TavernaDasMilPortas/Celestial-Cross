using UnityEngine;

public class UnitRuntimeConfigurator : MonoBehaviour
{
    [SerializeField] private Unit unit;
    [SerializeField] private SpriteRenderer unitSpriteRenderer;

    public void Initialize(UnitData unitData, PetData petData = null)
    {
        if (unit == null)
        {
            Debug.LogError("Unit component is not assigned in the UnitRuntimeConfigurator.", this);
            return;
        }

        if (unitSpriteRenderer == null)
        {
            Debug.LogError("Unit Sprite Renderer is not assigned in the UnitRuntimeConfigurator.", this);
            return;
        }

        // Assign the Scriptable Objects
        unit.unitData = unitData;
        unit.petData = petData;

        // Configure visual components
        if (unitData != null && unitData.icon != null)
        {
            unitSpriteRenderer.sprite = unitData.icon;
        }
        else
        {
            Debug.LogWarning($"Sprite for UnitData '{unitData.name}' is not set.", this);
        }

        // Here you can add more configuration logic as needed,
        // for example, setting up animator controllers, materials, etc.
        // For now, we'll just set the name for clarity in the hierarchy.
        gameObject.name = $"Unit_{unitData.name}";

        // Initialize the unit's internal state
        unit.Initialize();
    }
}
