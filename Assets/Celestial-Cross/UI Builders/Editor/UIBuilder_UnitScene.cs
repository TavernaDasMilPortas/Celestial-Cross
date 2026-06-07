using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Scenes.Unit;
using CelestialCross.UI.Skills;

namespace CelestialCross.UIBuilders.Editor
{
    public class UIBuilder_UnitScene : UnityEditor.Editor
    {
        [MenuItem("Celestial Cross/UI Builders/Scenes/Unit Scene (Novo)")]
        public static void BuildUnitScene()
        {
            var canvasObj = new GameObject("Canvas_UnitScene", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvasRT = canvasObj.GetComponent<RectTransform>();
            canvasRT.anchorMin = Vector2.zero;
            canvasRT.anchorMax = Vector2.one;
            canvasRT.offsetMin = Vector2.zero;
            canvasRT.offsetMax = Vector2.zero;
            canvasRT.localScale = Vector3.one;
            canvasRT.localPosition = Vector3.zero;
            canvasRT.anchoredPosition = Vector2.zero;

            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGO.transform.SetParent(canvasObj.transform, false);
            SetFullscreen(bgGO.GetComponent<RectTransform>());
            bgGO.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 1f);

            var controller = canvasObj.AddComponent<UnitSceneController>();

            // Auto-carregar catálogos
            string[] unitGuids = AssetDatabase.FindAssets("t:UnitCatalog");
            if (unitGuids.Length > 0) controller.unitCatalog = AssetDatabase.LoadAssetAtPath<UnitCatalog>(AssetDatabase.GUIDToAssetPath(unitGuids[0]));

            string[] petGuids = AssetDatabase.FindAssets("t:PetCatalog");
            if (petGuids.Length > 0) controller.petCatalog = AssetDatabase.LoadAssetAtPath<PetCatalog>(AssetDatabase.GUIDToAssetPath(petGuids[0]));
            
            string[] artGuids = AssetDatabase.FindAssets("t:ArtifactSetCatalog");
            if (artGuids.Length > 0) controller.artifactSetCatalog = AssetDatabase.LoadAssetAtPath<ArtifactSetCatalog>(AssetDatabase.GUIDToAssetPath(artGuids[0]));

            // Banner
            var bannerObj = new GameObject("UnitBanner", typeof(RectTransform), typeof(Image), typeof(UnitBannerUI));
            bannerObj.transform.SetParent(canvasObj.transform, false);
            var bannerRT = bannerObj.GetComponent<RectTransform>();
            bannerRT.anchorMin = new Vector2(0, 0.9f); bannerRT.anchorMax = new Vector2(1, 1);
            bannerRT.offsetMin = bannerRT.offsetMax = Vector2.zero;
            bannerObj.GetComponent<Image>().color = new Color(0.12f, 0.1f, 0.15f, 1f);
            
            var bannerTextObj = new GameObject("BannerText", typeof(RectTransform), typeof(TextMeshProUGUI));
            bannerTextObj.transform.SetParent(bannerObj.transform, false);
            SetFullscreen(bannerTextObj.GetComponent<RectTransform>());
            var bannerTMP = bannerTextObj.GetComponent<TextMeshProUGUI>();
            bannerTMP.text = "Detalhes da Unidade";
            bannerTMP.alignment = TextAlignmentOptions.Center; bannerTMP.fontSize = 40; bannerTMP.color = Color.white;
            
            var bannerUI = bannerObj.GetComponent<UnitBannerUI>();
            bannerUI.backgroundImage = bannerObj.GetComponent<Image>();
            bannerUI.bannerText = bannerTMP;
            controller.bannerUI = bannerUI;

            // Back Button
            var backBtnObj = new GameObject("Btn_BackToHub", typeof(RectTransform), typeof(Image), typeof(Button));
            backBtnObj.transform.SetParent(canvasObj.transform, false);
            var bbRT = backBtnObj.GetComponent<RectTransform>();
            bbRT.anchorMin = new Vector2(0.02f, 0.9f); bbRT.anchorMax = new Vector2(0.15f, 0.98f);
            bbRT.offsetMin = bbRT.offsetMax = Vector2.zero;
            backBtnObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
            CreateText(backBtnObj.transform, "Text", Vector2.zero, Vector2.one, "Voltar", 24).alignment = TextAlignmentOptions.Center;
            controller.backButton = backBtnObj.GetComponent<Button>();

            // Unit List (Horizontal Scroll)
            var uListScroll = new GameObject("UnitListScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            uListScroll.transform.SetParent(canvasObj.transform, false);
            var ulsRT = uListScroll.GetComponent<RectTransform>();
            ulsRT.anchorMin = new Vector2(0, 0.02f); ulsRT.anchorMax = new Vector2(1, 0.14f);
            ulsRT.offsetMin = ulsRT.offsetMax = Vector2.zero;
            uListScroll.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f, 1f);

            var uListViewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            uListViewport.transform.SetParent(uListScroll.transform, false);
            SetFullscreen(uListViewport.GetComponent<RectTransform>());
            uListViewport.GetComponent<Image>().color = Color.white;
            uListViewport.GetComponent<Mask>().showMaskGraphic = false;

            var uListContent = new GameObject("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            uListContent.transform.SetParent(uListViewport.transform, false);
            var ulcRT = uListContent.GetComponent<RectTransform>();
            ulcRT.anchorMin = new Vector2(0, 0); ulcRT.anchorMax = new Vector2(0, 1);
            ulcRT.pivot = new Vector2(0, 0.5f);
            ulcRT.sizeDelta = new Vector2(0, 0);
            ulcRT.offsetMin = ulcRT.offsetMax = Vector2.zero;

            var uhlg = uListContent.GetComponent<HorizontalLayoutGroup>();
            uhlg.childControlWidth = false; uhlg.childControlHeight = true;
            uhlg.childForceExpandWidth = false; uhlg.childForceExpandHeight = true;
            uhlg.spacing = 15; uhlg.padding = new RectOffset(15, 15, 10, 10);

            uListScroll.GetComponent<ScrollRect>().content = ulcRT;
            uListScroll.GetComponent<ScrollRect>().viewport = uListViewport.GetComponent<RectTransform>();
            uListScroll.GetComponent<ScrollRect>().horizontal = true;
            uListScroll.GetComponent<ScrollRect>().vertical = false;
            controller.unitListContainer = ulcRT;

            var uBtnPrefab = new GameObject("UnitBtnPrefab", typeof(RectTransform), typeof(Image), typeof(Button));
            uBtnPrefab.transform.SetParent(canvasObj.transform, false);
            uBtnPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
            uBtnPrefab.SetActive(false);
            controller.unitListButtonPrefab = uBtnPrefab;

            // Main Panel (Unit Profile + Tabs)
            var mainPanelObj = new GameObject("UnitMainPanel", typeof(RectTransform), typeof(Image), typeof(UnitMainPanel));
            mainPanelObj.transform.SetParent(canvasObj.transform, false);
            var mainRT = mainPanelObj.GetComponent<RectTransform>();
            mainRT.anchorMin = new Vector2(0.05f, 0.54f); mainRT.anchorMax = new Vector2(0.95f, 0.95f);
            mainRT.offsetMin = mainRT.offsetMax = Vector2.zero;
            mainPanelObj.GetComponent<Image>().color = new Color(0.15f, 0.12f, 0.2f, 1f);
            var mainPanel = mainPanelObj.GetComponent<UnitMainPanel>();
            controller.mainPanel = mainPanel;

            // Main Panel Info (Sprite, Name, Level, XP)
            var uSpriteGO = new GameObject("UnitSprite", typeof(RectTransform), typeof(Image));
            uSpriteGO.transform.SetParent(mainPanelObj.transform, false);
            var usRT = uSpriteGO.GetComponent<RectTransform>();
            usRT.anchorMin = new Vector2(0.05f, 0.2f); usRT.anchorMax = new Vector2(0.45f, 0.95f);
            usRT.offsetMin = usRT.offsetMax = Vector2.zero;
            mainPanel.unitSpriteImage = uSpriteGO.GetComponent<Image>();
            mainPanel.unitSpriteImage.preserveAspect = true;

            mainPanel.unitNameText = CreateText(mainPanelObj.transform, "UnitName", new Vector2(0.5f, 0.7f), new Vector2(0.95f, 0.95f), "Nome", 36);
            mainPanel.unitLevelText = CreateText(mainPanelObj.transform, "UnitLevel", new Vector2(0.5f, 0.5f), new Vector2(0.95f, 0.7f), "Lv. 1", 24);

            var xpBarGO = new GameObject("XpBar", typeof(RectTransform), typeof(Image));
            xpBarGO.transform.SetParent(mainPanelObj.transform, false);
            var xpRT = xpBarGO.GetComponent<RectTransform>();
            xpRT.anchorMin = new Vector2(0.5f, 0.35f); xpRT.anchorMax = new Vector2(0.95f, 0.45f);
            xpRT.offsetMin = xpRT.offsetMax = Vector2.zero;
            xpBarGO.GetComponent<Image>().color = Color.black; // Background

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(xpBarGO.transform, false);
            SetFullscreen(fill.GetComponent<RectTransform>());
            
            var fillImage = fill.GetComponent<Image>();
            fillImage.color = Color.yellow;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 0f;
            
            mainPanel.unitXpFillImage = fillImage;

            mainPanel.unitXpText = CreateText(mainPanelObj.transform, "XpText", new Vector2(0.5f, 0.2f), new Vector2(0.95f, 0.3f), "0 / 100", 18);

            // Tabs Bar
            var tabsBarObj = new GameObject("TabsBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tabsBarObj.transform.SetParent(mainPanelObj.transform, false);
            var tabsBarRT = tabsBarObj.GetComponent<RectTransform>();
            tabsBarRT.anchorMin = new Vector2(0, 0); tabsBarRT.anchorMax = new Vector2(1, 0.15f);
            tabsBarRT.offsetMin = tabsBarRT.offsetMax = Vector2.zero;
            var hlg = tabsBarObj.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10; hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.childControlWidth = true; hlg.childForceExpandWidth = true;

            for (int i = 0; i < 5; i++)
            {
                var tBtnObj = new GameObject($"Tab_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                tBtnObj.transform.SetParent(tabsBarObj.transform, false);
                tBtnObj.GetComponent<Image>().color = Color.gray;
                mainPanel.tabButtons[i] = tBtnObj.GetComponent<Button>();
                var txt = CreateText(tBtnObj.transform, "Text", Vector2.zero, Vector2.one, mainPanel.tabNames[i], 18);
                txt.alignment = TextAlignmentOptions.Center;
            }

            // Detail Container
            var detailObj = new GameObject("DetailContainer", typeof(RectTransform), typeof(Image));
            detailObj.transform.SetParent(canvasObj.transform, false);
            var detailRT = detailObj.GetComponent<RectTransform>();
            detailRT.anchorMin = new Vector2(0.05f, 0.16f); detailRT.anchorMax = new Vector2(0.95f, 0.52f);
            detailRT.offsetMin = detailRT.offsetMax = Vector2.zero;
            detailObj.GetComponent<Image>().color = new Color(0.1f, 0.08f, 0.12f, 1f);
            controller.detailContainer = detailRT;

            // Scroll Collection
            var scrollObj = new GameObject("ScrollCollectionContainer", typeof(RectTransform), typeof(Image));
            scrollObj.transform.SetParent(canvasObj.transform, false);
            var scrollRT = scrollObj.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0.85f); scrollRT.anchorMax = new Vector2(1, 0.98f);
            scrollRT.offsetMin = scrollRT.offsetMax = Vector2.zero;
            scrollObj.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 1f);
            controller.scrollCollectionContainer = scrollRT;

            // Modals
            var modalsObj = new GameObject("Modals_Container", typeof(RectTransform));
            modalsObj.transform.SetParent(canvasObj.transform, false);
            SetFullscreen(modalsObj.GetComponent<RectTransform>());

            var petModal = CreatePetModal(modalsObj.transform);
            var artModal = CreateArtifactModal(modalsObj.transform);
            var miniModal = CreateMiniInfoModal(modalsObj.transform);
            var selModal = CreateSkillSelectionModal(modalsObj.transform);
            var branchModal = CreateSkillBranchModal(modalsObj.transform);

            // Detail Panels
            controller.attributesDetailPanel = CreateAttributesPanel(detailObj.transform).gameObject;
            controller.petDetailPanel = CreatePetPanel(detailObj.transform, petModal).gameObject;
            controller.equipmentDetailPanel = CreateEquipmentPanel(detailObj.transform, artModal, miniModal).gameObject;

            controller.constellationDetailPanel = CreateConstellationPanel(detailObj.transform).gameObject;
            controller.abilitiesDetailPanel = CreateAbilitiesPanel(detailObj.transform, selModal, branchModal).gameObject;

            Selection.activeGameObject = canvasObj;

            // Marca como modificado e salva a cena para garantir a serialização
            EditorUtility.SetDirty(controller);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(controller.gameObject.scene);

            Debug.Log("UIBuilder_UnitScene refeito com painéis preenchidos e salvos.");
        }

        private static void SetFullscreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string defaultText, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var txt = go.GetComponent<TextMeshProUGUI>();
            txt.text = defaultText; txt.fontSize = fontSize; txt.color = Color.white;
            return txt;
        }

        private static UnitDetailPanel_Attributes CreateAttributesPanel(Transform parent)
        {
            var go = new GameObject("Panel_Attributes", typeof(RectTransform), typeof(Image), typeof(UnitDetailPanel_Attributes));
            go.transform.SetParent(parent, false);
            SetFullscreen(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().color = new Color(0,0,0,0);
            var comp = go.GetComponent<UnitDetailPanel_Attributes>();

            comp.hpText = CreateText(go.transform, "HP", new Vector2(0.1f, 0.8f), new Vector2(0.5f, 0.9f), "HP:", 24);
            comp.atkText = CreateText(go.transform, "ATK", new Vector2(0.1f, 0.7f), new Vector2(0.5f, 0.8f), "ATK:", 24);
            comp.defText = CreateText(go.transform, "DEF", new Vector2(0.1f, 0.6f), new Vector2(0.5f, 0.7f), "DEF:", 24);
            comp.spdText = CreateText(go.transform, "SPD", new Vector2(0.1f, 0.5f), new Vector2(0.5f, 0.6f), "SPD:", 24);

            comp.critRateText = CreateText(go.transform, "CRate", new Vector2(0.5f, 0.8f), new Vector2(0.9f, 0.9f), "C.RATE:", 24);
            comp.critDmgText = CreateText(go.transform, "CDmg", new Vector2(0.5f, 0.7f), new Vector2(0.9f, 0.8f), "C.DMG:", 24);
            comp.effectAccText = CreateText(go.transform, "Acc", new Vector2(0.5f, 0.6f), new Vector2(0.9f, 0.7f), "ACC:", 24);
            comp.effectResText = CreateText(go.transform, "Res", new Vector2(0.5f, 0.5f), new Vector2(0.9f, 0.6f), "RES:", 24);

            go.SetActive(false);
            return comp;
        }

        private static UnitDetailPanel_Pet CreatePetPanel(Transform parent, UnitPetSelectModal modal)
        {
            var go = new GameObject("Panel_Pet", typeof(RectTransform), typeof(Image), typeof(UnitDetailPanel_Pet));
            go.transform.SetParent(parent, false);
            SetFullscreen(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().color = new Color(0,0,0,0);
            var comp = go.GetComponent<UnitDetailPanel_Pet>();

            comp.petSelectModal = modal;

            var equipped = new GameObject("EquippedContainer", typeof(RectTransform));
            equipped.transform.SetParent(go.transform, false);
            SetFullscreen(equipped.GetComponent<RectTransform>());
            comp.petEquippedContainer = equipped;

            var empty = new GameObject("EmptyContainer", typeof(RectTransform));
            empty.transform.SetParent(go.transform, false);
            SetFullscreen(empty.GetComponent<RectTransform>());
            comp.petEmptyContainer = empty;

            CreateText(empty.transform, "EmptyText", Vector2.zero, Vector2.one, "Nenhum Pet Equipado", 30).alignment = TextAlignmentOptions.Center;

            var sprite = new GameObject("PetSprite", typeof(RectTransform), typeof(Image));
            sprite.transform.SetParent(equipped.transform, false);
            var sRT = sprite.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.1f, 0.3f); sRT.anchorMax = new Vector2(0.4f, 0.9f);
            sRT.offsetMin = sRT.offsetMax = Vector2.zero;
            comp.petSpriteImage = sprite.GetComponent<Image>();
            comp.petSpriteImage.preserveAspect = true;

            comp.petNameText = CreateText(equipped.transform, "PetName", new Vector2(0.5f, 0.7f), new Vector2(0.9f, 0.9f), "Nome do Pet", 30);
            comp.petStatsText = CreateText(equipped.transform, "PetStats", new Vector2(0.5f, 0.5f), new Vector2(0.9f, 0.7f), "HP: 0 | ATK: 0", 20);

            var skillIcon = new GameObject("SkillIcon", typeof(RectTransform), typeof(Image));
            skillIcon.transform.SetParent(equipped.transform, false);
            var skRT = skillIcon.GetComponent<RectTransform>();
            skRT.anchorMin = new Vector2(0.5f, 0.2f); skRT.anchorMax = new Vector2(0.6f, 0.4f);
            skRT.offsetMin = skRT.offsetMax = Vector2.zero;
            comp.skillIconImage = skillIcon.GetComponent<Image>();

            comp.skillDescText = CreateText(equipped.transform, "SkillDesc", new Vector2(0.65f, 0.2f), new Vector2(0.95f, 0.4f), "Descricao...", 16);

            var btnGO = new GameObject("Btn_SelectPet", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(go.transform, false);
            var btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.3f, 0.05f); btnRT.anchorMax = new Vector2(0.7f, 0.15f);
            btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
            btnGO.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.3f, 1f);
            CreateText(btnGO.transform, "Text", Vector2.zero, Vector2.one, "Selecionar Pet", 24).alignment = TextAlignmentOptions.Center;
            comp.selectPetButton = btnGO.GetComponent<Button>();

            go.SetActive(false);
            return comp;
        }

        private static UnitDetailPanel_Equipment CreateEquipmentPanel(Transform parent, UnitArtifactSelectModal artModal, ArtifactMiniInfoModal miniModal)
        {
            var go = new GameObject("Panel_Equipment", typeof(RectTransform), typeof(Image), typeof(UnitDetailPanel_Equipment));
            go.transform.SetParent(parent, false);
            SetFullscreen(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().color = new Color(0,0,0,0);
            var comp = go.GetComponent<UnitDetailPanel_Equipment>();

            comp.artifactSelectModal = artModal;
            comp.miniInfoModal = miniModal;

            var grid = new GameObject("SlotsGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            grid.transform.SetParent(go.transform, false);
            var gRT = grid.GetComponent<RectTransform>();
            gRT.anchorMin = new Vector2(0.05f, 0.2f); gRT.anchorMax = new Vector2(0.95f, 0.95f);
            gRT.offsetMin = gRT.offsetMax = Vector2.zero;
            
            var glg = grid.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(150, 150); glg.spacing = new Vector2(20, 20);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount; glg.constraintCount = 3;

            for (int i = 0; i < 6; i++)
            {
                var slot = new GameObject($"Slot_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                slot.transform.SetParent(grid.transform, false);
                slot.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);
                comp.artifactSlotButtons[i] = slot.GetComponent<Button>();

                var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                icon.transform.SetParent(slot.transform, false);
                SetFullscreen(icon.GetComponent<RectTransform>());
                comp.artifactSlotImages[i] = icon.GetComponent<Image>();
                comp.artifactSlotImages[i].gameObject.SetActive(false); // Initially hidden if empty
            }

            var skillsContainer = new GameObject("SkillsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            skillsContainer.transform.SetParent(go.transform, false);
            var skRT = skillsContainer.GetComponent<RectTransform>();
            skRT.anchorMin = new Vector2(0.05f, 0.05f); skRT.anchorMax = new Vector2(0.95f, 0.15f);
            skRT.offsetMin = skRT.offsetMax = Vector2.zero;
            comp.skillsContainer = skillsContainer.transform;

            var skillPrefab = new GameObject("SetSkillPrefab", typeof(RectTransform), typeof(Image));
            skillPrefab.transform.SetParent(go.transform, false);
            skillPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
            skillPrefab.SetActive(false);
            comp.skillIconPrefab = skillPrefab;

            go.SetActive(false);
            return comp;
        }

        private static UnitDetailPanel_Constellation CreateConstellationPanel(Transform parent)
        {
            var go = new GameObject("Panel_Constellation", typeof(RectTransform), typeof(Image), typeof(UnitDetailPanel_Constellation));
            go.transform.SetParent(parent, false);
            SetFullscreen(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().color = new Color(0,0,0,0);
            var comp = go.GetComponent<UnitDetailPanel_Constellation>();

            // Nodes Container
            var nodesCont = new GameObject("NodesContainer", typeof(RectTransform));
            nodesCont.transform.SetParent(go.transform, false);
            var nRT = nodesCont.GetComponent<RectTransform>();
            nRT.anchorMin = new Vector2(0.1f, 0.1f); nRT.anchorMax = new Vector2(0.5f, 0.9f);
            nRT.offsetMin = nRT.offsetMax = Vector2.zero;
            comp.nodesContainer = nRT;

            for(int i = 0; i < 5; i++)
            {
                var line = new GameObject($"Line_{i}", typeof(RectTransform), typeof(Image));
                line.transform.SetParent(nRT, false);
                line.GetComponent<Image>().color = Color.white;
                comp.connectionLines[i] = line.GetComponent<Image>();
            }

            for(int i = 0; i < 6; i++)
            {
                var star = new GameObject($"Star_{i}", typeof(RectTransform), typeof(Image));
                star.transform.SetParent(nRT, false);
                var sRT = star.GetComponent<RectTransform>();
                sRT.sizeDelta = new Vector2(40, 40);
                star.GetComponent<Image>().color = Color.gray;
                comp.starIcons[i] = star.GetComponent<Image>();
            }

            // Info Panel
            var info = new GameObject("InfoPanel", typeof(RectTransform), typeof(Image));
            info.transform.SetParent(go.transform, false);
            var iRT = info.GetComponent<RectTransform>();
            iRT.anchorMin = new Vector2(0.55f, 0.3f); iRT.anchorMax = new Vector2(0.95f, 0.9f);
            iRT.offsetMin = iRT.offsetMax = Vector2.zero;
            info.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);
            comp.infoPanel = info;

            comp.skillNameText = CreateText(info.transform, "SkillName", new Vector2(0.05f, 0.8f), new Vector2(0.95f, 0.95f), "Habilidade", 24);
            comp.skillNameText.alignment = TextAlignmentOptions.Center;
            comp.skillDescText = CreateText(info.transform, "SkillDesc", new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.75f), "Descrição da habilidade", 18);
            comp.skillDescText.alignment = TextAlignmentOptions.TopLeft;

            // Insignias e Botão
            comp.insigniaCountText = CreateText(go.transform, "InsigniaText", new Vector2(0.55f, 0.15f), new Vector2(0.95f, 0.25f), "Insígnias: 0", 20);
            comp.insigniaCountText.alignment = TextAlignmentOptions.Center;

            var btnGO = new GameObject("Btn_Upgrade", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(go.transform, false);
            var btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.6f, 0.05f); btnRT.anchorMax = new Vector2(0.9f, 0.15f);
            btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
            btnGO.GetComponent<Image>().color = new Color(0.8f, 0.6f, 0.1f, 1f);
            CreateText(btnGO.transform, "Text", Vector2.zero, Vector2.one, "Melhorar", 24).alignment = TextAlignmentOptions.Center;
            comp.upgradeButton = btnGO.GetComponent<Button>();

            go.SetActive(false);
            return comp;
        }

        private static UnitDetailPanel_Abilities CreateAbilitiesPanel(Transform parent, SkillSelectionModal selModal, SkillBranchModal branchModal)
        {
            var go = new GameObject("Panel_Abilities", typeof(RectTransform), typeof(Image), typeof(UnitDetailPanel_Abilities));
            go.transform.SetParent(parent, false);
            SetFullscreen(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().color = new Color(0,0,0,0);
            var comp = go.GetComponent<UnitDetailPanel_Abilities>();
            
            comp.selectionModal = selModal;
            comp.branchModal = branchModal;

            var grid = new GameObject("SlotsGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            grid.transform.SetParent(go.transform, false);
            var gRT = grid.GetComponent<RectTransform>();
            gRT.anchorMin = new Vector2(0.1f, 0.1f); gRT.anchorMax = new Vector2(0.9f, 0.9f);
            gRT.offsetMin = gRT.offsetMax = Vector2.zero;
            
            var glg = grid.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(150, 150); glg.spacing = new Vector2(50, 50);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount; glg.constraintCount = 2;

            // Basic
            var basicBtn = new GameObject("BasicSlot", typeof(RectTransform), typeof(Image), typeof(Button));
            basicBtn.transform.SetParent(grid.transform, false);
            basicBtn.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
            comp.basicSkillButton = basicBtn.GetComponent<Button>();
            comp.basicSkillText = CreateText(basicBtn.transform, "Text", new Vector2(0, -0.3f), new Vector2(1, 0), "Básico", 20);
            comp.basicSkillText.alignment = TextAlignmentOptions.Center;

            // Move
            var moveBtn = new GameObject("MoveSlot", typeof(RectTransform), typeof(Image), typeof(Button));
            moveBtn.transform.SetParent(grid.transform, false);
            moveBtn.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
            comp.movementSkillButton = moveBtn.GetComponent<Button>();
            comp.movementSkillText = CreateText(moveBtn.transform, "Text", new Vector2(0, -0.3f), new Vector2(1, 0), "Move", 20);
            comp.movementSkillText.alignment = TextAlignmentOptions.Center;

            // Slot 1
            var s1Btn = new GameObject("Slot1", typeof(RectTransform), typeof(Image), typeof(Button));
            s1Btn.transform.SetParent(grid.transform, false);
            s1Btn.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
            comp.slot1SkillButton = s1Btn.GetComponent<Button>();
            comp.slot1SkillText = CreateText(s1Btn.transform, "Text", new Vector2(0, -0.3f), new Vector2(1, 0), "Slot 1", 20);
            comp.slot1SkillText.alignment = TextAlignmentOptions.Center;

            // Slot 2
            var s2Btn = new GameObject("Slot2", typeof(RectTransform), typeof(Image), typeof(Button));
            s2Btn.transform.SetParent(grid.transform, false);
            s2Btn.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
            comp.slot2SkillButton = s2Btn.GetComponent<Button>();
            comp.slot2SkillText = CreateText(s2Btn.transform, "Text", new Vector2(0, -0.3f), new Vector2(1, 0), "Slot 2", 20);
            comp.slot2SkillText.alignment = TextAlignmentOptions.Center;

            go.SetActive(false);
            return comp;
        }

        private static UnitPetSelectModal CreatePetModal(Transform parent)
        {
            var go = new GameObject("UnitPetSelectModal", typeof(RectTransform), typeof(Image), typeof(UnitPetSelectModal));
            go.transform.SetParent(parent, false);
            SetFullscreen(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            var comp = go.GetComponent<UnitPetSelectModal>();

            var window = new GameObject("Window", typeof(RectTransform), typeof(Image));
            window.transform.SetParent(go.transform, false);
            var wRT = window.GetComponent<RectTransform>();
            wRT.sizeDelta = new Vector2(900, 1200);
            window.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

            CreateText(window.transform, "Title", new Vector2(0.1f, 0.9f), new Vector2(0.9f, 1f), "Selecione um Pet", 36).alignment = TextAlignmentOptions.Center;

            var scroll = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            scroll.transform.SetParent(window.transform, false);
            var sRT = scroll.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.05f, 0.15f); sRT.anchorMax = new Vector2(0.95f, 0.88f);
            sRT.offsetMin = sRT.offsetMax = Vector2.zero;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scroll.transform, false);
            SetFullscreen(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
            content.transform.SetParent(viewport.transform, false);
            var cRT = content.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0, 1); cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            var glg = content.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(200, 250); glg.spacing = new Vector2(20, 20);

            scroll.GetComponent<ScrollRect>().content = cRT;
            scroll.GetComponent<ScrollRect>().viewport = viewport.GetComponent<RectTransform>();
            comp.gridContainer = cRT;

            var equipBtnObj = new GameObject("Btn_Equip", typeof(RectTransform), typeof(Image), typeof(Button));
            equipBtnObj.transform.SetParent(window.transform, false);
            var eRT = equipBtnObj.GetComponent<RectTransform>();
            eRT.anchorMin = new Vector2(0.6f, 0.02f); eRT.anchorMax = new Vector2(0.95f, 0.12f);
            eRT.offsetMin = eRT.offsetMax = Vector2.zero;
            equipBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 1f);
            CreateText(equipBtnObj.transform, "Text", Vector2.zero, Vector2.one, "Equipar", 24).alignment = TextAlignmentOptions.Center;
            comp.equipButton = equipBtnObj.GetComponent<Button>();

            var closeBtnObj = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(window.transform, false);
            var clRT = closeBtnObj.GetComponent<RectTransform>();
            clRT.anchorMin = new Vector2(0.05f, 0.02f); clRT.anchorMax = new Vector2(0.4f, 0.12f);
            clRT.offsetMin = clRT.offsetMax = Vector2.zero;
            closeBtnObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
            CreateText(closeBtnObj.transform, "Text", Vector2.zero, Vector2.one, "Fechar", 24).alignment = TextAlignmentOptions.Center;
            comp.closeButton = closeBtnObj.GetComponent<Button>();

            go.SetActive(false);
            return comp;
        }

        private static UnitArtifactSelectModal CreateArtifactModal(Transform parent)
        {
            var go = new GameObject("UnitArtifactSelectModal", typeof(RectTransform), typeof(Image), typeof(UnitArtifactSelectModal));
            go.transform.SetParent(parent, false);
            SetFullscreen(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            var comp = go.GetComponent<UnitArtifactSelectModal>();

            var window = new GameObject("Window", typeof(RectTransform), typeof(Image));
            window.transform.SetParent(go.transform, false);
            var wRT = window.GetComponent<RectTransform>();
            wRT.sizeDelta = new Vector2(900, 1200);
            window.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

            CreateText(window.transform, "Title", new Vector2(0.1f, 0.9f), new Vector2(0.9f, 1f), "Selecione um Artefato", 36).alignment = TextAlignmentOptions.Center;

            var scroll = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            scroll.transform.SetParent(window.transform, false);
            var sRT = scroll.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.05f, 0.15f); sRT.anchorMax = new Vector2(0.95f, 0.88f);
            sRT.offsetMin = sRT.offsetMax = Vector2.zero;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scroll.transform, false);
            SetFullscreen(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
            content.transform.SetParent(viewport.transform, false);
            var cRT = content.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0, 1); cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            var glg = content.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(200, 250); glg.spacing = new Vector2(20, 20);

            scroll.GetComponent<ScrollRect>().content = cRT;
            scroll.GetComponent<ScrollRect>().viewport = viewport.GetComponent<RectTransform>();
            comp.gridContainer = cRT;

            var equipBtnObj = new GameObject("Btn_Equip", typeof(RectTransform), typeof(Image), typeof(Button));
            equipBtnObj.transform.SetParent(window.transform, false);
            var eRT = equipBtnObj.GetComponent<RectTransform>();
            eRT.anchorMin = new Vector2(0.6f, 0.02f); eRT.anchorMax = new Vector2(0.95f, 0.12f);
            eRT.offsetMin = eRT.offsetMax = Vector2.zero;
            equipBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 1f);
            CreateText(equipBtnObj.transform, "Text", Vector2.zero, Vector2.one, "Equipar", 24).alignment = TextAlignmentOptions.Center;
            comp.equipButton = equipBtnObj.GetComponent<Button>();

            var closeBtnObj = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(window.transform, false);
            var clRT = closeBtnObj.GetComponent<RectTransform>();
            clRT.anchorMin = new Vector2(0.05f, 0.02f); clRT.anchorMax = new Vector2(0.4f, 0.12f);
            clRT.offsetMin = clRT.offsetMax = Vector2.zero;
            closeBtnObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
            CreateText(closeBtnObj.transform, "Text", Vector2.zero, Vector2.one, "Fechar", 24).alignment = TextAlignmentOptions.Center;
            comp.closeButton = closeBtnObj.GetComponent<Button>();

            go.SetActive(false);
            return comp;
        }

        private static ArtifactMiniInfoModal CreateMiniInfoModal(Transform parent)
        {
            var go = new GameObject("ArtifactMiniInfoModal", typeof(RectTransform), typeof(Image), typeof(ArtifactMiniInfoModal));
            go.transform.SetParent(parent, false);
            SetFullscreen(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            var comp = go.GetComponent<ArtifactMiniInfoModal>();

            var window = new GameObject("Window", typeof(RectTransform), typeof(Image));
            window.transform.SetParent(go.transform, false);
            var wRT = window.GetComponent<RectTransform>();
            wRT.sizeDelta = new Vector2(600, 800);
            window.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(window.transform, false);
            var iRT = icon.GetComponent<RectTransform>();
            iRT.anchorMin = new Vector2(0.1f, 0.6f); iRT.anchorMax = new Vector2(0.4f, 0.9f);
            iRT.offsetMin = iRT.offsetMax = Vector2.zero;
            comp.iconImage = icon.GetComponent<Image>();

            comp.statsText = CreateText(window.transform, "Stats", new Vector2(0.5f, 0.3f), new Vector2(0.9f, 0.9f), "Stats", 20);
            
            var alterBtn = new GameObject("Btn_Alter", typeof(RectTransform), typeof(Image), typeof(Button));
            alterBtn.transform.SetParent(window.transform, false);
            var aRT = alterBtn.GetComponent<RectTransform>();
            aRT.anchorMin = new Vector2(0.55f, 0.05f); aRT.anchorMax = new Vector2(0.95f, 0.15f);
            aRT.offsetMin = aRT.offsetMax = Vector2.zero;
            alterBtn.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 1f);
            CreateText(alterBtn.transform, "Text", Vector2.zero, Vector2.one, "Alterar", 24).alignment = TextAlignmentOptions.Center;
            comp.alterButton = alterBtn.GetComponent<Button>();

            var closeBtn = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtn.transform.SetParent(window.transform, false);
            var cRT = closeBtn.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.05f, 0.05f); cRT.anchorMax = new Vector2(0.45f, 0.15f);
            cRT.offsetMin = cRT.offsetMax = Vector2.zero;
            closeBtn.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
            CreateText(closeBtn.transform, "Text", Vector2.zero, Vector2.one, "Fechar", 24).alignment = TextAlignmentOptions.Center;
            comp.closeButton = closeBtn.GetComponent<Button>();

            go.SetActive(false);
            return comp;
        }

        private static SkillSelectionModal CreateSkillSelectionModal(Transform parent)
        {
            var go = new GameObject("SkillSelectionModal", typeof(RectTransform), typeof(Image), typeof(SkillSelectionModal));
            go.transform.SetParent(parent, false);
            SetFullscreen(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            var comp = go.GetComponent<SkillSelectionModal>();
            comp.modalRoot = go;

            var window = new GameObject("Window", typeof(RectTransform), typeof(Image));
            window.transform.SetParent(go.transform, false);
            var wRT = window.GetComponent<RectTransform>();
            wRT.sizeDelta = new Vector2(900, 1200);
            window.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

            var titleText = CreateText(window.transform, "Title", new Vector2(0.1f, 0.9f), new Vector2(0.9f, 1f), "Selecione uma Habilidade", 36);
            titleText.alignment = TextAlignmentOptions.Center;

            var scroll = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            scroll.transform.SetParent(window.transform, false);
            var sRT = scroll.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.05f, 0.15f); sRT.anchorMax = new Vector2(0.95f, 0.88f);
            sRT.offsetMin = sRT.offsetMax = Vector2.zero;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scroll.transform, false);
            SetFullscreen(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
            content.transform.SetParent(viewport.transform, false);
            var cRT = content.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0, 1); cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            var glg = content.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(200, 250); glg.spacing = new Vector2(20, 20);

            scroll.GetComponent<ScrollRect>().content = cRT;
            scroll.GetComponent<ScrollRect>().viewport = viewport.GetComponent<RectTransform>();
            comp.optionsContainer = cRT;

            // Option Prefab
            var optPrefab = new GameObject("OptionPrefab", typeof(RectTransform), typeof(Image), typeof(Button));
            optPrefab.transform.SetParent(go.transform, false);
            optPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 250);
            CreateText(optPrefab.transform, "Text", Vector2.zero, Vector2.one, "Skill", 20).alignment = TextAlignmentOptions.Center;
            optPrefab.SetActive(false);
            comp.optionPrefab = optPrefab;

            var equipBtnObj = new GameObject("Btn_Equip", typeof(RectTransform), typeof(Image), typeof(Button));
            equipBtnObj.transform.SetParent(window.transform, false);
            var eRT = equipBtnObj.GetComponent<RectTransform>();
            eRT.anchorMin = new Vector2(0.6f, 0.02f); eRT.anchorMax = new Vector2(0.95f, 0.12f);
            eRT.offsetMin = eRT.offsetMax = Vector2.zero;
            equipBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 1f);
            CreateText(equipBtnObj.transform, "Text", Vector2.zero, Vector2.one, "Equipar", 24).alignment = TextAlignmentOptions.Center;
            comp.equipButton = equipBtnObj.GetComponent<Button>();

            var closeBtnObj = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(window.transform, false);
            var clRT = closeBtnObj.GetComponent<RectTransform>();
            clRT.anchorMin = new Vector2(0.05f, 0.02f); clRT.anchorMax = new Vector2(0.4f, 0.12f);
            clRT.offsetMin = clRT.offsetMax = Vector2.zero;
            closeBtnObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
            CreateText(closeBtnObj.transform, "Text", Vector2.zero, Vector2.one, "Fechar", 24).alignment = TextAlignmentOptions.Center;
            comp.closeButton = closeBtnObj.GetComponent<Button>();

            go.SetActive(false);
            return comp;
        }

        private static SkillBranchModal CreateSkillBranchModal(Transform parent)
        {
            var go = new GameObject("SkillBranchModal", typeof(RectTransform), typeof(Image), typeof(SkillBranchModal));
            go.transform.SetParent(parent, false);
            SetFullscreen(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            var comp = go.GetComponent<SkillBranchModal>();
            comp.modalRoot = go;

            var window = new GameObject("Window", typeof(RectTransform), typeof(Image));
            window.transform.SetParent(go.transform, false);
            var wRT = window.GetComponent<RectTransform>();
            wRT.sizeDelta = new Vector2(900, 1200);
            window.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

            comp.skillNameText = CreateText(window.transform, "Title", new Vector2(0.1f, 0.9f), new Vector2(0.9f, 1f), "Ramificações", 36);
            comp.skillNameText.alignment = TextAlignmentOptions.Center;

            comp.skillDescText = CreateText(window.transform, "Desc", new Vector2(0.1f, 0.8f), new Vector2(0.9f, 0.9f), "Descrição da habilidade", 24);
            comp.skillDescText.alignment = TextAlignmentOptions.Center;

            var changeBtn = new GameObject("Btn_Change", typeof(RectTransform), typeof(Image), typeof(Button));
            changeBtn.transform.SetParent(window.transform, false);
            var chRT = changeBtn.GetComponent<RectTransform>();
            chRT.anchorMin = new Vector2(0.6f, 0.02f); chRT.anchorMax = new Vector2(0.95f, 0.12f);
            chRT.offsetMin = chRT.offsetMax = Vector2.zero;
            changeBtn.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 1f);
            CreateText(changeBtn.transform, "Text", Vector2.zero, Vector2.one, "Trocar", 24).alignment = TextAlignmentOptions.Center;
            comp.changeButton = changeBtn.GetComponent<Button>();

            var unequipBtn = new GameObject("Btn_Unequip", typeof(RectTransform), typeof(Image), typeof(Button));
            unequipBtn.transform.SetParent(window.transform, false);
            var uqRT = unequipBtn.GetComponent<RectTransform>();
            uqRT.anchorMin = new Vector2(0.35f, 0.02f); uqRT.anchorMax = new Vector2(0.55f, 0.12f);
            uqRT.offsetMin = uqRT.offsetMax = Vector2.zero;
            unequipBtn.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.2f, 1f);
            CreateText(unequipBtn.transform, "Text", Vector2.zero, Vector2.one, "Desequipar", 24).alignment = TextAlignmentOptions.Center;
            comp.unequipButton = unequipBtn.GetComponent<Button>();

            var closeBtn = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtn.transform.SetParent(window.transform, false);
            var cRT = closeBtn.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.05f, 0.02f); cRT.anchorMax = new Vector2(0.3f, 0.12f);
            cRT.offsetMin = cRT.offsetMax = Vector2.zero;
            closeBtn.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
            CreateText(closeBtn.transform, "Text", Vector2.zero, Vector2.one, "Fechar", 24).alignment = TextAlignmentOptions.Center;
            comp.closeButton = closeBtn.GetComponent<Button>();

            var scroll = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            scroll.transform.SetParent(window.transform, false);
            var sRT = scroll.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.05f, 0.15f); sRT.anchorMax = new Vector2(0.95f, 0.75f);
            sRT.offsetMin = sRT.offsetMax = Vector2.zero;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scroll.transform, false);
            SetFullscreen(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
            content.transform.SetParent(viewport.transform, false);
            var coRT = content.GetComponent<RectTransform>();
            coRT.anchorMin = new Vector2(0, 1); coRT.anchorMax = new Vector2(1, 1);
            coRT.pivot = new Vector2(0.5f, 1);
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;
            vlg.spacing = 20; vlg.padding = new RectOffset(10, 10, 10, 10);

            scroll.GetComponent<ScrollRect>().content = coRT;
            scroll.GetComponent<ScrollRect>().viewport = viewport.GetComponent<RectTransform>();
            comp.tiersContainer = coRT;

            // Prefabs
            var tierPrefab = new GameObject("TierPrefab", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            tierPrefab.transform.SetParent(go.transform, false);
            tierPrefab.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 1f);
            var tpVLG = tierPrefab.GetComponent<VerticalLayoutGroup>();
            tpVLG.childControlWidth = true; tpVLG.childControlHeight = true;
            tpVLG.childForceExpandHeight = false; tpVLG.childForceExpandWidth = true;
            tpVLG.spacing = 10; tpVLG.padding = new RectOffset(10, 10, 10, 10);
            
            var tierTitle = CreateText(tierPrefab.transform, "TitleText", Vector2.zero, Vector2.zero, "Tier X", 24);
            tierTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40);
            tierTitle.alignment = TextAlignmentOptions.Center;

            var optContainer = new GameObject("OptionsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            optContainer.transform.SetParent(tierPrefab.transform, false);
            var hlg = optContainer.GetComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = false; hlg.childControlHeight = false;
            hlg.childForceExpandHeight = false; hlg.childForceExpandWidth = false;
            hlg.spacing = 20; hlg.childAlignment = TextAnchor.MiddleCenter;

            tierPrefab.SetActive(false);
            comp.tierPrefab = tierPrefab;

            var optPrefab = new GameObject("OptionPrefab", typeof(RectTransform), typeof(Image), typeof(Button));
            optPrefab.transform.SetParent(go.transform, false);
            optPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 150);
            optPrefab.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f, 1f);
            var optText = CreateText(optPrefab.transform, "Text", Vector2.zero, Vector2.one, "Opt", 18);
            optText.alignment = TextAlignmentOptions.Center;
            optPrefab.SetActive(false);
            comp.optionPrefab = optPrefab;

            go.SetActive(false);
            return comp;
        }
    }
}
