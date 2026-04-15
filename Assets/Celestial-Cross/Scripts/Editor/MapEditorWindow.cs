#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Map Editor Window — Celestial Cross
/// Provides a visual tileset painter for PhaseMap ScriptableObjects.
/// </summary>
public class MapEditorWindow : OdinEditorWindow
{
    // ─── Menu Item ────────────────────────────────────────────────────────────

    [MenuItem("Celestial Cross/Map Editor")]
    private static void OpenWindow()
    {
        var window = GetWindow<MapEditorWindow>("Map Editor");
        window.minSize = new Vector2(900, 600);
        window.Show();
    }

    // ─── Configuration ────────────────────────────────────────────────────────

    [BoxGroup("Setup", ShowLabel = false)]
    [HorizontalGroup("Setup/Row")]
    [AssetsOnly, Required]
    [LabelWidth(75)]
    [OnValueChanged(nameof(OnPhaseMapChanged))]
    public PhaseMap phaseMap;

    [HorizontalGroup("Setup/Row")]
    [AssetsOnly]
    [LabelWidth(75)]
    [OnValueChanged(nameof(RefreshTileDefList))]
    public TileDefinition[] tileLibrary = new TileDefinition[0];

    [HorizontalGroup("Setup/Row")]
    [LabelWidth(40), PropertyRange(1, 30)]
    [OnValueChanged(nameof(OnDimensionChanged))]
    public int width = 8;

    [HorizontalGroup("Setup/Row")]
    [LabelWidth(45), PropertyRange(1, 30)]
    [OnValueChanged(nameof(OnDimensionChanged))]
    public int height = 6;

    // ─── Layer Selection ──────────────────────────────────────────────────────

    private enum EditLayer { TileType = 0, Walkable = 1, Sprite = 2 }

    [BoxGroup("Layers", ShowLabel = false)]
    [HorizontalGroup("Layers/Row")]
    [LabelText("Active Layer"), LabelWidth(90)]
    [EnumToggleButtons]
    [ShowInInspector]
    [OnValueChanged(nameof(OnLayerChanged))]
    private EditLayer activeLayer = EditLayer.TileType;

    [HorizontalGroup("Layers/Row")]
    [LabelText("Erase Mode"), LabelWidth(80), ToggleLeft]
    [ShowInInspector]
    private bool eraseMode = false;

    // ─── Palette (Layer 1 & 3) ────────────────────────────────────────────────

    [BoxGroup("Palette")]
    [ShowIf(nameof(IsLayerTileType))]
    [HideLabel]
    [ShowInInspector]
    [OnValueChanged(nameof(OnTileDefSelected))]
    [ValueDropdown(nameof(GetTileDefDropdown))]
    private TileDefinition selectedTileDef;

    [FoldoutGroup("Palette/Manage Library", expanded: false)]
    [ShowIf(nameof(IsLayerSprite))]
    [ShowInInspector]
    [HideLabel]
    [InfoBox("Arraste sprites do Project aqui. Clique no grid abaixo para selecionar o ativo.", InfoMessageType.None)]
    [ListDrawerSettings(ShowPaging = false, ShowItemCount = true, ShowFoldout = false, DraggableItems = true)]
    private List<Sprite> spriteLibrary = new List<Sprite>();

    private Sprite selectedSprite;
    private int selectedSpriteIndex = -1;

    // ─── State ────────────────────────────────────────────────────────────────

    private Vector2 gridScrollPos;
    private Vector2 sidebarScrollPos;
    private Vector2 headerScrollPos;
    private float sidebarWidth = 260f;
    private float headerHeight = 240f;
    private bool isResizingSidebar;
    private bool isResizingHeader;
    private const float CellSize = 42f;
    private const float CellPadding = 1f;
    private const float ThumbSize = 52f;
    private const float ThumbPad = 4f;

    // ─── Callbacks ────────────────────────────────────────────────────────────

    private void OnPhaseMapChanged()
    {
        if (phaseMap == null) return;
        width = phaseMap.width;
        height = phaseMap.height;
        phaseMap.Resize(width, height);
        Repaint();
    }

    private void OnDimensionChanged()
    {
        if (phaseMap == null) return;
        Undo.RecordObject(phaseMap, "Resize PhaseMap");
        phaseMap.Resize(width, height);
        EditorUtility.SetDirty(phaseMap);
        Repaint();
    }

    private void OnLayerChanged() => Repaint();
    private void OnTileDefSelected() => Repaint();
    private void RefreshTileDefList() => Repaint();

    private IEnumerable<ValueDropdownItem<TileDefinition>> GetTileDefDropdown()
    {
        if (tileLibrary == null) yield break;
        foreach (var def in tileLibrary)
        {
            if (def == null) continue;
            yield return new ValueDropdownItem<TileDefinition>($"[{def.id}] {def.displayName}", def);
        }
    }

    // ─── Actions ──────────────────────────────────────────────────────────────

    [HorizontalGroup("Actions")]
    [Button(ButtonSizes.Large), GUIColor(0.3f, 0.8f, 0.3f)]
    private void SaveAsset()
    {
        if (phaseMap == null) return;
        EditorUtility.SetDirty(phaseMap);
        AssetDatabase.SaveAssets();
        Debug.Log($"[MapEditor] PhaseMap '{phaseMap.name}' salvo.");
    }

    [HorizontalGroup("Actions")]
    [Button(ButtonSizes.Large), GUIColor(0.8f, 0.4f, 0.2f)]
    private void ClearAll()
    {
        if (phaseMap == null) return;
        if (!EditorUtility.DisplayDialog("Limpar Mapa", "Isso apagará todos os dados das 3 camadas. Tem certeza?", "Sim, limpar", "Cancelar")) return;
        Undo.RecordObject(phaseMap, "Clear PhaseMap");
        phaseMap.Resize(phaseMap.width, phaseMap.height);
        EditorUtility.SetDirty(phaseMap);
        Repaint();
    }

    // ─── HYBRID OnImGUI ───────────────────────────────────────────────────────────

    protected override void OnImGUI()
    {
        Event e = Event.current;

        // 1. HEADER (Odin - Auto Layout)
        headerScrollPos = EditorGUILayout.BeginScrollView(headerScrollPos, GUILayout.Height(headerHeight));
        base.OnImGUI(); 
        EditorGUILayout.EndScrollView();

        // 2. VERTICAL RESIZE HANDLE
        Rect vHandleRect = GUILayoutUtility.GetRect(position.width, 5f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(vHandleRect, new Color(0.12f, 0.12f, 0.12f));
        EditorGUIUtility.AddCursorRect(vHandleRect, MouseCursor.ResizeVertical);
        if (e.type == EventType.MouseDown && vHandleRect.Contains(e.mousePosition)) isResizingHeader = true;
        if (isResizingHeader && e.rawType == EventType.MouseUp) isResizingHeader = false;
        if (isResizingHeader && e.type == EventType.MouseDrag)
        {
            headerHeight += e.delta.y;
            headerHeight = Mathf.Clamp(headerHeight, 100f, position.height - 150f);
            Repaint();
        }

        if (phaseMap == null) return;

        // 3. PAINTER AREA (Hybrid Manual Layout)
        // Reserve a single Rect for the entire painter area to avoid automatic GUILayout padding
        Rect bodyRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        if (bodyRect.height <= 0) return; // Wait for layout pass

        // Sidebar Sub-Rect
        Rect sidebarRect = new Rect(bodyRect.x, bodyRect.y, sidebarWidth, bodyRect.height);
        
        // Horizontal Handle Sub-Rect
        Rect hHandleRect = new Rect(bodyRect.x + sidebarWidth, bodyRect.y, 4f, bodyRect.height);
        
        // Map Canvas Sub-Rect
        float canvasX = hHandleRect.xMax;
        Rect canvasViewportRect = new Rect(canvasX, bodyRect.y, bodyRect.width - (canvasX - bodyRect.x), bodyRect.height);

        // A. Draw Sidebar
        float legendH = 22f;
        int thumbCols = Mathf.Max(1, Mathf.FloorToInt((sidebarWidth - 15f) / (ThumbSize + ThumbPad)));
        bool showPalette = activeLayer == EditLayer.Sprite && spriteLibrary != null && spriteLibrary.Count > 0;
        int paletteRows = showPalette ? Mathf.CeilToInt((float)spriteLibrary.Count / thumbCols) : 0;
        float paletteH = showPalette ? (paletteRows * (ThumbSize + ThumbPad) + ThumbPad + 22f) : 0f;

        sidebarScrollPos = GUI.BeginScrollView(sidebarRect, sidebarScrollPos, new Rect(0, 0, sidebarWidth - 15f, legendH + paletteH + 40f));
        DrawLegend(new Rect(5, 5, sidebarWidth - 15f, legendH));
        if (showPalette)
        {
            DrawSpritePalette(new Rect(5, 5 + legendH + 10, sidebarWidth - 15f, paletteH), thumbCols);
        }
        GUI.EndScrollView();

        // B. Draw Horizontal Handle
        EditorGUI.DrawRect(hHandleRect, new Color(0.08f, 0.08f, 0.08f));
        EditorGUIUtility.AddCursorRect(hHandleRect, MouseCursor.ResizeHorizontal);
        if (e.type == EventType.MouseDown && hHandleRect.Contains(e.mousePosition)) isResizingSidebar = true;
        if (isResizingSidebar && e.rawType == EventType.MouseUp) isResizingSidebar = false;
        if (isResizingSidebar && e.type == EventType.MouseDrag)
        {
            sidebarWidth += e.delta.x;
            sidebarWidth = Mathf.Clamp(sidebarWidth, 140f, position.width - 200f);
            Repaint();
        }

        // C. Draw Map Canvas
        float gridW = phaseMap.width * (CellSize + CellPadding);
        float gridH = phaseMap.height * (CellSize + CellPadding);
        Rect gridContentRect = new Rect(0, 0, gridW, gridH);
        
        // Using false, false for scrollbars lets them appear only when needed
        gridScrollPos = GUI.BeginScrollView(canvasViewportRect, gridScrollPos, gridContentRect, false, false);
        DrawGrid(gridContentRect); // Drawn at (0,0) relative to scroll content
        GUI.EndScrollView();
    }

    private void DrawSpritePalette(Rect area, int columns)
    {
        var headerStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10, normal = { textColor = new Color(0.8f, 0.8f, 1f) } };
        string selName = selectedSprite != null ? selectedSprite.name : "nenhum";
        GUI.Label(new Rect(area.x, area.y, area.width, 18), $"Seleção: {selName}", headerStyle);

        float startY = area.y + 22f;
        Event e = Event.current;

        for (int i = 0; i < spriteLibrary.Count; i++)
        {
            var sprite = spriteLibrary[i];
            if (sprite == null) continue;

            int col = i % columns;
            int row = i / columns;
            Rect thumbRect = new Rect(area.x + col * (ThumbSize + ThumbPad), startY + row * (ThumbSize + ThumbPad), ThumbSize, ThumbSize);

            bool isSelected = (selectedSpriteIndex == i);
            EditorGUI.DrawRect(thumbRect, isSelected ? new Color(0.15f, 0.3f, 0.55f) : new Color(0.2f, 0.2f, 0.2f));
            GUI.DrawTextureWithTexCoords(new Rect(thumbRect.x + 2, thumbRect.y + 2, thumbRect.width - 4, thumbRect.height - 4), sprite.texture, GetSpriteUV(sprite));
            DrawCellBorder(thumbRect, isSelected ? new Color(1f, 0.9f, 0.2f) : new Color(0.1f, 0.1f, 0.1f), isSelected ? 2f : 1f);

            if (e.type == EventType.MouseDown && e.button == 0 && thumbRect.Contains(e.mousePosition))
            {
                selectedSpriteIndex = i;
                selectedSprite = sprite;
                e.Use();
                Repaint();
            }
        }
    }

    private void DrawLegend(Rect area)
    {
        float cursorX = area.x;
        var labelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.white }, alignment = TextAnchor.MiddleLeft };

        switch (activeLayer)
        {
            case EditLayer.TileType:
                cursorX = DrawColorBox(area, cursorX, Color.gray, "Vazio", labelStyle);
                if (tileLibrary != null) foreach (var def in tileLibrary) if (def != null) cursorX = DrawColorBox(area, cursorX, def.editorTint, def.displayName, labelStyle);
                break;
            case EditLayer.Walkable:
                cursorX = DrawColorBox(area, cursorX, new Color(0.3f, 0.8f, 0.3f, 0.8f), "Ok", labelStyle);
                cursorX = DrawColorBox(area, cursorX, new Color(0.9f, 0.2f, 0.2f, 0.8f), "Block", labelStyle);
                break;
            case EditLayer.Sprite:
                GUI.Label(area, "Click: aplica sprite | Right: remove", labelStyle);
                break;
        }
    }

    private float DrawColorBox(Rect area, float cursorX, Color color, string label, GUIStyle labelStyle)
    {
        float size = 10f;
        float labelW = labelStyle.CalcSize(new GUIContent(label)).x + 4f;
        EditorGUI.DrawRect(new Rect(cursorX, area.y + area.height / 2 - size / 2, size, size), color);
        GUI.Label(new Rect(cursorX + size + 3, area.y, labelW, area.height), label, labelStyle);
        return cursorX + size + labelW + 10f;
    }

    private void DrawGrid(Rect canvas)
    {
        EditorGUI.DrawRect(canvas, new Color(0.12f, 0.12f, 0.12f));
        Event e = Event.current;
        bool paint = (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && (e.button == 0 || e.button == 1);

        for (int y = phaseMap.height - 1; y >= 0; y--)
        {
            int row = (phaseMap.height - 1) - y;
            for (int x = 0; x < phaseMap.width; x++)
            {
                Rect r = new Rect(canvas.x + x * (CellSize + CellPadding), canvas.y + row * (CellSize + CellPadding), CellSize, CellSize);
                DrawCell(r, x, y);
                if (paint && r.Contains(e.mousePosition))
                {
                    PaintCell(x, y, e.button == 1 || eraseMode);
                    e.Use();
                }
            }
        }
    }

    private void DrawCell(Rect r, int x, int y)
    {
        EditorGUI.DrawRect(r, GetCellColor(x, y));
        DrawCellBorder(r, new Color(0.05f, 0.05f, 0.05f), 1f);
        
        switch (activeLayer)
        {
            case EditLayer.TileType: DrawTileTypeContent(r, x, y); break;
            case EditLayer.Walkable: DrawWalkableContent(r, x, y); break;
            case EditLayer.Sprite: DrawSpriteContent(r, x, y); break;
        }
        var style = new GUIStyle(EditorStyles.miniLabel) { fontSize = 7, alignment = TextAnchor.LowerRight, normal = { textColor = new Color(1, 1, 1, 0.2f) } };
        GUI.Label(r, $"{x},{y}", style);
    }

    private Color GetCellColor(int x, int y)
    {
        if (activeLayer == EditLayer.Walkable) return phaseMap.GetWalkable(x, y) ? new Color(0.2f, 0.5f, 0.2f) : new Color(0.6f, 0.15f, 0.15f);
        int id = phaseMap.GetTileId(x, y);
        if (id < 0) return new Color(0.2f, 0.2f, 0.2f);
        var def = tileLibrary?.FirstOrDefault(d => d != null && d.id == id);
        return def != null ? def.editorTint : new Color(0.4f, 0.4f, 0.4f);
    }

    private void DrawTileTypeContent(Rect r, int x, int y)
    {
        int id = phaseMap.GetTileId(x, y);
        var def = tileLibrary?.FirstOrDefault(d => d != null && d.id == id);
        if (def?.defaultSprite != null) GUI.DrawTextureWithTexCoords(new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 14), def.defaultSprite.texture, GetSpriteUV(def.defaultSprite));
    }

    private void DrawWalkableContent(Rect r, int x, int y)
    {
        bool w = phaseMap.GetWalkable(x, y);
        var style = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        GUI.Label(r, w ? "✓" : "✕", style);
    }

    private void DrawSpriteContent(Rect r, int x, int y)
    {
        Sprite s = phaseMap.GetSpriteOverride(x, y);
        if (s != null) GUI.DrawTextureWithTexCoords(new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4), s.texture, GetSpriteUV(s));
    }

    private static Rect GetSpriteUV(Sprite s)
    {
        if (s == null) return new Rect(0, 0, 1, 1);
        return new Rect(s.rect.x / s.texture.width, s.rect.y / s.texture.height, s.rect.width / s.texture.width, s.rect.height / s.texture.height);
    }

    private static void DrawCellBorder(Rect r, Color c, float t)
    {
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, t), c);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - t, r.width, t), c);
        EditorGUI.DrawRect(new Rect(r.x, r.y, t, r.height), c);
        EditorGUI.DrawRect(new Rect(r.xMax - t, r.y, t, r.height), c);
    }

    private void PaintCell(int x, int y, bool erase)
    {
        Undo.RecordObject(phaseMap, "Paint");
        if (activeLayer == EditLayer.TileType) phaseMap.tiles[y].columns[x] = erase ? -1 : (selectedTileDef?.id ?? -1);
        else if (activeLayer == EditLayer.Walkable) phaseMap.walkableOverrides[y].columns[x] = erase;
        else if (activeLayer == EditLayer.Sprite) phaseMap.spriteOverrides[y].columns[x] = erase ? null : selectedSprite;
        EditorUtility.SetDirty(phaseMap);
        Repaint();
    }

    private bool IsLayerTileType() => activeLayer == EditLayer.TileType;
    private bool IsLayerSprite() => activeLayer == EditLayer.Sprite;
}
#endif
