using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TheraBytes.BetterUi;

namespace CelestialCross.Editor
{
    /// <summary>
    /// Editor tool that migrates standard Unity UI components to Better UI (TheraBytes) equivalents.
    /// Preserves all serialized properties, fixes cross-component references, and supports Undo.
    /// Access via menu: Tools > Better UI Migration Tool
    /// </summary>
    public class BetterUIMigrationTool : EditorWindow
    {
        // ──────────────────────────────────────────────
        // Types
        // ──────────────────────────────────────────────

        private enum MigrationScope
        {
            ActiveScene,
            AllOpenScenes,
            SelectedObjects,
            SelectedPrefabs
        }

        /// <summary>
        /// Defines a mapping from a standard Unity UI component to its Better UI replacement.
        /// </summary>
        private class ComponentMapping
        {
            public Type SourceType;
            public Type TargetType;
            public string DisplayName;
            public bool Enabled;
            public int ProcessingOrder; // lower = processed first

            public ComponentMapping(Type source, Type target, string display, int order, bool enabled = true)
            {
                SourceType = source;
                TargetType = target;
                DisplayName = display;
                ProcessingOrder = order;
                Enabled = enabled;
            }
        }

        /// <summary>
        /// Stores information about a component that needs migration.
        /// </summary>
        private class MigrationEntry
        {
            public GameObject GameObject;
            public Component OldComponent;
            public Type TargetType;
            public string MappingName;
        }

        /// <summary>
        /// Stores a reference from another component's serialized property to a component being migrated.
        /// </summary>
        private class ReferenceRecord
        {
            public Component ReferencingComponent;
            public string PropertyPath;
            public Component OldTarget;
        }

        /// <summary>
        /// Log entry for the results panel.
        /// </summary>
        private class LogEntry
        {
            public enum LogLevel { Info, Success, Warning, Error }
            public LogLevel Level;
            public string Message;

            public LogEntry(LogLevel level, string message)
            {
                Level = level;
                Message = message;
            }
        }

        // ──────────────────────────────────────────────
        // Fields
        // ──────────────────────────────────────────────

        private MigrationScope _scope = MigrationScope.ActiveScene;
        private bool _registerUndo = true;
        private bool _dryRun = false;

        private List<ComponentMapping> _mappings;
        private List<MigrationEntry> _scanResults = new List<MigrationEntry>();
        private Dictionary<Type, int> _scanCounts = new Dictionary<Type, int>();
        private List<LogEntry> _log = new List<LogEntry>();

        private Vector2 _mappingsScroll;
        private Vector2 _logScroll;
        private bool _hasScanned = false;
        private bool _hasMigrated = false;

        // Layout toggles
        private bool _showLayoutMappings = false;

        // ──────────────────────────────────────────────
        // Menu Item
        // ──────────────────────────────────────────────

        [MenuItem("Tools/Better UI Migration Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<BetterUIMigrationTool>("Better UI Migration");
            window.minSize = new Vector2(480, 600);
            window.Show();
        }

        // ──────────────────────────────────────────────
        // Initialization
        // ──────────────────────────────────────────────

        private void OnEnable()
        {
            InitMappings();
        }

        private void InitMappings()
        {
            _mappings = new List<ComponentMapping>
            {
                // UI Elements — ordered by dependency chain
                new ComponentMapping(typeof(Image),       typeof(BetterImage),      "Image → BetterImage",           10),
                new ComponentMapping(typeof(RawImage),    typeof(BetterRawImage),    "RawImage → BetterRawImage",     10),
                new ComponentMapping(typeof(Text),        typeof(BetterText),        "Text → BetterText",             15),
                new ComponentMapping(typeof(Selectable),  typeof(BetterSelectable),  "Selectable → BetterSelectable", 20),
                new ComponentMapping(typeof(Button),      typeof(BetterButton),      "Button → BetterButton",         30),
                new ComponentMapping(typeof(Toggle),      typeof(BetterToggle),      "Toggle → BetterToggle",         30),
                new ComponentMapping(typeof(Slider),      typeof(BetterSlider),      "Slider → BetterSlider",         30),
                new ComponentMapping(typeof(Dropdown),    typeof(BetterDropdown),    "Dropdown → BetterDropdown",     40),
                new ComponentMapping(typeof(InputField),  typeof(BetterInputField),  "InputField → BetterInputField", 40),
                new ComponentMapping(typeof(Scrollbar),   typeof(BetterScrollbar),   "Scrollbar → BetterScrollbar",   45),
                new ComponentMapping(typeof(ScrollRect),  typeof(BetterScrollRect),  "ScrollRect → BetterScrollRect", 50),
                new ComponentMapping(typeof(ToggleGroup), typeof(BetterToggleGroup), "ToggleGroup → BetterToggleGroup", 25),

                // Layout Components — disabled by default
                new ComponentMapping(typeof(HorizontalLayoutGroup), typeof(BetterHorizontalLayoutGroup),
                    "HorizontalLayoutGroup → BetterHorizontalLayoutGroup", 60, false),
                new ComponentMapping(typeof(VerticalLayoutGroup),   typeof(BetterVerticalLayoutGroup),
                    "VerticalLayoutGroup → BetterVerticalLayoutGroup",     60, false),
                new ComponentMapping(typeof(GridLayoutGroup),       typeof(BetterGridLayoutGroup),
                    "GridLayoutGroup → BetterGridLayoutGroup",             60, false),
                new ComponentMapping(typeof(ContentSizeFitter),     typeof(BetterContentSizeFitter),
                    "ContentSizeFitter → BetterContentSizeFitter",         65, false),
                new ComponentMapping(typeof(AspectRatioFitter),     typeof(BetterAspectRatioFitter),
                    "AspectRatioFitter → BetterAspectRatioFitter",         65, false),
                new ComponentMapping(typeof(LayoutElement),         typeof(BetterLayoutElement),
                    "LayoutElement → BetterLayoutElement",                 65, false),
            };
        }

        // ──────────────────────────────────────────────
        // GUI
        // ──────────────────────────────────────────────

        private void OnGUI()
        {
            // Header
            EditorGUILayout.Space(8);
            DrawHeader();
            EditorGUILayout.Space(4);

            // Scope Selection
            DrawScopeSection();
            EditorGUILayout.Space(4);

            // Component Mappings
            DrawMappingsSection();
            EditorGUILayout.Space(4);

            // Options
            DrawOptionsSection();
            EditorGUILayout.Space(8);

            // Action Buttons
            DrawActionButtons();
            EditorGUILayout.Space(4);

            // Results / Log
            DrawLogSection();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("Better UI Migration Tool", headerStyle, GUILayout.Height(28));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            var subtitleStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 11 };
            EditorGUILayout.LabelField("Replace standard Unity UI components with Better UI (TheraBytes)", subtitleStyle);
        }

        private void DrawScopeSection()
        {
            EditorGUILayout.LabelField("Migration Scope", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            _scope = (MigrationScope)EditorGUILayout.EnumPopup("Scope", _scope);
            EditorGUI.indentLevel--;

            if (_scope == MigrationScope.SelectedObjects && Selection.gameObjects.Length == 0)
            {
                EditorGUILayout.HelpBox("No GameObjects selected in the Hierarchy.", MessageType.Warning);
            }
            else if (_scope == MigrationScope.SelectedPrefabs)
            {
                var prefabs = GetSelectedPrefabs();
                if (prefabs.Count == 0)
                    EditorGUILayout.HelpBox("No Prefab assets selected in the Project window.", MessageType.Warning);
                else
                    EditorGUILayout.HelpBox($"{prefabs.Count} prefab(s) selected.", MessageType.Info);
            }
        }

        private void DrawMappingsSection()
        {
            EditorGUILayout.LabelField("Components to Migrate", EditorStyles.boldLabel);

            _mappingsScroll = EditorGUILayout.BeginScrollView(_mappingsScroll, GUILayout.MaxHeight(200));
            EditorGUI.indentLevel++;

            // UI Elements (non-layout)
            for (int i = 0; i < _mappings.Count; i++)
            {
                var m = _mappings[i];
                if (IsLayoutMapping(m)) continue;
                m.Enabled = EditorGUILayout.ToggleLeft(m.DisplayName, m.Enabled);
            }

            // Layout foldout
            _showLayoutMappings = EditorGUILayout.Foldout(_showLayoutMappings, "Layout Components (optional)");
            if (_showLayoutMappings)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < _mappings.Count; i++)
                {
                    var m = _mappings[i];
                    if (!IsLayoutMapping(m)) continue;
                    m.Enabled = EditorGUILayout.ToggleLeft(m.DisplayName, m.Enabled);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndScrollView();
        }

        private void DrawOptionsSection()
        {
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            _registerUndo = EditorGUILayout.ToggleLeft(
                new GUIContent("Register Undo (recommended)",
                    "Register all changes with Unity's Undo system so you can Ctrl+Z to revert."),
                _registerUndo);

            _dryRun = EditorGUILayout.ToggleLeft(
                new GUIContent("Dry Run (preview only — no changes)",
                    "Only scan and report what would be changed, without actually modifying anything."),
                _dryRun);

            EditorGUI.indentLevel--;
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var scanColor = new Color(0.3f, 0.7f, 1f);
            var migrateColor = _dryRun ? new Color(1f, 0.85f, 0.3f) : new Color(0.3f, 1f, 0.5f);

            GUI.backgroundColor = scanColor;
            if (GUILayout.Button("  Scan  ", GUILayout.Height(32), GUILayout.MinWidth(100)))
            {
                RunScan();
            }

            GUI.backgroundColor = migrateColor;
            string migrateLabel = _dryRun ? "  Dry Run  " : "  Migrate  ";
            EditorGUI.BeginDisabledGroup(!_hasScanned || _scanResults.Count == 0);
            if (GUILayout.Button(migrateLabel, GUILayout.Height(32), GUILayout.MinWidth(120)))
            {
                if (_dryRun)
                {
                    Log(LogEntry.LogLevel.Info, "── DRY RUN ── No changes will be made.");
                    LogDryRunResults();
                }
                else
                {
                    if (EditorUtility.DisplayDialog("Confirm Migration",
                        $"This will replace {_scanResults.Count} component(s) with Better UI equivalents.\n\n" +
                        (_registerUndo ? "You can undo with Ctrl+Z." : "WARNING: Undo is DISABLED!") +
                        "\n\nProceed?",
                        "Migrate", "Cancel"))
                    {
                        if (_scope == MigrationScope.SelectedPrefabs)
                            RunPrefabMigration(GetSelectedPrefabs());
                        else
                            RunMigration();
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLogSection()
        {
            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);

            if (_hasScanned && !_hasMigrated)
            {
                // Show scan summary
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                foreach (var kvp in _scanCounts.OrderByDescending(x => x.Value))
                {
                    EditorGUILayout.LabelField($"  {kvp.Key.Name}:  {kvp.Value} found");
                }
                EditorGUILayout.LabelField($"  ──────────────────────");
                EditorGUILayout.LabelField($"  Total components to migrate:  {_scanResults.Count}");
                EditorGUILayout.EndVertical();
            }

            if (_log.Count > 0)
            {
                _logScroll = EditorGUILayout.BeginScrollView(_logScroll, GUILayout.ExpandHeight(true));
                foreach (var entry in _log)
                {
                    var style = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true };
                    string prefix;
                    switch (entry.Level)
                    {
                        case LogEntry.LogLevel.Success: prefix = "<color=#66ff66>✓</color>"; break;
                        case LogEntry.LogLevel.Warning: prefix = "<color=#ffcc00>⚠</color>"; break;
                        case LogEntry.LogLevel.Error:   prefix = "<color=#ff4444>✗</color>"; break;
                        default:                        prefix = "<color=#88bbff>ℹ</color>"; break;
                    }
                    EditorGUILayout.LabelField($"{prefix} {entry.Message}", style);
                }
                EditorGUILayout.EndScrollView();
            }
            else if (!_hasScanned)
            {
                EditorGUILayout.HelpBox("Click 'Scan' to find components that can be migrated.", MessageType.Info);
            }
        }

        // ──────────────────────────────────────────────
        // Scan
        // ──────────────────────────────────────────────

        private void RunScan()
        {
            _scanResults.Clear();
            _scanCounts.Clear();
            _log.Clear();
            _hasScanned = true;
            _hasMigrated = false;

            var enabledMappings = _mappings.Where(m => m.Enabled).OrderBy(m => m.ProcessingOrder).ToList();
            if (enabledMappings.Count == 0)
            {
                Log(LogEntry.LogLevel.Warning, "No component types selected for migration.");
                return;
            }

            // For prefabs, we need special handling to load/unload
            List<GameObject> rootObjects;
            List<GameObject> prefabRootsToUnload = null;

            if (_scope == MigrationScope.SelectedPrefabs)
            {
                rootObjects = new List<GameObject>();
                prefabRootsToUnload = new List<GameObject>();
                var prefabs = GetSelectedPrefabs();
                foreach (var p in prefabs)
                {
                    string path = AssetDatabase.GetAssetPath(p);
                    var root = PrefabUtility.LoadPrefabContents(path);
                    if (root != null)
                    {
                        rootObjects.Add(root);
                        prefabRootsToUnload.Add(root);
                    }
                }
            }
            else
            {
                rootObjects = GetScopeRoots();
            }

            if (rootObjects.Count == 0)
            {
                Log(LogEntry.LogLevel.Warning, "No GameObjects found in the selected scope.");
                return;
            }

            try
            {
                // Collect ALL components in scope (for reference scanning)
                var allGameObjects = new List<GameObject>();
                foreach (var root in rootObjects)
                {
                    allGameObjects.AddRange(GetAllChildren(root));
                }

                Log(LogEntry.LogLevel.Info, $"Scanning {allGameObjects.Count} GameObjects...");

                // Find components to migrate
                foreach (var go in allGameObjects)
                {
                    var components = go.GetComponents<Component>();
                    foreach (var comp in components)
                    {
                        if (comp == null) continue; // Missing script

                        foreach (var mapping in enabledMappings)
                        {
                            // EXACT type match — do NOT match subclasses (like BetterImage)
                            if (comp.GetType() == mapping.SourceType)
                            {
                                _scanResults.Add(new MigrationEntry
                                {
                                    GameObject = go,
                                    OldComponent = comp,
                                    TargetType = mapping.TargetType,
                                    MappingName = mapping.DisplayName
                                });

                                if (!_scanCounts.ContainsKey(mapping.SourceType))
                                    _scanCounts[mapping.SourceType] = 0;
                                _scanCounts[mapping.SourceType]++;
                            }
                        }
                    }
                }

                Log(LogEntry.LogLevel.Info, $"Scan complete. Found {_scanResults.Count} component(s) to migrate.");
            }
            finally
            {
                // Unload any prefabs we loaded for scanning
                if (prefabRootsToUnload != null)
                {
                    foreach (var root in prefabRootsToUnload)
                    {
                        PrefabUtility.UnloadPrefabContents(root);
                    }
                    // Clear scan results for prefabs since the objects are now unloaded
                    // The user will re-scan during actual migration
                }
            }

            Repaint();
        }

        // ──────────────────────────────────────────────
        // Migration
        // ──────────────────────────────────────────────


        private void RunMigration()
        {
            _log.Clear();
            _hasMigrated = true;

            if (_scanResults.Count == 0)
            {
                Log(LogEntry.LogLevel.Warning, "Nothing to migrate.");
                return;
            }

            var enabledMappings = _mappings.Where(m => m.Enabled).ToList();
            var sortedEntries = _scanResults
                .OrderBy(e =>
                {
                    var mapping = enabledMappings.FirstOrDefault(m => m.TargetType == e.TargetType);
                    return mapping?.ProcessingOrder ?? 999;
                })
                .ToList();

            List<GameObject> rootObjects = GetScopeRoots();
            var allGameObjects = new List<GameObject>();
            foreach (var root in rootObjects)
            {
                allGameObjects.AddRange(GetAllChildren(root));
            }

            // 1. PRE-SCAN REFERENCES
            var oldComponents = new HashSet<Component>(_scanResults.Select(e => e.OldComponent));
            var referencesToFix = new List<ReferenceRecord>();

            foreach (var go in allGameObjects)
            {
                foreach (var comp in go.GetComponents<Component>())
                {
                    if (comp == null) continue;

                    var so = new SerializedObject(comp);
                    var prop = so.GetIterator();
                    while (prop.NextVisible(true))
                    {
                        if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                            prop.objectReferenceValue != null &&
                            oldComponents.Contains(prop.objectReferenceValue))
                        {
                            referencesToFix.Add(new ReferenceRecord
                            {
                                ReferencingComponent = comp,
                                PropertyPath = prop.propertyPath,
                                OldTarget = prop.objectReferenceValue as Component
                            });
                        }
                    }
                    so.Dispose();
                }
            }

            int successCount = 0;
            int failCount = 0;
            int refFixCount = 0;

            string undoGroupName = "Better UI Migration";
            if (_registerUndo) Undo.SetCurrentGroupName(undoGroupName);
            int undoGroup = _registerUndo ? Undo.GetCurrentGroup() : -1;

            var replacementMap = new Dictionary<Component, Component>();

            // 2. MIGRATE COMPONENTS
            foreach (var entry in sortedEntries)
            {
                try
                {
                    var oldComp = entry.OldComponent;
                    var targetType = entry.TargetType;

                    if (oldComp == null) continue;

                    if (_registerUndo)
                        Undo.RegisterCompleteObjectUndo(entry.GameObject, $"Migrate {oldComp.GetType().Name} to {targetType.Name}");

                    int compIndex = GetComponentIndex(entry.GameObject, oldComp);

                    // 1. Save specific Graphic/Image properties that BetterUI overrides
                    Color savedColor = Color.white;
                    Sprite savedSprite = null;
                    Texture savedTexture = null;
                    Rect savedUvRect = new Rect();

                    if (oldComp is UnityEngine.UI.Graphic graphic) savedColor = graphic.color;
                    if (oldComp is UnityEngine.UI.Image image) savedSprite = image.sprite;
                    if (oldComp is UnityEngine.UI.RawImage rawImage)
                    {
                        savedTexture = rawImage.texture;
                        savedUvRect = rawImage.uvRect;
                    }

                    // 2. Copy properties
                    UnityEditorInternal.ComponentUtility.CopyComponent(oldComp);

                    // 3. Destroy old component
                    if (_registerUndo)
                        Undo.DestroyObjectImmediate(oldComp);
                    else
                        DestroyImmediate(oldComp, true);

                    // 4. Add new component
                    Component newComp;
                    if (_registerUndo)
                        newComp = Undo.AddComponent(entry.GameObject, targetType);
                    else
                        newComp = entry.GameObject.AddComponent(targetType);

                    if (newComp != null)
                    {
                        // 5. Paste properties (this pastes Unity base fields but bypasses C# setters)
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(newComp);

                        // 6. Explicitly invoke C# setters to trigger BetterUI's internal fallback sync
                        if (newComp is TheraBytes.BetterUi.BetterImage bImage)
                        {
                            bImage.color = savedColor;
                            bImage.sprite = savedSprite;
                        }
                        else if (newComp is TheraBytes.BetterUi.BetterRawImage bRawImage)
                        {
                            bRawImage.color = savedColor;
                            bRawImage.texture = savedTexture;
                            bRawImage.uvRect = savedUvRect;
                        }

                        TryReorderComponent(entry.GameObject, newComp, compIndex);

                        replacementMap[entry.OldComponent] = newComp;
                        successCount++;
                        Log(LogEntry.LogLevel.Success, $"{entry.GameObject.name}: {entry.MappingName} migrated successfully.");
                    }
                    else
                    {
                        failCount++;
                        Log(LogEntry.LogLevel.Error, $"FAILED {entry.GameObject.name}: {entry.MappingName}");
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    Log(LogEntry.LogLevel.Error, $"FAILED {entry.GameObject.name}: {entry.MappingName} — {ex.Message}");
                    Debug.LogException(ex);
                }
            }

            // 3. RESTORE REFERENCES
            if (referencesToFix.Count > 0)
            {
                Log(LogEntry.LogLevel.Info, "Restoring references...");
                foreach (var rec in referencesToFix)
                {
                    Component refComp = rec.ReferencingComponent;
                    if (replacementMap.TryGetValue(rec.ReferencingComponent, out Component migratedRefComp))
                        refComp = migratedRefComp;

                    Component targetComp = rec.OldTarget;
                    if (replacementMap.TryGetValue(rec.OldTarget, out Component migratedTargetComp))
                        targetComp = migratedTargetComp;

                    if (refComp == null || targetComp == null) continue;

                    var so = new SerializedObject(refComp);
                    var prop = so.FindProperty(rec.PropertyPath);
                    if (prop != null)
                    {
                        prop.objectReferenceValue = targetComp;
                        so.ApplyModifiedProperties();
                        refFixCount++;
                    }
                }
                Log(LogEntry.LogLevel.Info, $"Fixed {refFixCount} reference(s).");
            }

            if (_registerUndo)
                Undo.CollapseUndoOperations(undoGroup);

            // Mark scenes dirty
            if (_scope != MigrationScope.SelectedPrefabs)
            {
                foreach (var root in rootObjects)
                {
                    EditorSceneManager.MarkSceneDirty(root.scene);
                }
            }

            Log(LogEntry.LogLevel.Info, "────────────────────────────────────────");
            Log(LogEntry.LogLevel.Success, $"Migration complete: {successCount} succeeded, {failCount} failed, {refFixCount} references restored.");

            if (failCount > 0)
                Log(LogEntry.LogLevel.Warning, "Check the Console for error details on failed migrations.");

            _hasScanned = false;
            _scanResults.Clear();
            _scanCounts.Clear();

            Repaint();
        }

        // (SyncBetterUiFallbackSettings removed)

        // ──────────────────────────────────────────────
        // Prefab Support
        // ──────────────────────────────────────────────

        /// <summary>
        /// Gets selected prefab assets from the Project window.
        /// </summary>
        private List<GameObject> GetSelectedPrefabs()
        {
            var prefabs = new List<GameObject>();

            foreach (var obj in Selection.objects)
            {
                if (obj is GameObject go)
                {
                    string path = AssetDatabase.GetAssetPath(go);
                    if (!string.IsNullOrEmpty(path) && path.EndsWith(".prefab"))
                    {
                        prefabs.Add(go);
                    }
                }
            }

            return prefabs;
        }

        /// <summary>
        /// Runs migration on prefab assets.
        /// Opens each prefab, migrates, and saves.
        /// </summary>
        private void RunPrefabMigration(List<GameObject> prefabAssets)
        {
            _log.Clear();
            _hasMigrated = true;

            var enabledMappings = _mappings.Where(m => m.Enabled).OrderBy(m => m.ProcessingOrder).ToList();
            
            int totalSuccess = 0;
            int totalFail = 0;
            int totalRefs = 0;

            foreach (var prefabAsset in prefabAssets)
            {
                string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
                Log(LogEntry.LogLevel.Info, $"── Processing prefab: {prefabPath} ──");

                // Open prefab for editing
                var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                if (prefabRoot == null)
                {
                    Log(LogEntry.LogLevel.Error, $"Failed to load prefab: {prefabPath}");
                    continue;
                }

                try
                {
                    var allGOs = GetAllChildren(prefabRoot);
                    var entries = new List<MigrationEntry>();

                    // Scan
                    foreach (var go in allGOs)
                    {
                        foreach (var comp in go.GetComponents<Component>())
                        {
                            if (comp == null) continue;
                            foreach (var mapping in enabledMappings)
                            {
                                if (comp.GetType() == mapping.SourceType)
                                {
                                    entries.Add(new MigrationEntry
                                    {
                                        GameObject = go,
                                        OldComponent = comp,
                                        TargetType = mapping.TargetType,
                                        MappingName = mapping.DisplayName
                                    });
                                }
                            }
                        }
                    }

                    if (entries.Count == 0)
                    {
                        Log(LogEntry.LogLevel.Info, "  No components to migrate in this prefab.");
                        PrefabUtility.UnloadPrefabContents(prefabRoot);
                        continue;
                    }

                    // Sort by processing order
                    entries = entries.OrderBy(e =>
                    {
                        var m = enabledMappings.FirstOrDefault(x => x.TargetType == e.TargetType);
                        return m?.ProcessingOrder ?? 999;
                    }).ToList();

                    // 1. PRE-SCAN REFERENCES
                    var oldComponents = new HashSet<Component>(entries.Select(e => e.OldComponent));
                    var referencesToFix = new List<ReferenceRecord>();

                    foreach (var go in allGOs)
                    {
                        foreach (var comp in go.GetComponents<Component>())
                        {
                            if (comp == null) continue;

                            var so = new SerializedObject(comp);
                            var prop = so.GetIterator();
                            while (prop.NextVisible(true))
                            {
                                if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                                    prop.objectReferenceValue != null &&
                                    oldComponents.Contains(prop.objectReferenceValue))
                                {
                                    referencesToFix.Add(new ReferenceRecord
                                    {
                                        ReferencingComponent = comp,
                                        PropertyPath = prop.propertyPath,
                                        OldTarget = prop.objectReferenceValue as Component
                                    });
                                }
                            }
                            so.Dispose();
                        }
                    }

                    // Migrate (no Undo inside prefab editing context)
                    bool originalUndo = _registerUndo;
                    _registerUndo = false;
                    var replacementMap = new Dictionary<Component, Component>();

                    // 2. MIGRATE COMPONENTS
                    foreach (var entry in entries)
                    {
                        try
                        {
                            var oldComp = entry.OldComponent;
                            var targetType = entry.TargetType;
                            int compIndex = GetComponentIndex(entry.GameObject, oldComp);

                            // Save properties
                            Color savedColor = Color.white;
                            Sprite savedSprite = null;
                            Texture savedTexture = null;
                            Rect savedUvRect = new Rect();

                            if (oldComp is UnityEngine.UI.Graphic graphic) savedColor = graphic.color;
                            if (oldComp is UnityEngine.UI.Image image) savedSprite = image.sprite;
                            if (oldComp is UnityEngine.UI.RawImage rawImage)
                            {
                                savedTexture = rawImage.texture;
                                savedUvRect = rawImage.uvRect;
                            }

                            UnityEditorInternal.ComponentUtility.CopyComponent(oldComp);
                            DestroyImmediate(oldComp, true);
                            
                            Component newComp = entry.GameObject.AddComponent(targetType);
                            if (newComp != null)
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentValues(newComp);

                                // Explicitly invoke C# setters
                                if (newComp is TheraBytes.BetterUi.BetterImage bImage)
                                {
                                    bImage.color = savedColor;
                                    bImage.sprite = savedSprite;
                                }
                                else if (newComp is TheraBytes.BetterUi.BetterRawImage bRawImage)
                                {
                                    bRawImage.color = savedColor;
                                    bRawImage.texture = savedTexture;
                                    bRawImage.uvRect = savedUvRect;
                                }

                                TryReorderComponent(entry.GameObject, newComp, compIndex);
                                replacementMap[entry.OldComponent] = newComp;
                                totalSuccess++;
                                Log(LogEntry.LogLevel.Success, $"  {entry.GameObject.name}: {entry.MappingName}");
                            }
                            else
                            {
                                totalFail++;
                                Log(LogEntry.LogLevel.Error, $"  FAILED: {entry.GameObject.name}: {entry.MappingName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            totalFail++;
                            Log(LogEntry.LogLevel.Error, $"  FAILED: {entry.GameObject.name}: {entry.MappingName} — {ex.Message}");
                            Debug.LogException(ex);
                        }
                    }

                    _registerUndo = originalUndo;

                    // 3. RESTORE REFERENCES
                    if (referencesToFix.Count > 0)
                    {
                        foreach (var rec in referencesToFix)
                        {
                            Component refComp = rec.ReferencingComponent;
                            if (replacementMap.TryGetValue(rec.ReferencingComponent, out Component migratedRefComp))
                                refComp = migratedRefComp;

                            Component targetComp = rec.OldTarget;
                            if (replacementMap.TryGetValue(rec.OldTarget, out Component migratedTargetComp))
                                targetComp = migratedTargetComp;

                            if (refComp == null || targetComp == null) continue;

                            var so = new SerializedObject(refComp);
                            var prop = so.FindProperty(rec.PropertyPath);
                            if (prop != null)
                            {
                                prop.objectReferenceValue = targetComp;
                                so.ApplyModifiedProperties();
                                totalRefs++;
                            }
                        }
                    }

                    // Save the prefab
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                    Log(LogEntry.LogLevel.Success, $"  Saved prefab: {prefabPath}");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }

            Log(LogEntry.LogLevel.Info, "────────────────────────────────────────");
            Log(LogEntry.LogLevel.Success,
                $"Prefab migration complete: {totalSuccess} succeeded, {totalFail} failed, {totalRefs} references restored.");

            _hasScanned = false;
            _scanResults.Clear();
            _scanCounts.Clear();
            Repaint();
        }

        // ──────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────

        /// <summary>
        /// Gets root GameObjects based on the current scope setting.
        /// </summary>
        private List<GameObject> GetScopeRoots()
        {
            var roots = new List<GameObject>();

            switch (_scope)
            {
                case MigrationScope.ActiveScene:
                    roots.AddRange(SceneManager.GetActiveScene().GetRootGameObjects());
                    break;

                case MigrationScope.AllOpenScenes:
                    for (int i = 0; i < SceneManager.sceneCount; i++)
                    {
                        var scene = SceneManager.GetSceneAt(i);
                        if (scene.isLoaded)
                            roots.AddRange(scene.GetRootGameObjects());
                    }
                    break;

                case MigrationScope.SelectedObjects:
                    roots.AddRange(Selection.gameObjects);
                    break;

                case MigrationScope.SelectedPrefabs:
                    // Prefabs are loaded/unloaded directly in RunScan and RunPrefabMigration
                    // This method is not used for prefabs
                    break;
            }

            return roots;
        }

        /// <summary>
        /// Returns all GameObjects in the hierarchy under (and including) root.
        /// </summary>
        private List<GameObject> GetAllChildren(GameObject root)
        {
            var result = new List<GameObject>();
            CollectChildrenRecursive(root, result);
            return result;
        }

        private void CollectChildrenRecursive(GameObject obj, List<GameObject> result)
        {
            result.Add(obj);
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                CollectChildrenRecursive(obj.transform.GetChild(i).gameObject, result);
            }
        }

        /// <summary>
        /// Gets the index of a component in its GameObject's component list.
        /// </summary>
        private int GetComponentIndex(GameObject go, Component comp)
        {
            var components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == comp) return i;
            }
            return -1;
        }

        /// <summary>
        /// Attempts to reorder a component to its original index position.
        /// Uses MoveComponentUp/MoveComponentDown via internal Unity API.
        /// </summary>
        private void TryReorderComponent(GameObject go, Component comp, int targetIndex)
        {
            if (targetIndex < 0) return;

            // Get current components and find where our new component is
            var components = go.GetComponents<Component>();
            int currentIndex = -1;
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == comp)
                {
                    currentIndex = i;
                    break;
                }
            }

            if (currentIndex < 0 || currentIndex == targetIndex) return;

            // Move the component up to its target position
            // (newly added components are at the bottom)
            int movesNeeded = currentIndex - targetIndex;
            for (int i = 0; i < movesNeeded; i++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentUp(comp);
            }
        }

        private bool IsLayoutMapping(ComponentMapping mapping)
        {
            return mapping.SourceType == typeof(HorizontalLayoutGroup)
                || mapping.SourceType == typeof(VerticalLayoutGroup)
                || mapping.SourceType == typeof(GridLayoutGroup)
                || mapping.SourceType == typeof(ContentSizeFitter)
                || mapping.SourceType == typeof(AspectRatioFitter)
                || mapping.SourceType == typeof(LayoutElement);
        }

        private void LogDryRunResults()
        {
            foreach (var entry in _scanResults)
            {
                Log(LogEntry.LogLevel.Info, $"  Would migrate: {entry.GameObject.name} — {entry.MappingName}");
            }
            Log(LogEntry.LogLevel.Info, $"Total: {_scanResults.Count} component(s) would be migrated.");
        }

        private void Log(LogEntry.LogLevel level, string message)
        {
            _log.Add(new LogEntry(level, message));

            // Also log to Unity console for persistent record
            switch (level)
            {
                case LogEntry.LogLevel.Error:
                    Debug.LogError($"[BetterUIMigration] {message}");
                    break;
                case LogEntry.LogLevel.Warning:
                    Debug.LogWarning($"[BetterUIMigration] {message}");
                    break;
                default:
                    Debug.Log($"[BetterUIMigration] {message}");
                    break;
            }
        }
    }
}
