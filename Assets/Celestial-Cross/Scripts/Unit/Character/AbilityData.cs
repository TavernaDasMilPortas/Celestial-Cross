using UnityEngine;

[CreateAssetMenu(menuName = "Units/Ability Data")]
public class AbilityData : ScriptableObject
{
    public string abilityName;
    [TextArea(2, 6)] public string description;
}
