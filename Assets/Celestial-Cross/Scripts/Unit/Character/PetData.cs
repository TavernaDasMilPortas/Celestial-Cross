using UnityEngine;

[CreateAssetMenu(menuName = "Units/Pet Data")]
public class PetData : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public CombatStats baseStats = new CombatStats(5, 2, 0, 1, 10, 0);
    public AbilityData ability;
}
