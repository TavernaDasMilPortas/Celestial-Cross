using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Giulia_UI;
using CelestialCross.UI.Skills;
using System.Collections.Generic;

namespace CelestialCross.EditorArea {
    public class UIBuilder_InventoryUI : EditorWindow {
        
        [MenuItem("Celestial Cross/3. UI Builders/1. Screens/Rest Scene Layout (Inventory)")]
        public static void GenerateInventoryUI() {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) {
                var canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
                canvas = canvasGO.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
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

            // Auto-preencher catálogos, abas e grades para garantir que os dados carreguem
            if (inventory.unitCatalog == null) {
                string[] guids = AssetDatabase.FindAssets("t:UnitCatalog");
                if (guids.Length > 0) {
                    inventory.unitCatalog = AssetDatabase.LoadAssetAtPath<UnitCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }
            if (inventory.petCatalog == null) {
                string[] guids = AssetDatabase.FindAssets("t:PetCatalog");
                if (guids.Length > 0) {
                    inventory.petCatalog = AssetDatabase.LoadAssetAtPath<PetCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }
            if (inventory.artifactSetCatalog == null) {
                string[] guids = AssetDatabase.FindAssets("t:ArtifactSetCatalog");
                if (guids.Length > 0) {
                    inventory.artifactSetCatalog = AssetDatabase.LoadAssetAtPath<ArtifactSetCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }
            if (inventory.levelingConfig == null) {
                string[] guids = AssetDatabase.FindAssets("t:LevelingConfig");
                if (guids.Length > 0) {
                    inventory.levelingConfig = AssetDatabase.LoadAssetAtPath<LevelingConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            var tb = inventory.transform.Find("TabsBar");
            if (tb != null) {
                var tabsList = new List<InventoryTab>();
                foreach (Transform t in tb) {
                    var tab = t.GetComponent<InventoryTab>();
                    if (tab != null) tabsList.Add(tab);
                }
                inventory.tabs = tabsList.ToArray();
            }

            var gridNames = new string[] { "GridPocoes", "GridArmas", "GridSuprimentos" };
            var gridsList = new List<RectTransform>();
            foreach (var name in gridNames) {
                var grid = inventory.transform.Find(name) as RectTransform;
                if (grid == null) {
                    for (int i = 0; i < inventory.transform.childCount; i++) {
                        var child = inventory.transform.GetChild(i);
                        if (child.name.StartsWith("BottomScroll_")) {
                            var found = child.Find($"Viewport/{name}");
                            if (found != null) {
                                grid = found as RectTransform;
                                break;
                            }
                        }
                    }
                }
                if (grid == null) {
                    Debug.LogWarning($"[UIBuilder] Grade '{name}' não encontrada no painel ou nos scrolls. Criando nova grade...");
                    var gridGO = new GameObject(name, typeof(RectTransform), typeof(GridLayoutGroup));
                    gridGO.transform.SetParent(inventory.transform, false);
                    grid = gridGO.GetComponent<RectTransform>();
                }
                if (grid != null) {
                    gridsList.Add(grid);
                }
            }
            inventory.gridContainers = gridsList.ToArray();

            EnsureBackToHubButton(restManager, canvas);
            ConfigureGrids(inventory);
            EnsureSplitLayout(inventory);
            
            // Garantir que a aba do inventário de itens esteja corretamente linkada se existir
            var itemsPanel = Object.FindObjectOfType<ItemsInventoryUI>(true);
            if (itemsPanel != null)
            {
                inventory.itemsInventoryPanel = itemsPanel;
            }
            
            // Garantir que o modal de melhorias e de pets estejam vinculados
            if (inventory.upgradeModal == null)
            {
                inventory.upgradeModal = Object.FindObjectOfType<ArtifactUpgradeModal>(true);
            }
            if (inventory.petManageModal == null)
            {
                inventory.petManageModal = Object.FindObjectOfType<PetManageModal>(true);
            }

            EditorUtility.SetDirty(inventory);
            EditorUtility.SetDirty(restManager);
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("Rest Scene / InventoryUI estruturado e refatorado com sucesso!");
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

            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, rm.GoToHubScene);
        }
        
        private static void ConfigureGrids(InventoryUI target) {
            if (target.gridContainers == null) return;

            for (int i = 0; i < target.gridContainers.Length; i++) {
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

                var fitter = target.gridContainers[i].GetComponent<ContentSizeFitter>();
                if (fitter == null)
                    fitter = target.gridContainers[i].gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        private static void EnsureSplitLayout(InventoryUI target) {
            int tabCount = target.tabs != null ? target.tabs.Length : 0;
            if (tabCount <= 0) return;

            EnsureTabsBar(target, tabCount);

            var canvas = target.GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // Garantir que exista um slotPrefab modelo na cena
            var prefabName = "UnitSlotPrefab_TEMP";
            var slotPrefabTR = target.transform.Find(prefabName);
            GameObject slotPrefabGO;
            if (slotPrefabTR == null) {
                slotPrefabGO = new GameObject(prefabName, typeof(RectTransform), typeof(Image), typeof(Button));
                slotPrefabGO.transform.SetParent(target.transform, false);
                slotPrefabGO.SetActive(false); // Inativo por padrão (modelo)

                var rt = slotPrefabGO.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100, 100);

                var img = slotPrefabGO.GetComponent<Image>();
                img.color = new Color(0.18f, 0.15f, 0.22f, 1f); // Violeta escuro para o slot

                var outline = slotPrefabGO.AddComponent<UnityEngine.UI.Outline>();
                outline.effectColor = new Color(0.35f, 0.3f, 0.45f, 0.5f);
                outline.effectDistance = new Vector2(1.5f, 1.5f);

                // Filho Icon
                var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(slotPrefabGO.transform, false);
                var iRt = iconGO.GetComponent<RectTransform>();
                iRt.anchorMin = Vector2.zero; iRt.anchorMax = Vector2.one;
                iRt.offsetMin = new Vector2(6, 6); iRt.offsetMax = new Vector2(-6, -6);
                var iconImg = iconGO.GetComponent<Image>();
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;

                // Filho Label
                var labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelGO.transform.SetParent(slotPrefabGO.transform, false);
                var lRt = labelGO.GetComponent<RectTransform>();
                lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
                lRt.offsetMin = new Vector2(8, 8); lRt.offsetMax = new Vector2(-8, -8);
                var labelTxt = labelGO.GetComponent<TextMeshProUGUI>();
                labelTxt.fontSize = 15;
                labelTxt.alignment = TextAlignmentOptions.Center;
                labelTxt.color = Color.white;
                labelTxt.enableWordWrapping = true;
                labelTxt.raycastTarget = false;
            } else {
                slotPrefabGO = slotPrefabTR.gameObject;
            }
            target.slotPrefab = slotPrefabGO;

            // Salvar temporariamente os gridContainers para não serem destruídos com o BottomScroll
            if (target.gridContainers != null) {
                for (int i = 0; i < target.gridContainers.Length; i++) {
                    if (target.gridContainers[i] != null) {
                        target.gridContainers[i].SetParent(target.transform, false);
                    }
                }
            }

            // Limpeza limpa dos painéis antigos para reconstrução segura
            var toDestroy = new List<GameObject>();
            for (int i = 0; i < target.transform.childCount; i++) {
                var child = target.transform.GetChild(i).gameObject;
                if (child.name.StartsWith("TopPanel_") || child.name.StartsWith("BottomScroll_")) {
                    toDestroy.Add(child);
                }
            }
            foreach (var go in toDestroy) {
                Object.DestroyImmediate(go);
            }

            target.topPanels = new RectTransform[tabCount];
            target.topPanelTexts = new TextMeshProUGUI[tabCount];
            target.bottomScrollRoots = new GameObject[tabCount];

            // 1) Criar os TopPanels e BottomScrolls
            for (int i = 0; i < tabCount; i++) {
                var panelGO = new GameObject($"TopPanel_{i}", typeof(RectTransform), typeof(Image));
                panelGO.transform.SetParent(target.transform, false);

                var rt = (RectTransform)panelGO.transform;
                rt.anchorMin = new Vector2(0, 0.55f);
                rt.anchorMax = new Vector2(1, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.offsetMin = new Vector2(16, 16);
                rt.offsetMax = new Vector2(-16, -80f - 16); // 80px TabsBar height + 16px padding

                var img = panelGO.GetComponent<Image>();
                img.color = new Color(0.12f, 0.10f, 0.15f, 0.95f); // Violeta escuro harmônico

                var textGO = new GameObject("DetailsText", typeof(RectTransform), typeof(TextMeshProUGUI));
                textGO.transform.SetParent(panelGO.transform, false);

                var textRt = (RectTransform)textGO.transform;
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = new Vector2(16, 16);
                textRt.offsetMax = new Vector2(-16, -16);

                var tmp = textGO.GetComponent<TextMeshProUGUI>();
                tmp.enableWordWrapping = true;
                tmp.fontSize = 24;
                tmp.color = Color.white;
                tmp.text = "(selecione um item abaixo)";
                tmp.raycastTarget = false;
                tmp.alignment = TextAlignmentOptions.Center;

                target.topPanels[i] = rt;
                target.topPanelTexts[i] = tmp;

                // Reconstruir wrappers de scroll na parte inferior (BottomScroll)
                var content = (target.gridContainers != null && i < target.gridContainers.Length) ? target.gridContainers[i] : null;
                if (content != null) {
                    var scrollGO = new GameObject($"BottomScroll_{i}", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
                    scrollGO.transform.SetParent(target.transform, false);
                    var scrollRT = (RectTransform)scrollGO.transform;
                    scrollRT.anchorMin = new Vector2(0, 0);
                    scrollRT.anchorMax = new Vector2(1, 0.55f);
                    scrollRT.offsetMin = new Vector2(16, 16);
                    scrollRT.offsetMax = new Vector2(-16, -16);

                    var scrollImage = scrollGO.GetComponent<Image>();
                    scrollImage.color = new Color(0.08f, 0.07f, 0.1f, 0.8f);

                    var mask = scrollGO.GetComponent<Mask>();
                    mask.showMaskGraphic = false;

                    var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
                    viewportGO.transform.SetParent(scrollGO.transform, false);
                    var viewportRT = (RectTransform)viewportGO.transform;
                    viewportRT.anchorMin = Vector2.zero;
                    viewportRT.anchorMax = Vector2.one;
                    viewportRT.offsetMin = viewportRT.offsetMax = Vector2.zero;
                    var viewportMask = viewportGO.GetComponent<Mask>();
                    viewportMask.showMaskGraphic = false;
                    var viewportImage = viewportGO.GetComponent<Image>();
                    viewportImage.color = new Color(1, 1, 1, 0.02f);
                    viewportImage.raycastTarget = false;

                    content.SetParent(viewportRT, false);
                    content.anchorMin = new Vector2(0, 1);
                    content.anchorMax = new Vector2(1, 1);
                    content.pivot = new Vector2(0.5f, 1);
                    content.anchoredPosition = Vector2.zero;
                    content.offsetMin = content.offsetMax = Vector2.zero;

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

            // 2) Customizações Detalhadas do TopPanel_0 (Unidades)
            if (target.topPanels.Length > 0) {
                var unitPanel = target.topPanels[0];
                if (target.topPanelTexts[0] != null)
                    target.topPanelTexts[0].gameObject.SetActive(false); // Ocultar o texto default

                // --- METADE ESQUERDA: ProfileContainer ---
                var profileGO = new GameObject("ProfileContainer", typeof(RectTransform));
                profileGO.transform.SetParent(unitPanel, false);
                var profileRT = (RectTransform)profileGO.transform;
                profileRT.anchorMin = new Vector2(0f, 0f);
                profileRT.anchorMax = new Vector2(0.4f, 1f);
                profileRT.offsetMin = new Vector2(16, 16);
                profileRT.offsetMax = new Vector2(0, -16);

                // Ícone da Unidade
                var iconGO = new GameObject("UnitIcon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(profileRT, false);
                var iconRT = (RectTransform)iconGO.transform;
                iconRT.anchorMin = new Vector2(0.05f, 0.65f);
                iconRT.anchorMax = new Vector2(0.95f, 0.95f);
                iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;
                target.unitIconImage = iconGO.GetComponent<Image>();
                target.unitIconImage.preserveAspect = true;

                // Nível & XP (Seção de Nível)
                var xpSecGO = new GameObject("Section_LevelXP", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                xpSecGO.transform.SetParent(profileRT, false);
                var xpSecRT = (RectTransform)xpSecGO.transform;
                xpSecRT.anchorMin = new Vector2(0.05f, 0.52f);
                xpSecRT.anchorMax = new Vector2(0.95f, 0.62f);
                xpSecRT.offsetMin = xpSecRT.offsetMax = Vector2.zero;
                var hlg = xpSecGO.GetComponent<HorizontalLayoutGroup>();
                hlg.spacing = 10; hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = false; hlg.childForceExpandWidth = false;

                var lvTextGO = new GameObject("Txt_Level", typeof(RectTransform), typeof(TextMeshProUGUI));
                lvTextGO.transform.SetParent(xpSecGO.transform, false);
                target.unitLevelText = lvTextGO.GetComponent<TextMeshProUGUI>();
                target.unitLevelText.fontSize = 18;
                target.unitLevelText.text = "Lv. --";
                target.unitLevelText.color = Color.white;
                lvTextGO.GetComponent<RectTransform>().sizeDelta = new Vector2(70, 30);

                var bar = new GameObject("XPBar_BG", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                bar.SetParent(xpSecGO.transform, false); 
                bar.sizeDelta = new Vector2(120, 12);
                bar.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
                
                var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fillGO.transform.SetParent(bar, false);
                target.unitXPBar = fillGO.GetComponent<Image>();
                target.unitXPBar.rectTransform.anchorMin = Vector2.zero; 
                target.unitXPBar.rectTransform.anchorMax = new Vector2(0.5f, 1);
                target.unitXPBar.rectTransform.offsetMin = target.unitXPBar.rectTransform.offsetMax = Vector2.zero;
                target.unitXPBar.color = Color.cyan;

                var xpValTextGO = new GameObject("Txt_XPValue", typeof(RectTransform), typeof(TextMeshProUGUI));
                xpValTextGO.transform.SetParent(xpSecGO.transform, false);
                target.unitXPText = xpValTextGO.GetComponent<TextMeshProUGUI>();
                target.unitXPText.fontSize = 12;
                target.unitXPText.text = "-- / --";
                target.unitXPText.color = Color.white;
                xpValTextGO.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 25);

                // Stats da Unidade (com suporte a CritDmg e EffectRes)
                var statsGO = new GameObject("UnitStats", typeof(RectTransform), typeof(TextMeshProUGUI));
                statsGO.transform.SetParent(profileRT, false);
                var statsRT = (RectTransform)statsGO.transform;
                statsRT.anchorMin = new Vector2(0.05f, 0.10f);
                statsRT.anchorMax = new Vector2(0.95f, 0.48f);
                statsRT.offsetMin = statsRT.offsetMax = Vector2.zero;
                target.unitStatsText = statsGO.GetComponent<TextMeshProUGUI>();
                target.unitStatsText.fontSize = 15;
                target.unitStatsText.enableWordWrapping = true;
                target.unitStatsText.color = Color.white;
                target.unitStatsText.alignment = TextAlignmentOptions.TopLeft;

                // UnitAbilities (Representação dinâmica)
                var abilitiesGO = new GameObject("UnitAbilities", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                abilitiesGO.transform.SetParent(profileRT, false);
                target.unitAbilitiesContainer = (RectTransform)abilitiesGO.transform;
                target.unitAbilitiesContainer.anchorMin = new Vector2(0.05f, 0f);
                target.unitAbilitiesContainer.anchorMax = new Vector2(0.95f, 0.08f);
                target.unitAbilitiesContainer.offsetMin = target.unitAbilitiesContainer.offsetMax = Vector2.zero;
                var hLG = abilitiesGO.GetComponent<HorizontalLayoutGroup>();
                hLG.childControlWidth = false; hLG.childForceExpandWidth = false;
                hLG.childControlHeight = true; hLG.childForceExpandHeight = true;
                hLG.spacing = 8;
                hLG.childAlignment = TextAnchor.MiddleLeft;

                // --- METADE DIREITA: SubPanelContainer ---
                var subContainerGO = new GameObject("SubPanelContainer", typeof(RectTransform));
                subContainerGO.transform.SetParent(unitPanel, false);
                var subContainerRT = (RectTransform)subContainerGO.transform;
                subContainerRT.anchorMin = new Vector2(0.4f, 0f);
                subContainerRT.anchorMax = new Vector2(1f, 1f);
                subContainerRT.offsetMin = new Vector2(16, 16);
                subContainerRT.offsetMax = new Vector2(-16, -16);

                // SubTabsBar (Navegação interna da unidade)
                var subTabsBarGO = new GameObject("SubTabsBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                subTabsBarGO.transform.SetParent(subContainerRT, false);
                var subTabsBarRT = (RectTransform)subTabsBarGO.transform;
                subTabsBarRT.anchorMin = new Vector2(0f, 1f);
                subTabsBarRT.anchorMax = new Vector2(1f, 1f);
                subTabsBarRT.pivot = new Vector2(0.5f, 1f);
                subTabsBarRT.offsetMin = new Vector2(0, -50);
                subTabsBarRT.offsetMax = new Vector2(0, 0);
                var stHLG = subTabsBarGO.GetComponent<HorizontalLayoutGroup>();
                stHLG.spacing = 8;
                stHLG.childAlignment = TextAnchor.MiddleCenter;
                stHLG.childControlWidth = true; stHLG.childControlHeight = true;
                stHLG.childForceExpandWidth = true; stHLG.childForceExpandHeight = true;

                target.unitSubTabEquipButton = CreateSubTabButton(subTabsBarGO.transform, "Btn_SubTabEquip", "Equipar");
                target.unitSubTabConstellationButton = CreateSubTabButton(subTabsBarGO.transform, "Btn_SubTabConstel", "Constel.");
                target.unitSubTabSkillsButton = CreateSubTabButton(subTabsBarGO.transform, "Btn_SubTabSkills", "Habilidades");

                // SUB-PAINEL 1: Equipamentos (SubPanel_Equip)
                var spEquipGO = new GameObject("SubPanel_Equip", typeof(RectTransform));
                spEquipGO.transform.SetParent(subContainerRT, false);
                var spEquipRT = (RectTransform)spEquipGO.transform;
                spEquipRT.anchorMin = Vector2.zero; spEquipRT.anchorMax = Vector2.one;
                spEquipRT.offsetMin = Vector2.zero; spEquipRT.offsetMax = new Vector2(0, -60);
                target.unitSubPanelEquip = spEquipGO;

                var equipGO = new GameObject("EquipContainer", typeof(RectTransform), typeof(GridLayoutGroup));
                equipGO.transform.SetParent(spEquipRT, false);
                target.unitEquipContainer = (RectTransform)equipGO.transform;
                target.unitEquipContainer.anchorMin = Vector2.zero; target.unitEquipContainer.anchorMax = Vector2.one;
                target.unitEquipContainer.offsetMin = target.unitEquipContainer.offsetMax = Vector2.zero;
                var eqGrid = equipGO.GetComponent<GridLayoutGroup>();
                eqGrid.cellSize = new Vector2(100, 80);
                eqGrid.spacing = new Vector2(10, 10);
                eqGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                eqGrid.constraintCount = 2; // 2 colunas
                eqGrid.childAlignment = TextAnchor.MiddleCenter;

                CelestialCross.Artifacts.ArtifactType[] slotTypes = 
                    (CelestialCross.Artifacts.ArtifactType[])global::System.Enum.GetValues(typeof(CelestialCross.Artifacts.ArtifactType));
                
                for (int slotIdx = 0; slotIdx < 7; slotIdx++) {
                    int sIdx = slotIdx;
                    bool isPetSlot = sIdx == 6;
                    var sType = isPetSlot ? default : slotTypes[Mathf.Min(slotIdx, slotTypes.Length - 1)];
                    
                    string slotName = isPetSlot ? "Pet" : sType.ToString();
                    var bGO = new GameObject($"SlotBtn_{slotName}", typeof(RectTransform), typeof(Image), typeof(Button));
                    bGO.transform.SetParent(target.unitEquipContainer, false);
                    bGO.GetComponent<Image>().color = new Color(0.2f, 0.18f, 0.22f, 1f);
                    var btn = bGO.GetComponent<Button>();
                    
                    var tGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    tGO.transform.SetParent(bGO.transform, false);
                    var tRT = (RectTransform)tGO.transform;
                    tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
                    tRT.offsetMin = tRT.offsetMax = Vector2.zero;
                    
                    var tTMP = tGO.GetComponent<TextMeshProUGUI>();
                    tTMP.fontSize = 13;
                    tTMP.alignment = TextAlignmentOptions.Center;
                    tTMP.enableWordWrapping = true;
                    tTMP.text = slotName + "\n<color=#888>(Vazio)</color>";
                    tTMP.raycastTarget = false;
                    
                    target.unitEquipButtons[slotIdx] = btn;
                    target.unitEquipTexts[slotIdx] = tTMP;
                }

                // SUB-PAINEL 2: Constelação (SubPanel_Constellation)
                var spConstGO = new GameObject("SubPanel_Constellation", typeof(RectTransform));
                spConstGO.transform.SetParent(subContainerRT, false);
                var spConstRT = (RectTransform)spConstGO.transform;
                spConstRT.anchorMin = Vector2.zero; spConstRT.anchorMax = Vector2.one;
                spConstRT.offsetMin = Vector2.zero; spConstRT.offsetMax = new Vector2(0, -60);
                spConstGO.SetActive(false);
                target.unitSubPanelConstellation = spConstGO;

                var starsGridGO = new GameObject("StarsGrid", typeof(RectTransform), typeof(GridLayoutGroup));
                starsGridGO.transform.SetParent(spConstRT, false);
                var sgRT = (RectTransform)starsGridGO.transform;
                sgRT.anchorMin = new Vector2(0.1f, 0.35f);
                sgRT.anchorMax = new Vector2(0.9f, 0.95f);
                sgRT.offsetMin = sgRT.offsetMax = Vector2.zero;
                var sgGrid = starsGridGO.GetComponent<GridLayoutGroup>();
                sgGrid.cellSize = new Vector2(50, 50);
                sgGrid.spacing = new Vector2(12, 12);
                sgGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                sgGrid.constraintCount = 3;
                sgGrid.childAlignment = TextAnchor.MiddleCenter;

                for (int i = 0; i < 6; i++) {
                    var starGO = new GameObject($"Star_{i}", typeof(RectTransform), typeof(Image));
                    starGO.transform.SetParent(starsGridGO.transform, false);
                    var starImg = starGO.GetComponent<Image>();
                    starImg.color = Color.gray;
                    target.constellationStars[i] = starImg;
                }

                var insTextGO = new GameObject("Txt_Insignia", typeof(RectTransform), typeof(TextMeshProUGUI));
                insTextGO.transform.SetParent(spConstRT, false);
                var insRT = (RectTransform)insTextGO.transform;
                insRT.anchorMin = new Vector2(0.1f, 0.20f);
                insRT.anchorMax = new Vector2(0.9f, 0.30f);
                insRT.offsetMin = insRT.offsetMax = Vector2.zero;
                target.insigniaCountText = insTextGO.GetComponent<TextMeshProUGUI>();
                target.insigniaCountText.fontSize = 15;
                target.insigniaCountText.text = "Insígnias: 0";
                target.insigniaCountText.color = Color.white;
                target.insigniaCountText.alignment = TextAlignmentOptions.Center;

                var openConstGO = new GameObject("Btn_OpenConstellation", typeof(RectTransform), typeof(Image), typeof(Button));
                openConstGO.transform.SetParent(spConstRT, false);
                var ocRT = (RectTransform)openConstGO.transform;
                ocRT.anchorMin = new Vector2(0.1f, 0.02f);
                ocRT.anchorMax = new Vector2(0.9f, 0.18f);
                ocRT.offsetMin = ocRT.offsetMax = Vector2.zero;
                openConstGO.GetComponent<Image>().color = new Color(0.8f, 0.6f, 0.2f, 1f);
                target.constellationButton = openConstGO.GetComponent<Button>();
                
                var ocText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                ocText.transform.SetParent(openConstGO.transform, false);
                var octRT = (RectTransform)ocText.transform;
                octRT.anchorMin = Vector2.zero; octRT.anchorMax = Vector2.one;
                octRT.offsetMin = octRT.offsetMax = Vector2.zero;
                var octTMP = ocText.GetComponent<TextMeshProUGUI>();
                octTMP.fontSize = 16;
                octTMP.alignment = TextAlignmentOptions.Center;
                octTMP.text = "ABRIR CONSTELAÇÃO";
                octTMP.color = Color.white;

                // SUB-PAINEL 3: Habilidades (SubPanel_Skills) -> Contém o SkillTabUI
                var spSkillsGO = new GameObject("SubPanel_Skills", typeof(RectTransform), typeof(SkillTabUI));
                spSkillsGO.transform.SetParent(subContainerRT, false);
                var spSkillsRT = (RectTransform)spSkillsGO.transform;
                spSkillsRT.anchorMin = Vector2.zero; spSkillsRT.anchorMax = Vector2.one;
                spSkillsRT.offsetMin = Vector2.zero; spSkillsRT.offsetMax = new Vector2(0, -60);
                spSkillsGO.SetActive(false);
                target.unitSubPanelSkills = spSkillsGO;
                target.skillTab = spSkillsGO.GetComponent<SkillTabUI>();
                target.skillTab.unitCatalog = target.unitCatalog;

                var slotsContainerGO = new GameObject("SlotsContainer", typeof(RectTransform), typeof(GridLayoutGroup));
                slotsContainerGO.transform.SetParent(spSkillsRT, false);
                var scRT = (RectTransform)slotsContainerGO.transform;
                scRT.anchorMin = new Vector2(0.05f, 0.05f);
                scRT.anchorMax = new Vector2(0.95f, 0.95f);
                scRT.offsetMin = scRT.offsetMax = Vector2.zero;
                var scGrid = slotsContainerGO.GetComponent<GridLayoutGroup>();
                scGrid.cellSize = new Vector2(100, 80);
                scGrid.spacing = new Vector2(12, 12);
                scGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                scGrid.constraintCount = 2; // 2 colunas
                scGrid.childAlignment = TextAnchor.MiddleCenter;

                target.skillTab.basicSkillButton = CreateSkillSlotButton(slotsContainerGO.transform, "BasicSlot", "Básico", out target.skillTab.basicSkillText);
                target.skillTab.movementSkillButton = CreateSkillSlotButton(slotsContainerGO.transform, "MovementSlot", "Mover", out target.skillTab.movementSkillText);
                target.skillTab.slot1SkillButton = CreateSkillSlotButton(slotsContainerGO.transform, "Slot1", "Slot 1", out target.skillTab.slot1SkillText);
                target.skillTab.slot2SkillButton = CreateSkillSlotButton(slotsContainerGO.transform, "Slot2", "Slot 2", out target.skillTab.slot2SkillText);

                // Configurar Modais sob o Canvas
                var selectionModal = Object.FindObjectOfType<SkillSelectionModal>(true);
                if (selectionModal == null) selectionModal = CreateSelectionModal(canvas.transform);
                selectionModal.unitCatalog = target.unitCatalog;
                target.skillTab.selectionModal = selectionModal;

                var branchModal = Object.FindObjectOfType<SkillBranchModal>(true);
                if (branchModal == null) branchModal = CreateBranchModal(canvas.transform);
                target.skillTab.branchModal = branchModal;

                // Limpar qualquer duplicata de SkillTabUI solta na cena
                var oldSkillTab = canvas.transform.Find("SkillTabUI");
                if (oldSkillTab != null && oldSkillTab.gameObject != spSkillsGO) {
                    Object.DestroyImmediate(oldSkillTab.gameObject);
                }
            }

            // 3) Customizações Detalhadas do TopPanel_1 (Pets)
            if (target.topPanels.Length > 1) {
                var petPanel = target.topPanels[1];
                
                var btnManagePetGO = new GameObject("Btn_ManagePet", typeof(RectTransform), typeof(Image), typeof(Button));
                btnManagePetGO.transform.SetParent(petPanel, false);
                var mRT = (RectTransform)btnManagePetGO.transform;
                mRT.anchorMin = new Vector2(1f, 0f); mRT.anchorMax = new Vector2(1f, 0f);
                mRT.pivot = new Vector2(1f, 0f);
                mRT.anchoredPosition = new Vector2(-20, 20);
                mRT.sizeDelta = new Vector2(150, 45);
                btnManagePetGO.GetComponent<Image>().color = new Color(0.1f, 0.5f, 0.7f, 1f);
                target.managePetButton = btnManagePetGO.GetComponent<Button>();

                var mText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                mText.transform.SetParent(btnManagePetGO.transform, false);
                var mtRT = (RectTransform)mText.transform;
                mtRT.anchorMin = Vector2.zero; mtRT.anchorMax = Vector2.one;
                mtRT.offsetMin = mtRT.offsetMax = Vector2.zero;
                var mtTMP = mText.GetComponent<TextMeshProUGUI>();
                mtTMP.fontSize = 16;
                mtTMP.alignment = TextAlignmentOptions.Center;
                mtTMP.text = "Gerenciar Pet";
                mtTMP.color = Color.white;
            }

            // 4) Customizações Detalhadas do TopPanel_2 (Artefatos)
            if (target.topPanels.Length > 2) {
                var artPanel = target.topPanels[2];

                // Voltar
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

                // Equipar
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

                // Remover
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

                // Gerenciar
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
                mTMP.fontSize = 24; mTMP.text = "Melhorar";
                target.manageArtifactButton = mGO.GetComponent<Button>();
                target.manageArtifactButton.gameObject.SetActive(false);
            }

            if (target.tabsBar != null)
                target.tabsBar.SetAsLastSibling();
        }

        private static void EnsureTabsBar(InventoryUI target, int tabCount) {
            if (target.tabsBar == null) {
                var existing = target.transform.Find("TabsBar") as RectTransform;
                if (existing != null) {
                    target.tabsBar = existing;
                }
                else {
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

            for (int i = 0; i < tabCount; i++) {
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

        private static Button CreateSubTabButton(Transform parent, string name, string label) {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.2f, 0.18f, 0.22f, 1f);
            var btn = go.GetComponent<Button>();

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
            textRt.offsetMin = textRt.offsetMax = Vector2.zero;
            
            var textComp = textGo.GetComponent<TextMeshProUGUI>();
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.text = label;
            textComp.color = Color.gray;
            textComp.fontSize = 15;
            textComp.raycastTarget = false;

            return btn;
        }

        private static Button CreateSkillSlotButton(Transform parent, string name, string label, out TextMeshProUGUI textComp) {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.25f, 0.22f, 0.3f, 1f);
            var btn = go.GetComponent<Button>();

            // Fundo de texto semitransparente na parte inferior (0% a 35% da altura)
            var bgGo = new GameObject("TextBG", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(go.transform, false);
            var bgRt = (RectTransform)bgGo.transform;
            bgRt.anchorMin = new Vector2(0f, 0f);
            bgRt.anchorMax = new Vector2(1f, 0.35f);
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            bgGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(bgGo.transform, false);
            var textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
            textRt.offsetMin = textRt.offsetMax = Vector2.zero;
            
            textComp = textGo.GetComponent<TextMeshProUGUI>();
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.text = $"<b>{label}</b>\n<color=#ffb>Vazio</color>";
            textComp.color = Color.white;
            textComp.fontSize = 12;
            textComp.enableWordWrapping = true;
            textComp.raycastTarget = false;

            return btn;
        }

        private static SkillSelectionModal CreateSelectionModal(Transform parent) {
            var modalGo = new GameObject("SkillSelectionModal", typeof(RectTransform), typeof(SkillSelectionModal));
            modalGo.transform.SetParent(parent, false);
            var rt = (RectTransform)modalGo.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(modalGo.transform, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.85f);

            // Trocado de VerticalLayoutGroup para GridLayoutGroup para organizar as habilidades como grid de ícones
            var optionsGo = new GameObject("OptionsContainer", typeof(RectTransform), typeof(GridLayoutGroup));
            optionsGo.transform.SetParent(modalGo.transform, false);
            var optRt = (RectTransform)optionsGo.transform;
            optRt.anchorMin = new Vector2(0.1f, 0.1f);
            optRt.anchorMax = new Vector2(0.9f, 0.9f);
            optRt.offsetMin = optRt.offsetMax = Vector2.zero;

            var grid = optionsGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(80f, 80f);
            grid.spacing = new Vector2(10f, 10f);
            grid.childAlignment = TextAnchor.UpperCenter;

            TextMeshProUGUI dummyText;
            var optionPrefab = CreateSkillSelectionOptionPrefab(modalGo.transform, "OptionPrefab", out dummyText).gameObject;
            optionPrefab.SetActive(false);

            var closeBtn = CreateSimpleButton(modalGo.transform, "CloseButton", "Fechar");
            ((RectTransform)closeBtn.transform).anchorMin = new Vector2(0.8f, 0.9f);
            ((RectTransform)closeBtn.transform).anchorMax = new Vector2(0.95f, 0.98f);
            ((RectTransform)closeBtn.transform).offsetMin = ((RectTransform)closeBtn.transform).offsetMax = Vector2.zero;

            var comp = modalGo.GetComponent<SkillSelectionModal>();
            comp.modalRoot = modalGo;
            comp.optionsContainer = optRt;
            comp.optionPrefab = optionPrefab;
            comp.closeButton = closeBtn;

            modalGo.SetActive(false);
            return comp;
        }

        private static SkillBranchModal CreateBranchModal(Transform parent) {
            var modalGo = new GameObject("SkillBranchModal", typeof(RectTransform), typeof(SkillBranchModal));
            modalGo.transform.SetParent(parent, false);
            var rt = (RectTransform)modalGo.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(modalGo.transform, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.85f);

            // Container do Header (Nome e Descrição)
            var headerGo = new GameObject("HeaderPanel", typeof(RectTransform));
            headerGo.transform.SetParent(modalGo.transform, false);
            var headerRt = (RectTransform)headerGo.transform;
            headerRt.anchorMin = new Vector2(0.1f, 0.75f);
            headerRt.anchorMax = new Vector2(0.9f, 0.95f);
            headerRt.offsetMin = headerRt.offsetMax = Vector2.zero;

            // Nome da Habilidade
            var nameGo = new GameObject("SkillNameText", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameGo.transform.SetParent(headerGo.transform, false);
            var nameRt = (RectTransform)nameGo.transform;
            nameRt.anchorMin = new Vector2(0f, 0.5f);
            nameRt.anchorMax = new Vector2(1f, 1f);
            nameRt.offsetMin = nameRt.offsetMax = Vector2.zero;
            var nameText = nameGo.GetComponent<TextMeshProUGUI>();
            nameText.text = "Nome da Habilidade";
            nameText.fontSize = 32;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;

            // Descrição da Habilidade
            var descGo = new GameObject("SkillDescText", typeof(RectTransform), typeof(TextMeshProUGUI));
            descGo.transform.SetParent(headerGo.transform, false);
            var descRt = (RectTransform)descGo.transform;
            descRt.anchorMin = new Vector2(0f, 0f);
            descRt.anchorMax = new Vector2(1f, 0.5f);
            descRt.offsetMin = descRt.offsetMax = Vector2.zero;
            var descText = descGo.GetComponent<TextMeshProUGUI>();
            descText.text = "Descrição da Habilidade";
            descText.fontSize = 20;
            descText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            descText.alignment = TextAlignmentOptions.Center;

            var tiersGo = new GameObject("TiersContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            tiersGo.transform.SetParent(modalGo.transform, false);
            var optRt = (RectTransform)tiersGo.transform;
            optRt.anchorMin = new Vector2(0.1f, 0.2f);
            optRt.anchorMax = new Vector2(0.9f, 0.72f);
            optRt.offsetMin = optRt.offsetMax = Vector2.zero;
            
            var vlg = tiersGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            var tierPrefab = new GameObject("TierPrefab", typeof(RectTransform), typeof(VerticalLayoutGroup));
            tierPrefab.transform.SetParent(modalGo.transform, false);
            var tierPrefabLayout = tierPrefab.GetComponent<VerticalLayoutGroup>();
            tierPrefabLayout.spacing = 5;
            tierPrefabLayout.childAlignment = TextAnchor.MiddleCenter;
            tierPrefabLayout.childControlHeight = true;
            tierPrefabLayout.childControlWidth = true;
            tierPrefabLayout.childForceExpandHeight = false;
            tierPrefabLayout.childForceExpandWidth = true;

            var tierTitle = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            tierTitle.transform.SetParent(tierPrefab.transform, false);
            tierTitle.GetComponent<TextMeshProUGUI>().text = "Tier";
            
            var tierOptions = new GameObject("OptionsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tierOptions.transform.SetParent(tierPrefab.transform, false);
            var tierOptionsLayout = tierOptions.GetComponent<HorizontalLayoutGroup>();
            tierOptionsLayout.spacing = 20;
            tierOptionsLayout.childAlignment = TextAnchor.MiddleCenter;
            tierOptionsLayout.childControlHeight = true;
            tierOptionsLayout.childControlWidth = true;
            tierOptionsLayout.childForceExpandHeight = false;
            tierOptionsLayout.childForceExpandWidth = true;

            tierPrefab.SetActive(false);

            TextMeshProUGUI dummyText;
            var optionPrefab = CreateSkillSelectionOptionPrefab(modalGo.transform, "OptionPrefab", out dummyText).gameObject;
            optionPrefab.SetActive(false);

            // Change Skill Button
            var changeBtn = CreateSimpleButton(modalGo.transform, "ChangeSkillButton", "Mudar Habilidade");
            var changeRt = (RectTransform)changeBtn.transform;
            changeRt.anchorMin = new Vector2(0.2f, 0.05f);
            changeRt.anchorMax = new Vector2(0.45f, 0.15f);
            changeRt.offsetMin = changeRt.offsetMax = Vector2.zero;

            // Unequip Button
            var unequipBtn = CreateSimpleButton(modalGo.transform, "UnequipSkillButton", "Desequipar");
            var unequipRt = (RectTransform)unequipBtn.transform;
            unequipRt.anchorMin = new Vector2(0.55f, 0.05f);
            unequipRt.anchorMax = new Vector2(0.8f, 0.15f);
            unequipRt.offsetMin = unequipRt.offsetMax = Vector2.zero;

            var closeBtn = CreateSimpleButton(modalGo.transform, "CloseButton", "Fechar");
            ((RectTransform)closeBtn.transform).anchorMin = new Vector2(0.85f, 0.9f);
            ((RectTransform)closeBtn.transform).anchorMax = new Vector2(0.95f, 0.97f);
            ((RectTransform)closeBtn.transform).offsetMin = ((RectTransform)closeBtn.transform).offsetMax = Vector2.zero;

            var comp = modalGo.GetComponent<SkillBranchModal>();
            comp.modalRoot = modalGo;
            comp.tiersContainer = optRt;
            comp.tierPrefab = tierPrefab;
            comp.optionPrefab = optionPrefab;
            comp.closeButton = closeBtn;
            comp.changeButton = changeBtn;
            comp.unequipButton = unequipBtn;
            comp.skillNameText = nameText;
            comp.skillDescText = descText;

            modalGo.SetActive(false);
            return comp;
        }

        private static Button CreateSimpleButton(Transform parent, string name, string label) {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f, 1f);
            
            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
            textRt.offsetMin = textRt.offsetMax = Vector2.zero;
            var textComp = textGo.GetComponent<TextMeshProUGUI>();
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.text = label;
            textComp.color = Color.white;
            textComp.enableWordWrapping = true;
            textComp.raycastTarget = false;

            return go.GetComponent<Button>();
        }

        private static Button CreateSkillSelectionOptionPrefab(Transform parent, string name, out TextMeshProUGUI textComp) {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f, 1f);
            
            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
            textRt.offsetMin = textRt.offsetMax = Vector2.zero;
            textComp = textGo.GetComponent<TextMeshProUGUI>();
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.text = name;
            textComp.color = Color.white;
            textComp.enableWordWrapping = true;
            textComp.raycastTarget = false;

            return go.GetComponent<Button>();
        }

        [MenuItem("Celestial Cross/3. UI Builders/4. Utilities/Add Manage Artifact Button")]
        public static void CreateManageArtifactButton() {
            var activeObj = UnityEditor.Selection.activeGameObject;
            if (activeObj == null) {
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
