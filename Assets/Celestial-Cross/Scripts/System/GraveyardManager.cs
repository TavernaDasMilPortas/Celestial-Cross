using System.Collections.Generic;
using UnityEngine;

public class GraveyardManager : MonoBehaviour
{
    public static GraveyardManager Instance { get; private set; }

    private List<DeadUnitInfo> deadUnits = new List<DeadUnitInfo>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void AddDeadUnit(Unit unit)
    {
        DeadUnitInfo info = new DeadUnitInfo(unit.UnitData.UnitID, unit.transform.position);
        if (!deadUnits.Contains(info))
        {
            deadUnits.Add(info);
            Debug.Log($"Unidade {unit.UnitData.displayName} movida para o cemitério.");
        }
    }

    public List<DeadUnitInfo> GetDeadUnits()
    {
        return deadUnits;
    }
}

[System.Serializable]
public struct DeadUnitInfo
{
    public string UnitID;
    public Vector3 Position;

    public DeadUnitInfo(string unitID, Vector3 position)
    {
        UnitID = unitID;
        Position = position;
    }
}
