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
        [MenuItem("Celestial Cross/3. UI Builders/Gacha Animation Builder")]
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

        [Button("5. Full Update Supreme Reveal", ButtonSizes.Large)]
        [GUIColor(1f, 0.5f, 0.8f)]
        private void ApplySupremeRevealUpdate()
        {
            GachaAnimationController controller = FindObjectOfType<GachaAnimationController>(true);
            if (controller != null)
            {
                if (controller.supremeRevealContainer == null)
                {
                    GameObject supremeContainerGo = new GameObject("SupremeRevealContainer", typeof(RectTransform), typeof(CanvasGroup));
                    supremeContainerGo.transform.SetParent(controller.transform, false);
                    RectTransform supremeRect = supremeContainerGo.GetComponent<RectTransform>();
                    supremeRect.anchorMin = Vector2.zero;
                    supremeRect.anchorMax = Vector2.one;
                    supremeRect.sizeDelta = Vector2.zero;
                    supremeContainerGo.SetActive(false);
                    controller.supremeRevealContainer = supremeRect;

                    GameObject silhouetteGo = new GameObject("SupremeSilhouetteImage", typeof(RectTransform), typeof(Image));
                    silhouetteGo.transform.SetParent(supremeContainerGo.transform, false);
                    RectTransform silhouetteRect = silhouetteGo.GetComponent<RectTransform>();
                    silhouetteRect.anchorMin = new Vector2(0.5f, 0.5f);
                    silhouetteRect.anchorMax = new Vector2(0.5f, 0.5f);
                    silhouetteRect.sizeDelta = new Vector2(800, 1000);
                    controller.supremeSilhouetteImage = silhouetteGo.GetComponent<Image>();

                    GameObject splashGo = new GameObject("SupremeSplashImage", typeof(RectTransform), typeof(Image));
                    splashGo.transform.SetParent(supremeContainerGo.transform, false);
                    RectTransform splashRect = splashGo.GetComponent<RectTransform>();
                    splashRect.anchorMin = new Vector2(0.5f, 0.5f);
                    splashRect.anchorMax = new Vector2(0.5f, 0.5f);
                    splashRect.sizeDelta = new Vector2(800, 1000);
                    var splashImage = splashGo.GetComponent<Image>();
                    splashImage.color = new Color(1f, 1f, 1f, 0f);
                    controller.supremeSplashImage = splashImage;

                    GameObject nameTxtGo = new GameObject("SupremeNameText", typeof(RectTransform), typeof(TextMeshProUGUI));
                    nameTxtGo.transform.SetParent(supremeContainerGo.transform, false);
                    RectTransform nameTxtRect = nameTxtGo.GetComponent<RectTransform>();
                    nameTxtRect.anchorMin = new Vector2(0.5f, 0.2f);
                    nameTxtRect.anchorMax = new Vector2(0.5f, 0.2f);
                    nameTxtRect.sizeDelta = new Vector2(1000, 100);
                    var tmpro = nameTxtGo.GetComponent<TextMeshProUGUI>();
                    tmpro.text = "SUPREME CHARACTER";
                    tmpro.alignment = TextAlignmentOptions.Center;
                    tmpro.fontSize = 72;
                    tmpro.fontStyle = FontStyles.Bold | FontStyles.Italic;
                    tmpro.color = new Color(1f, 0.2f, 0.2f, 0f); // Reddish alpha 0
                    controller.supremeNameText = tmpro;

                    if (controller.whiteFlashPanel != null)
                    {
                        supremeContainerGo.transform.SetSiblingIndex(controller.whiteFlashPanel.transform.GetSiblingIndex());
                    }

                    // Move buttons to the end
                    if (controller.btnSkip != null) controller.btnSkip.transform.SetAsLastSibling();
                    if (controller.btnContinue != null) controller.btnContinue.transform.SetAsLastSibling();
                }

                if (controller.silhouetteMaterial == null)
                {
                    Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Celestial-Cross/Shaders/UI_Silhouette_Mat.mat");
                    if (mat == null)
                    {
                        Shader shader = Shader.Find("UI/Silhouette");
                        if (shader != null)
                        {
                            mat = new Material(shader);
                            AssetDatabase.CreateAsset(mat, "Assets/Celestial-Cross/Shaders/UI_Silhouette_Mat.mat");
                        }
                        else
                        {
                            Debug.LogWarning("Shader UI/Silhouette not found. Please create it first.");
                        }
                    }
                    controller.silhouetteMaterial = mat;
                }

                EditorUtility.SetDirty(controller);
                Debug.Log("Supreme Reveal Update: Modificações aplicadas!");
            }
            else
            {
                Debug.LogError("GachaAnimationController não encontrado!");
            }
        }
        [Button("6. Build Reward Detail Modal", ButtonSizes.Large)]
        private void BuildRewardDetailModal()
        {
            var controller = FindObjectOfType<GachaAnimationController>();
            if (controller != null)
            {
                if (controller.rewardDetailModal != null)
                {
                    Debug.Log("O Modal já existe e está referenciado no Controller.");
                    return;
                }

                // Cria o root do Modal
                GameObject modalRoot = new GameObject("RewardDetailModal", typeof(RectTransform));
                modalRoot.transform.SetParent(controller.transform, false);
                RectTransform rootRect = modalRoot.GetComponent<RectTransform>();
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.sizeDelta = Vector2.zero;
                modalRoot.SetActive(false); // Inicia oculto
                controller.rewardDetailModal = modalRoot;

                // Cria o background invisível ou escurecido que serve como botão para fechar
                GameObject bgOverlay = new GameObject("ModalBackgroundOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
                bgOverlay.transform.SetParent(modalRoot.transform, false);
                RectTransform bgRect = bgOverlay.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;
                var bgImg = bgOverlay.GetComponent<Image>();
                bgImg.color = new Color(0, 0, 0, 0.8f);
                var btnClose = bgOverlay.GetComponent<Button>();
                btnClose.transition = Selectable.Transition.None;
                controller.modalCloseButton = btnClose;

                // Cria o painel flutuante
                GameObject panel = new GameObject("DetailsPanel", typeof(RectTransform), typeof(Image));
                panel.transform.SetParent(modalRoot.transform, false);
                RectTransform panelRect = panel.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.sizeDelta = new Vector2(500, 600);
                var panelImg = panel.GetComponent<Image>();
                panelImg.color = new Color(0.1f, 0.1f, 0.12f, 1f); // Dark background

                // Cria o Icon
                GameObject iconGo = new GameObject("ItemIcon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(panel.transform, false);
                RectTransform iconRect = iconGo.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.7f);
                iconRect.anchorMax = new Vector2(0.5f, 0.7f);
                iconRect.sizeDelta = new Vector2(200, 200);
                iconRect.anchoredPosition = new Vector2(0, 0);
                controller.modalIcon = iconGo.GetComponent<Image>();

                // Cria o Nome
                GameObject nameGo = new GameObject("ItemNameText", typeof(RectTransform), typeof(TextMeshProUGUI));
                nameGo.transform.SetParent(panel.transform, false);
                RectTransform nameRect = nameGo.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0.5f, 0.4f);
                nameRect.anchorMax = new Vector2(0.5f, 0.4f);
                nameRect.sizeDelta = new Vector2(450, 100);
                nameRect.anchoredPosition = new Vector2(0, 0);
                var tmproName = nameGo.GetComponent<TextMeshProUGUI>();
                tmproName.text = "Item Name";
                tmproName.alignment = TextAlignmentOptions.Center;
                tmproName.fontSize = 42;
                tmproName.fontStyle = FontStyles.Bold;
                controller.modalName = tmproName;

                // Cria Raridade
                GameObject rarityGo = new GameObject("ItemRarityText", typeof(RectTransform), typeof(TextMeshProUGUI));
                rarityGo.transform.SetParent(panel.transform, false);
                RectTransform rarityRect = rarityGo.GetComponent<RectTransform>();
                rarityRect.anchorMin = new Vector2(0.5f, 0.25f);
                rarityRect.anchorMax = new Vector2(0.5f, 0.25f);
                rarityRect.sizeDelta = new Vector2(400, 60);
                rarityRect.anchoredPosition = new Vector2(0, 0);
                var tmproRarity = rarityGo.GetComponent<TextMeshProUGUI>();
                tmproRarity.text = "Rarity";
                tmproRarity.alignment = TextAlignmentOptions.Center;
                tmproRarity.fontSize = 28;
                tmproRarity.fontStyle = FontStyles.Bold | FontStyles.Italic;
                controller.modalRarityText = tmproRarity;

                // Cria Descrição / Quantidade
                GameObject descGo = new GameObject("ItemDescText", typeof(RectTransform), typeof(TextMeshProUGUI));
                descGo.transform.SetParent(panel.transform, false);
                RectTransform descRect = descGo.GetComponent<RectTransform>();
                descRect.anchorMin = new Vector2(0.5f, 0.1f);
                descRect.anchorMax = new Vector2(0.5f, 0.1f);
                descRect.sizeDelta = new Vector2(400, 100);
                descRect.anchoredPosition = new Vector2(0, 0);
                var tmproDesc = descGo.GetComponent<TextMeshProUGUI>();
                tmproDesc.text = "Description";
                tmproDesc.alignment = TextAlignmentOptions.Center;
                tmproDesc.fontSize = 24;
                tmproDesc.color = new Color(0.8f, 0.8f, 0.8f, 1f);
                controller.modalDesc = tmproDesc;

                EditorUtility.SetDirty(controller);
                Debug.Log("Reward Detail Modal construído e anexado ao Controller com sucesso!");
            }
            else
            {
                Debug.LogError("GachaAnimationController não encontrado!");
            }
        }

        [Button("6. Build Gacha Unit Reward Modal", ButtonSizes.Large)]
        [GUIColor(1f, 0.5f, 0.1f)]
        private void BuildGachaUnitRewardModal()
        {
            var controller = FindObjectOfType<GachaAnimationController>();
            if (controller != null)
            {
                if (controller.gachaUnitRewardModal != null)
                {
                    Debug.Log("O GachaUnitRewardModal já existe e está referenciado no Controller.");
                    return;
                }

                GameObject modalRoot = new GameObject("GachaUnitRewardModal", typeof(RectTransform), typeof(CelestialCross.Gacha.UI.GachaUnitRewardModal));
                modalRoot.transform.SetParent(controller.transform, false);
                RectTransform rootRect = modalRoot.GetComponent<RectTransform>();
                rootRect.anchorMin = Vector2.zero; rootRect.anchorMax = Vector2.one; rootRect.sizeDelta = Vector2.zero;
                modalRoot.SetActive(false);
                var comp = modalRoot.GetComponent<CelestialCross.Gacha.UI.GachaUnitRewardModal>();
                controller.gachaUnitRewardModal = comp;

                GameObject bgOverlay = new GameObject("BackgroundOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
                bgOverlay.transform.SetParent(modalRoot.transform, false);
                RectTransform bgRect = bgOverlay.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one; bgRect.sizeDelta = Vector2.zero;
                var bgImg = bgOverlay.GetComponent<Image>(); bgImg.color = new Color(0, 0, 0, 0.8f);
                var btnClose = bgOverlay.GetComponent<Button>(); btnClose.transition = Selectable.Transition.None;
                comp.closeButton = btnClose;

                GameObject panel = new GameObject("ContentPanel", typeof(RectTransform), typeof(Image));
                panel.transform.SetParent(modalRoot.transform, false);
                RectTransform panelRect = panel.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0.5f); panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.sizeDelta = new Vector2(600, 800);
                var panelImg = panel.GetComponent<Image>(); panelImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

                GameObject spriteGo = new GameObject("SpriteImage", typeof(RectTransform), typeof(Image));
                spriteGo.transform.SetParent(panel.transform, false);
                RectTransform spriteRect = spriteGo.GetComponent<RectTransform>();
                spriteRect.anchorMin = new Vector2(0.5f, 0.8f); spriteRect.anchorMax = new Vector2(0.5f, 0.8f);
                spriteRect.sizeDelta = new Vector2(250, 250); spriteRect.anchoredPosition = Vector2.zero;
                comp.spriteImage = spriteGo.GetComponent<Image>(); comp.spriteImage.preserveAspect = true;

                GameObject nameGo = new GameObject("NameText", typeof(RectTransform), typeof(TextMeshProUGUI));
                nameGo.transform.SetParent(panel.transform, false);
                RectTransform nameRect = nameGo.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0.5f, 0.6f); nameRect.anchorMax = new Vector2(0.5f, 0.6f);
                nameRect.sizeDelta = new Vector2(500, 50); nameRect.anchoredPosition = Vector2.zero;
                var tmproName = nameGo.GetComponent<TextMeshProUGUI>();
                tmproName.text = "Unit Name"; tmproName.alignment = TextAlignmentOptions.Center; tmproName.fontSize = 36; tmproName.fontStyle = FontStyles.Bold;
                comp.nameText = tmproName;

                GameObject rarityGo = new GameObject("RarityText", typeof(RectTransform), typeof(TextMeshProUGUI));
                rarityGo.transform.SetParent(panel.transform, false);
                RectTransform rarityRect = rarityGo.GetComponent<RectTransform>();
                rarityRect.anchorMin = new Vector2(0.5f, 0.55f); rarityRect.anchorMax = new Vector2(0.5f, 0.55f);
                rarityRect.sizeDelta = new Vector2(500, 40); rarityRect.anchoredPosition = Vector2.zero;
                var tmproRarity = rarityGo.GetComponent<TextMeshProUGUI>();
                tmproRarity.text = "Rarity"; tmproRarity.alignment = TextAlignmentOptions.Center; tmproRarity.fontSize = 24; tmproRarity.color = Color.yellow;
                comp.rarityText = tmproRarity;

                GameObject starsGo = new GameObject("StarsContainer", typeof(RectTransform), typeof(UnityEngine.UI.HorizontalLayoutGroup));
                starsGo.transform.SetParent(panel.transform, false);
                RectTransform starsRect = starsGo.GetComponent<RectTransform>();
                starsRect.anchorMin = new Vector2(0.5f, 0.48f); starsRect.anchorMax = new Vector2(0.5f, 0.48f);
                starsRect.sizeDelta = new Vector2(300, 50); starsRect.anchoredPosition = Vector2.zero;
                var hg = starsGo.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                hg.childAlignment = TextAnchor.MiddleCenter; hg.childControlWidth = false; hg.childControlHeight = false; hg.spacing = 5;
                comp.starsContainer = starsGo.transform;

                GameObject statsGo = new GameObject("StatsText", typeof(RectTransform), typeof(TextMeshProUGUI));
                statsGo.transform.SetParent(panel.transform, false);
                RectTransform statsRect = statsGo.GetComponent<RectTransform>();
                statsRect.anchorMin = new Vector2(0.5f, 0.3f); statsRect.anchorMax = new Vector2(0.5f, 0.3f);
                statsRect.sizeDelta = new Vector2(500, 150); statsRect.anchoredPosition = Vector2.zero;
                var tmproStats = statsGo.GetComponent<TextMeshProUGUI>();
                tmproStats.text = "Stats"; tmproStats.alignment = TextAlignmentOptions.Center; tmproStats.fontSize = 26;
                comp.statsText = tmproStats;

                GameObject skillsGo = new GameObject("SkillsContainer", typeof(RectTransform), typeof(UnityEngine.UI.HorizontalLayoutGroup));
                skillsGo.transform.SetParent(panel.transform, false);
                RectTransform skillsRect = skillsGo.GetComponent<RectTransform>();
                skillsRect.anchorMin = new Vector2(0.5f, 0.1f); skillsRect.anchorMax = new Vector2(0.5f, 0.1f);
                skillsRect.sizeDelta = new Vector2(400, 80); skillsRect.anchoredPosition = Vector2.zero;
                var hgSkills = skillsGo.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                hgSkills.childAlignment = TextAnchor.MiddleCenter; hgSkills.childControlWidth = false; hgSkills.childControlHeight = false; hgSkills.spacing = 20;
                comp.skillsContainer = skillsGo.transform;

                EditorUtility.SetDirty(controller);
                EditorUtility.SetDirty(comp);
                Debug.Log("Gacha Unit Reward Modal construído e anexado ao Controller com sucesso!");
            }
        }

        [Button("7. Fix & Instantiate Prefab Modals", ButtonSizes.Large)]
        [GUIColor(0.2f, 0.9f, 0.4f)]
        private void FixPrefabModals()
        {
            var controller = FindObjectOfType<GachaAnimationController>();
            if (controller == null)
            {
                Debug.LogError("GachaAnimationController não encontrado!");
                return;
            }

            bool changed = false;

            // Busca automática caso estejam vazios
            if (controller.artifactActionModal == null) controller.artifactActionModal = FindPrefabOfType<CelestialCross.Scenes.Unit.ArtifactActionModal>();
            if (controller.artifactUpgradeModal == null) controller.artifactUpgradeModal = FindPrefabOfType<CelestialCross.Giulia_UI.ArtifactUpgradeModal>();
            if (controller.gachaUnitRewardModal != null && controller.gachaUnitRewardModal.petSkillModal == null)
                controller.gachaUnitRewardModal.petSkillModal = FindPrefabOfType<CelestialCross.Scenes.Inventory.PetSkillModal>();

            if (controller.artifactActionModal != null && PrefabUtility.IsPartOfPrefabAsset(controller.artifactActionModal))
            {
                var instance = (CelestialCross.Scenes.Unit.ArtifactActionModal)PrefabUtility.InstantiatePrefab(controller.artifactActionModal, controller.transform);
                instance.gameObject.SetActive(false);
                controller.artifactActionModal = instance;
                changed = true;
            }
            if (controller.artifactUpgradeModal != null && PrefabUtility.IsPartOfPrefabAsset(controller.artifactUpgradeModal))
            {
                var instance = (CelestialCross.Giulia_UI.ArtifactUpgradeModal)PrefabUtility.InstantiatePrefab(controller.artifactUpgradeModal, controller.transform);
                instance.gameObject.SetActive(false);
                controller.artifactUpgradeModal = instance;
                changed = true;
            }
            if (controller.gachaUnitRewardModal != null && controller.gachaUnitRewardModal.petSkillModal != null && PrefabUtility.IsPartOfPrefabAsset(controller.gachaUnitRewardModal.petSkillModal))
            {
                var instance = (CelestialCross.Scenes.Inventory.PetSkillModal)PrefabUtility.InstantiatePrefab(controller.gachaUnitRewardModal.petSkillModal, controller.transform);
                instance.gameObject.SetActive(false);
                controller.gachaUnitRewardModal.petSkillModal = instance;
                EditorUtility.SetDirty(controller.gachaUnitRewardModal);
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(controller);
                Debug.Log("Prefabs localizados magicamente, instanciados e atribuídos com sucesso!");
            }
            else
            {
                Debug.Log("Todos os modais já estão instanciados na cena corretamente.");
            }
        }

        private T FindPrefabOfType<T>() where T : MonoBehaviour
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null)
                {
                    T comp = go.GetComponent<T>();
                    if (comp != null) return comp;
                }
            }
            return null;
        }
    }
}