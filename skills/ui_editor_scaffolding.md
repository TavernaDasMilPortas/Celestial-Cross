# UI Editor Scaffolding Skill

## Context
When creating UI screens, panels, and menus in Unity, procedural generation (using `new GameObject()` and instantiating components using `Awake()` or `Start()` at runtime) leads to hard-to-maintain code, invisible Inspector structures, and fragile object references.

## The Strategy
We use the **Editor Scaffolding Paradigm**:
1. Create a pure **C# Editor Script** (`UIBuilder_[Name].cs`) with a `[MenuItem]` function.
2. Execute the procedural generation **once inside the Unity Editor** to scaffold the entire UI hierarchy.
3. Save the resulting Canvas/Panel as a **Prefab**.
4. Use standard serialized `[Header("UI References")] public Button xyz;` in the *Runtime* Scripts to link elements via the Unity Inspector.

## Workflow Execution Steps
Whenever the user asks to build or refactor a UI component:
1. **Analyze Requirements**: See what UI components, Texts, and LayoutGroups are needed.
2. **Create the Editor Tool**: Write an `EditorWindow` or `MenuItem` script (in a folder like `Editor/`) that programmatically instantiates the `RectTransform`, `Image`, `Button`, `TextMeshProUGUI`, etc. Example:  `var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));`
3. **Run the Tool**: Generate the raw UI object directly dynamically in the Unity Editor Hierarchy.
4. **Save Prefab**: Ask the user to drag the generated UI into the `Prefabs` folder.
5. **Runtime Wiring**: The actual runtime script (e.g. `InventoryUI.cs`) should ONLY contain `public Button` variables and an `Init()` / `WireUpFixedButtons()` method to manually call `onClick.AddListener(...)` for the dynamic logic!
6. **No Runtime new GameObject()**: Never use `new GameObject()` inside `Start()` for fixed UI layouts! use Prefab instantiation if dynamic rows are needed.