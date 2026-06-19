using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.UI;

namespace CelestialCross.EditorScripts
{
    public class CombatUIBuilder : EditorWindow
    {
        [MenuItem("Celestial Cross/UI/Build P5 Combat Intro")]
        public static void BuildP5CombatUI()
        {
            Debug.Log("[CombatUIBuilder] Iniciando construção da UI de Combate P5...");

            Canvas mainCanvas = Object.FindFirstObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("Nenhum Canvas encontrado na cena!");
                return;
            }

            // 1. Configurar IntroModalUI
            IntroModalUI introModal = Object.FindFirstObjectByType<IntroModalUI>(FindObjectsInactive.Include);
            if (introModal != null)
            {
                RectTransform panel = introModal.panelTransform;
                if (panel != null)
                {
                    // Forçar tela cheia
                    panel.anchorMin = Vector2.zero;
                    panel.anchorMax = Vector2.one;
                    panel.offsetMin = Vector2.zero;
                    panel.offsetMax = Vector2.zero;

                    // Adicionar Image de fundo se não houver
                    Image bg = panel.GetComponent<Image>();
                    if (bg == null)
                    {
                        bg = panel.gameObject.AddComponent<Image>();
                        bg.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Fundo escuro
                    }
                    introModal.backgroundImage = bg;

                    // Configurar stageImageContainer se não houver
                    if (introModal.stageImageContainer == null && introModal.stageText != null)
                    {
                        GameObject containerObj = new GameObject("StageImageContainer", typeof(RectTransform), typeof(Image));
                        containerObj.transform.SetParent(panel, false);
                        RectTransform containerRect = containerObj.GetComponent<RectTransform>();
                        
                        // Centralizado, um pouco abaixo
                        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
                        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
                        containerRect.anchoredPosition = new Vector2(0, -50);
                        containerRect.sizeDelta = new Vector2(600, 150);

                        Image containerImg = containerObj.GetComponent<Image>();
                        containerImg.color = Color.black; // Fundo do nome da fase

                        introModal.stageText.transform.SetParent(containerRect, false);
                        introModal.stageText.rectTransform.anchoredPosition = Vector2.zero;
                        
                        introModal.stageImageContainer = containerRect;
                    }

                    // Configurar White Flash
                    if (introModal.whiteFlashOverlay == null)
                    {
                        GameObject flashObj = new GameObject("WhiteFlashOverlay", typeof(RectTransform), typeof(Image));
                        flashObj.transform.SetParent(introModal.transform, false);
                        RectTransform flashRect = flashObj.GetComponent<RectTransform>();
                        flashRect.anchorMin = Vector2.zero;
                        flashRect.anchorMax = Vector2.one;
                        flashRect.offsetMin = Vector2.zero;
                        flashRect.offsetMax = Vector2.zero;

                        Image flashImg = flashObj.GetComponent<Image>();
                        flashImg.color = Color.white;
                        introModal.whiteFlashOverlay = flashImg;
                        
                        // Garante que o flash fique por cima de tudo no modal
                        flashObj.transform.SetAsLastSibling();
                    }
                }
                EditorUtility.SetDirty(introModal);
            }
            else
            {
                Debug.LogWarning("IntroModalUI não encontrado na cena.");
            }

            // 2. Criar ou configurar EnemyFocusSkipUI
            EnemyFocusSkipUI skipUI = Object.FindFirstObjectByType<EnemyFocusSkipUI>(FindObjectsInactive.Include);
            if (skipUI == null)
            {
                GameObject skipObj = new GameObject("EnemyFocusSkipUI", typeof(RectTransform), typeof(CanvasGroup), typeof(Button), typeof(EnemyFocusSkipUI));
                skipObj.transform.SetParent(mainCanvas.transform, false);
                
                RectTransform skipRect = skipObj.GetComponent<RectTransform>();
                skipRect.anchorMin = Vector2.zero;
                skipRect.anchorMax = Vector2.one;
                skipRect.offsetMin = Vector2.zero;
                skipRect.offsetMax = Vector2.zero;

                // Fundo transparente para o botão ocupar a tela toda
                Image skipImg = skipObj.AddComponent<Image>();
                skipImg.color = new Color(0, 0, 0, 0.01f); // Quase invisível, mas "clicável"

                // Texto "Pular"
                GameObject textObj = new GameObject("SkipText", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(skipObj.transform, false);
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(1, 0);
                textRect.anchorMax = new Vector2(1, 0);
                textRect.anchoredPosition = new Vector2(-150, 100);
                textRect.sizeDelta = new Vector2(200, 50);

                TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
                text.text = ">> PULAR";
                text.alignment = TextAlignmentOptions.Right;
                text.fontSize = 36;
                text.color = Color.white;

                skipUI = skipObj.GetComponent<EnemyFocusSkipUI>();
                EditorUtility.SetDirty(skipUI);
            }

            // 3. Configurar GameRenderTween na RawImage do jogo
            RawImage[] rawImages = Object.FindObjectsByType<RawImage>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            RawImage gameView = null;
            foreach (var img in rawImages)
            {
                if (img.texture != null && img.texture is RenderTexture)
                {
                    gameView = img;
                    break;
                }
            }

            if (gameView != null)
            {
                // Força tela cheia
                RectTransform rt = gameView.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }

                GameRenderTween renderTween = gameView.GetComponent<GameRenderTween>();
                if (renderTween == null)
                {
                    renderTween = gameView.gameObject.AddComponent<GameRenderTween>();
                    renderTween.combatZoomMultiplier = 1.6f; 
                    renderTween.introZoomMultiplier = 1.0f;
                    EditorUtility.SetDirty(gameView);
                    Debug.Log("GameRenderTween adicionado na RawImage e configurado para FullScreen: " + gameView.name);
                }
            }
            else
            {
                Debug.LogWarning("Nenhuma RawImage com RenderTexture encontrada para adicionar o GameRenderTween.");
            }

            Debug.Log("[CombatUIBuilder] Concluído! Lembre-se de salvar a cena.");
        }
    }
}
