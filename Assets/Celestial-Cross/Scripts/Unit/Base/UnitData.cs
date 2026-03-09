using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Units/Unit Data")]
public class UnitData : ScriptableObject
{
    public string displayName;

    [Header("Legacy Stats")]
    public int maxHealth;
    public int speed;

    [Header("Magical Girl Stats")]
    public CombatStats baseStats = new CombatStats(30, 10, 6, 7, 7, 1);
    public AbilityData characterAbility;
    public PetData defaultPet;

    [SerializeReference]
    public List<UnitActionData> actions = new();

    public CombatStats GetCombinedStats(PetData equippedPet = null)
    {
        PetData selectedPet = equippedPet != null ? equippedPet : defaultPet;
        CombatStats total = baseStats;

        if (selectedPet != null)
            total += selectedPet.baseStats;

        return total;
    }

    public AbilityData GetCharacterAbility() => characterAbility;

    public AbilityData GetPetAbility(PetData equippedPet = null)
    {
        PetData selectedPet = equippedPet != null ? equippedPet : defaultPet;
        return selectedPet != null ? selectedPet.ability : null;
    }

    public IEnumerable<IExecutableDefinitionData> GetExecutableDefinitions(PetData equippedPet = null)
    {
        foreach (var action in actions)
        {
            if (action != null)
                yield return action;
        }

        AbilityData charAbility = GetCharacterAbility();
        if (charAbility != null && charAbility.IsActive && charAbility.GetExecutableDefinition() != null)
            yield return charAbility.GetExecutableDefinition();

        AbilityData petAbility = GetPetAbility(equippedPet);
        if (petAbility != null && petAbility.IsActive && petAbility.GetExecutableDefinition() != null)
            yield return petAbility.GetExecutableDefinition();
    }
}
