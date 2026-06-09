#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Utilitário do Editor para configurar automaticamente o sistema de setas indicadoras na cena de combate.
/// </summary>
public class EnemyPointerSetupUtility : Editor
{
    [MenuItem("Celestial Cross/4. Tools/Misc/Setup Enemy Pointers")]
    public static void SetupSystem()
    {
        // 1. Localiza o Canvas ativo na cena
        Canvas mainCanvas = GameObject.FindFirstObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            EditorUtility.DisplayDialog("Erro", 
                "Não foi possível encontrar nenhum Canvas na cena de combate ativa.\n\n" +
                "Por favor, crie um Canvas na cena antes de rodar este utilitário.", 
                "OK");
            return;
        }

        // 2. Cria ou localiza o diretório de Arte UI
        string artDir = "Assets/Celestial-Cross/Art/UI";
        if (!System.IO.Directory.Exists(artDir))
        {
            System.IO.Directory.CreateDirectory(artDir);
            AssetDatabase.Refresh();
        }

        // 3. Gera a textura de seta proceduralmente
        string spritePath = artDir + "/ProceduralArrow.png";
        Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (arrowSprite == null)
        {
            Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            
            // Preenche fundo com pixels transparentes
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            tex.SetPixels(pixels);

            // Desenha um triângulo (seta) apontando para a direita (+X)
            // Vértice pontudo em (54, 32), base reta ligando (12, 12) a (12, 52)
            for (int x = 12; x <= 54; x++)
            {
                float progress = (x - 12) / 42f;
                int height = Mathf.RoundToInt(20 * (1f - progress));
                for (int y = 32 - height; y <= 32 + height; y++)
                {
                    if (y >= 0 && y < 64)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                }
            }
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(spritePath, bytes);
            AssetDatabase.ImportAsset(spritePath);

            // Configura como Sprite 2D
            TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            Debug.Log("[EnemyPointerSetup] Textura e Sprite procedurais criados.");
        }

        // 4. Cria o Prefab da Seta UI
        string prefabDir = "Assets/Celestial-Cross/Prefabs/UI";
        if (!System.IO.Directory.Exists(prefabDir))
        {
            System.IO.Directory.CreateDirectory(prefabDir);
            AssetDatabase.Refresh();
        }

        string prefabPath = prefabDir + "/EnemyArrowPointer.prefab";
        GameObject arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (arrowPrefab == null)
        {
            GameObject tempGo = new GameObject("EnemyArrowPointer_Temp", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rect = tempGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(50f, 50f);

            // Ajusta o pivô da rotação no centro para girar adequadamente
            rect.pivot = new Vector2(0.5f, 0.5f);

            Image img = tempGo.GetComponent<Image>();
            img.sprite = arrowSprite;
            img.color = Color.red; // Cor padrão vermelha para perigo/inimigos

            Button btn = tempGo.GetComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;

            arrowPrefab = PrefabUtility.SaveAsPrefabAsset(tempGo, prefabPath);
            DestroyImmediate(tempGo);
            Debug.Log("[EnemyPointerSetup] Prefab de seta criado.");
        }

        // 5. Configura o objeto EnemyPointerManager no Canvas
        Transform childTransform = mainCanvas.transform.Find("EnemyPointerManager");
        GameObject managerGo;
        if (childTransform != null)
        {
            managerGo = childTransform.gameObject;
        }
        else
        {
            managerGo = new GameObject("EnemyPointerManager", typeof(RectTransform));
            managerGo.transform.SetParent(mainCanvas.transform, false);
            
            // Configura o RectTransform para esticar e preencher todo o Canvas
            RectTransform managerRect = managerGo.GetComponent<RectTransform>();
            managerRect.anchorMin = Vector2.zero;
            managerRect.anchorMax = Vector2.one;
            managerRect.sizeDelta = Vector2.zero;
            managerRect.anchoredPosition = Vector2.zero;

            Undo.RegisterCreatedObjectUndo(managerGo, "Create EnemyPointerManager");
        }

        EnemyPointerManager script = managerGo.GetComponent<EnemyPointerManager>();
        if (script == null)
        {
            script = managerGo.AddComponent<EnemyPointerManager>();
        }

        // Usa SerializedObject para atrelar a referência ao campo privado serializado
        SerializedObject so = new SerializedObject(script);
        SerializedProperty arrowProp = so.FindProperty("arrowPrefab");
        if (arrowProp != null)
        {
            arrowProp.objectReferenceValue = arrowPrefab;
            so.ApplyModifiedProperties();
        }

        // 6. Notifica e salva mudanças na cena
        EditorUtility.SetDirty(managerGo);
        EditorUtility.SetDirty(mainCanvas.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mainCanvas.gameObject.scene);

        EditorUtility.DisplayDialog("Sucesso", 
            $"Sistema de setas indicadoras configurado com sucesso!\n\n" +
            $"Objeto: {mainCanvas.name}/EnemyPointerManager\n" +
            $"Prefab: {prefabPath}\n\n" +
            $"As setas serão instanciadas diretamente como filhas dele.", 
            "OK");
    }
}
#endif
