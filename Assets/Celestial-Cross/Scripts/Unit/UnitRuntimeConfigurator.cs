using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Artifacts;

public class UnitRuntimeConfigurator : MonoBehaviour
{
    [SerializeField] private Unit unit;
    [SerializeField] private SpriteRenderer unitSpriteRenderer;

    [Header("Artifacts (Option B)")]
    [SerializeField] private ArtifactSetCatalog artifactSetCatalog;

    public void Initialize(UnitData unitData, PetData petData = null)
    {
        if (unit == null)
        {
            Debug.LogError("Unit component is not assigned in the UnitRuntimeConfigurator.", this);
            return;
        }

        // Assign the Scriptable Objects
        unit.unitData = unitData;
        unit.petData = petData;

        // Inject equipped artifacts from save-data BEFORE initializing (affects MaxHealth/Stats)
        TryConfigureArtifactsFromAccount(unitData);

        // Configure visual components, if a renderer is provided
        if (unitSpriteRenderer != null)
        {
            if (unitData != null && unitData.icon != null)
            {
                unitSpriteRenderer.sprite = unitData.icon;
            }
            else
            {
                Debug.LogWarning($"Sprite for UnitData '{unitData.name}' is not set.", this);
            }
        }

        // Here you can add more configuration logic as needed,
        // for example, setting up animator controllers, materials, etc.
        // For now, we'll just set the name for clarity in the hierarchy.
        gameObject.name = $"Unit_{unitData.name}";

        // Initialize the unit's internal state
        unit.Initialize();
    }

    private void TryConfigureArtifactsFromAccount(UnitData unitData)
    {
        if (unit == null || unitData == null) return;
        if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null) return;

        // UnitLoadout is keyed by UnitID
        var loadout = AccountManager.Instance.PlayerAccount.GetLoadoutForUnit(unitData.UnitID);
        if (loadout == null) return;

        var equipped = new List<ArtifactInstanceData>();
        var ids = loadout.GetEquippedArtifactIDs();
        for (int i = 0; i < ids.Count; i++)
        {
            var data = AccountManager.Instance.PlayerAccount.GetArtifactByGuid(ids[i]);
            if (data != null)
                equipped.Add(data);
        }

        unit.ConfigureArtifactsFromSaveData(equipped, artifactSetCatalog);
    }
}
