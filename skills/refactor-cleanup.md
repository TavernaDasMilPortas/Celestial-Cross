# Refactoring and Cleanup Guidelines

## Description
Ensure comprehensive removal of legacy references, duplicate definitions, and broken links across the codebase after performing structural changes, renaming, or refactoring.

## Triggering Context
Use this skill whenever modifying class names, deleting obsolete features, altering architectures (e.g., converting ScriptableObjects to standard C# classes), or changing namespaces.

## General Rules
1. **Search Before and After**: Always use `grep_search` to find all references to the old class names, old properties, or old files before considering a refactoring complete.
2. **Remove Obsolete Files**: If renaming a file or migrating logic from one file to another (e.g., `ClassSO.cs` -> `Class.cs`), explicitly delete the old file to prevent duplicate definition errors (CS0101) and duplicate attributes (CS0579).
3. **Verify Compilation**: Always rebuild or perform compilation checks (`dotnet build` or equivalent) after wide-reaching changes to uncover missing namespaces or broken types (CS0246).
4. **Clean up Scene and Prefab references**: Unity scenes and prefabs have serialized references. Refactoring script names or removing fields can break these. Document what changes so the developer can fix them, or use tools to fix them if requested.
5. **No Orphaned Logic**: Ensure no downstream managers or controllers are clinging to deprecated variables (e.g., legacy `ArtifactsToDrop`). Replace or remove the fallback logic.

## Steps to Execute
1. Identify the artifact being refactored (e.g., `BaseLootTableSO`).
2. Implement the new architecture (e.g., `BaseLootTable`).
3. Delete the legacy files using terminal commands (`Remove-Item`).
4. Perform a workspace-wide search for the legacy terms using `grep_search`.
5. Update all failing files found in the search.
6. Run a compilation check to guarantee no CS errors have been left behind.