using UnityEngine;

[CreateAssetMenu(menuName = "Units/Ability Data")]
public class AbilityData : ScriptableObject
{
    [Header("Common")]
    public string id;
    public string abilityName;
    [TextArea(2, 6)] public string description;
    public Sprite icon;
    public AbilityType abilityType = AbilityType.Passive;

    [Header("Passive")]
    public PassiveAbilityData passive = new PassiveAbilityData();

    [Header("Active")]
    public ActiveAbilityData active = new ActiveAbilityData();

    public bool IsPassive => abilityType == AbilityType.Passive;
    public bool IsActive => abilityType == AbilityType.Active;

    public IExecutableDefinitionData GetExecutableDefinition()
    {
        return IsActive ? active : null;
    }
}
