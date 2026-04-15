using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Data.Pets;

[CreateAssetMenu(fileName = "AccountBootstrapConfig", menuName = "Celestial Cross/Account/Bootstrap Config")]
public class AccountBootstrapConfig : ScriptableObject
{
    public int StartingMoney = 100;
    public int StartingEnergy = 50;
    public int StartingStardust = 0;
    public int StartingStarMaps = 10; // Mapas das Estrelas Iniciais

    [Header("Initial Setup")]
    [Tooltip("If false, the account will start with no pets. Useful for tutorial dungeons future design.")]
    public bool GrantStartingPets = true;

    public List<UnitData> StartingUnits = new List<UnitData>();
    public List<PetSpeciesSO> StartingPets = new List<PetSpeciesSO>();
}
