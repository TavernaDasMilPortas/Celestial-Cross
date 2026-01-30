using UnityEngine;
using System;

public class GridTile : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    public bool IsOccupied;

    [SerializeField] private Renderer tileRenderer;

    [Header("Colors")]
    [SerializeField] private Color baseColor = Color.gray;
    [SerializeField] private Color highlightColor = Color.green;
    [SerializeField] private Color selectedColor = Color.yellow;

    public event Action OnHighlight;
    public event Action OnClearHighlight;
    public event Action OnSelect;

    private MaterialPropertyBlock propertyBlock;

    // IDs possíveis de cor (compatibilidade total)
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private int activeColorProperty = -1;

    public void Init(Vector2Int pos)
    {
        GridPosition = pos;
        IsOccupied = false;

        EnsureRenderer();
        EnsurePropertyBlock();
        DetectColorProperty();

        OnHighlight = null;
        OnClearHighlight = null;
        OnSelect = null;

        OnHighlight += ApplyHighlight;
        OnClearHighlight += ClearHighlight;
        OnSelect += ApplySelected;

        Clear();
    }

    // =====================
    // API PÚBLICA
    // =====================

    public void Highlight()
    {
        ApplyColor(highlightColor);
    }

    public void Clear()
    {
        ApplyColor(baseColor);
    }

    public void Select()
    {
        ApplyColor(selectedColor);
    }

    // =====================
    // VISUAL
    // =====================

    void ApplyHighlight() => ApplyColor(highlightColor);
    void ApplySelected() => ApplyColor(selectedColor);
    void ClearHighlight() => ApplyColor(baseColor);

    void ApplyColor(Color color)
    {
        if (activeColorProperty == -1)
            return;

        tileRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(activeColorProperty, color);
        tileRenderer.SetPropertyBlock(propertyBlock);
    }

    // =====================
    // SETUP SEGURO
    // =====================

    void EnsureRenderer()
    {
        if (tileRenderer != null) return;

        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer == null)
            tileRenderer = GetComponentInChildren<Renderer>();

        if (tileRenderer == null)
            Debug.LogError($"[GridTile] Nenhum Renderer encontrado em {name}");
    }

    void EnsurePropertyBlock()
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
    }

    void DetectColorProperty()
    {
        if (tileRenderer == null || tileRenderer.sharedMaterial == null)
            return;

        var mat = tileRenderer.sharedMaterial;

        if (mat.HasProperty(BaseColorId))
        {
            activeColorProperty = BaseColorId;
            Debug.Log($"[GridTile] Usando _BaseColor em {name}");
        }
        else if (mat.HasProperty(ColorId))
        {
            activeColorProperty = ColorId;
            Debug.Log($"[GridTile] Usando _Color em {name}");
        }
        else
        {
            Debug.LogError(
                $"[GridTile] Shader do tile '{name}' não possui _Color nem _BaseColor"
            );
        }
    }
}
