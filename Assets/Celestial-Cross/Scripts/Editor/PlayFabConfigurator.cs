using UnityEditor;
using PlayFab;
using UnityEngine;
using System.IO;

namespace CelestialCross.Editor
{
    public class PlayFabConfigurator
    {
        [InitializeOnLoadMethod]
        public static void ForceConfigure()
        {
            PlayFabSharedSettings settings = Resources.Load<PlayFabSharedSettings>("PlayFabSharedSettings");
            
            if (settings == null)
            {
                string resourcesPath = "Assets/Resources";
                if (!AssetDatabase.IsValidFolder(resourcesPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                settings = ScriptableObject.CreateInstance<PlayFabSharedSettings>();
                AssetDatabase.CreateAsset(settings, resourcesPath + "/PlayFabSharedSettings.asset");
                Debug.Log("[PlayFabConfigurator] Arquivo PlayFabSharedSettings gerado com sucesso na pasta Resources.");
            }
            
            if (settings.TitleId != "1206FE")
            {
                settings.TitleId = "1206FE";
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                Debug.Log("[PlayFabConfigurator] Title ID injetado com sucesso: 1206FE!");
            }
        }
    }
}
