using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Giulia_UI.Editor
{
    public static class VictoryRewardUIBuilder
    {
        [MenuItem("Celestial Cross/UI Builders/Generate Victory Reward UI")]
        public static void GenerateUI()
        {
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Root
            GameObject uiRoot = new GameObject("VictoryRewardUI_Container", typeof(RectTransform));
            uiRoot.transform.SetParent(canvas.transform, false);
            RectTransform rootRt = uiRoot.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            Image rootBg = uiRoot.AddComponent<Image>();
            rootBg.color = new Color(0, 0, 0, 0.85f);
            rootBg.raycastTarget = true;

            // Panel
            GameObject panel = new GameObject("MainPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(uiRoot.transform, false);
            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.1f, 0.1f);
            panelRt.anchorMax = new Vector2(0.9f, 0.9f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.18f, 1f);

            // Title
            var titleTxt = CreateText(panel.transform, "Vitória!", 36, new Vector2(0, 0.9f), new Vector2(1, 1f), Color.yellow);
            
            // Basics
            var moneyAndEnergyTxt = CreateText(panel.transform, "Dinheiro: +X   Energia: +Y", 24, new Vector2(0, 0.8f), new Vector2(1, 0.9f), Color.white);

            // Sub Title
            CreateText(panel.transform, "Artefatos Encontrados", 24, new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.78f), Color.white);

            // Scroll/Content Area
            GameObject scrollArea = new GameObject("ItemsScrollArea", typeof(RectTransform), typeof(Image));
            scrollArea.transform.SetParent(panel.transform, false);
            RectTransform scrollRt = scrollArea.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.05f, 0.2f);
            scrollRt.anchorMax = new Vector2(0.95f, 0.7f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;
            scrollArea.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f, 1f);

            // Grid
            GameObject itemsContent = new GameObject("ItemsContent", typeof(RectTransform));
            itemsContent.transform.SetParent(scrollArea.transform, false);
            RectTransform itemsRt = itemsContent.GetComponent<RectTransform>();
            itemsRt.anchorMin = Vector2.zero;
            itemsRt.anchorMax = Vector2.one;
            itemsRt.offsetMin = Vector2.zero;
            itemsRt.offsetMax = Vector2.zero;
            
            var glg = itemsContent.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(200, 100);
            glg.spacing = new Vector2(15, 15);
            glg.padding = new RectOffset(10, 10, 10, 10);
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperCenter;

            var noArtifactsTxt = CreateText(itemsContent.transform, "(Nenhum Artefato nesta tentativa)", 20, Vector2.zero, Vector2.one, Color.gray);
            noArtifactsTxt.gameObject.SetActive(false); // managed by script

            // Button
            GameObject btnGo = new GameObject("Btn_Continue", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(panel.transform, false);
            RectTransform btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.3f, 0.05f);
            btnRt.anchorMax = new Vector2(0.7f, 0.15f);
            btnRt.offsetMin = Vector2.zero;
            btnRt.offsetMax = Vector2.zero;
            btnGo.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.4f, 1f);
            CreateText(btnGo.transform, "Coletar e Voltar", 28, Vector2.zero, Vector2.one, Color.white);

            // Add the Main Component
            var uiScript = uiRoot.AddComponent<VictoryRewardUI>();
            var serializedObject = new SerializedObject(uiScript);
            serializedObject.FindProperty("rootContainer").objectReferenceValue = uiRoot;
            serializedObject.FindProperty("itemsContent").objectReferenceValue = itemsContent.transform;
            serializedObject.FindProperty("moneyAndEnergyText").objectReferenceValue = moneyAndEnergyTxt.GetComponent<TMP_Text>();
            serializedObject.FindProperty("noArtifactsText").objectReferenceValue = noArtifactsTxt.GetComponent<TMP_Text>();
            serializedObject.FindProperty("continueButton").objectReferenceValue = btnGo.GetComponent<Button>();

            // Let's create an artifact prefab proxy
            var fakeArtifactGo = new GameObject("ArtifactItem_PrefabProxy", typeof(RectTransform), typeof(Image));
            fakeArtifactGo.transform.SetParent(itemsContent.transform, false);
            fakeArtifactGo.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 100);
            fakeArtifactGo.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
            CreateText(fakeArtifactGo.transform, "SlotName (Set)\n1* Lv.1", 18, new Vector2(0, 0.5f), new Vector2(1, 1f), Color.white);
            CreateText(fakeArtifactGo.transform, "+123 Stat", 16, new Vector2(0, 0), new Vector2(1, 0.5f), Color.yellow);
            
            fakeArtifactGo.SetActive(false);
            serializedObject.FindProperty("artifactItemPrefab").objectReferenceValue = fakeArtifactGo;

            serializedObject.ApplyModifiedProperties();
            
            Selection.activeGameObject = uiRoot;
            Debug.Log("VictoryRewardUI generated! You can now adjust anchors and visuals, and ideally save it as a Prefab.");
        }

        private static GameObject CreateText(Transform parent, string content, int fontSize, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject txtGo = new GameObject("Text_" + content.Replace(" ", ""), typeof(RectTransform));
            txtGo.transform.SetParent(parent, false);
            RectTransform rt = txtGo.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var txt = txtGo.AddComponent<TextMeshProUGUI>();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = TextAlignmentOptions.Center;
            txt.enableWordWrapping = true;

            return txtGo;
        }
    }
}