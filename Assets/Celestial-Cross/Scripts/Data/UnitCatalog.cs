using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitCatalog", menuName = "RPG/Unit Catalog")]
public class UnitCatalog : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public UnitData unitData;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    public UnitData GetUnitData(string unitId)
    {
        if (string.IsNullOrWhiteSpace(unitId))
            return null;

        for (int i = 0; i < entries.Count; i++)
        {
            var data = entries[i]?.unitData;
            if (data == null)
                continue;

            if (data.UnitID == unitId)
                return data;
        }

        return null;
    }

    public List<UnitData> GetAllUnitData()
    {
        var result = new List<UnitData>();
        for (int i = 0; i < entries.Count; i++)
        {
            var data = entries[i]?.unitData;
            if (data != null)
                result.Add(data);
        }
        return result;
    }
}
