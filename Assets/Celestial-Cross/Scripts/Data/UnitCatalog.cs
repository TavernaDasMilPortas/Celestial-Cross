using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitCatalog", menuName = "RPG/Unit Catalog")]
public class UnitCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [SerializeField, HideInInspector] private string unitID;
        public string UnitID => unitID;
        public GameObject Prefab;
        public UnitData UnitData;

        public void SyncUnitIDFromUnitData()
        {
            if (UnitData == null) return;
            if (string.IsNullOrWhiteSpace(UnitData.UnitID)) return;
            unitID = UnitData.UnitID;
        }
    }

    public List<Entry> Entries = new List<Entry>();

    private void OnValidate()
    {
        if (Entries == null) return;

        foreach (var entry in Entries)
        {
            if (entry == null) continue;
            entry.SyncUnitIDFromUnitData();
        }
    }

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
