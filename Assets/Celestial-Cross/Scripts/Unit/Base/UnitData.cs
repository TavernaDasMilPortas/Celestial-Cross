using UnityEngine;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Units/Unit Data")]
public class UnitData : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string unitID;
    public string UnitID => unitID;
    public string displayName;

    [Header("UI")]
    public Sprite icon;

    [Header("Stats")]
    public CombatStats baseStats = new CombatStats(30, 10, 6, 7, 7, 1);

    [Header("Abilities (Blueprints)")]
    [Tooltip("Lista de habilidades e passivas usando o novo sistema de Blueprints.")]
    public List<AbilityBlueprint> abilities = new();

    [Header("Actions (Native)")]
    [SerializeReference]
    public List<UnitActionData> nativeActions = new();

    public CombatStats GetCombinedStats(PetData equippedPet = null)
    {
        CombatStats total = baseStats;
        if (equippedPet != null)
            total += equippedPet.baseStats;
        return total;
    }

    public List<AbilityBlueprint> GetAbilities() => abilities;

    public AbilityBlueprint GetPetAbility(PetData equippedPet = null)
    {
        return equippedPet != null ? equippedPet.ability : null;
    }

    // Adaptado para Unit.cs - UnitActionContext se comunica com UnitActionData
    public IEnumerable<UnitActionData> GetExecutableDefinitions(PetData equippedPet = null)
    {
        foreach (var action in nativeActions)
        {
            if (action != null)
                yield return action;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        string assetPath = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrWhiteSpace(assetPath))
            return;

        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrWhiteSpace(guid) || unitID == guid)
            return;

        unitID = guid;
        EditorUtility.SetDirty(this);
    }
#endif
}

