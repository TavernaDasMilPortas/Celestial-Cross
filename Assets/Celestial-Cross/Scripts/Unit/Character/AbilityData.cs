using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Combat;

[CreateAssetMenu(menuName = "Units/Ability Data")]
public class AbilityData : ScriptableObject
{
    [Header("Common")]
    public string id;
    public string abilityName;
    [TextArea(2, 6)] public string description;
    public Sprite icon;
    public AbilityType abilityType = AbilityType.Passive;

    [Header("Weaver Passives (For Passive or Active abilities)")]
    public List<WeaverPassiveEntry> weaverPassives = new();

    [Header("Active Action (Only if Active type)")]
    public ActiveAbilityData active = new ActiveAbilityData();

    public bool IsPassive => abilityType == AbilityType.Passive;
    public bool IsActive => abilityType == AbilityType.Active;

    public IExecutableDefinitionData GetExecutableDefinition()
    {
        return IsActive ? active : null;
    }
}

[System.Serializable]
public class WeaverPassiveEntry
{
    public string entryName;
    public CombatHook trigger;
    
    [SerializeReference]
    public List<AbilityEffectBase> effects = new();
}
