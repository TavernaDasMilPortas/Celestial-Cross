using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Scenes.Hub;

namespace CelestialCross.UI.Builders
{
    public class HubUIBuilder
    {
        [MenuItem("Celestial Cross/3. UI Builders/1. Screens/Hub Navigation UI")]
        public static void GenerateHubUI()
        {
            // 1. Setup Canvas
            var canvasGo = new GameObject("HubUI_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // 2. Main Container
            var container = CreatePanel("Container", canvasGo.transform, new Color(0, 0, 0, 0.8f));
            var containerRt = container.GetComponent<RectTransform>();
            Stretch(containerRt);

            // 3. Top Bar
            var topBar = CreatePanel("TopBar", container.transform, new Color(0, 0, 0, 0.5f));
            var topBarRt = topBar.GetComponent<RectTransform>();
            topBarRt.anchorMin = new Vector2(0, 1);
            topBarRt.anchorMax = new Vector2(1, 1);
            topBarRt.pivot = new Vector2(0.5f, 1);
            topBarRt.sizeDelta = new Vector2(0, 150);
            topBarRt.anchoredPosition = Vector2.zero;

            var txtMoney = CreateText("Txt_Money", topBar.transform, "Dinheiro: 0", 40, new Vector2(-350, 0));
            var txtEnergy = CreateText("Txt_Energy", topBar.transform, "Energia: 0", 40, new Vector2(0, 0));

            var btnInv = CreateButton("Btn_Inventory", topBar.transform, "Inventário", new Vector2(200, 100), new Vector2(100, 0));
            var btnUnit = CreateButton("Btn_Unit", topBar.transform, "Unidades", new Vector2(200, 100), new Vector2(320, 0));
            var btnShop = CreateButton("Btn_Shop", topBar.transform, "Loja", new Vector2(200, 100), new Vector2(540, 0));

            // 4. Stack Panel
            var stackPanel = CreatePanel("StackPanel", container.transform, new Color(0, 0, 0, 0f));
            var stackPanelRt = stackPanel.GetComponent<RectTransform>();
            Stretch(stackPanelRt);
            stackPanelRt.offsetMax = new Vector2(0, -150); // Below TopBar

            var titleText = CreateText("StackTitle", stackPanel.transform, "Modo de Jogo", 60, new Vector2(0, 750));
            var btnBack = CreateButton("Btn_Back", stackPanel.transform, "Voltar", new Vector2(200, 100), new Vector2(-400, 750));

            var scrollView = CreateScrollView("StackScrollView", stackPanel.transform);

            // 5. Bottom Sheet
            var bottomSheet = CreatePanel("BottomSheet", container.transform, new Color(0.1f, 0.1f, 0.1f, 1f));
            var bsRt = bottomSheet.GetComponent<RectTransform>();
            bsRt.anchorMin = new Vector2(0, 0);
            bsRt.anchorMax = new Vector2(1, 0);
            bsRt.pivot = new Vector2(0.5f, 0);
            bsRt.sizeDelta = new Vector2(0, 800);
            bsRt.anchoredPosition = new Vector2(0, -1000); // Hidden by default

            // Header Elements
            var bsIconGo = new GameObject("Icon", typeof(RectTransform), typeof(TheraBytes.BetterUi.BetterImage));
            bsIconGo.transform.SetParent(bottomSheet.transform, false);
            var bsIconRt = bsIconGo.GetComponent<RectTransform>();
            bsIconRt.sizeDelta = new Vector2(150, 150);
            bsIconRt.anchoredPosition = new Vector2(-350, 300);

            var bsTitle = CreateText("BSTitle", bottomSheet.transform, "Node Title", 50, new Vector2(50, 300));
            
            // Info Texts
            var bsEnergy = CreateText("BSEnergy", bottomSheet.transform, "Energia: 0", 35, new Vector2(0, 200));
            var bsAttempts = CreateText("BSAttempts", bottomSheet.transform, "Tentativas: Ilimitadas", 35, new Vector2(0, 150));
            var bsReset = CreateText("BSReset", bottomSheet.transform, "Reseta: Nunca", 35, new Vector2(0, 100));

            // Containers
            var bsCostsContainerGo = new GameObject("CostsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            bsCostsContainerGo.transform.SetParent(bottomSheet.transform, false);
            var costsRt = bsCostsContainerGo.GetComponent<RectTransform>();
            costsRt.sizeDelta = new Vector2(800, 100);
            costsRt.anchoredPosition = new Vector2(0, 0);
            var costsHlg = bsCostsContainerGo.GetComponent<HorizontalLayoutGroup>();
            costsHlg.childAlignment = TextAnchor.MiddleCenter;
            costsHlg.spacing = 20;

            var bsRewardsContainerGo = new GameObject("RewardsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            bsRewardsContainerGo.transform.SetParent(bottomSheet.transform, false);
            var rewardsRt = bsRewardsContainerGo.GetComponent<RectTransform>();
            rewardsRt.sizeDelta = new Vector2(800, 150);
            rewardsRt.anchoredPosition = new Vector2(0, -150);
            var rewardsHlg = bsRewardsContainerGo.GetComponent<HorizontalLayoutGroup>();
            rewardsHlg.childAlignment = TextAnchor.MiddleCenter;
            rewardsHlg.spacing = 20;

            // Buttons
            var btnBsStart = CreateButton("Btn_Start", bottomSheet.transform, "Iniciar", new Vector2(400, 120), new Vector2(250, -320));
            var btnBsClose = CreateButton("Btn_Close", bottomSheet.transform, "Fechar", new Vector2(400, 120), new Vector2(-250, -320));

            // Prefabs for Bottom Sheet
            var itemCostPrefab = CreateButton("ItemCostPrefab", canvasGo.transform, "Item", new Vector2(200, 80), Vector2.zero);
            itemCostPrefab.SetActive(false);
            var rewardIconPrefab = new GameObject("RewardIconPrefab", typeof(RectTransform), typeof(TheraBytes.BetterUi.BetterImage));
            rewardIconPrefab.transform.SetParent(canvasGo.transform, false);
            rewardIconPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            rewardIconPrefab.SetActive(false);

            // Attach BottomSheetController
            var bsController = bottomSheet.AddComponent<BottomSheetController>();
            bsController.panelTransform = bsRt;
            bsController.nodeIcon = bsIconGo.GetComponent<TheraBytes.BetterUi.BetterImage>();
            bsController.titleText = bsTitle.GetComponent<TMP_Text>();
            bsController.energyText = bsEnergy.GetComponent<TMP_Text>();
            bsController.attemptsText = bsAttempts.GetComponent<TMP_Text>();
            bsController.resetText = bsReset.GetComponent<TMP_Text>();
            bsController.rewardsContainer = bsRewardsContainerGo.transform;
            bsController.itemCostsContainer = bsCostsContainerGo.transform;
            bsController.itemCostPrefab = itemCostPrefab;
            bsController.rewardIconPrefab = rewardIconPrefab;
            bsController.btnStart = btnBsStart.GetComponent<Button>();
            bsController.btnClose = btnBsClose.GetComponent<Button>();

            // 6. Generic Card Prefab
            var cardPrefab = CreateCardPrefab("HubCardPrefab", canvasGo.transform);
            cardPrefab.SetActive(false);

            // 7. Auto-configure Controller
            var controller = Object.FindObjectOfType<HubSceneController>();
            if (controller != null)
            {
                var so = new SerializedObject(controller);
                
                so.FindProperty("stackPanel").objectReferenceValue = stackPanel;
                so.FindProperty("stackTitleText").objectReferenceValue = titleText.GetComponent<TMP_Text>();
                so.FindProperty("stackContentContainer").objectReferenceValue = scrollView.transform.Find("Viewport/Content");
                so.FindProperty("btnBack").objectReferenceValue = btnBack.GetComponent<Button>();
                
                so.FindProperty("btnGoInventory").objectReferenceValue = btnInv.GetComponent<Button>();
                so.FindProperty("btnGoUnit").objectReferenceValue = btnUnit.GetComponent<Button>();
                so.FindProperty("btnGoShop").objectReferenceValue = btnShop.GetComponent<Button>();
                
                so.FindProperty("moneyText").objectReferenceValue = txtMoney.GetComponent<TMP_Text>();
                so.FindProperty("energyText").objectReferenceValue = txtEnergy.GetComponent<TMP_Text>();

                so.FindProperty("genericCardPrefab").objectReferenceValue = cardPrefab.GetComponent<HubCardUI>();
                so.FindProperty("bottomSheet").objectReferenceValue = bsController;

                so.ApplyModifiedProperties();
                Debug.Log("[HubUIBuilder] HubSceneController configurado com sucesso para Stack Navigation!");
            }

            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Hub UI");
            Selection.activeGameObject = canvasGo;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TheraBytes.BetterUi.BetterImage));
            go.transform.SetParent(parent, false);
            go.GetComponent<TheraBytes.BetterUi.BetterImage>().color = color;
            return go;
        }

        private static GameObject CreateText(string name, Transform parent, string textStr, int fontSize, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TMP_Text));
            go.transform.SetParent(parent, false);
            
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(800, 150);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = textStr;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            return go;
        }

        private static GameObject CreateButton(string name, Transform parent, string label, Vector2 size, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TheraBytes.BetterUi.BetterImage), typeof(TheraBytes.BetterUi.BetterButton));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            
            go.GetComponent<TheraBytes.BetterUi.BetterImage>().color = new Color(0.2f, 0.4f, 0.6f, 1f);

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TMP_Text));
            txtGo.transform.SetParent(go.transform, false);
            Stretch(txtGo.GetComponent<RectTransform>());

            var text = txtGo.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 40;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            return go;
        }

        private static GameObject CreateScrollView(string name, Transform parent)
        {
            var svGo = new GameObject(name, typeof(RectTransform), typeof(TheraBytes.BetterUi.BetterImage), typeof(ScrollRect));
            svGo.transform.SetParent(parent, false);
            var svRt = (RectTransform)svGo.transform;
            svRt.sizeDelta = new Vector2(900, 1300);
            svRt.anchoredPosition = new Vector2(0, 0);
            svGo.GetComponent<TheraBytes.BetterUi.BetterImage>().color = new Color(0, 0, 0, 0.5f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(TheraBytes.BetterUi.BetterImage), typeof(Mask));
            viewport.transform.SetParent(svGo.transform, false);
            var viewportRt = (RectTransform)viewport.transform;
            Stretch(viewportRt);
            viewport.GetComponent<Mask>().showMaskGraphic = false;
            viewport.GetComponent<TheraBytes.BetterUi.BetterImage>().color = Color.white;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = (RectTransform)content.transform;
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.spacing = 20;
            vlg.childControlHeight = false; 
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            
            var csf = content.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = svGo.GetComponent<ScrollRect>();
            sr.content = contentRt;
            sr.viewport = viewportRt;
            sr.horizontal = false;
            sr.vertical = true;

            return svGo;
        }

        private static GameObject CreateCardPrefab(string name, Transform parent)
        {
            var go = CreateButton(name, parent, "", new Vector2(900, 200), Vector2.zero);
            var card = go.AddComponent<HubCardUI>();
            card.buttonComponent = go.GetComponent<Button>();

            // Icon Image
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(TheraBytes.BetterUi.BetterImage));
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(150, 150);
            iconRt.anchoredPosition = new Vector2(-350, 0);
            card.iconImage = iconGo.GetComponent<TheraBytes.BetterUi.BetterImage>();

            // Title & Subtitle
            var title = CreateText("Title", go.transform, "Card Title", 45, new Vector2(50, 40));
            title.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Left;
            card.titleText = title.GetComponent<TMP_Text>();

            var sub = CreateText("Subtitle", go.transform, "Subtitle", 30, new Vector2(50, -40));
            sub.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Left;
            card.subtitleText = sub.GetComponent<TMP_Text>();

            // Energy Text
            var energy = CreateText("EnergyText", go.transform, "⚡ 10", 35, new Vector2(300, 40));
            energy.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Right;
            card.energyText = energy.GetComponent<TMP_Text>();

            // Progress Bar (BG + Fill)
            var progBg = CreatePanel("ProgressBG", go.transform, new Color(0, 0, 0, 0.5f));
            var progBgRt = progBg.GetComponent<RectTransform>();
            progBgRt.sizeDelta = new Vector2(300, 20);
            progBgRt.anchoredPosition = new Vector2(250, -40);

            var progFill = CreatePanel("ProgressFill", progBg.transform, new Color(0.2f, 0.8f, 0.2f, 1f));
            var progFillRt = progFill.GetComponent<RectTransform>();
            Stretch(progFillRt);
            var fillImg = progFill.GetComponent<TheraBytes.BetterUi.BetterImage>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 0.5f;
            card.progressBar = fillImg;

            // Status Icon
            var statusIconGo = new GameObject("StatusIcon", typeof(RectTransform), typeof(TheraBytes.BetterUi.BetterImage));
            statusIconGo.transform.SetParent(go.transform, false);
            var statusRt = statusIconGo.GetComponent<RectTransform>();
            statusRt.sizeDelta = new Vector2(80, 80);
            statusRt.anchoredPosition = new Vector2(400, 0);
            card.statusIcon = statusIconGo.GetComponent<TheraBytes.BetterUi.BetterImage>();

            // Lock Overlay
            var lockOverlay = CreatePanel("LockOverlay", go.transform, new Color(0, 0, 0, 0.7f));
            Stretch(lockOverlay.GetComponent<RectTransform>());
            card.lockOverlay = lockOverlay;

            return go;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
