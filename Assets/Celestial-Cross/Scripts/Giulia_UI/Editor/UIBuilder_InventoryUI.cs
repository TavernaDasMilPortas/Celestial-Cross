using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Giulia_UI;
using System.Collections.Generic;

namespace CelestialCross.EditorArea {
    public class UIBuilder_InventoryUI : EditorWindow {
        
        [MenuItem("Celestial Cross/UI Builders/Generate Rest Scene Layout (Inventory)")]
        public static void GenerateInventoryUI() {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) {
                var canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
                canvas = canvasGO.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
            }
            
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            var restManager = Object.FindObjectOfType<RestSceneManager>();
            if (restManager == null) {
                var rmGO = new GameObject("RestSceneManager", typeof(RestSceneManager));
                restManager = rmGO.GetComponent<RestSceneManager>();
                restManager.mainCanvas = canvas;
            }

            InventoryUI inventory = Object.FindObjectOfType<InventoryUI>();
            if (inventory == null) {
                var invGO = new GameObject("InventoryPanel", typeof(RectTransform), typeof(InventoryUI));
                invGO.transform.SetParent(canvas.transform, false);
                var rt = invGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                inventory = invGO.GetComponent<InventoryUI>();
                restManager.inventoryPanel = invGO;
            }

            EnsureBackToHubButton(restManager, canvas);
            ConfigureGrids(inventory);
            EnsureSplitLayout(inventory);
            
            EditorUtility.SetDirty(inventory);
            EditorUtility.SetDirty(restManager);
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("Rest Scene / InventoryUI estruturado com sucesso e configurado (Botão Hub, Canvas, EventSystem, Manager)!");
        }

        private static void EnsureBackToHubButton(RestSceneManager rm, Canvas canvas) {
            if (rm.backToHubButton != null) return;
            
            var go = new GameObject("Btn_BackToHub", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(canvas.transform, false);
            
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(20, -20);
            rt.sizeDelta = new Vector2(200, 60);

            go.GetComponent<Image>().color = new Color(0.8f, 0.4f, 0.2f, 1f);
            var btn = go.GetComponent<Button>();
            rm.backToHubButton = btn;

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(go.transform, false);
            var txtRt = (RectTransform)txtGo.transform;
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;

            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "Voltar (Hub)";
            tmp.color = Color.white;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;

            // Ligar evento de persistência editor time
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, rm.GoToHubScene);
        }
        
private static void ConfigureGrids(InventoryUI target)
    {
        if (target.gridContainers == null)
            return;

        for (int i = 0; i < target.gridContainers.Length; i++)
        {
            if (target.gridContainers[i] == null) continue;

            GridLayoutGroup grid = target.gridContainers[i].GetComponent<GridLayoutGroup>();
            if (grid == null)
                grid = target.gridContainers[i].gameObject.AddComponent<GridLayoutGroup>();

            grid.cellSize = target.cellSize;
            grid.spacing = target.cellSpacing;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Mathf.Max(1, target.columns);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(16, 16, 16, 16);

            // Helps ScrollRect content size.
            var fitter = target.gridContainers[i].GetComponent<ContentSizeFitter>();
            if (fitter == null)
                fitter = target.gridContainers[i].gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    private static void EnsureSplitLayout(InventoryUI target)
    {
        // This script lives on InventoryPanel in RestScene.
        // The old scene has only Tabs + Grids; we create TopPanels and wrap grids in ScrollRects.
        int tabCount = target.tabs != null ? target.tabs.Length : 0;
        if (tabCount <= 0) return;

        EnsureTabsBar(target, tabCount);

        bool rebuildTopPanels = target.topPanels == null || target.topPanels.Length != tabCount;
        if (!rebuildTopPanels) 
        { 
            for (int i = 0; i < target.topPanels.Length; i++) 
                if (target.topPanels[i] == null) rebuildTopPanels = true; 
        }

        // 1) Top panels (one per tab)
        if (rebuildTopPanels)
        {
            target.topPanels = new RectTransform[tabCount];
            target.topPanelTexts = new TextMeshProUGUI[tabCount];

            for (int i = 0; i < tabCount; i++)
            {
                var panelGO = new GameObject($"TopPanel_{(int)i}", typeof(RectTransform), typeof(Image));
                panelGO.transform.SetParent(target.transform, false);

                var rt = (RectTransform)panelGO.transform;
                rt.anchorMin = new Vector2(0, 1f - Mathf.Clamp01(target.topPanelHeightNormalized));
                rt.anchorMax = new Vector2(1, 1);
                rt.offsetMin = new Vector2(16, 16);
                rt.offsetMax = new Vector2(-16, -80f - 8);

                var img = panelGO.GetComponent<Image>();
                img.color = new Color(0, 0, 0, 0.25f);
                img.raycastTarget = false;

                var textGO = new GameObject("DetailsText", typeof(RectTransform), typeof(TextMeshProUGUI));
                textGO.transform.SetParent(panelGO.transform, false);

                var textRt = (RectTransform)textGO.transform;
                textRt.anchorMin = new Vector2(0, 0);
                textRt.anchorMax = new Vector2(1, 1);
                textRt.offsetMin = new Vector2(16, 16);
                textRt.offsetMax = new Vector2(-16, -16);

                var tmp = textGO.GetComponent<TextMeshProUGUI>();
                tmp.enableWordWrapping = true;
                tmp.fontSize = 28;
                tmp.color = Color.white;
                tmp.text = "(selecione um item abaixo)";
                tmp.raycastTarget = false;

                target.topPanels[i] = rt;
                target.topPanelTexts[i] = tmp;
            }

            // --- Phase 3 & 4 additions ---
            // Unit Top Panel (Index 0): Add a container for profile and 6 equipment slots
            if (target.topPanels.Length > 0)
            {
                var unitPanel = target.topPanels[0];

                if (target.topPanelTexts[0] != null)
                    target.topPanelTexts[0].gameObject.SetActive(false);

                // --- LEFT HALF: Profile (Icon + Stats/Abilities) ---
                var profileGO = new GameObject("ProfileContainer", typeof(RectTransform));
                profileGO.transform.SetParent(unitPanel, false);
                var profileRT = (RectTransform)profileGO.transform;
                profileRT.anchorMin = new Vector2(0, 0);
                profileRT.anchorMax = new Vector2(0.4f, 1);
                profileRT.offsetMin = new Vector2(16, 16);
                profileRT.offsetMax = new Vector2(0, -16);

                var iconGO = new GameObject("UnitIcon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(profileRT, false);
                var iconRT = (RectTransform)iconGO.transform;
                iconRT.anchorMin = new Vector2(0.1f, 0.45f);
                iconRT.anchorMax = new Vector2(0.9f, 1f);
                iconRT.offsetMin = Vector2.zero;
                iconRT.offsetMax = Vector2.zero;
                target.unitIconImage = iconGO.GetComponent<Image>();
                target.unitIconImage.preserveAspect = true;

                var statsGO = new GameObject("UnitStats", typeof(RectTransform), typeof(TextMeshProUGUI));
                statsGO.transform.SetParent(profileRT, false);
                var statsRT = (RectTransform)statsGO.transform;
                statsRT.anchorMin = new Vector2(0, 0f);
                statsRT.anchorMax = new Vector2(1, 0.45f);
                statsRT.offsetMin = new Vector2(0, 48); // 48px space for abilities
                statsRT.offsetMax = new Vector2(0, -8);
                target.unitStatsText = statsGO.GetComponent<TextMeshProUGUI>();
                target.unitStatsText.fontSize = 18; // Menor para caber tudo
                target.unitStatsText.enableWordWrapping = true;
                target.unitStatsText.color = Color.white;
                target.unitStatsText.alignment = TextAlignmentOptions.TopLeft;

                var abilitiesGO = new GameObject("UnitAbilities", typeof(RectTransform), typeof(UnityEngine.UI.HorizontalLayoutGroup));
                abilitiesGO.transform.SetParent(profileRT, false);
                target.unitAbilitiesContainer = (RectTransform)abilitiesGO.transform;
                target.unitAbilitiesContainer.anchorMin = new Vector2(0, 0);
                target.unitAbilitiesContainer.anchorMax = new Vector2(1, 0);
                target.unitAbilitiesContainer.offsetMin = new Vector2(0, 0);
                target.unitAbilitiesContainer.offsetMax = new Vector2(0, 48);
                var hLG = abilitiesGO.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                hLG.childControlWidth = false; hLG.childForceExpandWidth = false;
                hLG.childControlHeight = true; hLG.childForceExpandHeight = true;
                hLG.spacing = 8;
                hLG.childAlignment = TextAnchor.MiddleLeft;

                // --- RIGHT HALF: Equip slots ---
                var equipGO = new GameObject("EquipContainer", typeof(RectTransform), typeof(GridLayoutGroup));
                equipGO.transform.SetParent(unitPanel, false);
                target.unitEquipContainer = (RectTransform)equipGO.transform;
                target.unitEquipContainer.anchorMin = new Vector2(0.4f, 0);
                target.unitEquipContainer.anchorMax = new Vector2(1, 1);
                target.unitEquipContainer.offsetMin = new Vector2(16, 16);
                target.unitEquipContainer.offsetMax = new Vector2(-16, -16);
                
                var grid = equipGO.GetComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(80, 80);
                grid.spacing = new Vector2(10, 10);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 2; // 2 cols x 3 rows = 6 slots
                grid.childAlignment = TextAnchor.MiddleCenter;

                CelestialCross.Artifacts.ArtifactType[] slotTypes = 
                    (CelestialCross.Artifacts.ArtifactType[])global::System.Enum.GetValues(typeof(CelestialCross.Artifacts.ArtifactType));
                
                for (int slotIdx = 0; slotIdx < 7; slotIdx++)
                {
                    int sIdx = slotIdx;
                    bool isPetSlot = sIdx == 6;
                    var sType = isPetSlot ? default : slotTypes[Mathf.Min(slotIdx, slotTypes.Length - 1)];
                    
                    string slotName = isPetSlot ? "Pet" : sType.ToString();
                    var bGO = new GameObject($"SlotBtn_{slotName}", typeof(RectTransform), typeof(Image), typeof(Button));
                    bGO.transform.SetParent(target.unitEquipContainer, false);
                    bGO.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);
                    var btn = bGO.GetComponent<Button>();
                    
                    var tGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    tGO.transform.SetParent(bGO.transform, false);
                    var tRT = (RectTransform)tGO.transform;
                    tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
                    tRT.offsetMin = tRT.offsetMax = Vector2.zero;
                    
                    var tTMP = tGO.GetComponent<TextMeshProUGUI>();
                    tTMP.fontSize = 15;
                    tTMP.alignment = TextAlignmentOptions.Center;
                    tTMP.enableWordWrapping = true;
                    tTMP.text = slotName + "\n<color=#888>(Vazio)</color>";
                    tTMP.raycastTarget = false;
                    
                    target.unitEquipButtons[slotIdx] = btn;
                    target.unitEquipTexts[slotIdx] = tTMP;
                }
            }

            // Artifact Top Panel (Index 2): Add Equip/Cancel buttons
            if (target.topPanels.Length > 2)
            {
                var artPanel = target.topPanels[2];
                // Cancel
                var cGO = new GameObject("CancelEquipBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                cGO.transform.SetParent(artPanel, false);
                var cRT = (RectTransform)cGO.transform;
                cRT.anchorMin = new Vector2(1, 1); cRT.anchorMax = new Vector2(1, 1);
                cRT.pivot = new Vector2(1, 1);
                cRT.anchoredPosition = new Vector2(-16, -16);
                cRT.sizeDelta = new Vector2(100, 40);
                cGO.GetComponent<Image>().color = Color.red * 0.7f;
                var cText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                cText.transform.SetParent(cGO.transform, false);
                var cTMP = cText.GetComponent<TextMeshProUGUI>();
                cTMP.rectTransform.anchorMin = Vector2.zero; cTMP.rectTransform.anchorMax = Vector2.one;
                cTMP.rectTransform.offsetMin = cTMP.rectTransform.offsetMax = Vector2.zero;
                cTMP.alignment = TextAlignmentOptions.Center;
                cTMP.fontSize = 20; cTMP.text = "Voltar";
                target.cancelEquipButton = cGO.GetComponent<Button>();
                target.cancelEquipButton.gameObject.SetActive(false);


                // Equip
                var eGO = new GameObject("EquipBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                eGO.transform.SetParent(artPanel, false);
                var eRT = (RectTransform)eGO.transform;
                eRT.anchorMin = new Vector2(1, 0); eRT.anchorMax = new Vector2(1, 0);
                eRT.pivot = new Vector2(1, 0);
                eRT.anchoredPosition = new Vector2(-16, 16);
                eRT.sizeDelta = new Vector2(120, 50);
                eGO.GetComponent<Image>().color = Color.green * 0.7f;
                var eText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                eText.transform.SetParent(eGO.transform, false);
                var eTMP = eText.GetComponent<TextMeshProUGUI>();
                eTMP.rectTransform.anchorMin = Vector2.zero; eTMP.rectTransform.anchorMax = Vector2.one;
                eTMP.rectTransform.offsetMin = eTMP.rectTransform.offsetMax = Vector2.zero;
                eTMP.alignment = TextAlignmentOptions.Center;
                eTMP.fontSize = 24; eTMP.text = "Equipar";
                target.equipArtifactButton = eGO.GetComponent<Button>();
                target.equipArtifactText = eTMP;
                target.equipArtifactButton.gameObject.SetActive(false);


                // Unequip
                var ueGO = new GameObject("UnequipBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                ueGO.transform.SetParent(artPanel, false);
                var ueRT = (RectTransform)ueGO.transform;
                ueRT.anchorMin = new Vector2(1, 0); ueRT.anchorMax = new Vector2(1, 0);
                ueRT.pivot = new Vector2(1, 0);
                ueRT.anchoredPosition = new Vector2(-150, 16);
                ueRT.sizeDelta = new Vector2(120, 50);
                ueGO.GetComponent<Image>().color = new Color(0.8f, 0.3f, 0.3f, 0.9f);
                var ueText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                ueText.transform.SetParent(ueGO.transform, false);
                var ueTMP = ueText.GetComponent<TextMeshProUGUI>();
                ueTMP.rectTransform.anchorMin = Vector2.zero; ueTMP.rectTransform.anchorMax = Vector2.one;
                ueTMP.rectTransform.offsetMin = ueTMP.rectTransform.offsetMax = Vector2.zero;
                ueTMP.alignment = TextAlignmentOptions.Center;
                ueTMP.fontSize = 24; ueTMP.text = "Remover";
                target.unequipArtifactButton = ueGO.GetComponent<Button>();
                target.unequipArtifactButton.gameObject.SetActive(false);

                // Manage
                var mGO = new GameObject("ManageBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                mGO.transform.SetParent(artPanel, false);
                var mRT = (RectTransform)mGO.transform;
                mRT.anchorMin = new Vector2(1, 0); mRT.anchorMax = new Vector2(1, 0);
                mRT.pivot = new Vector2(1, 0);
                mRT.anchoredPosition = new Vector2(-16, 16);
                mRT.sizeDelta = new Vector2(120, 50);
                mGO.GetComponent<Image>().color = Color.cyan * 0.7f;
                var mText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                mText.transform.SetParent(mGO.transform, false);
                var mTMP = mText.GetComponent<TextMeshProUGUI>();
                mTMP.rectTransform.anchorMin = Vector2.zero; mTMP.rectTransform.anchorMax = Vector2.one;
                mTMP.rectTransform.offsetMin = mTMP.rectTransform.offsetMax = Vector2.zero;
                mTMP.alignment = TextAlignmentOptions.Center;
                mTMP.fontSize = 24; mTMP.text = "Gerenciar";
                target.manageArtifactButton = mGO.GetComponent<Button>();
                target.manageArtifactButton.gameObject.SetActive(false);

            }
            // -----------------------------
        }

        // 2) Bottom scroll wrappers
        if (target.gridContainers == null || target.gridContainers.Length == 0)
            return;

        bool rebuildBottomScrolls = target.bottomScrollRoots == null || target.bottomScrollRoots.Length != target.gridContainers.Length;
        if (!rebuildBottomScrolls)
        {
            for (int i = 0; i < target.bottomScrollRoots.Length; i++)
                if (target.bottomScrollRoots[i] == null) rebuildBottomScrolls = true;
        }

        if (rebuildBottomScrolls)
        {
            target.bottomScrollRoots = new GameObject[target.gridContainers.Length];

        for (int i = 0; i < target.gridContainers.Length; i++)
        {
            var content = target.gridContainers[i];
            if (content == null) continue;

            // If already inside a ScrollRect, reuse it.
            var existingScroll = content.GetComponentInParent<ScrollRect>();
            if (existingScroll != null && existingScroll.content == content)
            {
                target.bottomScrollRoots[i] = existingScroll.gameObject;
                continue;
            }

            // Create scroll root
            var scrollGO = new GameObject($"BottomScroll_{(int)i}", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
            scrollGO.transform.SetParent(target.transform, false);
            var scrollRT = (RectTransform)scrollGO.transform;
            scrollRT.anchorMin = new Vector2(0, 0);
            scrollRT.anchorMax = new Vector2(1, 1f - Mathf.Clamp01(target.topPanelHeightNormalized));
            scrollRT.offsetMin = new Vector2(16, 16);
            scrollRT.offsetMax = new Vector2(-16, -16);

            var scrollImage = scrollGO.GetComponent<Image>();
            scrollImage.color = new Color(0, 0, 0, 0.15f);

            var mask = scrollGO.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            // Viewport
            var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGO.transform.SetParent(scrollGO.transform, false);
            var viewportRT = (RectTransform)viewportGO.transform;
            viewportRT.anchorMin = new Vector2(0, 0);
            viewportRT.anchorMax = new Vector2(1, 1);
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;
            var viewportMask = viewportGO.GetComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            var viewportImage = viewportGO.GetComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0.02f);
            viewportImage.raycastTarget = false;

            // Reparent content under viewport
            content.SetParent(viewportRT, false);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.offsetMin = new Vector2(0, 0);
            content.offsetMax = new Vector2(0, 0);

            var scroll = scrollGO.GetComponent<ScrollRect>();
            scroll.viewport = viewportRT;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;

            target.bottomScrollRoots[i] = scrollGO;
        }
        }

        // Ensure the tabs are always on top of the dynamic content.
        if (target.tabsBar != null)
            target.tabsBar.SetAsLastSibling();
    }

    private static void EnsureTabsBar(InventoryUI target, int tabCount)
    {
        if (target.tabsBar == null)
        {
            var existing = target.transform.Find("TabsBar") as RectTransform;
            if (existing != null)
            {
                target.tabsBar = existing;
            }
            else
            {
                var barGO = new GameObject("TabsBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                barGO.transform.SetParent(target.transform, false);
                target.tabsBar = (RectTransform)barGO.transform;

                target.tabsBar.anchorMin = new Vector2(0, 1);
                target.tabsBar.anchorMax = new Vector2(1, 1);
                target.tabsBar.pivot = new Vector2(0.5f, 1);
                target.tabsBar.anchoredPosition = Vector2.zero;
                target.tabsBar.sizeDelta = new Vector2(0, 80f);

                var layout = barGO.GetComponent<HorizontalLayoutGroup>();
                layout.padding = new RectOffset(16, 16, 12, 12);
                layout.spacing = 12f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
            }
        }

        // Reparent tabs into the bar (keeps current buttons/images intact).
        for (int i = 0; i < tabCount; i++)
        {
            if (target.tabs[i] == null) continue;

            var tabRT = target.tabs[i].GetComponent<RectTransform>();
            if (tabRT == null) continue;

            if (tabRT.parent != target.tabsBar)
                tabRT.SetParent(target.tabsBar, false);

            tabRT.anchorMin = new Vector2(0.5f, 0.5f);
            tabRT.anchorMax = new Vector2(0.5f, 0.5f);
            tabRT.pivot = new Vector2(0.5f, 0.5f);
            tabRT.anchoredPosition = Vector2.zero;
        }

        target.tabsBar.SetAsLastSibling();
    }

    [MenuItem("Celestial Cross/UI Builders/Add Manage Artifact Button")]
    public static void CreateManageArtifactButton()
    {
        var activeObj = UnityEditor.Selection.activeGameObject;
        if (activeObj == null)
        {
            Debug.LogWarning("Selecione onde o botao Melhora (ManageBtn) deve ser criado no Editor de Cena (Hierarchy) antes de clicar no gerador!");
            return;
        }

        var mGO = new GameObject("ManageBtn", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
        mGO.transform.SetParent(activeObj.transform, false);
        var mRT = (RectTransform)mGO.transform;
        mRT.anchorMin = new Vector2(1, 0); mRT.anchorMax = new Vector2(1, 0);
        mRT.pivot = new Vector2(1, 0);
        mRT.anchoredPosition = new Vector2(-16, 16);
        mRT.sizeDelta = new Vector2(120, 50);
        mGO.GetComponent<UnityEngine.UI.Image>().color = UnityEngine.Color.cyan * 0.7f;
        
        var mText = new GameObject("Text", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        mText.transform.SetParent(mGO.transform, false);
        var mTMP = mText.GetComponent<TMPro.TextMeshProUGUI>();
        mTMP.rectTransform.anchorMin = Vector2.zero; mTMP.rectTransform.anchorMax = Vector2.one;
        mTMP.rectTransform.offsetMin = mTMP.rectTransform.offsetMax = Vector2.zero;
        mTMP.alignment = TMPro.TextAlignmentOptions.Center;
        mTMP.fontSize = 24; 
        mTMP.text = "Melhorar";
        mTMP.color = UnityEngine.Color.white;

        UnityEditor.Selection.activeGameObject = mGO;
        Debug.Log("ManageBtn criado com sucesso!");
    }
}
}
