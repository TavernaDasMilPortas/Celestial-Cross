using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Artifacts;

public class UnitRuntimeConfigurator : MonoBehaviour
{
    [SerializeField] private Unit unit;
    [SerializeField] private SpriteRenderer unitSpriteRenderer;

    [Header("Artifacts (Option B)")]
    [SerializeField] private ArtifactSetCatalog artifactSetCatalog;

    public void Initialize(UnitData unitData, CelestialCross.Data.Pets.RuntimePetData runtimePetData = null, CelestialCross.Data.Pets.PetSpeciesSO petSpeciesData = null)
    {
        // Auto-detecção: se a referência serializada perdeu-se (ex: troca de Robot → EnemyUnit),
        // tenta resolver automaticamente antes de falhar.
        if (unit == null)
        {
            unit = GetComponent<Unit>();
            if (unit != null)
            {
                Debug.LogWarning($"[UnitRuntimeConfigurator] Campo 'unit' estava vazio — resolvido automaticamente para '{unit.GetType().Name}'. Atualize a referência no prefab para evitar este aviso.", this);
            }
            else
            {
                Debug.LogError("[UnitRuntimeConfigurator] Nenhum componente Unit encontrado neste GameObject. A injeção de dados não será feita.", this);
                return;
            }
        }

        if (unitSpriteRenderer == null)
        {
            unitSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (unitSpriteRenderer != null)
            {
                Debug.LogWarning($"[UnitRuntimeConfigurator] Campo 'unitSpriteRenderer' estava vazio — resolvido automaticamente via GetComponentInChildren. Atualize a referência no prefab.", this);
            }
        }

        // Assign the Scriptable Objects
        unit.unitData = unitData;
        unit.runtimePetData = runtimePetData;
        unit.petSpeciesData = petSpeciesData;

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

        var loadout = AccountManager.Instance.PlayerAccount.GetLoadoutForUnit(unitData.UnitID);
        if (loadout == null) return;

        if (!loadout.hasInitializedDefaultSkills && unitData.skillTreeConfig != null)
        {
            loadout.InitializeDefaults(unitData.skillTreeConfig);
            AccountManager.Instance.SaveAccount();
        }

        unit.ConfigureLoadout(loadout);

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
