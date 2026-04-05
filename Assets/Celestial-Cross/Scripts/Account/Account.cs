using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Account
{
    public int Money;
    public int Energy;

    // Usaremos os IDs para referenciar os ScriptableObjects
    public List<string> OwnedUnitIDs = new List<string>();
    public List<string> OwnedPetIDs = new List<string>();
    public List<string> OwnedArtifactIDs = new List<string>(); // IDs (GUIDs) dos ArtifactInstances na conta
    public List<UnitLoadout> UnitLoadouts = new List<UnitLoadout>();

    public Account()
    {
        Money = 100; // Valor inicial
        Energy = 50; // Valor inicial
        OwnedUnitIDs = new List<string>();
        OwnedPetIDs = new List<string>();
        OwnedArtifactIDs = new List<string>();
        UnitLoadouts = new List<UnitLoadout>();
    }

    public UnitLoadout GetLoadoutForUnit(string unitID)
    {
        foreach (var loadout in UnitLoadouts)
        {
            if (loadout.UnitID == unitID)
                return loadout;
        }
        
        // Se ela não tem um ainda no Save, criamos um em branco para a lógica funcionar bem
        var newLoadout = new UnitLoadout(unitID);
        UnitLoadouts.Add(newLoadout);
        return newLoadout;
    }
}
