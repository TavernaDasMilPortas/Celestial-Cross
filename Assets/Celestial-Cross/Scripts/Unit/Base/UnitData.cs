using UnityEngine;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities;

[CreateAssetMenu(menuName = "Units/Unit Data")]
public class UnitData : ScriptableObject
{
    public string displayName;

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
}

