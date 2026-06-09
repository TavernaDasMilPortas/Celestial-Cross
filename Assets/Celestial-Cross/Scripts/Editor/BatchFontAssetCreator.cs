using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

namespace CelestialCross.Editor
{
    public class BatchFontAssetCreator : UnityEditor.Editor
    {
        [MenuItem("Celestial Cross/UI Tools/Batch Create TMP Fonts (De Fontes Selecionadas)", false, 10)]
        public static void CreateTMPFontsFromSelection()
        {
            // Pega todas as fontes (TTF/OTF) selecionadas. DeepAssets permite selecionar pastas e pegar tudo dentro.
            Object[] selectedObjects = Selection.GetFiltered(typeof(Font), SelectionMode.DeepAssets);
            
            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("[Batch Font Creator] Nenhuma fonte (.ttf ou .otf) foi selecionada na aba Project. Selecione as fontes ou a pasta que as contém e tente novamente.");
                return;
            }

            int count = 0;
            int skipped = 0;

            foreach (Object obj in selectedObjects)
            {
                Font font = obj as Font;
                if (font == null) continue;

                string fontPath = AssetDatabase.GetAssetPath(font);
                if (string.IsNullOrEmpty(fontPath)) continue;

                string directory = Path.GetDirectoryName(fontPath);
                string assetName = font.name + " SDF.asset";
                string newAssetPath = Path.Combine(directory, assetName).Replace("\\", "/");

                // Verifica se o asset TMP já existe para não sobrescrever
                if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(newAssetPath) != null)
                {
                    Debug.Log($"[Batch Font Creator] Ignorado (Já existe): {newAssetPath}");
                    skipped++;
                    continue;
                }

                EditorUtility.DisplayProgressBar("Gerando TMP Font Assets", $"Processando: {font.name}", (float)count / selectedObjects.Length);

                // Cria o Font Asset do TextMeshPro usando configurações padrões de SDF
                TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font);
                
                if (fontAsset != null)
                {
                    AssetDatabase.CreateAsset(fontAsset, newAssetPath);
                    count++;
                    Debug.Log($"[Batch Font Creator] Sucesso: {newAssetPath}");
                }
                else
                {
                    Debug.LogError($"[Batch Font Creator] Falha ao criar Font Asset para: {font.name}");
                }
            }

            EditorUtility.ClearProgressBar();

            if (count > 0 || skipped > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[Batch Font Creator] Processo concluído! {count} criadas | {skipped} já existiam.");
            }
        }
        
        [MenuItem("Assets/Create/TextMeshPro/Batch Create TMP Fonts (SDF)", false, 110)]
        public static void ContextMenuCreateTMPFonts()
        {
            CreateTMPFontsFromSelection();
        }
    }
}
