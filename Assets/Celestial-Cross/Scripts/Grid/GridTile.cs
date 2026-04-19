using UnityEngine;
using System;

public class GridTile : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    public bool IsOccupied;
    public Unit OccupyingUnit;

    // ── Gameplay properties ──────────────────────────────────────────────────
    public bool IsPlayerSpawnZone { get; private set; }
    public bool IsWalkable { get; private set; } = true;

    // ── Visual renderers ─────────────────────────────────────────────────────
    [SerializeField] private Renderer tileRenderer;
    [SerializeField] private SpriteRenderer visualSpriteRenderer;

    [Header("Colors (Darken)")]
    [SerializeField] private Color baseColor = Color.gray;
    [SerializeField] private Color executionColor = new Color(0.15f, 0.15f, 0.15f, 1f);

    private MaterialPropertyBlock propertyBlock;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private int activeColorProperty = -1;

    // ── Visual state flags ───────────────────────────────────────────────────
    private bool isExecution = false;
    private bool isSelected = false;
    private bool isAreaCenter = false;
    private bool isAreaPreview = false;
    private bool isHighlight = false;

    // ─────────────────────────────────────────────────────────────────────────
    // INIT
    // ─────────────────────────────────────────────────────────────────────────

    public void Init(Vector2Int pos)
    {
        GridPosition = pos;
        IsOccupied = false;

        EnsureRenderer();
        EnsurePropertyBlock();
        DetectColorProperty();

        HardClearAllStates();
    }

    public void ApplyDefinition(TileDefinition definition)
    {
        if (definition == null) return;
        IsPlayerSpawnZone = definition.isPlayerSpawnZone;
        IsWalkable = definition.isWalkable;
    }

    /// <summary>
    /// Applies walkable state from PhaseMap Layer 2 (overrides the TileDefinition default).
    /// </summary>
    public void ApplyWalkableOverride(bool walkable)
    {
        IsWalkable = walkable;
    }

    /// <summary>
    /// Applies a visual sprite from PhaseMap Layer 3. Falls back to TileDefinition.defaultSprite if null.
    /// </summary>
    public void ApplySprite(Sprite sprite)
    {
        if (visualSpriteRenderer == null) return;
        
        visualSpriteRenderer.sprite = sprite;
        visualSpriteRenderer.gameObject.SetActive(sprite != null);

        // Ensure a tiny offset to avoid Z-fighting with the 3D mesh face
        if (sprite != null)
        {
            var p = visualSpriteRenderer.transform.localPosition;
            if (p.y <= 0.001f) p.y = 0.505f; // Standard cube top
            visualSpriteRenderer.transform.localPosition = p;
        }
    }

    public void AddSpriteLayer(Sprite sprite, int layerIndex)
    {
        if (visualSpriteRenderer == null || sprite == null) return;
        
        GameObject overlay = new GameObject($"SpriteLayer_{layerIndex}");
        overlay.transform.SetParent(visualSpriteRenderer.transform.parent);
        
        // Match the base visual sprite transform exactly
        overlay.transform.localPosition = visualSpriteRenderer.transform.localPosition + new Vector3(0, 0.001f * layerIndex, 0); // Reduced Y distance
        overlay.transform.localRotation = visualSpriteRenderer.transform.localRotation;
        overlay.transform.localScale = visualSpriteRenderer.transform.localScale;

        SpriteRenderer sr = overlay.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = layerIndex; // Ensure correct drawing order
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUBLIC API — Visual State Flags
    // ─────────────────────────────────────────────────────────────────────────

    public bool IsHighlighted => isHighlight;
    public bool IsSelected => isSelected;
    public bool IsAreaPreview => isAreaPreview;
    public bool IsAreaCenter => isAreaCenter;

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

    // ─────────────────────────────────────────────────────────────────────────
    // VISUAL UPDATE — Only Darken uses MaterialPropertyBlock now.
    // Highlight/Select/Area overlays are handled by HighlightOverlayPool (Feature B).
    // ─────────────────────────────────────────────────────────────────────────

    void UpdateVisuals()
    {
        // Only the execution darken affects the base tile material.
        if (isExecution)
            ApplyColor(executionColor);
        else
            ApplyColor(baseColor);
            
        GridMap.Instance?.MarkHighlightsDirty();
    }

    void ApplyColor(Color color)
    {
        if (activeColorProperty == -1) return;

        tileRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(activeColorProperty, color);
        tileRenderer.SetPropertyBlock(propertyBlock);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SETUP HELPERS
    // ─────────────────────────────────────────────────────────────────────────

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
        if (tileRenderer == null || tileRenderer.sharedMaterial == null) return;

        var mat = tileRenderer.sharedMaterial;

        if (mat.HasProperty(BaseColorId))
            activeColorProperty = BaseColorId;
        else if (mat.HasProperty(ColorId))
            activeColorProperty = ColorId;
        else
            Debug.LogError($"[GridTile] Shader '{name}' não possui _Color nem _BaseColor");
    }
}
