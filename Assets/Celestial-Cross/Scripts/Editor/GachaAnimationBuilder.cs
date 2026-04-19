using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Gacha.UI;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace CelestialCross.EditorTools
{
    public class GachaAnimationBuilder : OdinEditorWindow
    {
        [MenuItem("Celestial Cross/Tools/Gacha Animation Builder")]
        private static void OpenWindow()
        {
            GetWindow<GachaAnimationBuilder>("Gacha Builder").Show();
        }

        [InfoBox("Cria e organiza os assets iniciais e a hierarquia da tela de animação do Gacha.")]
        
        [Button("1. Scaffold Animation Canvas", ButtonSizes.Large)]
        private void BuildAnimationScene()
        {
            // Criar Root Canvas se não existir
            GameObject root = new GameObject("GachaAnimationCanvas");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50; // Para garantir que fique acima do gacha

            root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            root.AddComponent<GraphicRaycaster>();

            // Script de Controle
            var controller = root.AddComponent<GachaAnimationController>();

            // Fundo / Background Panel (Página de Caderno)
            GameObject bgPanel = new GameObject("BackgroundPanel");
            bgPanel.transform.SetParent(root.transform, false);
            var bgRect = bgPanel.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgPanel.AddComponent<Image>();
            bgImg.color = new Color(0.95f, 0.93f, 0.88f); // Cor de papel creme
            controller.backgroundPanel = bgPanel;

            // Spawn Areas
            GameObject stickerArea = new GameObject("StickerSpawnArea");
            stickerArea.transform.SetParent(root.transform, false);
            RectTransform sAreaRect = stickerArea.AddComponent<RectTransform>();
            sAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
            sAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
            controller.stickerSpawnArea = stickerArea.transform;

            // Botão de Continuar
            GameObject btnContGo = new GameObject("Btn_ContinueAnim");
            btnContGo.transform.SetParent(root.transform, false);
            RectTransform btnRect = btnContGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.1f);
            btnRect.anchorMax = new Vector2(0.5f, 0.1f);
            btnRect.sizeDelta = new Vector2(250, 60);
            btnRect.anchoredPosition = Vector2.zero;
            var btnImg = btnContGo.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 1f); // Azul
            var btnComp = btnContGo.AddComponent<Button>();
            controller.btnContinue = btnComp;

            GameObject btnTxt = new GameObject("Text (TMP)");
            btnTxt.transform.SetParent(btnContGo.transform, false);
            RectTransform txtRect = btnTxt.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one; txtRect.sizeDelta = Vector2.zero;
            var tmpro = btnTxt.AddComponent<TextMeshProUGUI>();
            tmpro.text = "Continuar";
            tmpro.alignment = TextAlignmentOptions.Center;
            tmpro.color = Color.white;
            tmpro.fontSize = 24;

            Selection.activeGameObject = root;
            Debug.Log("Gacha Animation Canvas scaffolded!");
        }

        [Button("2. Create Base Prefabs", ButtonSizes.Large)]
        private void CreatePrefabs()
        {
            string path = "Assets/Celestial-Cross/Prefabs/GachaUI";
            if (!AssetDatabase.IsValidFolder("Assets/Celestial-Cross/Prefabs")) AssetDatabase.CreateFolder("Assets/Celestial-Cross", "Prefabs");
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder("Assets/Celestial-Cross/Prefabs", "GachaUI");

            // PREFAB 1: Star Sticker
            GameObject star = new GameObject("StarStickerPrefab");
            var starRect = star.AddComponent<RectTransform>();
            starRect.sizeDelta = new Vector2(100, 100);
            var starImg = star.AddComponent<Image>();
            // Add a simple star/outline shape here ideally
            string starPrefabPath = $"{path}/DefaultStarSticker.prefab";
            PrefabUtility.SaveAsPrefabAsset(star, starPrefabPath, out bool sSuccess);
            DestroyImmediate(star);

            // PREFAB 2: Stamp Prize
            GameObject stamp = new GameObject("PrizeStampPrefab");
            var stampRect = stamp.AddComponent<RectTransform>();
            stampRect.sizeDelta = new Vector2(120, 140);
            var stampImg = stamp.AddComponent<Image>();
            stampImg.color = Color.white; // Base de papel

            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(stamp.transform, false);
            var iconRect = icon.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(90, 90);
            iconRect.anchoredPosition = new Vector2(0, 10);
            icon.AddComponent<Image>().color = Color.gray;

            GameObject txt = new GameObject("NameText");
            txt.transform.SetParent(stamp.transform, false);
            var txtRect = txt.AddComponent<RectTransform>();
            txtRect.sizeDelta = new Vector2(110, 30);
            txtRect.anchoredPosition = new Vector2(0, -50);
            var tmpro = txt.AddComponent<TextMeshProUGUI>();
            tmpro.text = "Item Name";
            tmpro.fontSize = 14;
            tmpro.alignment = TextAlignmentOptions.Center;
            tmpro.color = Color.black;

            string stampPrefabPath = $"{path}/DefaultPrizeStamp.prefab";
            PrefabUtility.SaveAsPrefabAsset(stamp, stampPrefabPath, out bool stSuccess);
            DestroyImmediate(stamp);

            Debug.Log($"Prefabs saved to {path}");
        }

        [Button("3. Inject Overlay into Shop Scene", ButtonSizes.Large)]
        [GUIColor(0.2f, 1f, 0.2f)]
        private void InjectIntoShopScene()
        {
            ShopSceneUI shopUI = FindObjectOfType<ShopSceneUI>();
            if (shopUI == null)
            {
                Debug.LogError("Não foi encontrado o script ShopSceneUI na cena atual. Abra a cena do Shop primeiro.");
                return;
            }

            // Verifica se já existe um GachaAnimationCanvas
            GachaAnimationController animController = FindObjectOfType<GachaAnimationController>(true);
            
            if (animController == null)
            {
                // Constroi o Canvas de Animação
                BuildAnimationScene();
                animController = FindObjectOfType<GachaAnimationController>();
            }

            // Reposiciona o GachaAnimationCanvas para dentro da Hierarquia do Shop se necessário
            // Para ser um Overlay, pode ser um canvas separado com sortingOrder mais alto
            Canvas rootCanvas = animController.GetComponent<Canvas>();
            if (rootCanvas != null)
            {
                rootCanvas.sortingOrder = 50; // Bem alto para ficar acima de popups
            }

            // Carrega os Prefabs e Auto-atribui
            string starPath = "Assets/Celestial-Cross/Prefabs/GachaUI/DefaultStarSticker.prefab";
            
            GameObject starPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(starPath);

            if (starPrefab != null) animController.starStickerPrefab = starPrefab;

            // Preenche o botão Continue
            if (animController.btnContinue == null)
            {
                Transform btnTransform = animController.transform.Find("Btn_ContinueAnim");
                if (btnTransform != null) animController.btnContinue = btnTransform.GetComponent<Button>();
            }

            // Linkar no ShopSceneUI
            SerializedObject soShop = new SerializedObject(shopUI);
            soShop.FindProperty("animationController").objectReferenceValue = animController;
            soShop.ApplyModifiedProperties();

            // Esconder o Canvas por padrão
            animController.gameObject.SetActive(false);

            Debug.Log("Sucesso! Animação de Gacha inserida e conectada ao ShopSceneUI com sucesso!");
        }

        [Button("4. Full Update Constellation & Skip", ButtonSizes.Large)]
        [GUIColor(0.1f, 0.8f, 1f)]
        private void ApplyConstellationUpdate()
        {
            string path = "Assets/Celestial-Cross/Prefabs/GachaUI";
            string starPrefabPath = $"{path}/DefaultStarSticker.prefab";
            GameObject starPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(starPrefabPath);
            string stampPrefabPath = $"{path}/DefaultPrizeStamp.prefab";
            GameObject stampPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(stampPrefabPath);
            
            string linePrefabPath = $"{path}/DefaultUILine.prefab";
            GameObject linePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(linePrefabPath);
            if (linePrefab == null)
            {
                GameObject line = new GameObject("UILinePrefab", typeof(RectTransform), typeof(Image));
                RectTransform rect = line.GetComponent<RectTransform>();
                rect.pivot = new Vector2(0, 0.5f);
                rect.sizeDelta = new Vector2(100, 4);
                PrefabUtility.SaveAsPrefabAsset(line, linePrefabPath);
                DestroyImmediate(line);
                linePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(linePrefabPath);
            }

            GachaAnimationController controller = FindObjectOfType<GachaAnimationController>(true);
            if (controller != null)
            {
                controller.uiLinePrefab = linePrefab;
                if(starPrefab != null) controller.starStickerPrefab = starPrefab;
                if(stampPrefab != null) controller.prizeStampPrefab = stampPrefab;

                if (controller.btnSkip == null)
                {
                    GameObject btnSkipGo = new GameObject("Btn_SkipAnim");
                    btnSkipGo.transform.SetParent(controller.transform, false);
                    RectTransform btnRect = btnSkipGo.AddComponent<RectTransform>();
                    btnRect.anchorMin = new Vector2(0.9f, 0.9f);
                    btnRect.anchorMax = new Vector2(0.9f, 0.9f);
                    btnRect.sizeDelta = new Vector2(150, 50);
                    btnRect.anchoredPosition = new Vector2(-100, -50);
                    
                    var btnImg = btnSkipGo.AddComponent<Image>();
                    btnImg.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
                    var btnComp = btnSkipGo.AddComponent<Button>();
                    controller.btnSkip = btnComp;

                    GameObject txt = new GameObject("Text");
                    txt.transform.SetParent(btnSkipGo.transform, false);
                    RectTransform txtRect = txt.AddComponent<RectTransform>();
                    txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one; txtRect.sizeDelta = Vector2.zero;
                    var tmpro = txt.AddComponent<TextMeshProUGUI>();
                    tmpro.text = "Pular";
                    tmpro.alignment = TextAlignmentOptions.Center;
                    tmpro.color = Color.white;
                    tmpro.fontSize = 20;

                    UnityEditor.Events.UnityEventTools.AddPersistentListener(btnComp.onClick, controller.SkipAnimation);
                }

                if (controller.whiteFlashPanel == null)
                {
                    GameObject flash = new GameObject("WhiteFlashPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
                    flash.transform.SetParent(controller.transform, false);
                    RectTransform flashRect = flash.GetComponent<RectTransform>();
                    flashRect.anchorMin = Vector2.zero; 
                    flashRect.anchorMax = Vector2.one; 
                    flashRect.sizeDelta = Vector2.zero;
                    flash.GetComponent<Image>().color = Color.white;
                    var cg = flash.GetComponent<CanvasGroup>();
                    cg.alpha = 0;
                    cg.blocksRaycasts = false;
                    controller.whiteFlashPanel = cg;
                    flash.SetActive(false);
                    flash.transform.SetAsLastSibling(); // Por cima de tudo

                    // Puxar botoes pro fim
                    controller.btnSkip.transform.SetAsLastSibling();
                    if(controller.btnContinue != null) controller.btnContinue.transform.SetAsLastSibling();
                }

                if (controller.stampsSpawnArea == null)
                {
                    GameObject stampsArea = new GameObject("StampsSpawnArea");
                    stampsArea.transform.SetParent(controller.transform, false);
                    RectTransform stAreaRect = stampsArea.AddComponent<RectTransform>();
                    stAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
                    stAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
                    controller.stampsSpawnArea = stampsArea.transform;
                    if(controller.stickerSpawnArea != null) {
                        stampsArea.transform.SetSiblingIndex(controller.stickerSpawnArea.GetSiblingIndex() + 1);
                    }
                }

                EditorUtility.SetDirty(controller);
                Debug.Log("Constellation Update: Modificacoes aplicadas com Flash e Selos originais!");
            }
            else
            {
                Debug.LogWarning("Controlador de Animação não encontrado na cena atual!");
            }
        }
    }
}