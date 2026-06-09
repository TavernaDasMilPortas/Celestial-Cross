#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Scenes.Inventory;

namespace CelestialCross.UIBuilders
{
    public class UIBuilder_InventoryScene : EditorWindow
    {
        [MenuItem("Celestial Cross/UI Builders/Build Inventory Scene")]
        public static void BuildScene()
        {
            // Limpa canvas existente se houver para evitar duplicatas e conflitos de Singleton
            GameObject oldCanvas = GameObject.Find("Canvas_Inventory");
            if (oldCanvas != null)
            {
                Object.DestroyImmediate(oldCanvas);
            }

            // ──────────────────────────────────────────────
            // 1. CANVAS + EVENT SYSTEM
            // ──────────────────────────────────────────────
            GameObject canvasGO = new GameObject("Canvas_Inventory", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
            canvasRT.anchorMin = Vector2.zero;
            canvasRT.anchorMax = Vector2.one;
            canvasRT.offsetMin = Vector2.zero;
            canvasRT.offsetMax = Vector2.zero;
            canvasRT.localScale = Vector3.one;
            canvasRT.localPosition = Vector3.zero;
            canvasRT.anchoredPosition = Vector2.zero;

            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // ──────────────────────────────────────────────
            // 2. SCENE CONTROLLER + AUTO-LOAD CATALOGS
            // ──────────────────────────────────────────────
            InventorySceneController controller = canvasGO.AddComponent<InventorySceneController>();

            // Auto-carregar catálogos (padrão do projeto)
            string[] petGuids = AssetDatabase.FindAssets("t:PetCatalog");
            if (petGuids.Length > 0)
                controller.petCatalog = AssetDatabase.LoadAssetAtPath<PetCatalog>(AssetDatabase.GUIDToAssetPath(petGuids[0]));

            string[] artGuids = AssetDatabase.FindAssets("t:ArtifactSetCatalog");
            if (artGuids.Length > 0)
                controller.artifactSetCatalog = AssetDatabase.LoadAssetAtPath<ArtifactSetCatalog>(AssetDatabase.GUIDToAssetPath(artGuids[0]));

            // ──────────────────────────────────────────────
            // 3. BACKGROUND
            // ──────────────────────────────────────────────
            GameObject bgGO = CreateUIObject("Background", canvasGO.transform, stretch: true);
            Image bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.08f, 0.12f, 1f);
            bgImg.raycastTarget = false;

            // ──────────────────────────────────────────────
            // 4. BACK BUTTON (top-left, above tabs)
            // ──────────────────────────────────────────────
            GameObject backBtnGO = CreateButton("BackButton", canvasGO.transform, "< Voltar", 22);
            SetAnchors(backBtnGO, 0f, 0.94f, 0.12f, 1f, new RectOffset(15, 0, 8, 8));
            backBtnGO.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.22f, 0.9f);
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                backBtnGO.GetComponent<Button>().onClick,
                new UnityEngine.Events.UnityAction(controller.ReturnToHub));

            // ──────────────────────────────────────────────
            // 5. TAB BAR (top strip: 0.88 → 0.94)
            // ──────────────────────────────────────────────
            GameObject tabBarGO = CreateUIObject("TabBarContainer", canvasGO.transform);
            SetAnchors(tabBarGO, 0f, 0.88f, 1f, 0.94f, new RectOffset(0, 0, 0, 0));
            Image tabBarBg = tabBarGO.AddComponent<Image>();
            tabBarBg.color = new Color(0.12f, 0.12f, 0.18f, 1f);
            HorizontalLayoutGroup tabBarHLG = tabBarGO.AddComponent<HorizontalLayoutGroup>();
            tabBarHLG.childControlWidth = true;
            tabBarHLG.childControlHeight = true;
            tabBarHLG.childForceExpandWidth = true;
            tabBarHLG.spacing = 4;
            tabBarHLG.padding = new RectOffset(80, 80, 4, 4);
            controller.tabBarContainer = tabBarGO.GetComponent<RectTransform>();

            // ──────────────────────────────────────────────
            // 6. TOP PANEL (detail area: 0.52 → 0.88)
            // ──────────────────────────────────────────────
            GameObject topPanelGO = CreateUIObject("TopPanelContainer", canvasGO.transform);
            SetAnchors(topPanelGO, 0f, 0.52f, 1f, 0.88f, new RectOffset(15, 15, 10, 10));
            Image topPanelBg = topPanelGO.AddComponent<Image>();
            topPanelBg.color = new Color(0.13f, 0.13f, 0.19f, 0.95f);
            controller.topPanelContainer = topPanelGO.GetComponent<RectTransform>();

            // ──────────────────────────────────────────────
            // 7. GRID CONTAINER (scroll area: 0 → 0.52)
            // ──────────────────────────────────────────────
            GameObject gridContainerGO = CreateUIObject("GridContainer", canvasGO.transform);
            SetAnchors(gridContainerGO, 0f, 0f, 1f, 0.52f, new RectOffset(15, 15, 10, 10));
            controller.gridContainer = gridContainerGO.GetComponent<RectTransform>();

            // ──────────────────────────────────────────────
            // 8. MODALS CONTAINER (fullscreen overlay)
            // ──────────────────────────────────────────────
            GameObject modalsGO = CreateUIObject("ModalsContainer", canvasGO.transform, stretch: true);
            controller.modalContainer = modalsGO.GetComponent<RectTransform>();

            // ──────────────────────────────────────────────
            // 9. CREATE PREFABS
            // ──────────────────────────────────────────────
            GameObject slotPrefab = CreateSlotPrefab();
            controller.slotPrefab = slotPrefab;

            GameObject speciesIconPrefab = CreateSpeciesIconPrefab();
            GameObject filterIconPrefab = CreateFilterIconPrefab();

            // ──────────────────────────────────────────────
            // 10. BUILD TABS
            // ──────────────────────────────────────────────
            PetTabPanel petTab = BuildPetTab(controller, tabBarGO.transform, topPanelGO.transform, gridContainerGO.transform);
            ArtifactTabPanel artifactTab = BuildArtifactTab(controller, tabBarGO.transform, topPanelGO.transform, gridContainerGO.transform);
            ConsumableTabPanel consumableTab = BuildConsumableTab(controller, tabBarGO.transform, topPanelGO.transform, gridContainerGO.transform);
            ItemTabPanel itemTab = BuildItemTab(controller, tabBarGO.transform, topPanelGO.transform, gridContainerGO.transform);

            controller.tabPanels.Add(petTab);
            controller.tabPanels.Add(artifactTab);
            controller.tabPanels.Add(consumableTab);
            controller.tabPanels.Add(itemTab);

            // ──────────────────────────────────────────────
            // 10.5. PET RELEASE MANAGER
            // ──────────────────────────────────────────────
            CelestialCross.System.PetReleaseManager releaseMgr = canvasGO.AddComponent<CelestialCross.System.PetReleaseManager>();
            string[] releaseGuids = AssetDatabase.FindAssets("t:PetReleaseConfigSO");
            if (releaseGuids.Length > 0)
            {
                releaseMgr.ReleaseConfig = AssetDatabase.LoadAssetAtPath<CelestialCross.Data.Pets.PetReleaseConfigSO>(AssetDatabase.GUIDToAssetPath(releaseGuids[0]));
            }

            // Deativar todos os painéis e scrollviews de abas, exceto a primeira aba (Pets) por padrão
            if (artifactTab.topPanelContent != null) artifactTab.topPanelContent.SetActive(false);
            if (artifactTab.scrollView != null) artifactTab.scrollView.SetActive(false);

            if (consumableTab.topPanelContent != null) consumableTab.topPanelContent.SetActive(false);
            if (consumableTab.scrollView != null) consumableTab.scrollView.SetActive(false);

            if (itemTab.topPanelContent != null) itemTab.topPanelContent.SetActive(false);
            if (itemTab.scrollView != null) itemTab.scrollView.SetActive(false);

            if (petTab.topPanelContent != null) petTab.topPanelContent.SetActive(true);
            if (petTab.scrollView != null) petTab.scrollView.SetActive(true);

            // ──────────────────────────────────────────────
            // 11. BUILD MODALS
            // ──────────────────────────────────────────────
            BuildModals(modalsGO.transform, controller, speciesIconPrefab, filterIconPrefab, petTab);

            Selection.activeGameObject = canvasGO;
            
            // Marca como modificado e salva a cena para garantir a serialização
            EditorUtility.SetDirty(controller);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(controller.gameObject.scene);
            
            Debug.Log("[UIBuilder_InventoryScene] Cena construída com sucesso! Catálogos carregados automaticamente e salvos.");
        }

        // ================================================================
        // HELPER: Create a UI GameObject
        // ================================================================
        private static GameObject CreateUIObject(string name, Transform parent, bool stretch = false)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            if (stretch)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            return go;
        }

        // ================================================================
        // HELPER: Set anchors with padding offsets
        // ================================================================
        private static void SetAnchors(GameObject go, float aMinX, float aMinY, float aMaxX, float aMaxY, RectOffset padding)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(aMinX, aMinY);
            rt.anchorMax = new Vector2(aMaxX, aMaxY);
            rt.offsetMin = new Vector2(padding.left, padding.bottom);
            rt.offsetMax = new Vector2(-padding.right, -padding.top);
        }

        // ================================================================
        // HELPER: Create a styled button
        // ================================================================
        private static GameObject CreateButton(string name, Transform parent, string text, int fontSize = 24)
        {
            GameObject btnGO = CreateUIObject(name, parent);
            Image img = btnGO.AddComponent<Image>();
            img.color = new Color(0.22f, 0.22f, 0.32f, 1f);
            Button btn = btnGO.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.3f, 0.3f, 0.45f, 1f);
            cb.pressedColor = new Color(0.18f, 0.18f, 0.28f, 1f);
            btn.colors = cb;

            GameObject txtGO = CreateUIObject("Text", btnGO.transform, stretch: true);
            TextMeshProUGUI txt = txtGO.AddComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            txt.raycastTarget = false;

            return btnGO;
        }

        // ================================================================
        // HELPER: Create a text element with anchor-based positioning
        // ================================================================
        private static TextMeshProUGUI CreateText(string name, Transform parent, string defaultText, int fontSize,
            float aMinX, float aMinY, float aMaxX, float aMaxY, TextAlignmentOptions align = TextAlignmentOptions.Left)
        {
            GameObject go = CreateUIObject(name, parent);
            SetAnchors(go, aMinX, aMinY, aMaxX, aMaxY, new RectOffset(5, 5, 2, 2));
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        // ================================================================
        // PREFAB: Inventory Slot (grid item)
        // ================================================================
        private static GameObject CreateSlotPrefab()
        {
            GameObject slotGO = new GameObject("InventorySlotPrefab");
            RectTransform rt = slotGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(130, 150);

            Image bg = slotGO.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.28f, 1f);
            slotGO.AddComponent<Button>();

            // Icon — fills most of the slot
            GameObject iconGO = CreateUIObject("Icon", slotGO.transform);
            SetAnchors(iconGO, 0.05f, 0.2f, 0.95f, 0.95f, new RectOffset(0, 0, 0, 0));
            Image icon = iconGO.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            // Label — bottom strip
            GameObject labelGO = CreateUIObject("Label", slotGO.transform);
            SetAnchors(labelGO, 0f, 0f, 1f, 0.2f, new RectOffset(2, 2, 0, 0));
            TextMeshProUGUI txt = labelGO.AddComponent<TextMeshProUGUI>();
            txt.alignment = TextAlignmentOptions.Center;
            txt.fontSize = 14;
            txt.text = "";
            txt.color = new Color(0.8f, 0.8f, 0.8f);
            txt.raycastTarget = false;

            // Save as prefab
            EnsurePrefabDir();
            string path = "Assets/Celestial-Cross/Prefabs/UI/InventorySlotPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(slotGO, path);
            Object.DestroyImmediate(slotGO);
            return prefab;
        }

        // ================================================================
        // PREFAB: Species icon (for PetFilterModal)
        // ================================================================
        private static GameObject CreateSpeciesIconPrefab()
        {
            GameObject iconGO = new GameObject("SpeciesIconPrefab");
            RectTransform rt = iconGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90, 90);

            Image img = iconGO.AddComponent<Image>();
            img.preserveAspect = true;
            iconGO.AddComponent<Button>();

            // Selection outline (child 0 — toggled on/off)
            GameObject outline = CreateUIObject("SelectionOutline", iconGO.transform, stretch: true);
            Image outlineImg = outline.AddComponent<Image>();
            outlineImg.color = new Color(1f, 0.85f, 0.2f, 0.6f);
            outlineImg.raycastTarget = false;
            // Make outline slightly larger
            RectTransform oRT = outline.GetComponent<RectTransform>();
            oRT.offsetMin = new Vector2(-4, -4);
            oRT.offsetMax = new Vector2(4, 4);
            outline.SetActive(false);

            EnsurePrefabDir();
            string path = "Assets/Celestial-Cross/Prefabs/UI/SpeciesIconPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(iconGO, path);
            Object.DestroyImmediate(iconGO);
            return prefab;
        }

        // ================================================================
        // PREFAB: Filter icon (for ArtifactFilterModal set/type grid)
        // ================================================================
        private static GameObject CreateFilterIconPrefab()
        {
            GameObject iconGO = new GameObject("FilterIconPrefab");
            RectTransform rt = iconGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);

            Image img = iconGO.AddComponent<Image>();
            img.preserveAspect = true;
            iconGO.AddComponent<Button>();

            // Highlight child
            GameObject highlight = CreateUIObject("Highlight", iconGO.transform, stretch: true);
            Image hlImg = highlight.AddComponent<Image>();
            hlImg.color = new Color(0.4f, 0.7f, 1f, 0.5f);
            hlImg.raycastTarget = false;
            highlight.SetActive(false);

            // Label child
            GameObject labelGO = CreateUIObject("Label", iconGO.transform, stretch: true);
            TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 14;
            label.color = Color.white;
            label.raycastTarget = false;

            EnsurePrefabDir();
            string path = "Assets/Celestial-Cross/Prefabs/UI/FilterIconPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(iconGO, path);
            Object.DestroyImmediate(iconGO);
            return prefab;
        }

        // ================================================================
        // SCROLL VIEW with GridLayoutGroup
        // ================================================================
        private static GameObject CreateScrollView(string name, Transform parent)
        {
            GameObject svGO = CreateUIObject(name, parent, stretch: true);
            ScrollRect scrollRect = svGO.AddComponent<ScrollRect>();
            Image bg = svGO.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.06f, 0.1f, 0.6f);

            GameObject viewportGO = CreateUIObject("Viewport", svGO.transform, stretch: true);
            viewportGO.AddComponent<RectMask2D>();
            Image vpBg = viewportGO.AddComponent<Image>();
            vpBg.color = Color.clear;

            GameObject contentGO = CreateUIObject("Content", viewportGO.transform);
            RectTransform contentRT = contentGO.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);

            GridLayoutGroup grid = contentGO.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(130, 150);
            grid.spacing = new Vector2(15, 15);
            grid.padding = new RectOffset(15, 15, 15, 15);
            grid.constraint = GridLayoutGroup.Constraint.Flexible;

            ContentSizeFitter fitter = contentGO.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRT;
            scrollRect.viewport = viewportGO.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            return svGO;
        }

        private static TMP_Dropdown CreateDropdown(string name, Transform parent,
            float aMinX, float aMinY, float aMaxX, float aMaxY)
        {
            GameObject ddGO = CreateUIObject(name, parent);
            SetAnchors(ddGO, aMinX, aMinY, aMaxX, aMaxY, new RectOffset(5, 5, 2, 2));

            Image img = ddGO.AddComponent<Image>();
            img.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            TMP_Dropdown dd = ddGO.AddComponent<TMP_Dropdown>();

            // Caption label
            GameObject labelGO = CreateUIObject("Label", ddGO.transform, stretch: true);
            RectTransform lrt = labelGO.GetComponent<RectTransform>();
            lrt.offsetMin = new Vector2(10, 2);
            lrt.offsetMax = new Vector2(-25, -2);
            TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
            label.color = new Color(0.15f, 0.15f, 0.15f);
            label.fontSize = 16;
            label.alignment = TextAlignmentOptions.Left;
            dd.captionText = label;

            // Arrow indicator
            GameObject arrowGO = CreateUIObject("Arrow", ddGO.transform);
            SetAnchors(arrowGO, 0.85f, 0.15f, 0.95f, 0.85f, new RectOffset(0, 0, 0, 0));
            Image arrowImg = arrowGO.AddComponent<Image>();
            arrowImg.color = new Color(0.3f, 0.3f, 0.3f);
            arrowImg.raycastTarget = false;

            // Template (disabled by default, will expand below the dropdown)
            GameObject templateGO = CreateUIObject("Template", ddGO.transform);
            RectTransform templateRT = templateGO.GetComponent<RectTransform>();
            templateRT.anchorMin = new Vector2(0f, 0f);
            templateRT.anchorMax = new Vector2(1f, 0f);
            templateRT.pivot = new Vector2(0.5f, 1f);
            templateRT.sizeDelta = new Vector2(0f, 150f);
            templateRT.anchoredPosition = new Vector2(0f, -2f);

            Image tempBg = templateGO.AddComponent<Image>();
            tempBg.color = new Color(0.95f, 0.95f, 0.95f, 1f);

            ScrollRect sr = templateGO.AddComponent<ScrollRect>();

            // Viewport
            GameObject viewportGO = CreateUIObject("Viewport", templateGO.transform, stretch: true);
            RectTransform viewportRT = viewportGO.GetComponent<RectTransform>();
            viewportGO.AddComponent<RectMask2D>();
            Image vpBg = viewportGO.AddComponent<Image>();
            vpBg.color = Color.clear;

            // Content
            GameObject contentGO = CreateUIObject("Content", viewportGO.transform);
            RectTransform contentRT = contentGO.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot = new Vector2(0.5f, 1f);
            contentRT.sizeDelta = new Vector2(0f, 28f);

            VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            ContentSizeFitter fitter = contentGO.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = contentRT;
            sr.viewport = viewportRT;
            sr.horizontal = false;
            sr.vertical = true;

            // Item (the prototype cloned by TMP_Dropdown)
            GameObject itemGO = CreateUIObject("Item", contentGO.transform);
            RectTransform itemRT = itemGO.GetComponent<RectTransform>();
            itemRT.sizeDelta = new Vector2(0f, 28f);

            Toggle itemToggle = itemGO.AddComponent<Toggle>();
            itemToggle.isOn = false;

            // Item Background
            GameObject itemBgGO = CreateUIObject("Item Background", itemGO.transform, stretch: true);
            Image itemBgImg = itemBgGO.AddComponent<Image>();
            itemBgImg.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            itemToggle.targetGraphic = itemBgImg;

            // Item Checkmark
            GameObject itemCheckGO = CreateUIObject("Item Checkmark", itemGO.transform);
            SetAnchors(itemCheckGO, 0.05f, 0.2f, 0.15f, 0.8f, new RectOffset(0, 0, 0, 0));
            Image checkImg = itemCheckGO.AddComponent<Image>();
            checkImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            itemToggle.graphic = checkImg;

            // Item Label
            GameObject itemLabelGO = CreateUIObject("Item Label", itemGO.transform, stretch: true);
            RectTransform itemLabelRT = itemLabelGO.GetComponent<RectTransform>();
            itemLabelRT.offsetMin = new Vector2(25, 2);
            itemLabelRT.offsetMax = new Vector2(-10, -2);
            TextMeshProUGUI itemLabel = itemLabelGO.AddComponent<TextMeshProUGUI>();
            itemLabel.fontSize = 14;
            itemLabel.color = new Color(0.15f, 0.15f, 0.15f);
            itemLabel.alignment = TextAlignmentOptions.Left;

            dd.template = templateRT;
            dd.itemText = itemLabel;

            // Deactivate the template so it only shows up when clicked
            templateGO.SetActive(false);

            return dd;
        }

        // ================================================================
        // TAB: Pets
        // ================================================================
        private static PetTabPanel BuildPetTab(InventorySceneController ctrl,
            Transform tabBar, Transform topPanel, Transform gridArea)
        {
            // Tab button
            GameObject tabBtn = CreateButton("TabButton_Pets", tabBar, "Pets", 20);

            // Top content (detail view)
            GameObject topContent = CreateUIObject("TopContent_Pets", topPanel, stretch: true);

            // Scroll grid
            GameObject scrollView = CreateScrollView("Scroll_Pets", gridArea);

            // Wire component
            PetTabPanel panel = ctrl.gameObject.AddComponent<PetTabPanel>();
            panel.tabButton = tabBtn.GetComponent<Button>();
            panel.tabButtonImage = tabBtn.GetComponent<Image>();
            panel.topPanelContent = topContent;
            panel.gridContent = scrollView.GetComponent<ScrollRect>().content.gameObject;
            panel.scrollView = scrollView;

            // ── Top Detail Layout ──
            // Left: Pet sprite (0→0.3)
            GameObject spriteGO = CreateUIObject("PetSprite", topContent.transform);
            SetAnchors(spriteGO, 0f, 0f, 0.3f, 1f, new RectOffset(10, 5, 10, 10));
            panel.petSpriteImage = spriteGO.AddComponent<Image>();
            panel.petSpriteImage.preserveAspect = true;
            panel.petSpriteImage.color = new Color(1, 1, 1, 0.3f); // placeholder transparency

            // Right: Info area (0.3→1)
            GameObject infoGO = CreateUIObject("InfoArea", topContent.transform);
            SetAnchors(infoGO, 0.3f, 0f, 1f, 1f, new RectOffset(10, 10, 5, 5));

            // Icon (small, top-left of info)
            GameObject petIconGO = CreateUIObject("PetIcon", infoGO.transform);
            SetAnchors(petIconGO, 0f, 0.7f, 0.08f, 1f, new RectOffset(0, 0, 2, 2));
            panel.petIconImage = petIconGO.AddComponent<Image>();
            panel.petIconImage.preserveAspect = true;

            // Name
            panel.petNameText = CreateText("PetName", infoGO.transform, "Selecione um Pet", 28,
                0.1f, 0.7f, 0.7f, 1f);

            // Stars container
            GameObject starsGO = CreateUIObject("StarsContainer", infoGO.transform);
            SetAnchors(starsGO, 0.1f, 0.55f, 0.5f, 0.7f, new RectOffset(0, 0, 0, 0));
            HorizontalLayoutGroup hlg = starsGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            panel.starsContainer = starsGO.transform;

            // Stats text
            panel.statsText = CreateText("StatsText", infoGO.transform, "HP: —\nATK: —\nDEF: —\nSPD: —", 18,
                0f, 0f, 0.55f, 0.55f);

            // Skill area
            GameObject skillGO = CreateUIObject("SkillIcon", infoGO.transform);
            SetAnchors(skillGO, 0.55f, 0.15f, 0.65f, 0.55f, new RectOffset(0, 0, 0, 0));
            panel.skillIconImage = skillGO.AddComponent<Image>();
            panel.skillIconImage.preserveAspect = true;

            panel.skillDescriptionText = CreateText("SkillDesc", infoGO.transform, "Habilidade: ...", 16,
                0.66f, 0.15f, 1f, 0.55f);

            // Filter button (top-right)
            GameObject filterBtn = CreateButton("FilterBtn", infoGO.transform, "Filtrar", 18);
            SetAnchors(filterBtn, 0.75f, 0.72f, 1f, 0.98f, new RectOffset(0, 0, 0, 0));
            panel.filterButton = filterBtn.GetComponent<Button>();

            // Release button (bottom-right)
            GameObject releaseBtn = CreateButton("ReleaseBtn", infoGO.transform, "Liberar", 16);
            SetAnchors(releaseBtn, 0.75f, 0.02f, 1f, 0.18f, new RectOffset(0, 0, 0, 0));
            panel.releaseButton = releaseBtn.GetComponent<Button>();

            // Star prefab (hidden, used for instantiation)
            GameObject starGO = new GameObject("StarPrefab");
            RectTransform starRT = starGO.AddComponent<RectTransform>();
            starRT.sizeDelta = new Vector2(22, 22);
            Image starImg = starGO.AddComponent<Image>();
            starImg.color = new Color(1f, 0.85f, 0.1f);
            starImg.raycastTarget = false;
            starGO.transform.SetParent(ctrl.transform, false);
            starGO.SetActive(false);
            panel.starPrefab = starGO;

            return panel;
        }

        // ================================================================
        // TAB: Artifacts
        // ================================================================
        private static ArtifactTabPanel BuildArtifactTab(InventorySceneController ctrl,
            Transform tabBar, Transform topPanel, Transform gridArea)
        {
            GameObject tabBtn = CreateButton("TabButton_Artifacts", tabBar, "Artefatos", 20);
            GameObject topContent = CreateUIObject("TopContent_Artifacts", topPanel, stretch: true);
            GameObject scrollView = CreateScrollView("Scroll_Artifacts", gridArea);

            ArtifactTabPanel panel = ctrl.gameObject.AddComponent<ArtifactTabPanel>();
            panel.tabButton = tabBtn.GetComponent<Button>();
            panel.tabButtonImage = tabBtn.GetComponent<Image>();
            panel.topPanelContent = topContent;
            panel.gridContent = scrollView.GetComponent<ScrollRect>().content.gameObject;
            panel.scrollView = scrollView;

            // ── Top Detail Layout ──
            // Left: Artifact icon (0→0.25)
            GameObject iconGO = CreateUIObject("ArtifactIcon", topContent.transform);
            SetAnchors(iconGO, 0f, 0f, 0.25f, 1f, new RectOffset(15, 5, 15, 15));
            panel.artifactIconImage = iconGO.AddComponent<Image>();
            panel.artifactIconImage.preserveAspect = true;
            panel.artifactIconImage.color = new Color(1, 1, 1, 0.3f);

            // Center: Info (0.25→0.7)
            GameObject infoGO = CreateUIObject("InfoArea", topContent.transform);
            SetAnchors(infoGO, 0.25f, 0f, 0.7f, 1f, new RectOffset(10, 10, 5, 5));

            panel.artifactNameText = CreateText("ArtifactName", infoGO.transform, "Selecione um Artefato", 24,
                0f, 0.8f, 1f, 1f);

            panel.artifactLevelText = CreateText("ArtifactLevel", infoGO.transform, "Lv. —", 20,
                0f, 0.6f, 0.4f, 0.8f);

            panel.mainStatText = CreateText("MainStat", infoGO.transform, "Stat Principal: —", 18,
                0f, 0.4f, 1f, 0.6f);

            panel.subStatsText = CreateText("SubStats", infoGO.transform, "", 16,
                0f, 0f, 0.6f, 0.4f);

            panel.setBonusText = CreateText("SetBonus", infoGO.transform, "", 14,
                0.6f, 0f, 1f, 0.4f);
            panel.setBonusText.fontStyle = FontStyles.Italic;
            panel.setBonusText.color = new Color(0.6f, 0.85f, 1f);

            // Right: Actions (0.7→1)
            GameObject actionsGO = CreateUIObject("Actions", topContent.transform);
            SetAnchors(actionsGO, 0.7f, 0f, 1f, 1f, new RectOffset(5, 10, 10, 10));
            VerticalLayoutGroup vlg = actionsGO.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = true;
            vlg.spacing = 8;
            vlg.padding = new RectOffset(5, 5, 5, 5);

            GameObject upgBtn = CreateButton("UpgradeBtn", actionsGO.transform, "Aprimorar", 18);
            panel.upgradeButton = upgBtn.GetComponent<Button>();

            GameObject sellBtn = CreateButton("SellBtn", actionsGO.transform, "Vender", 18);
            panel.sellButton = sellBtn.GetComponent<Button>();

            GameObject filterBtn = CreateButton("FilterBtn", actionsGO.transform, "Filtrar", 18);
            panel.filterButton = filterBtn.GetComponent<Button>();

            return panel;
        }

        // ================================================================
        // TAB: Consumables
        // ================================================================
        private static ConsumableTabPanel BuildConsumableTab(InventorySceneController ctrl,
            Transform tabBar, Transform topPanel, Transform gridArea)
        {
            GameObject tabBtn = CreateButton("TabButton_Consumables", tabBar, "Consumíveis", 20);
            GameObject topContent = CreateUIObject("TopContent_Consumables", topPanel, stretch: true);
            GameObject scrollView = CreateScrollView("Scroll_Consumables", gridArea);

            ConsumableTabPanel panel = ctrl.gameObject.AddComponent<ConsumableTabPanel>();
            panel.tabButton = tabBtn.GetComponent<Button>();
            panel.tabButtonImage = tabBtn.GetComponent<Image>();
            panel.topPanelContent = topContent;
            panel.gridContent = scrollView.GetComponent<ScrollRect>().content.gameObject;
            panel.scrollView = scrollView;

            // Placeholder text
            CreateText("Placeholder", topContent.transform, "Selecione um consumível...", 22,
                0.1f, 0.3f, 0.9f, 0.7f, TextAlignmentOptions.Center);

            return panel;
        }

        // ================================================================
        // TAB: Items
        // ================================================================
        private static ItemTabPanel BuildItemTab(InventorySceneController ctrl,
            Transform tabBar, Transform topPanel, Transform gridArea)
        {
            GameObject tabBtn = CreateButton("TabButton_Items", tabBar, "Itens", 20);
            GameObject topContent = CreateUIObject("TopContent_Items", topPanel, stretch: true);
            GameObject scrollView = CreateScrollView("Scroll_Items", gridArea);

            ItemTabPanel panel = ctrl.gameObject.AddComponent<ItemTabPanel>();
            panel.tabButton = tabBtn.GetComponent<Button>();
            panel.tabButtonImage = tabBtn.GetComponent<Image>();
            panel.topPanelContent = topContent;
            panel.gridContent = scrollView.GetComponent<ScrollRect>().content.gameObject;
            panel.scrollView = scrollView;

            // Placeholder text
            CreateText("Placeholder", topContent.transform, "Selecione um item...", 22,
                0.1f, 0.3f, 0.9f, 0.7f, TextAlignmentOptions.Center);

            return panel;
        }

        private static void BuildModals(Transform modalsParent, InventorySceneController ctrl,
            GameObject speciesIconPrefab, GameObject filterIconPrefab, PetTabPanel petTab)
        {
            // ── PET FILTER MODAL ──
            BuildPetFilterModal(modalsParent, ctrl, speciesIconPrefab);

            // ── ARTIFACT FILTER MODAL ──
            BuildArtifactFilterModal(modalsParent, ctrl, filterIconPrefab);

            // ── ARTIFACT UPGRADE SLIDER MODAL ──
            BuildUpgradeSliderModal(modalsParent, ctrl);

            // ── PET SKILL MODAL ──
            BuildPetSkillModal(modalsParent, ctrl, petTab);
        }

        private static void BuildPetFilterModal(Transform parent, InventorySceneController ctrl,
            GameObject speciesIconPrefab)
        {
            // Fullscreen dark overlay
            GameObject modalGO = CreateUIObject("PetFilterModal", parent, stretch: true);
            Image bg = modalGO.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.75f);

            PetFilterModal petFilter = modalGO.AddComponent<PetFilterModal>();
            ctrl.petFilterModal = petFilter;

            // Wire catalogs and prefabs
            petFilter.petCatalog = ctrl.petCatalog;
            petFilter.speciesIconPrefab = speciesIconPrefab;

            // Centered panel (70% x 70%)
            GameObject panelGO = CreateUIObject("Panel", modalGO.transform);
            SetAnchors(panelGO, 0.15f, 0.15f, 0.85f, 0.85f, new RectOffset(0, 0, 0, 0));
            Image panelBg = panelGO.AddComponent<Image>();
            panelBg.color = new Color(0.14f, 0.14f, 0.2f, 1f);

            // Title
            CreateText("Title", panelGO.transform, "Filtrar por Espécie", 28,
                0.05f, 0.9f, 0.7f, 1f);

            // Close button (top-right)
            GameObject closeBtn = CreateButton("CloseBtn", panelGO.transform, "X", 26);
            SetAnchors(closeBtn, 0.88f, 0.9f, 0.98f, 0.98f, new RectOffset(0, 0, 0, 0));
            petFilter.closeButton = closeBtn.GetComponent<Button>();

            // Species grid (scrollable area)
            GameObject scrollGO = CreateUIObject("SpeciesScroll", panelGO.transform);
            SetAnchors(scrollGO, 0.03f, 0.12f, 0.97f, 0.88f, new RectOffset(0, 0, 0, 0));
            ScrollRect sr = scrollGO.AddComponent<ScrollRect>();
            scrollGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.5f);

            GameObject vpGO = CreateUIObject("Viewport", scrollGO.transform, stretch: true);
            vpGO.AddComponent<RectMask2D>();

            GameObject contentGO = CreateUIObject("Content", vpGO.transform);
            RectTransform cRT = contentGO.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0, 1);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);

            GridLayoutGroup glg = contentGO.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(90, 90);
            glg.spacing = new Vector2(12, 12);
            glg.padding = new RectOffset(10, 10, 10, 10);
            ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = cRT;
            sr.viewport = vpGO.GetComponent<RectTransform>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;

            petFilter.speciesGridContainer = cRT;

            // Apply button (bottom center)
            GameObject applyBtn = CreateButton("ApplyBtn", panelGO.transform, "Aplicar Filtro", 22);
            SetAnchors(applyBtn, 0.3f, 0.02f, 0.7f, 0.1f, new RectOffset(0, 0, 0, 0));
            petFilter.applyFilterButton = applyBtn.GetComponent<Button>();

            modalGO.SetActive(false);
        }

        private static void BuildArtifactFilterModal(Transform parent, InventorySceneController ctrl,
            GameObject filterIconPrefab)
        {
            GameObject modalGO = CreateUIObject("ArtifactFilterModal", parent, stretch: true);
            Image bg = modalGO.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.75f);

            ArtifactFilterModal artFilter = modalGO.AddComponent<ArtifactFilterModal>();
            ctrl.artifactFilterModal = artFilter;
            artFilter.filterIconPrefab = filterIconPrefab;

            // Centered panel (75% x 80%)
            GameObject panelGO = CreateUIObject("Panel", modalGO.transform);
            SetAnchors(panelGO, 0.125f, 0.1f, 0.875f, 0.9f, new RectOffset(0, 0, 0, 0));
            Image panelBg = panelGO.AddComponent<Image>();
            panelBg.color = new Color(0.14f, 0.14f, 0.2f, 1f);

            // Title
            CreateText("Title", panelGO.transform, "Filtro Avançado de Artefatos", 26,
                0.05f, 0.92f, 0.7f, 1f);

            // Close
            GameObject closeBtn = CreateButton("CloseBtn", panelGO.transform, "X", 26);
            SetAnchors(closeBtn, 0.88f, 0.92f, 0.98f, 0.99f, new RectOffset(0, 0, 0, 0));
            artFilter.closeButton = closeBtn.GetComponent<Button>();

            // Sets grid area
            CreateText("SetsLabel", panelGO.transform, "Conjuntos:", 18,
                0.03f, 0.82f, 0.2f, 0.9f);
            GameObject setsGrid = CreateUIObject("SetsGrid", panelGO.transform);
            SetAnchors(setsGrid, 0.03f, 0.65f, 0.97f, 0.82f, new RectOffset(0, 0, 0, 0));
            GridLayoutGroup sglg = setsGrid.AddComponent<GridLayoutGroup>();
            sglg.cellSize = new Vector2(70, 70);
            sglg.spacing = new Vector2(8, 8);
            sglg.padding = new RectOffset(5, 5, 5, 5);
            artFilter.setsGridContainer = setsGrid.transform;

            // Types grid area
            CreateText("TypesLabel", panelGO.transform, "Tipo de Peça:", 18,
                0.03f, 0.55f, 0.25f, 0.63f);
            GameObject typesGrid = CreateUIObject("TypesGrid", panelGO.transform);
            SetAnchors(typesGrid, 0.03f, 0.42f, 0.97f, 0.55f, new RectOffset(0, 0, 0, 0));
            GridLayoutGroup tglg = typesGrid.AddComponent<GridLayoutGroup>();
            tglg.cellSize = new Vector2(70, 70);
            tglg.spacing = new Vector2(8, 8);
            tglg.padding = new RectOffset(5, 5, 5, 5);
            artFilter.typesGridContainer = typesGrid.transform;

            // Dropdowns section
            CreateText("MainStatLabel", panelGO.transform, "Stat Principal:", 16,
                0.03f, 0.34f, 0.25f, 0.4f);
            artFilter.mainStatDropdown = CreateDropdown("MainStatDD", panelGO.transform,
                0.25f, 0.34f, 0.65f, 0.4f);

            string[] subLabels = { "Sub 1:", "Sub 2:", "Sub 3:", "Sub 4:" };
            float yTop = 0.32f;
            for (int i = 0; i < 4; i++)
            {
                float yBot = yTop - 0.07f;
                CreateText("SubLabel_" + i, panelGO.transform, subLabels[i], 15,
                    0.03f, yBot, 0.15f, yTop);
                artFilter.subStatsDropdowns[i] = CreateDropdown("SubStatDD_" + i, panelGO.transform,
                    0.15f, yBot, 0.55f, yTop);
                yTop = yBot;
            }

            // Apply button
            GameObject applyBtn = CreateButton("ApplyBtn", panelGO.transform, "Aplicar Filtro", 22);
            SetAnchors(applyBtn, 0.3f, 0.02f, 0.7f, 0.08f, new RectOffset(0, 0, 0, 0));
            artFilter.applyFilterButton = applyBtn.GetComponent<Button>();

            modalGO.SetActive(false);
        }

        private static void BuildUpgradeSliderModal(Transform parent, InventorySceneController ctrl)
        {
            GameObject modalGO = CreateUIObject("ArtifactUpgradeSliderModal", parent, stretch: true);
            Image bg = modalGO.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.75f);

            ArtifactUpgradeSliderModal upgSlider = modalGO.AddComponent<ArtifactUpgradeSliderModal>();
            ctrl.upgradeSliderModal = upgSlider;

            // Centered panel (50% x 40%)
            GameObject panelGO = CreateUIObject("Panel", modalGO.transform);
            SetAnchors(panelGO, 0.25f, 0.3f, 0.75f, 0.7f, new RectOffset(0, 0, 0, 0));
            Image panelBg = panelGO.AddComponent<Image>();
            panelBg.color = new Color(0.14f, 0.14f, 0.2f, 1f);

            // Title
            CreateText("Title", panelGO.transform, "Aprimorar Artefato", 26,
                0.05f, 0.82f, 0.7f, 0.98f);

            // Close
            GameObject closeBtn = CreateButton("CloseBtn", panelGO.transform, "X", 24);
            SetAnchors(closeBtn, 0.88f, 0.85f, 0.97f, 0.97f, new RectOffset(0, 0, 0, 0));
            upgSlider.closeButton = closeBtn.GetComponent<Button>();

            // Level text
            upgSlider.levelText = CreateText("LevelText", panelGO.transform, "Level: — → —", 22,
                0.1f, 0.6f, 0.9f, 0.8f, TextAlignmentOptions.Center);

            // Slider
            GameObject sliderGO = new GameObject("LevelSlider");
            RectTransform sliderRT = sliderGO.AddComponent<RectTransform>();
            sliderRT.SetParent(panelGO.transform, false);
            SetAnchors(sliderGO, 0.1f, 0.4f, 0.9f, 0.55f, new RectOffset(0, 0, 0, 0));

            // Slider needs a background and fill
            GameObject sliderBg = CreateUIObject("Background", sliderGO.transform, stretch: true);
            Image sliderBgImg = sliderBg.AddComponent<Image>();
            sliderBgImg.color = new Color(0.25f, 0.25f, 0.3f);

            GameObject fillArea = CreateUIObject("Fill Area", sliderGO.transform, stretch: true);
            RectTransform faRT = fillArea.GetComponent<RectTransform>();
            faRT.offsetMin = new Vector2(5, 0);
            faRT.offsetMax = new Vector2(-5, 0);

            GameObject fill = CreateUIObject("Fill", fillArea.transform, stretch: true);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.7f, 1f);

            Slider slider = sliderGO.AddComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.wholeNumbers = true;
            upgSlider.levelSlider = slider;

            // Cost text
            upgSlider.costText = CreateText("CostText", panelGO.transform, "Custo: —", 20,
                0.1f, 0.22f, 0.9f, 0.38f, TextAlignmentOptions.Center);

            // Confirm button
            GameObject confirmBtn = CreateButton("ConfirmBtn", panelGO.transform, "Confirmar", 22);
            SetAnchors(confirmBtn, 0.25f, 0.04f, 0.75f, 0.18f, new RectOffset(0, 0, 0, 0));
            upgSlider.confirmButton = confirmBtn.GetComponent<Button>();

            modalGO.SetActive(false);
        }

        private static void BuildPetSkillModal(Transform parent, InventorySceneController ctrl, PetTabPanel petTab)
        {
            GameObject modalGO = CreateUIObject("PetSkillModal", parent, stretch: true);
            Image bg = modalGO.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.75f);

            PetSkillModal skillModal = modalGO.AddComponent<PetSkillModal>();
            petTab.skillModal = skillModal;

            // Centered panel (40% x 45%)
            GameObject panelGO = CreateUIObject("Panel", modalGO.transform);
            SetAnchors(panelGO, 0.3f, 0.275f, 0.7f, 0.725f, new RectOffset(0, 0, 0, 0));
            Image panelBg = panelGO.AddComponent<Image>();
            panelBg.color = new Color(0.14f, 0.14f, 0.2f, 1f);

            // Title
            CreateText("Title", panelGO.transform, "Detalhes da Habilidade", 24,
                0.05f, 0.82f, 0.7f, 0.98f);

            // Close
            GameObject closeBtn = CreateButton("CloseBtn", panelGO.transform, "X", 24);
            SetAnchors(closeBtn, 0.88f, 0.85f, 0.97f, 0.97f, new RectOffset(0, 0, 0, 0));
            skillModal.closeButton = closeBtn.GetComponent<Button>();

            // Skill Icon
            GameObject iconGO = CreateUIObject("SkillIcon", panelGO.transform);
            SetAnchors(iconGO, 0.4f, 0.52f, 0.6f, 0.82f, new RectOffset(0, 0, 0, 0));
            skillModal.skillIconImage = iconGO.AddComponent<Image>();
            skillModal.skillIconImage.preserveAspect = true;

            // Skill Name
            skillModal.skillNameText = CreateText("SkillNameText", panelGO.transform, "Nome da Habilidade", 22,
                0.05f, 0.38f, 0.95f, 0.50f, TextAlignmentOptions.Center);
            skillModal.skillNameText.fontStyle = FontStyles.Bold;

            // Skill Description
            skillModal.skillDescriptionText = CreateText("SkillDescText", panelGO.transform, "Descrição da Habilidade...", 18,
                0.05f, 0.05f, 0.95f, 0.35f, TextAlignmentOptions.Center);

            modalGO.SetActive(false);
        }

        // ================================================================
        // UTILITY
        // ================================================================
        private static void EnsurePrefabDir()
        {
            if (!global::System.IO.Directory.Exists("Assets/Celestial-Cross/Prefabs/UI"))
                global::System.IO.Directory.CreateDirectory("Assets/Celestial-Cross/Prefabs/UI");
        }
    }
}
#endif
