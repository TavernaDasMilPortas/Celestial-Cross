using System.Collections.Generic;
using CelestialCross.Artifacts;
using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactSetCatalog", menuName = "Celestial Cross/Artifacts/Artifact Set Catalog")]
public class ArtifactSetCatalog : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public ArtifactSet set;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    public ArtifactSet GetSetById(string setId)
    {
        if (string.IsNullOrWhiteSpace(setId))
            return null;

        for (int i = 0; i < entries.Count; i++)
        {
            var set = entries[i]?.set;
            if (set == null)
                continue;

            if (set.id == setId)
                return set;
        }

        return null;
    }

    public List<ArtifactSet> GetAllSets()
    {
        var result = new List<ArtifactSet>();
        for (int i = 0; i < entries.Count; i++)
        {
            var set = entries[i]?.set;
            if (set != null)
                result.Add(set);
        }
        return result;
    }
}
