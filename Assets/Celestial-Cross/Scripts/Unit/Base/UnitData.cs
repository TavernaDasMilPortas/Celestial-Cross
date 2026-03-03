using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Units/Unit Data")]
public class UnitData : ScriptableObject
{
    public string displayName;
    public int maxHealth;
    public int speed;

    [SerializeReference]
    public List<UnitActionData> actions = new();
}
