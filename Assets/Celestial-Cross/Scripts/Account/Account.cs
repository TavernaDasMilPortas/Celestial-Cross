using System.Collections.Generic;
using CelestialCross.Artifacts;
using UnityEngine;

[System.Serializable]
public class ItemQuantity
{
    public string ItemID; // Pode ser ID de soul, po��o, fragmento, etc.
    public int Quantity;

    public ItemQuantity(string itemId, int quantity)
    {
        ItemID = itemId;
        Quantity = quantity;
    }
}

[System.Serializable]
public class Account
{
    public int Money;
    public int Energy;
    public int Stardust;
    public int StarMaps; // Moeda premium/invocao do gacha

    // Usaremos os IDs para referenciar os ScriptableObjects
    public List<string> OwnedUnitIDs = new List<string>(); // Legacy para retrocompatibilidade
    public List<CelestialCross.Data.RuntimeUnitData> OwnedUnits = new List<CelestialCross.Data.RuntimeUnitData>();
    
    public List<string> OwnedPetIDs = new List<string>();
    public List<CelestialCross.Data.Pets.RuntimePetData> OwnedRuntimePets = new List<CelestialCross.Data.Pets.RuntimePetData>();
    public List<ArtifactInstanceData> OwnedArtifacts = new List<ArtifactInstanceData>();
    public List<UnitLoadout> UnitLoadouts = new List<UnitLoadout>();
    
    [Header("Sistema de Gacha (Pity)")]
    public List<CelestialCross.Gacha.GachaPityState> GachaPityStates = new List<CelestialCross.Gacha.GachaPityState>();
    
    [Header("Progresso Hist\uFFFDria")]
    public List<string> CompletedNodeIDs = new List<string>();

    [Header("Itens Gerais e Consum�veis")]
    public List<ItemQuantity> OwnedItems = new List<ItemQuantity>();

    public Account()
    {
        Money = 100; // Valor inicial
        Energy = 50; // Valor inicial
        Stardust = 0; // Valor inicial configurado em 0
        StarMaps = 0;
        OwnedUnitIDs = new List<string>();
        OwnedUnits = new List<CelestialCross.Data.RuntimeUnitData>();
        OwnedPetIDs = new List<string>();
        OwnedRuntimePets = new List<CelestialCross.Data.Pets.RuntimePetData>();
        OwnedArtifacts = new List<ArtifactInstanceData>();
        UnitLoadouts = new List<UnitLoadout>();
        GachaPityStates = new List<CelestialCross.Gacha.GachaPityState>();
        CompletedNodeIDs = new List<string>();
        OwnedItems = new List<ItemQuantity>();
    }

    public void EnsureInitialized()
    {
        OwnedUnitIDs ??= new List<string>();
        OwnedUnits ??= new List<CelestialCross.Data.RuntimeUnitData>();
        OwnedPetIDs ??= new List<string>();
        OwnedRuntimePets ??= new List<CelestialCross.Data.Pets.RuntimePetData>();
        OwnedArtifacts ??= new List<ArtifactInstanceData>();
        UnitLoadouts ??= new List<UnitLoadout>();
        GachaPityStates ??= new List<CelestialCross.Gacha.GachaPityState>();
        CompletedNodeIDs ??= new List<string>();
        OwnedItems ??= new List<ItemQuantity>();

        // Migrar units legacy
        if (OwnedUnitIDs != null && OwnedUnitIDs.Count > 0 && OwnedUnits.Count == 0)
        {
            foreach (var id in OwnedUnitIDs)
            {
                OwnedUnits.Add(new CelestialCross.Data.RuntimeUnitData(id, 4)); // Assume 4 estrelas padro inicial
            }
        }
    }

    // --- M�TODOS AUXILIARES: ITEMS ---
    public void AddItem(string itemId, int amount)
    {
        EnsureInitialized();
        var item = OwnedItems.Find(i => i.ItemID == itemId);
        if (item != null)
        {
            item.Quantity += amount;
        }
        else
        {
            OwnedItems.Add(new ItemQuantity(itemId, amount));
        }
    }

    public int GetItemCount(string itemId)
    {
        EnsureInitialized();
        var item = OwnedItems.Find(i => i.ItemID == itemId);
        return item != null ? item.Quantity : 0;
    }

    public bool RemoveItem(string itemId, int amount)
    {
        EnsureInitialized();
        var item = OwnedItems.Find(i => i.ItemID == itemId);
        if (item != null && item.Quantity >= amount)
        {
            item.Quantity -= amount;
            if (item.Quantity <= 0)
                OwnedItems.Remove(item);
            return true;
        }
        return false;
    }
    // ---------------------------------

    public UnitLoadout GetLoadoutForUnit(string unitID)
    {
        EnsureInitialized();

        foreach (var loadout in UnitLoadouts)
        {
            if (loadout.UnitID == unitID)
                return loadout;
        }
        
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

    public CelestialCross.Data.Pets.RuntimePetData GetPetByUUID(string uuid)
    {
        EnsureInitialized();

        if (string.IsNullOrEmpty(uuid) || OwnedRuntimePets == null)
            return null;

        for (int i = 0; i < OwnedRuntimePets.Count; i++)
        {
            var p = OwnedRuntimePets[i];
            if (p != null && p.UUID == uuid)
                return p;
        }

        return null;
    }

    public bool IsPetEquipped(string petId)
    {
        if (string.IsNullOrEmpty(petId)) return false;
        EnsureInitialized();
        foreach (var loadout in UnitLoadouts)
        {
            if (loadout.PetID == petId) return true;
        }
        return false;
    }

    public bool IsArtifactEquipped(string artifactGuid)
    {
        if (string.IsNullOrEmpty(artifactGuid)) return false;
        EnsureInitialized();
        foreach (var loadout in UnitLoadouts)
        {
            if (loadout.HelmetID == artifactGuid || loadout.ChestplateID == artifactGuid ||
                loadout.GlovesID == artifactGuid || loadout.BootsID == artifactGuid ||
                loadout.NecklaceID == artifactGuid || loadout.RingID == artifactGuid) return true;
        }
        return false;
    }

    public void UnequipPetFromAll(string petId)
    {
        if (string.IsNullOrEmpty(petId)) return;
        EnsureInitialized();
        foreach (var loadout in UnitLoadouts)
        {
            if (loadout.PetID == petId)
            {
                loadout.PetID = string.Empty;
            }
        }
    }

    public void UnequipArtifactFromAll(string artifactGuid)
    {
        if (string.IsNullOrEmpty(artifactGuid)) return;
        EnsureInitialized();
        foreach (var loadout in UnitLoadouts)
        {
            if (loadout.HelmetID == artifactGuid) loadout.HelmetID = string.Empty;
            if (loadout.ChestplateID == artifactGuid) loadout.ChestplateID = string.Empty;
            if (loadout.GlovesID == artifactGuid) loadout.GlovesID = string.Empty;
            if (loadout.BootsID == artifactGuid) loadout.BootsID = string.Empty;
            if (loadout.NecklaceID == artifactGuid) loadout.NecklaceID = string.Empty;
            if (loadout.RingID == artifactGuid) loadout.RingID = string.Empty;
        }
    }
}
