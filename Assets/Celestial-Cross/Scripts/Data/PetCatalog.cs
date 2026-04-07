using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PetCatalog", menuName = "RPG/Pet Catalog")]
public class PetCatalog : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public PetData petData;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    public PetData GetPetData(string petId)
    {
        if (string.IsNullOrWhiteSpace(petId))
            return null;

        for (int i = 0; i < entries.Count; i++)
        {
            var data = entries[i]?.petData;
            if (data == null)
                continue;

            if (data.PetID == petId)
                return data;
        }

        return null;
    }

    public List<PetData> GetAllPetData()
    {
        var result = new List<PetData>();
        for (int i = 0; i < entries.Count; i++)
        {
            var data = entries[i]?.petData;
            if (data != null)
                result.Add(data);
        }
        return result;
    }
}
