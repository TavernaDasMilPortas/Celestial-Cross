using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AccountProfile", menuName = "Account/Profile")]
public class AccountProfile : ScriptableObject
{
    public int Money = 100;
    public int Energy = 50;

    public List<UnitData> OwnedUnits = new List<UnitData>();
    public List<PetData> OwnedPets = new List<PetData>();
}
