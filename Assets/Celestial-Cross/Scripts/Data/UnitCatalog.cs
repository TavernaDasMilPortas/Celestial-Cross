using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitCatalog", menuName = "RPG/Unit Catalog")]
public class UnitCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string UnitID;
        public GameObject Prefab;
        public UnitData UnitData;
    }

    public List<Entry> Entries = new List<Entry>();

    public GameObject GetPrefab(string unitId)
    {
        if (string.IsNullOrWhiteSpace(unitId))
            return null;

        foreach (var entry in Entries)
        {
            if (entry == null) continue;
            if (string.Equals(entry.UnitID, unitId, StringComparison.OrdinalIgnoreCase))
                return entry.Prefab;
        }
        return null;
    }

    public UnitData GetUnitData(string unitId)
    {
        if (string.IsNullOrWhiteSpace(unitId))
            return null;

        foreach (var entry in Entries)
        {
            if (entry == null) continue;
            if (string.Equals(entry.UnitID, unitId, StringComparison.OrdinalIgnoreCase))
                return entry.UnitData;
        }
        return null;
    }
}
