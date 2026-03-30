using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AccountBootstrapConfig", menuName = "Account/Bootstrap Config")]
public class AccountBootstrapConfig : ScriptableObject
{
    public int StartingMoney = 100;
    public int StartingEnergy = 50;

    public List<UnitData> StartingUnits = new List<UnitData>();
    public List<PetData> StartingPets = new List<PetData>();
}
