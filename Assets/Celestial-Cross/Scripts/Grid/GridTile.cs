using UnityEngine;
using System;

public class GridTile : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    public bool IsOccupied;

    [SerializeField] private Renderer tileRenderer;

    [Header("Colors")]
    [SerializeField] private Color baseColor = Color.gray;
    [SerializeField] private Color executionColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color highlightColor = Color.green;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color areaPreviewColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private Color areaCenterColor = new Color(0.8f, 0.2f, 0f, 1f);

    public event Action OnHighlight;
    public event Action OnClearHighlight;
    public event Action OnSelect;

    private MaterialPropertyBlock propertyBlock;

    // IDs possíveis de cor (compatibilidade total)
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private int activeColorProperty = -1;

    // Estado visual empilhado (prioridades)
    private bool isExecution = false;
    private bool isSelected = false;
    private bool isAreaCenter = false;
    private bool isAreaPreview = false;
    private bool isHighlight = false;

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

        HardClearAllStates();
    }

    // =====================
    // API PÚBLICA (Flags)
    // =====================

    public void Highlight()
    {
        isHighlight = true;
        UpdateVisuals();
    }

    public void Clear()
    {
        isHighlight = false;
        UpdateVisuals();
    }

    public void Select()
    {
        isSelected = true;
        UpdateVisuals();
    }

    public void ClearSelect()
    {
        isSelected = false;
        UpdateVisuals();
    }

    public void PreviewArea()
    {
        isAreaPreview = true;
        UpdateVisuals();
    }

    public void ClearAreaPreview()
    {
        isAreaPreview = false;
        UpdateVisuals();
    }

    public void SetAreaCenter(bool state)
    {
        isAreaCenter = state;
        UpdateVisuals();
    }

    public void Darken()
    {
        isExecution = true;
        UpdateVisuals();
    }

    public void ClearDarken()
    {
        isExecution = false;
        UpdateVisuals();
    }
    
    public void HardClearAllStates()
    {
        isExecution = false;
        isSelected = false;
        isAreaCenter = false;
        isAreaPreview = false;
        isHighlight = false;
        UpdateVisuals();
    }

    // =====================
    // EVENT CALLBACKS (Mantidos para compatibilidade, caso usados externamente)
    // =====================

    void ApplyHighlight() => Highlight();
    void ApplySelected() => Select();
    void ClearHighlight() => Clear();

    // =====================
    // VISUAL UPDATE LOGIC
    // =====================

    void UpdateVisuals()
    {
        if (isExecution) ApplyColor(executionColor);
        else if (isSelected) ApplyColor(selectedColor);
        else if (isAreaCenter) ApplyColor(areaCenterColor);
        else if (isAreaPreview) ApplyColor(areaPreviewColor);
        else if (isHighlight) ApplyColor(highlightColor);
        else ApplyColor(baseColor);
    }

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
