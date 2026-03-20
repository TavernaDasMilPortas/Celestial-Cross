using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Units/Unit Data")]
public class UnitData : ScriptableObject
{
    public string displayName;

    [Header("Stats")]
    public CombatStats baseStats = new CombatStats(30, 10, 6, 7, 7, 1);

    [Header("Abilities")]
    public List<AbilityData> characterAbilities = new();

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

    public List<AbilityData> GetCharacterAbilities() => characterAbilities;

    public AbilityData GetPetAbility(PetData equippedPet = null)
    {
        return equippedPet != null ? equippedPet.ability : null;
    }

    public IEnumerable<IExecutableDefinitionData> GetExecutableDefinitions(PetData equippedPet = null)
    {
        foreach (var action in nativeActions)
        {
            if (action != null)
                yield return action;
        }

        foreach (var ability in characterAbilities)
        {
            if (ability != null && ability.IsActive && ability.GetExecutableDefinition() != null)
                yield return ability.GetExecutableDefinition();
        }

        AbilityData petAbility = GetPetAbility(equippedPet);
        if (petAbility != null && petAbility.IsActive && petAbility.GetExecutableDefinition() != null)
            yield return petAbility.GetExecutableDefinition();
    }
}
