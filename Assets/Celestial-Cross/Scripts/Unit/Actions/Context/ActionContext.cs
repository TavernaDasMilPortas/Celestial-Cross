using System.Collections.Generic;
using UnityEngine;

public class ActionContext
{
    public Unit source;
    public List<Unit> targets = new();
    public List<Vector2Int> targetPoints = new();
    public List<Vector2Int> affectedAreaCells = new();

    public ActionContext(Unit source)
    {
        this.source = source;
    }
}
