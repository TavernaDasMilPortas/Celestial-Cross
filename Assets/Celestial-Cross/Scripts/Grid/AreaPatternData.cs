using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Units/Area Pattern Data")]
public class AreaPatternData : ScriptableObject
{
    [Min(1)] public int width = 3;
    [Min(1)] public int height = 3;
    [Min(0)] public int originX = 1;
    [Min(0)] public int originY = 1;
    
    [Header("Rotation")]
    public bool canRotate;

    [ShowIf(nameof(canRotate))]
    public RotationType rotationType;

    [ShowIf(nameof(ShouldShowDiagonalPattern))]
    [InfoBox("Desenhe o padrão para a diagonal NORDESTE. O sistema rotacionará para as outras 3 diagonais.")]
    public List<AreaPatternRow> diagonalPattern = new();
    
    [ShowIf(nameof(ShouldShowDiagonalPattern))]
    public Vector2Int diagonalCenter;

    [SerializeField] private List<AreaPatternRow> rows = new();

    public IReadOnlyList<AreaPatternRow> Rows => rows;

    private bool ShouldShowDiagonalPattern()
    {
        return canRotate && rotationType == RotationType.EightDirections;
    }

    public bool IsActive(int x, int y)
    {
        if (y < 0 || y >= rows.Count)
            return false;

        if (x < 0 || x >= rows[y].cells.Count)
            return false;

        return rows[y].cells[x];
    }

    public void EnsureShape()
    {
        while (rows.Count < height)
            rows.Add(new AreaPatternRow(width));

        while (rows.Count > height)
            rows.RemoveAt(rows.Count - 1);

        foreach (var row in rows)
            row.Resize(width);

        if (ShouldShowDiagonalPattern())
        {
            while (diagonalPattern.Count < height)
                diagonalPattern.Add(new AreaPatternRow(width));

            while (diagonalPattern.Count > height)
                diagonalPattern.RemoveAt(diagonalPattern.Count - 1);

            foreach (var row in diagonalPattern)
                row.Resize(width);
        }

        originX = Mathf.Clamp(originX, 0, width - 1);
        originY = Mathf.Clamp(originY, 0, height - 1);
    }

    void OnValidate()
    {
        EnsureShape();
        EnsureDiagonalShape();
    }

    public void EnsureDiagonalShape()
    {
        if (diagonalPattern == null) diagonalPattern = new List<AreaPatternRow>();

        while (diagonalPattern.Count < height)
            diagonalPattern.Add(new AreaPatternRow(width));

        while (diagonalPattern.Count > height)
            diagonalPattern.RemoveAt(diagonalPattern.Count - 1);

        foreach (var row in diagonalPattern)
            row.Resize(width);
    }
}

public enum RotationType { FourDirections, EightDirections }

[Serializable]
public class AreaPatternRow
{
    public List<bool> cells = new();

    public AreaPatternRow(int width)
    {
        Resize(width);
    }

    public void Resize(int width)
    {
        while (cells.Count < width)
            cells.Add(false);

        while (cells.Count > width)
            cells.RemoveAt(cells.Count - 1);
    }
}
