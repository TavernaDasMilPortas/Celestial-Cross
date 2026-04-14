using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Data.Pets;

[CreateAssetMenu(fileName = "PetCatalog", menuName = "RPG/Pet Catalog")]
public class PetCatalog : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public PetSpeciesSO petSpecies;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    public PetSpeciesSO GetPetSpecies(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        for (int i = 0; i < entries.Count; i++)
        {
            var data = entries[i]?.petSpecies;
            if (data == null)
                continue;

            if (data.id == id)
                return data;
        }

        return null;
    }

    public List<PetSpeciesSO> GetAllPetSpecies()
    {
        var result = new List<PetSpeciesSO>();
        for (int i = 0; i < entries.Count; i++)
        {
            var data = entries[i]?.petSpecies;
            if (data != null)
                result.Add(data);
        }
        return result;
    }
}
