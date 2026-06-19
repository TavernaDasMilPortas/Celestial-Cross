using UnityEngine;
using UnityEditor;
using System.IO;

public class PathSpriteGenerator
{
    [MenuItem("Tools/Gerar Sprites de Caminho (Grid)")]
    public static void GenerateSprites()
    {
        string dir = "Assets/Sprites/GridPath";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        int size = 256;
        int halfThickness = 32; // Espessura total de 64 pixels

        Texture2D straight = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Texture2D corner = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Texture2D arrow = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Texture2D start = new Texture2D(size, size, TextureFormat.RGBA32, false);

        ClearTexture(straight);
        ClearTexture(corner);
        ClearTexture(arrow);
        ClearTexture(start);

        // A seta agora termina no CENTRO do tile (x = 128)
        int arrowTipX = size / 2;
        int arrowBaseX = arrowTipX - 40; // Cabeça da seta tem 40px de comprimento
        int arrowHalfWidth = 56; // Cabeça da seta um pouco mais larga que a linha

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // 1. LINHA RETA (Esquerda para Direita)
                float distToCenterY = Mathf.Abs(y - size / 2f);
                float alphaStraight = GetAlpha(distToCenterY, halfThickness);
                if (alphaStraight > 0)
                    straight.SetPixel(x, y, new Color(1, 1, 1, alphaStraight));

                // 2. CURVA DE 90 GRAUS (Esquerda para Baixo)
                float distToOrigin = Vector2.Distance(new Vector2(x, y), new Vector2(0, 0));
                float distToCenterArc = Mathf.Abs(distToOrigin - size / 2f);
                
                if (x <= size/2f + halfThickness + 2 && y <= size/2f + halfThickness + 2)
                {
                    float alphaCorner = GetAlpha(distToCenterArc, halfThickness);
                    if (alphaCorner > 0)
                        corner.SetPixel(x, y, new Color(1, 1, 1, alphaCorner));
                }

                // 3. SETA FINAL (Esquerda parando no centro)
                float alphaTail = 0;
                float alphaHead = 0;

                // Corpo da seta (vem da borda esquerda até a base da cabeça da seta)
                if (x <= arrowBaseX)
                {
                    alphaTail = GetAlpha(distToCenterY, halfThickness);
                }
                
                // Cabeça da seta
                if (x >= arrowBaseX - 1 && x <= arrowTipX + 1)
                {
                    float t = (float)(x - arrowBaseX) / (arrowTipX - arrowBaseX);
                    float currentHalfWidth = Mathf.Lerp(arrowHalfWidth, 0, t);
                    alphaHead = GetAlpha(distToCenterY, currentHalfWidth);
                    
                    if (x < arrowBaseX)
                        alphaHead *= Mathf.Clamp01(1f - (arrowBaseX - x));
                        
                    if (x > arrowTipX)
                        alphaHead *= Mathf.Clamp01(1f - (x - arrowTipX));
                }
                
                float alphaArrow = Mathf.Max(alphaTail, alphaHead);
                if (alphaArrow > 0)
                    arrow.SetPixel(x, y, new Color(1, 1, 1, alphaArrow));

                // 4. INICIO (Borda arredondada no centro, e linha reta para a direita)
                float alphaStart = 0;
                
                // Bolinha no centro perfeito
                float distToCenterPoint = Vector2.Distance(new Vector2(x, y), new Vector2(size/2f, size/2f));
                alphaStart = GetAlpha(distToCenterPoint, halfThickness);
                
                // Reta do centro para a direita (x >= 128)
                if (x >= size / 2f)
                {
                    float alphaStartLine = GetAlpha(distToCenterY, halfThickness);
                    alphaStart = Mathf.Max(alphaStart, alphaStartLine);
                }
                
                if (alphaStart > 0)
                    start.SetPixel(x, y, new Color(1, 1, 1, alphaStart));
            }
        }

        SaveTexture(straight, dir + "/Path_Straight.png");
        SaveTexture(corner, dir + "/Path_Corner.png");
        SaveTexture(arrow, dir + "/Path_Arrow.png");
        SaveTexture(start, dir + "/Path_Start.png");

        AssetDatabase.Refresh();
        
        ConfigureSpriteImport(dir + "/Path_Straight.png");
        ConfigureSpriteImport(dir + "/Path_Corner.png");
        ConfigureSpriteImport(dir + "/Path_Arrow.png");
        ConfigureSpriteImport(dir + "/Path_Start.png");

        Debug.Log($"<color=green><b>Sucesso!</b></color> 4 Sprites de Grid gerados com Anti-Aliasing perfeito na pasta: {dir}");
        
        // Tentamos auto-atribuir os sprites se o PathVisualizer existir na cena
        PathVisualizer visualizer = Object.FindFirstObjectByType<PathVisualizer>();
        if (visualizer != null)
        {
            visualizer.straightSprite = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "/Path_Straight.png");
            visualizer.cornerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "/Path_Corner.png");
            visualizer.arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "/Path_Arrow.png");
            visualizer.startSprite = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "/Path_Start.png");
            EditorUtility.SetDirty(visualizer);
            Debug.Log("[PathSpriteGenerator] As imagens foram injetadas automaticamente no PathVisualizer!");
        }
    }

    private static float GetAlpha(float distance, float allowedRadius)
    {
        float distToEdge = distance - allowedRadius;
        if (distToEdge <= 0) return 1f;
        return Mathf.Clamp01(1f - distToEdge); // Anti-Aliasing simples de 1 pixel
    }

    private static void ClearTexture(Texture2D tex)
    {
        Color clearColor = new Color(1, 1, 1, 0);
        Color[] colors = new Color[tex.width * tex.height];
        for (int i = 0; i < colors.Length; i++) colors[i] = clearColor;
        tex.SetPixels(colors);
    }

    private static void SaveTexture(Texture2D tex, string path)
    {
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Object.DestroyImmediate(tex);
    }

    private static void ConfigureSpriteImport(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }
}
