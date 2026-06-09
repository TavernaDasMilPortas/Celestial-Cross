#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using CelestialCross.Data.Pets;
using CelestialCross.Data.Dungeon;
// other namespaces if needed

namespace CelestialCross.EditorTools
{
    public class ReorganizeSOsTool
    {
        [MenuItem("Celestial Cross/4. Tools/Reorganize All ScriptableObjects")]
        public static void Reorganize()
        {
            string searchFolder = "Assets/Celestial-Cross";
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { searchFolder });
            
            int count = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                // Skip specific folders
                if (path.Contains("/Data/") || path.Contains("/Lixeira/") || path.Contains("/Resources/"))
                    continue;

                // Load to identify type
                ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so == null) continue;

                string destFolder = "Assets/Celestial-Cross/Data";

                // Function to extract relative path if it matches a known structure
                string GetMirroredPath(string searchKeyword, string targetRoot)
                {
                    int idx = path.IndexOf("/" + searchKeyword + "/", global::System.StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                    {
                        string subPath = path.Substring(idx + searchKeyword.Length + 2);
                        string dir = Path.GetDirectoryName(subPath).Replace("\\", "/");
                        if (!string.IsNullOrEmpty(dir))
                            return destFolder + "/" + targetRoot + "/" + dir;
                        return destFolder + "/" + targetRoot;
                    }
                    return null;
                }

                // 1. Try to mirror based on existing well-organized folders
                string mirroredDest = GetMirroredPath("Units", "Units") ?? 
                                      GetMirroredPath("Unit", "Units") ??
                                      GetMirroredPath("Phases", "Phases") ??
                                      GetMirroredPath("Habilities", "Habilities") ??
                                      GetMirroredPath("Abilities", "Habilities") ??
                                      GetMirroredPath("Catalogs", "Catalogs") ??
                                      GetMirroredPath("Gacha", "Gacha");

                if (mirroredDest != null)
                {
                    destFolder = mirroredDest;
                }
                else
                {
                    // 2. Fallback to semantic identification
                    string typeName = so.GetType().Name;

                    if (typeName.Contains("Ability") || typeName.Contains("Skill") || typeName.Contains("AreaPattern"))
                        destFolder += "/Habilities";
                    else if (typeName.Contains("UnitData") || typeName.Contains("PetSpecies"))
                        destFolder += "/Units";
                    else if (typeName.Contains("LevelData") || typeName.Contains("Dungeon") || typeName.Contains("Chapter"))
                        destFolder += "/Phases";
                    else if (typeName.Contains("Catalog") || typeName.Contains("Banner"))
                        destFolder += "/Catalogs";
                    else if (typeName.Contains("AI") || typeName.Contains("Pattern"))
                        destFolder += "/AI";
                    else if (typeName.Contains("Dialogue"))
                        destFolder += "/Dialogue";
                    else if (typeName.Contains("Grid") || typeName.Contains("Tile"))
                        destFolder += "/GridComponents";
                    else if (typeName.Contains("Account") || typeName.Contains("Config"))
                        destFolder += "/Account";
                    else
                        destFolder += "/Misc";
                }

                // Handle test files
                if (path.ToLower().Contains("/test/"))
                {
                    destFolder = destFolder.Replace("/Data/", "/Data/Test/");
                }

                // Ensure directory exists
                if (!AssetDatabase.IsValidFolder(destFolder))
                {
                    string[] parts = destFolder.Split('/');
                    string currentPath = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string nextPath = currentPath + "/" + parts[i];
                        if (!AssetDatabase.IsValidFolder(nextPath))
                        {
                            AssetDatabase.CreateFolder(currentPath, parts[i]);
                        }
                        currentPath = nextPath;
                    }
                }

                string fileName = Path.GetFileName(path);
                string destPath = destFolder + "/" + fileName;

                // Move asset
                string error = AssetDatabase.MoveAsset(path, destPath);
                if (string.IsNullOrEmpty(error))
                {
                    Debug.Log($"Moved: {fileName} -> {destPath}");
                    count++;
                }
                else
                {
                    Debug.LogWarning($"Failed to move {fileName}: {error}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Reorganization complete. Moved {count} ScriptableObjects.");
        }
    }
}
#endif
