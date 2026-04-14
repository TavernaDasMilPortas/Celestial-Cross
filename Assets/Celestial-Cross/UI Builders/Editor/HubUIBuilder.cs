using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.UI.Builders
{
    public class HubUIBuilder
    {
        [MenuItem("Tools/UI Builders/Generate Hub Navigation UI")]
        public static void GenerateHubUI()
        {
            // 1. Setup Canvas
            var canvasGo = new GameObject("HubUI_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            // 2. Main Container
            var container = CreatePanel("Container", canvasGo.transform, new Color(0, 0, 0, 0.8f));
            var containerRt = container.GetComponent<RectTransform>();
            Stretch(containerRt);

            // Top Bar
            var topBar = CreatePanel("TopBar", container.transform, new Color(0, 0, 0, 0.5f));
            var topBarRt = topBar.GetComponent<RectTransform>();
            topBarRt.anchorMin = new Vector2(0, 1);
            topBarRt.anchorMax = new Vector2(1, 1);
            topBarRt.pivot = new Vector2(0.5f, 1);
            topBarRt.sizeDelta = new Vector2(0, 150);
            topBarRt.anchoredPosition = Vector2.zero;

            var txtMoney = CreateText("Txt_Money", topBar.transform, "Dinheiro: 0", 40, new Vector2(-250, 0));
            var txtEnergy = CreateText("Txt_Energy", topBar.transform, "Energia: 0", 40, new Vector2(250, 0));

            // 3. Main Panel
            var mainPanel = CreatePanel("MainPanel", container.transform, new Color(0, 0, 0, 0f));
            Stretch(mainPanel.GetComponent<RectTransform>());

            CreateText("Title", mainPanel.transform, "Escolha o Modo", 60, new Vector2(0, 600));

            var categoriesContainerGo = new GameObject("CategoriesContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            categoriesContainerGo.transform.SetParent(mainPanel.transform, false);
            var categoriesContainerRt = (RectTransform)categoriesContainerGo.transform;
            categoriesContainerRt.sizeDelta = new Vector2(900, 800);
            categoriesContainerRt.anchoredPosition = new Vector2(0, 50);
            var catVlg = categoriesContainerGo.GetComponent<VerticalLayoutGroup>();
            catVlg.padding = new RectOffset(20, 20, 20, 20);
            catVlg.spacing = 20;
            catVlg.childControlWidth = true;
            catVlg.childControlHeight = false;

            var btnInv = CreateButton("Btn_Inventory", mainPanel.transform, "Inventário", new Vector2(500, 120), new Vector2(0, -600));

            // 4. Dungeons Panel
            var dungeonsPanel = CreatePanel("DungeonsPanel", container.transform, new Color(0, 0, 0.2f, 0.9f));
            Stretch(dungeonsPanel.GetComponent<RectTransform>());
            dungeonsPanel.SetActive(false);

            var titleDung = CreateText("Title", dungeonsPanel.transform, "Selecione uma Masmorra", 60, new Vector2(0, 600));
            var dungeonsScrollView = CreateScrollView("DungeonsScrollView", dungeonsPanel.transform);
            var btnBackDung = CreateButton("Btn_Back", dungeonsPanel.transform, "Voltar", new Vector2(500, 120), new Vector2(0, -700));

            // 5. Levels Panel
            var levelsPanel = CreatePanel("LevelsPanel", container.transform, new Color(0.2f, 0, 0, 0.9f));
            Stretch(levelsPanel.GetComponent<RectTransform>());
            levelsPanel.SetActive(false);

            var titleLev = CreateText("Title", levelsPanel.transform, "Selecione a Fase", 60, new Vector2(0, 600));
            var levelsScrollView = CreateScrollView("LevelsScrollView", levelsPanel.transform);
            var btnBackLevels = CreateButton("Btn_Back", levelsPanel.transform, "Voltar", new Vector2(500, 120), new Vector2(0, -700));

            // 6. Generic Button Prefab
            var genericBtn = CreateButton("GenericListButton", canvasGo.transform, "Item", new Vector2(800, 150), Vector2.zero);
            genericBtn.SetActive(false);

            // 7. Auto-configure Controller
            var controller = Object.FindObjectOfType<HubSceneController>();
            if (controller != null)
            {
                var so = new SerializedObject(controller);
                
                so.FindProperty("mainPanel").objectReferenceValue = mainPanel;
                so.FindProperty("dungeonsPanel").objectReferenceValue = dungeonsPanel;
                so.FindProperty("levelsPanel").objectReferenceValue = levelsPanel;
                
                so.FindProperty("dungeonsPanelTitle").objectReferenceValue = titleDung.GetComponent<TMP_Text>();
                so.FindProperty("levelsPanelTitle").objectReferenceValue = titleLev.GetComponent<TMP_Text>();
                
                so.FindProperty("mainCategoriesContainer").objectReferenceValue = categoriesContainerGo.transform;
                so.FindProperty("dungeonsContainer").objectReferenceValue = dungeonsScrollView.transform.Find("Viewport/Content");
                so.FindProperty("levelsContainer").objectReferenceValue = levelsScrollView.transform.Find("Viewport/Content");
                
                so.FindProperty("btnGoInventory").objectReferenceValue = btnInv.GetComponent<Button>();
                so.FindProperty("btnBackFromDungeons").objectReferenceValue = btnBackDung.GetComponent<Button>();
                so.FindProperty("btnBackFromLevels").objectReferenceValue = btnBackLevels.GetComponent<Button>();
                
                so.FindProperty("genericButtonPrefab").objectReferenceValue = genericBtn.GetComponent<Button>();

                // Assign the new Top Bar Texts
                so.FindProperty("moneyText").objectReferenceValue = txtMoney.GetComponent<TMP_Text>();
                so.FindProperty("energyText").objectReferenceValue = txtEnergy.GetComponent<TMP_Text>();

                so.ApplyModifiedProperties();
                Debug.Log("[HubUIBuilder] HubSceneController automaticamente configurado com Dinheiro e Energia!");
            }

            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Hub UI");
            Selection.activeGameObject = canvasGo;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static GameObject CreateText(string name, Transform parent, string textStr, int fontSize, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TMP_Text));
            go.transform.SetParent(parent, false);
            
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(900, 100);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = textStr;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            return go;
        }

        private static GameObject CreateButton(string name, Transform parent, string label, Vector2 size, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            
            go.GetComponent<Image>().color = new Color(0.2f, 0.4f, 0.6f, 1f);

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
            var svGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            svGo.transform.SetParent(parent, false);
            var svRt = (RectTransform)svGo.transform;
            svRt.sizeDelta = new Vector2(900, 1000);
            svRt.anchoredPosition = new Vector2(0, -100);
            svGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(svGo.transform, false);
            var viewportRt = (RectTransform)viewport.transform;
            Stretch(viewportRt);
            viewport.GetComponent<Mask>().showMaskGraphic = false;
            viewport.GetComponent<Image>().color = Color.white;

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

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
