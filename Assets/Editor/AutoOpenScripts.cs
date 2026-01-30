using UnityEditor;
using UnityEngine;

public class AutoOpenScripts : AssetPostprocessor
{
    // static void OnPostprocessAllAssets(
    //     string[] importedAssets,
    //     string[] deletedAssets,
    //     string[] movedAssets,
    //     string[] movedFromAssetPaths)
    // {
    //     foreach (var asset in importedAssets)
    //     {
    //         if (asset.EndsWith(".cs"))
    //         {
    //             var script = AssetDatabase.LoadAssetAtPath<MonoScript>(asset);
    //             if (script != null)
    //             {
    //                 AssetDatabase.OpenAsset(script);
    //             }
    //         }
    //     }
    //}
}
