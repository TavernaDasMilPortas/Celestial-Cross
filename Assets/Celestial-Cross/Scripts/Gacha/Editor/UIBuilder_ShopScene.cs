using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Gacha.Editor
{
    public class UIBuilder_ShopScene
    {
        [MenuItem("Celestial Cross/UI Builders/Generate Shop Scene Layout")]
        public static void GenerateShopUI()
        {
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("ShopUI_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                Debug.Log("[Shop UI Builder] Canvas criado.");
            }

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
                Debug.Log("[Shop UI Builder] EventSystem criado.");
            }

            // ==========================================
            // 1. ROOT
            // ==========================================
            GameObject rootUI = new GameObject("ShopUI_Root", typeof(RectTransform), typeof(Image));
            rootUI.transform.SetParent(canvas.transform, false);
            SetFullscreen(rootUI);
            rootUI.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f, 1f); // Fundo escuro

            var shopScript = rootUI.AddComponent<UI.ShopSceneUI>();
            SerializedObject so = new SerializedObject(shopScript);
            
            // ==========================================
            // 2. TOP BAR
            // ==========================================
            GameObject topBar = new GameObject("TopBar", typeof(RectTransform), typeof(Image));
            topBar.transform.SetParent(rootUI.transform, false);
            RectTransform topRt = topBar.GetComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0, 0.9f);
            topRt.anchorMax = new Vector2(1, 1);
            topRt.offsetMin = topRt.offsetMax = Vector2.zero;
            topBar.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 1f);

            var moneyTxt = CreateText(topBar.transform, "MoneyText", 24, new Vector2(0.85f, 0.2f), new Vector2(0.98f, 0.8f), Color.white);
            so.FindProperty("moneyText").objectReferenceValue = moneyTxt.GetComponent<TextMeshProUGUI>();

            var energyTxt = CreateText(topBar.transform, "EnergyText", 24, new Vector2(0.7f, 0.2f), new Vector2(0.83f, 0.8f), Color.white);
            // Ignore energy for shop

            var stardustTxt = CreateText(topBar.transform, "StardustText", 24, new Vector2(0.55f, 0.2f), new Vector2(0.68f, 0.8f), new Color(1f, 0.6f, 0f));
            so.FindProperty("stardustText").objectReferenceValue = stardustTxt.GetComponent<TextMeshProUGUI>();

            var mapsTxt = CreateText(topBar.transform, "StarMapsText", 24, new Vector2(0.4f, 0.2f), new Vector2(0.53f, 0.8f), new Color(0.2f, 0.8f, 1f));
            so.FindProperty("starMapsText").objectReferenceValue = mapsTxt.GetComponent<TextMeshProUGUI>();

            CreateText(topBar.transform, "Loja e InvocaÃ§Ãµes", 36, new Vector2(0.12f, 0.2f), new Vector2(0.3f, 0.8f), Color.yellow).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            var backHubGo = CreateButton(topBar.transform, "Btn_BackToHub", new Vector2(0.01f, 0.2f), new Vector2(0.1f, 0.8f), "Hub", new Color(0.8f, 0.4f, 0.2f));
            so.FindProperty("btnBackToHub").objectReferenceValue = backHubGo;
            so.FindProperty("hubSceneName").stringValue = "HubScene";

            // ==========================================
            // 3. TABS
            // ==========================================
            GameObject tabArea = new GameObject("TabsArea", typeof(RectTransform));
            tabArea.transform.SetParent(rootUI.transform, false);
            RectTransform tabRt = tabArea.GetComponent<RectTransform>();
            tabRt.anchorMin = new Vector2(0, 0.8f);
            tabRt.anchorMax = new Vector2(1, 0.9f);
            tabRt.offsetMin = tabRt.offsetMax = Vector2.zero;

            var btnBanners = CreateButton(tabArea.transform, "Btn_TabBanners", new Vector2(0.1f, 0.1f), new Vector2(0.3f, 0.9f), "Banners (Gacha)", new Color(0.5f, 0.2f, 0.8f));
            so.FindProperty("tabBannersBtn").objectReferenceValue = btnBanners;

            var btnExchange = CreateButton(tabArea.transform, "Btn_TabExchange", new Vector2(0.35f, 0.1f), new Vector2(0.55f, 0.9f), "Câmbio", new Color(0.2f, 0.5f, 0.8f));
            so.FindProperty("tabExchangeBtn").objectReferenceValue = btnExchange;

            // ==========================================
            // 4. CONTENT_BANNERS
            // ==========================================
            GameObject contentBanners = new GameObject("Content_Banners", typeof(RectTransform));
            contentBanners.transform.SetParent(rootUI.transform, false);
            SetAnchors(contentBanners.GetComponent<RectTransform>(), 0f, 0f, 1f, 0.8f);
            so.FindProperty("contentBanners").objectReferenceValue = contentBanners;

            // Splash Art
            GameObject splashGo = new GameObject("BannerSplashArt", typeof(RectTransform), typeof(Image));
            splashGo.transform.SetParent(contentBanners.transform, false);
            SetAnchors(splashGo.GetComponent<RectTransform>(), 0.1f, 0.1f, 0.9f, 0.95f);
            splashGo.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f); // Placeholder cinza
            so.FindProperty("bannerSplashArt").objectReferenceValue = splashGo.GetComponent<Image>();

            // Top Infos
            var bNameTxt = CreateText(splashGo.transform, "BannerNameTxt", 48, new Vector2(0.05f, 0.8f), new Vector2(0.95f, 0.95f), Color.white);
            bNameTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            so.FindProperty("bannerTitle").objectReferenceValue = bNameTxt.GetComponent<TextMeshProUGUI>();

            // Bottom Buttons
            var btnPull1 = CreateButton(splashGo.transform, "Btn_Pull1", new Vector2(0.5f, 0.05f), new Vector2(0.7f, 0.15f), "Rolar 1x", new Color(0.8f, 0.6f, 0.1f));
            so.FindProperty("btnPull1").objectReferenceValue = btnPull1;

            var btnPull10 = CreateButton(splashGo.transform, "Btn_Pull10", new Vector2(0.75f, 0.05f), new Vector2(0.95f, 0.15f), "Rolar 10x", new Color(0.9f, 0.2f, 0.2f));
            so.FindProperty("btnPull10").objectReferenceValue = btnPull10;

            var btnDetails = CreateButton(splashGo.transform, "Btn_Details", new Vector2(0.05f, 0.05f), new Vector2(0.2f, 0.1f), "Ver Detalhes", new Color(0.3f, 0.3f, 0.3f));
            so.FindProperty("btnDetails").objectReferenceValue = btnDetails;

            var pityInfoTxt = CreateText(splashGo.transform, "PityInfoTxt", 18, new Vector2(0.05f, 0.12f), new Vector2(0.4f, 0.22f), Color.yellow);
            pityInfoTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.BottomLeft;
            so.FindProperty("pityInfoText").objectReferenceValue = pityInfoTxt.GetComponent<TextMeshProUGUI>();

            var costInfoTxt = CreateText(splashGo.transform, "CostInfoTxt", 18, new Vector2(0.5f, 0.16f), new Vector2(0.95f, 0.25f), Color.white);
            costInfoTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.BottomRight;
            so.FindProperty("bannerCostInfo").objectReferenceValue = costInfoTxt.GetComponent<TextMeshProUGUI>();

            // ==========================================
            // 5. CONTENT_EXCHANGE
            // ==========================================
            GameObject contentExchange = new GameObject("Content_Exchange", typeof(RectTransform));
            contentExchange.transform.SetParent(rootUI.transform, false);
            SetAnchors(contentExchange.GetComponent<RectTransform>(), 0f, 0f, 1f, 0.8f);
            so.FindProperty("contentExchange").objectReferenceValue = contentExchange;

            CreateText(contentExchange.transform, "InfoExchangeTxt", 36, new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.9f), Color.white).GetComponent<TextMeshProUGUI>().text = "Câmbio de Moedas";

            var btnExchangeDust = CreateButton(contentExchange.transform, "Btn_Convert", new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.6f), "Trocar 100 Poeira por 1 Mapa das Estrelas", new Color(0.2f, 0.6f, 0.2f));
            so.FindProperty("btnConvertStardustToStarMaps").objectReferenceValue = btnExchangeDust;
            
            contentExchange.SetActive(false);

            // ==========================================
            // 6. GACHA RESULT MODAL
            // ==========================================
            GameObject resModal = new GameObject("GachaResultModal", typeof(RectTransform), typeof(Image));
            resModal.transform.SetParent(rootUI.transform, false);
            SetFullscreen(resModal);
            resModal.GetComponent<Image>().color = new Color(0, 0, 0, 0.9f);
            so.FindProperty("resultModal").objectReferenceValue = resModal;

            CreateText(resModal.transform, "TitleTxt", 48, new Vector2(0, 0.85f), new Vector2(1, 0.95f), Color.yellow).GetComponent<TextMeshProUGUI>().text = "Sorte Grande!";

            var btnCloseRes = CreateButton(resModal.transform, "Btn_CloseResult", new Vector2(0.4f, 0.05f), new Vector2(0.6f, 0.15f), "Continuar", new Color(0.3f, 0.3f, 0.3f));
            so.FindProperty("resultCloseBtn").objectReferenceValue = btnCloseRes;

            GameObject gridResult = new GameObject("ResultGridArea", typeof(RectTransform));
            gridResult.transform.SetParent(resModal.transform, false);
            SetAnchors(gridResult.GetComponent<RectTransform>(), 0.1f, 0.2f, 0.9f, 0.8f);
            var glg = gridResult.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(150, 200); // Cards grandes verticais
            glg.spacing = new Vector2(25, 25);
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.MiddleCenter;
            so.FindProperty("resultGridContent").objectReferenceValue = gridResult.transform;

            GameObject resItemPrefab = new GameObject("GachaResultItem_Prefab", typeof(RectTransform), typeof(Image));
            resItemPrefab.transform.SetParent(gridResult.transform, false);
            resItemPrefab.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f); // Fundo da cartinha
            CreateText(resItemPrefab.transform, "CardNameTxt", 20, new Vector2(0, 0.4f), new Vector2(1, 0.9f), Color.white);
            CreateText(resItemPrefab.transform, "CardRarityTxt", 16, new Vector2(0, 0), new Vector2(1, 0.3f), Color.yellow);
            so.FindProperty("resultItemPrefab").objectReferenceValue = resItemPrefab;
            
            resItemPrefab.SetActive(false);
            resModal.SetActive(false);

            // ==========================================
            // APPLY & SELECTION
            // ==========================================
            so.ApplyModifiedProperties();
            Selection.activeGameObject = rootUI;
            Debug.Log("[Shop UI Builder] UI Base da Shop Scene gerada com sucesso!");
        }

        private static GameObject CreateText(Transform parent, string name, int fontSize, Vector2 aMin, Vector2 aMax, Color col)
        {
            GameObject txtGo = new GameObject(name, typeof(RectTransform));
            txtGo.transform.SetParent(parent, false);
            SetAnchors(txtGo.GetComponent<RectTransform>(), aMin.x, aMin.y, aMax.x, aMax.y);

            var t = txtGo.AddComponent<TextMeshProUGUI>();
            t.fontSize = fontSize;
            t.color = col;
            t.alignment = TextAlignmentOptions.Center;
            t.enableWordWrapping = true;
            return txtGo;
        }

        private static Button CreateButton(Transform parent, string name, Vector2 aMin, Vector2 aMax, string label, Color col)
        {
            GameObject btnGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);
            SetAnchors(btnGo.GetComponent<RectTransform>(), aMin.x, aMin.y, aMax.x, aMax.y);
            
            btnGo.GetComponent<Image>().color = col;
            Button btn = btnGo.GetComponent<Button>();

            var txtGo = CreateText(btnGo.transform, "Text", 24, Vector2.zero, Vector2.one, Color.white);
            txtGo.GetComponent<TextMeshProUGUI>().text = label;

            return btn;
        }

        private static void SetFullscreen(GameObject go)
        {
            SetAnchors(go.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f);
        }

        private static void SetAnchors(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
        {
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}