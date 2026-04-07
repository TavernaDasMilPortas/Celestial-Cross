using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Artifacts;

using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Artifacts;

[System.Serializable]
public class UnitLoadout
{
    public string UnitID;
    
    [Header("Pet")]
    public string PetID; // Slot de Pet

    [Header("Artifact Instance GUIDs")]
    public string HelmetID;     // Slot 1
    public string ChestplateID; // Slot 2
    public string GlovesID;     // Slot 3
    public string BootsID;      // Slot 4
    public string NecklaceID;   // Slot 5
    public string RingID;       // Slot 6

    public UnitLoadout() {}

    public UnitLoadout(string unitID)
    {
        UnitID = unitID;
    }

    // Recupera uma lista apenas com os GUIDs que não estão nulos/vazios
    public List<string> GetEquippedArtifactIDs()
    {
        List<string> ids = new List<string>();
        if (!string.IsNullOrEmpty(HelmetID)) ids.Add(HelmetID);
        if (!string.IsNullOrEmpty(ChestplateID)) ids.Add(ChestplateID);
        if (!string.IsNullOrEmpty(GlovesID)) ids.Add(GlovesID);
        if (!string.IsNullOrEmpty(BootsID)) ids.Add(BootsID);
        if (!string.IsNullOrEmpty(NecklaceID)) ids.Add(NecklaceID);
        if (!string.IsNullOrEmpty(RingID)) ids.Add(RingID);
        return ids;
    }
}
