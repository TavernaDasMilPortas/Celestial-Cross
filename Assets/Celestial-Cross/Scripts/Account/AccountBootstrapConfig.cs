using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Data.Pets;

[CreateAssetMenu(fileName = "AccountBootstrapConfig", menuName = "Account/Bootstrap Config")]
public class AccountBootstrapConfig : ScriptableObject
{
    public int StartingMoney = 100;
    public int StartingEnergy = 50;

    [Header("Initial Setup")]
    [Tooltip("If false, the account will start with no pets. Useful for tutorial dungeons future design.")]
    public bool GrantStartingPets = true;

    public List<UnitData> StartingUnits = new List<UnitData>();
    public List<PetSpeciesSO> StartingPets = new List<PetSpeciesSO>();
}
