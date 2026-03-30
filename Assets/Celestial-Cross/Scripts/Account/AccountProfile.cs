using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AccountProfile", menuName = "Account/Profile")]
public class AccountProfile : ScriptableObject
{
    public int Money = 100;
    public int Energy = 50;

    public List<string> OwnedUnitIDs = new List<string>();
    public List<string> OwnedPetIDs = new List<string>();
}
