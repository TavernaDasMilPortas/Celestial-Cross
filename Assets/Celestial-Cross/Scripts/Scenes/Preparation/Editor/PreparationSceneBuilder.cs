#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Preparation;

namespace CelestialCross.EditorScripts
{
    public class PreparationSceneBuilder : EditorWindow
    {
        [MenuItem("Tools/Celestial Cross/Build Preparation Scene UI")]
        public static void BuildUI()
        {
            // Create Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Create EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Clear old PrepUI if it exists to regenerate
            Transform oldUI = canvas.transform.Find("PreparationUI_Generated");
            if (oldUI != null)
            {
                DestroyImmediate(oldUI.gameObject);
            }

            // Main Panel
            GameObject mainPanel = new GameObject("PreparationUI_Generated");
            mainPanel.transform.SetParent(canvas.transform, false);
            RectTransform mainRect = mainPanel.AddComponent<RectTransform>();
            mainRect.anchorMin = Vector2.zero;
            mainRect.anchorMax = Vector2.one;
            mainRect.sizeDelta = Vector2.zero;
            Image mainImg = mainPanel.AddComponent<Image>();
            mainImg.color = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark background

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(mainPanel.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -50);
            titleRect.sizeDelta = new Vector2(0, 80);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Prepare Your Team";
            titleText.fontSize = 50;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            // Owned Units Area
            GameObject ownedArea = new GameObject("OwnedUnitsArea");
            ownedArea.transform.SetParent(mainPanel.transform, false);
            RectTransform ownedRect = ownedArea.AddComponent<RectTransform>();
            ownedRect.anchorMin = new Vector2(0.05f, 0.3f);
            ownedRect.anchorMax = new Vector2(0.95f, 0.85f);
            ownedRect.sizeDelta = Vector2.zero;
            Image ownedImg = ownedArea.AddComponent<Image>();
            ownedImg.color = new Color(0, 0, 0, 0.5f);

            GameObject ownedGrid = new GameObject("Grid");
            ownedGrid.transform.SetParent(ownedArea.transform, false);
            RectTransform ownedGridRect = ownedGrid.AddComponent<RectTransform>();
            ownedGridRect.anchorMin = Vector2.zero;
            ownedGridRect.anchorMax = Vector2.one;
            ownedGridRect.sizeDelta = Vector2.zero;
            GridLayoutGroup ownedGLG = ownedGrid.AddComponent<GridLayoutGroup>();
            ownedGLG.cellSize = new Vector2(120, 120);
            ownedGLG.spacing = new Vector2(20, 20);
            ownedGLG.padding = new RectOffset(20, 20, 20, 20);
            ownedGLG.childAlignment = TextAnchor.UpperCenter;

            // Selected Units Area
            GameObject selectedArea = new GameObject("SelectedUnitsArea");
            selectedArea.transform.SetParent(mainPanel.transform, false);
            RectTransform selectedRect = selectedArea.AddComponent<RectTransform>();
            selectedRect.anchorMin = new Vector2(0.05f, 0.1f);
            selectedRect.anchorMax = new Vector2(0.7f, 0.25f);
            selectedRect.sizeDelta = Vector2.zero;
            Image selectedImg = selectedArea.AddComponent<Image>();
            selectedImg.color = new Color(0, 0, 0, 0.8f);

            GameObject selectedGrid = new GameObject("Grid");
            selectedGrid.transform.SetParent(selectedArea.transform, false);
            RectTransform selectedGridRect = selectedGrid.AddComponent<RectTransform>();
            selectedGridRect.anchorMin = Vector2.zero;
            selectedGridRect.anchorMax = Vector2.one;
            selectedGridRect.sizeDelta = Vector2.zero;
            GridLayoutGroup selectedGLG = selectedGrid.AddComponent<GridLayoutGroup>();
            selectedGLG.cellSize = new Vector2(100, 100);
            selectedGLG.spacing = new Vector2(15, 15);
            selectedGLG.padding = new RectOffset(15, 15, 15, 15);
            selectedGLG.childAlignment = TextAnchor.MiddleLeft;

            // Start Battle Button
            GameObject startBtnObj = new GameObject("StartBattleButton");
            startBtnObj.transform.SetParent(mainPanel.transform, false);
            RectTransform startBtnRect = startBtnObj.AddComponent<RectTransform>();
            startBtnRect.anchorMin = new Vector2(0.75f, 0.1f);
            startBtnRect.anchorMax = new Vector2(0.95f, 0.25f);
            startBtnRect.sizeDelta = Vector2.zero;
            Image startBtnImg = startBtnObj.AddComponent<Image>();
            startBtnImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            Button startBtn = startBtnObj.AddComponent<Button>();

            GameObject startBtnTextObj = new GameObject("Text");
            startBtnTextObj.transform.SetParent(startBtnObj.transform, false);
            RectTransform startBtnTextRect = startBtnTextObj.AddComponent<RectTransform>();
            startBtnTextRect.anchorMin = Vector2.zero;
            startBtnTextRect.anchorMax = Vector2.one;
            startBtnTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI startBtnText = startBtnTextObj.AddComponent<TextMeshProUGUI>();
            startBtnText.text = "START BATTLE";
            startBtnText.fontSize = 30;
            startBtnText.alignment = TextAlignmentOptions.Center;
            startBtnText.color = Color.white;

            // Count Text
            GameObject countTextObj = new GameObject("CountText");
            countTextObj.transform.SetParent(selectedArea.transform, false);
            RectTransform countTextRect = countTextObj.AddComponent<RectTransform>();
            countTextRect.anchorMin = new Vector2(0, 1);
            countTextRect.anchorMax = new Vector2(1, 1);
            countTextRect.pivot = new Vector2(0.5f, 0);
            countTextRect.anchoredPosition = new Vector2(0, 5);
            countTextRect.sizeDelta = new Vector2(0, 30);
            TextMeshProUGUI countText = countTextObj.AddComponent<TextMeshProUGUI>();
            countText.text = "Selecionadas: 0/3";
            countText.fontSize = 24;
            countText.alignment = TextAlignmentOptions.Left;
            countText.color = Color.white;

            // Create Button Prefab locally in the scene (can be prefabbified later by user)
            GameObject unitBtnObj = new GameObject("UnitButtonPrefab");
            unitBtnObj.transform.SetParent(mainPanel.transform, false);
            unitBtnObj.SetActive(false); // Hide the prefab template
            RectTransform unitBtnRect = unitBtnObj.AddComponent<RectTransform>();
            unitBtnRect.sizeDelta = new Vector2(120, 120);
            Image unitBtnImg = unitBtnObj.AddComponent<Image>();
            unitBtnImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            Button unitBtn = unitBtnObj.AddComponent<Button>();
            
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(unitBtnObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.25f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.sizeDelta = Vector2.zero;
            Image iconImg = iconObj.AddComponent<Image>();

            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(unitBtnObj.transform, false);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0.2f);
            nameRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Unit Name";
            nameText.fontSize = 16;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;

            GameObject outlineObj = new GameObject("SelectionOutline");
            outlineObj.transform.SetParent(unitBtnObj.transform, false);
            RectTransform outlineRect = outlineObj.AddComponent<RectTransform>();
            outlineRect.anchorMin = Vector2.zero;
            outlineRect.anchorMax = Vector2.one;
            outlineRect.sizeDelta = Vector2.zero;
            Image outlineImg = outlineObj.AddComponent<Image>();
            outlineImg.color = new Color(1f, 0.8f, 0f, 1f); // Gold outline
            outlineObj.SetActive(false);

            PreparationUnitButtonUI prepBtnUI = unitBtnObj.AddComponent<PreparationUnitButtonUI>();
            prepBtnUI.button = unitBtn;
            prepBtnUI.iconImage = iconImg;
            prepBtnUI.nameText = nameText;
            prepBtnUI.selectionOutline = outlineObj;
            prepBtnUI.canvasGroup = unitBtnObj.AddComponent<CanvasGroup>();

            // Ensure PreparationSceneController exists
            PreparationSceneController controller = FindObjectOfType<PreparationSceneController>();
            if (controller == null)
            {
                GameObject controllerObj = new GameObject("PreparationSceneController");
                controller = controllerObj.AddComponent<PreparationSceneController>();
            }

            // Bind everything using SerializedObject
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("ownedUnitsContainer").objectReferenceValue = ownedGrid.transform;
            so.FindProperty("ownedUnitButtonPrefab").objectReferenceValue = prepBtnUI;
            so.FindProperty("selectedUnitsContainer").objectReferenceValue = selectedGrid.transform;
            so.FindProperty("selectedUnitButtonPrefab").objectReferenceValue = prepBtnUI;
            so.FindProperty("selectedCountText").objectReferenceValue = countText;
            so.FindProperty("startBattleButton").objectReferenceValue = startBtn;
            so.ApplyModifiedProperties();

            Debug.Log("[PreparationSceneBuilder] UI gerada com sucesso! Prefab template e contêineres foram linkados ao controller.");
            
            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}
#endif
