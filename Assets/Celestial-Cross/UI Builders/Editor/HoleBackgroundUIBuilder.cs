using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using CelestialCross.UI;

namespace CelestialCross.UI.Builders
{
    public static class HoleBackgroundUIBuilder
    {
        [MenuItem("Celestial Cross/3. UI Builders/1. Screens/Generate Scrolling Hole Background")]
        public static void GenerateUI()
        {
            // 1. Procurar ou criar Canvas
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // 2. Criar Material com o Shader
            Shader holeShader = Shader.Find("UI/ScrollHoleMask");
            Material bgMaterial = null;
            if (holeShader != null)
            {
                bgMaterial = new Material(holeShader);
                bgMaterial.name = "Mat_ScrollHoleBackground";
                
                string matPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Celestial-Cross/GraphicAssets/Materials/Mat_ScrollHoleBackground.mat");
                AssetDatabase.CreateAsset(bgMaterial, matPath);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogWarning("Shader 'UI/ScrollHoleMask' não foi encontrado. O Material não será gerado automaticamente.");
            }

            // 3. Criar o GameObject vazio
            GameObject bgObj = new GameObject("HoleScrollingBackground");
            bgObj.transform.SetParent(canvas.transform, false);
            bgObj.transform.SetAsFirstSibling(); // Joga para o fundo
            
            // 4. Esticar na tela
            RectTransform rect = bgObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // 5. Adicionar a Imagem para renderizar junto com o Material
            Image img = bgObj.AddComponent<Image>();
            img.color = Color.white;
            if (bgMaterial != null)
            {
                img.material = bgMaterial;
            }
            
            // 6. Adicionar o Filtro de Cliques para o buraco interativo
            bgObj.AddComponent<UIHoleRaycastFilter>();

            // Forçar update nativo para ignorar cliques transparentes (neste filter basico da Image)
            img.alphaHitTestMinimumThreshold = 0.5f;

            Selection.activeGameObject = bgObj;
            Debug.Log("Fundo gerado! \n1. No GameObject selecionado (Image), adicione a Textura de Estrelas Seamless (Wrap Mode = Repeat em ambas imagens). \n2. No Material dentro do componente Image, posicione a 'Static Mask' com sua imagem de buraco preto.\n3. Adicione Physics / Physics2D Raycaster na Câmera Principal para clicar com mouse atrás da interface.");
        }
    }
}
