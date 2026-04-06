using System.Collections.Generic;
using CelestialCross.Artifacts;
using UnityEngine;

[System.Serializable]
public class Account
{
    public int Money;
    public int Energy;

    // Usaremos os IDs para referenciar os ScriptableObjects
    public List<string> OwnedUnitIDs = new List<string>();
    public List<string> OwnedPetIDs = new List<string>();
    public List<ArtifactInstanceData> OwnedArtifacts = new List<ArtifactInstanceData>();
    public List<UnitLoadout> UnitLoadouts = new List<UnitLoadout>();

    public Account()
    {
        Money = 100; // Valor inicial
        Energy = 50; // Valor inicial
        OwnedUnitIDs = new List<string>();
        OwnedPetIDs = new List<string>();
        OwnedArtifacts = new List<ArtifactInstanceData>();
        UnitLoadouts = new List<UnitLoadout>();
    }

    public void EnsureInitialized()
    {
        OwnedUnitIDs ??= new List<string>();
        OwnedPetIDs ??= new List<string>();
        OwnedArtifacts ??= new List<ArtifactInstanceData>();
        UnitLoadouts ??= new List<UnitLoadout>();
    }

    public UnitLoadout GetLoadoutForUnit(string unitID)
    {
        EnsureInitialized();

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

    public ArtifactInstanceData GetArtifactByGuid(string guid)
    {
        EnsureInitialized();

        if (string.IsNullOrEmpty(guid) || OwnedArtifacts == null)
            return null;

        for (int i = 0; i < OwnedArtifacts.Count; i++)
        {
            var artifact = OwnedArtifacts[i];
            if (artifact != null && artifact.idGUID == guid)
                return artifact;
        }

        return null;
    }
}
