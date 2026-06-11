using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Scenes.Unit;
using CelestialCross.Giulia_UI;

namespace CelestialCross.EditorScripts
{
    public class UIBuilder_UnitMasterUpdater : EditorWindow
    {
        [MenuItem("Celestial Cross/UI/Master Updater (Unidades)")]
        public static void RunMasterUpdater()
        {
            Debug.Log("[MasterUpdater] Iniciando atualização geral da UI...");

            // 1. Atualizar UnitDetailPanel_Pet (Texto +)
            var petPanel = Object.FindObjectOfType<UnitDetailPanel_Pet>(true);
            if (petPanel != null && petPanel.plusText == null)
            {
                Button targetBtn = petPanel.emptySlotButton != null ? petPanel.emptySlotButton : petPanel.petImageButton;
                if (targetBtn != null)
                {
                    GameObject plusObj = new GameObject("PlusText", typeof(RectTransform), typeof(TextMeshProUGUI));
                    plusObj.transform.SetParent(targetBtn.transform, false);
                    var rt = plusObj.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                    var tmp = plusObj.GetComponent<TextMeshProUGUI>();
                    tmp.text = "+"; tmp.alignment = TextAlignmentOptions.Center;
                    tmp.fontSize = 72; tmp.color = Color.white;
                    tmp.enableAutoSizing = true; tmp.fontSizeMin = 24; tmp.fontSizeMax = 72;
                    petPanel.plusText = tmp;
                    EditorUtility.SetDirty(petPanel);
                    Debug.Log("[MasterUpdater] Adicionado texto '+' no painel de Pet.");
                }
            }

            // 2. Atualizar UnitDetailPanel_Attributes (Clickable)
            var attrPanel = Object.FindObjectOfType<UnitDetailPanel_Attributes>(true);
            if (attrPanel != null)
            {
                var graphic = attrPanel.GetComponent<Graphic>();
                if (graphic == null)
                {
                    graphic = attrPanel.gameObject.AddComponent<Image>();
                    graphic.color = new Color(0, 0, 0, 0);
                    EditorUtility.SetDirty(attrPanel);
                    Debug.Log("[MasterUpdater] Adicionado Image transparente ao painel de atributos.");
                }
            }

            // 3. Atualizar UnitMainPanel (Leveling Config)
            var mainPanel = Object.FindObjectOfType<UnitMainPanel>(true);
            if (mainPanel != null && mainPanel.levelingConfig == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:LevelingConfig");
                if (guids.Length > 0)
                {
                    mainPanel.levelingConfig = AssetDatabase.LoadAssetAtPath<LevelingConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    EditorUtility.SetDirty(mainPanel);
                    Debug.Log("[MasterUpdater] LevelingConfig linkado.");
                }
            }

            // 4. Artifact Action Modal & Equipment Panel
            var eqPanel = Object.FindObjectOfType<UnitDetailPanel_Equipment>(true);
            if (eqPanel != null)
            {
                if (eqPanel.actionModal == null)
                {
                    var modalGO = new GameObject("ArtifactActionModal", typeof(RectTransform), typeof(Image), typeof(ArtifactActionModal));
                    var rt = modalGO.GetComponent<RectTransform>();
                    modalGO.transform.SetParent(eqPanel.transform.parent, false);
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                    modalGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

                    var modalComp = modalGO.GetComponent<ArtifactActionModal>();
                    var popupGO = new GameObject("PopupBg", typeof(RectTransform), typeof(Image));
                    popupGO.transform.SetParent(modalGO.transform, false);
                    var pRt = popupGO.GetComponent<RectTransform>();
                    pRt.sizeDelta = new Vector2(600, 800);
                    popupGO.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);

                    var imgGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                    imgGO.transform.SetParent(popupGO.transform, false);
                    imgGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 250);
                    imgGO.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 150);
                    modalComp.iconImage = imgGO.GetComponent<Image>();

                    var nameGO = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
                    nameGO.transform.SetParent(popupGO.transform, false);
                    nameGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 140);
                    nameGO.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 50);
                    var nameTxt = nameGO.GetComponent<TextMeshProUGUI>();
                    nameTxt.alignment = TextAlignmentOptions.Center; nameTxt.fontSize = 32;
                    modalComp.nameText = nameTxt;

                    var lvlGO = new GameObject("Level", typeof(RectTransform), typeof(TextMeshProUGUI));
                    lvlGO.transform.SetParent(popupGO.transform, false);
                    lvlGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 90);
                    lvlGO.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 40);
                    var lvlTxt = lvlGO.GetComponent<TextMeshProUGUI>();
                    lvlTxt.alignment = TextAlignmentOptions.Center; lvlTxt.fontSize = 24; lvlTxt.color = Color.yellow;
                    modalComp.levelText = lvlTxt;

                    var statsGO = new GameObject("Stats", typeof(RectTransform), typeof(TextMeshProUGUI));
                    statsGO.transform.SetParent(popupGO.transform, false);
                    statsGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -60);
                    statsGO.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 200);
                    var statsTxt = statsGO.GetComponent<TextMeshProUGUI>();
                    statsTxt.alignment = TextAlignmentOptions.TopLeft; statsTxt.fontSize = 22;
                    modalComp.statsText = statsTxt;

                    modalComp.upgradeButton = CreateBtn(popupGO.transform, "Btn_Upgrade", "Aprimorar", new Vector2(0, -220));
                    modalComp.changeButton = CreateBtn(popupGO.transform, "Btn_Change", "Trocar", new Vector2(0, -290));
                    modalComp.unequipButton = CreateBtn(popupGO.transform, "Btn_Unequip", "Desequipar", new Vector2(0, -360));
                    modalComp.closeButton = CreateBtn(popupGO.transform, "Btn_Close", "X", new Vector2(250, 350), new Vector2(50, 50));
                    modalComp.closeButton.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;

                    eqPanel.actionModal = modalComp;
                    modalGO.SetActive(false);
                    EditorUtility.SetDirty(eqPanel);
                    Debug.Log("[MasterUpdater] ArtifactActionModal criado.");
                }

                if (eqPanel.actionModal != null)
                {
                    bool changedActionModal = false;

                    if (eqPanel.actionModal.iconImage != null)
                    {
                        var outline = eqPanel.actionModal.iconImage.GetComponent<UnityEngine.UI.Outline>();
                        if (outline == null)
                        {
                            outline = eqPanel.actionModal.iconImage.gameObject.AddComponent<UnityEngine.UI.Outline>();
                            outline.effectDistance = new Vector2(3, -3);
                            outline.effectColor = Color.white;
                            changedActionModal = true;
                        }
                    }

                    if (eqPanel.actionModal.starsContainer == null)
                    {
                        var popupGO = eqPanel.actionModal.transform.Find("PopupBg");
                        if (popupGO != null)
                        {
                            var starsContGO = new GameObject("StarsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                            starsContGO.transform.SetParent(popupGO, false);
                            var stRT = starsContGO.GetComponent<RectTransform>();
                            stRT.anchoredPosition = new Vector2(0, 185); // Between icon(250) and name(140)
                            stRT.sizeDelta = new Vector2(200, 30);
                            
                            var stHLG = starsContGO.GetComponent<HorizontalLayoutGroup>();
                            stHLG.childAlignment = TextAnchor.MiddleCenter;
                            stHLG.childControlWidth = false;
                            stHLG.childControlHeight = false;
                            stHLG.spacing = 5;

                            eqPanel.actionModal.starsContainer = starsContGO.transform;
                            changedActionModal = true;
                        }
                    }

                    if (eqPanel.actionModal.starPrefab == null)
                    {
                        petPanel = Object.FindObjectOfType<UnitDetailPanel_Pet>(true);
                        if (petPanel != null && petPanel.starPrefab != null)
                        {
                            var newStar = Object.Instantiate(petPanel.starPrefab, eqPanel.actionModal.transform);
                            newStar.name = "DefaultStarPrefab";
                            newStar.SetActive(false);
                            eqPanel.actionModal.starPrefab = newStar;
                            changedActionModal = true;
                        }
                    }

                    if (changedActionModal)
                    {
                        EditorUtility.SetDirty(eqPanel.actionModal);
                        Debug.Log("[MasterUpdater] Outline e Stars adicionados ao ArtifactActionModal.");
                    }
                }

                if (eqPanel.artifactSelectModal != null && eqPanel.artifactSelectModal.backButton == null)
                {
                    eqPanel.artifactSelectModal.backButton = CreateBtn(eqPanel.artifactSelectModal.transform, "Btn_Back", "Voltar", new Vector2(-250, 350), new Vector2(100, 50));
                    EditorUtility.SetDirty(eqPanel.artifactSelectModal);
                    Debug.Log("[MasterUpdater] Botão Voltar adicionado ao SelectModal.");
                }

                // 5. Artifact Upgrade Modal & Slider
                if (eqPanel.upgradeModal == null)
                {
                    string[] upgGuids = AssetDatabase.FindAssets("ArtifactUpgradeModal t:GameObject");
                    GameObject upgPrefab = null;
                    foreach (var g in upgGuids)
                    {
                        var go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g));
                        if (go != null && go.GetComponent<ArtifactUpgradeModal>() != null) { upgPrefab = go; break; }
                    }

                    if (upgPrefab != null)
                    {
                        var instance = (GameObject)PrefabUtility.InstantiatePrefab(upgPrefab, eqPanel.transform.parent);
                        eqPanel.upgradeModal = instance.GetComponent<ArtifactUpgradeModal>();
                        instance.SetActive(false);
                        EditorUtility.SetDirty(eqPanel);
                        Debug.Log("[MasterUpdater] ArtifactUpgradeModal instanciado.");
                    }
                    else
                    {
                        Debug.LogWarning("[MasterUpdater] Prefab do ArtifactUpgradeModal não encontrado.");
                    }
                }

                if (eqPanel.upgradeModal != null)
                {
                    var serializedObject = new SerializedObject(eqPanel.upgradeModal);
                    var sliderProp = serializedObject.FindProperty("levelSlider");
                    var textProp = serializedObject.FindProperty("levelTargetText");

                    bool changed = false;

                    if (sliderProp.objectReferenceValue == null)
                    {
                        var sliderGO = new GameObject("Slider_Level", typeof(RectTransform), typeof(Slider));
                        sliderGO.transform.SetParent(eqPanel.upgradeModal.transform, false);
                        var sRt = sliderGO.GetComponent<RectTransform>();
                        sRt.sizeDelta = new Vector2(300, 30);
                        sRt.anchoredPosition = new Vector2(0, 100);

                        var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
                        bgGO.transform.SetParent(sliderGO.transform, false);
                        bgGO.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.25f); bgGO.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.75f);
                        bgGO.GetComponent<RectTransform>().offsetMin = bgGO.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                        bgGO.GetComponent<Image>().color = Color.gray;

                        var fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
                        fillAreaGO.transform.SetParent(sliderGO.transform, false);
                        fillAreaGO.GetComponent<RectTransform>().anchorMin = Vector2.zero; fillAreaGO.GetComponent<RectTransform>().anchorMax = Vector2.one;
                        fillAreaGO.GetComponent<RectTransform>().offsetMin = fillAreaGO.GetComponent<RectTransform>().offsetMax = Vector2.zero;

                        var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                        fillGO.transform.SetParent(fillAreaGO.transform, false);
                        fillGO.GetComponent<RectTransform>().anchorMin = Vector2.zero; fillGO.GetComponent<RectTransform>().anchorMax = Vector2.one;
                        fillGO.GetComponent<RectTransform>().offsetMin = fillGO.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                        fillGO.GetComponent<Image>().color = Color.green;

                        var handleAreaGO = new GameObject("Handle Slide Area", typeof(RectTransform));
                        handleAreaGO.transform.SetParent(sliderGO.transform, false);
                        handleAreaGO.GetComponent<RectTransform>().anchorMin = Vector2.zero; handleAreaGO.GetComponent<RectTransform>().anchorMax = Vector2.one;
                        handleAreaGO.GetComponent<RectTransform>().offsetMin = handleAreaGO.GetComponent<RectTransform>().offsetMax = Vector2.zero;

                        var handleGO = new GameObject("Handle", typeof(RectTransform), typeof(Image));
                        handleGO.transform.SetParent(handleAreaGO.transform, false);
                        handleGO.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 0);
                        handleGO.GetComponent<Image>().color = Color.white;

                        var slider = sliderGO.GetComponent<Slider>();
                        slider.fillRect = fillGO.GetComponent<RectTransform>();
                        slider.handleRect = handleGO.GetComponent<RectTransform>();
                        sliderProp.objectReferenceValue = slider;
                        changed = true;
                    }

                    if (textProp.objectReferenceValue == null)
                    {
                        var txtGO = new GameObject("Text_TargetLevel", typeof(RectTransform), typeof(TextMeshProUGUI));
                        txtGO.transform.SetParent(eqPanel.upgradeModal.transform, false);
                        txtGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 150);
                        txtGO.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
                        var tmp = txtGO.GetComponent<TextMeshProUGUI>();
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.fontSize = 24;
                        textProp.objectReferenceValue = tmp;
                        changed = true;
                    }

                    if (changed)
                    {
                        serializedObject.ApplyModifiedProperties();
                        Debug.Log("[MasterUpdater] Slider adicionado ao ArtifactUpgradeModal.");
                    }
                }
            }

            // 6. Constellation Panel Updates
            var constelPanel = Object.FindObjectOfType<UnitDetailPanel_Constellation>(true);
            petPanel = Object.FindObjectOfType<UnitDetailPanel_Pet>(true);
            if (constelPanel != null)
            {
                var branchModal = Object.FindObjectOfType<CelestialCross.UI.Skills.SkillBranchModal>(true);
                if (branchModal != null)
                {
                    constelPanel.branchModal = branchModal;
                    if (petPanel != null) petPanel.branchModal = branchModal;
                    
                    if (branchModal.skillIconImage == null)
                    {
                        var window = branchModal.transform.Find("Window");
                        if (window != null)
                        {
                            var existingIcon = window.Find("SkillIcon");
                            if (existingIcon != null)
                            {
                                branchModal.skillIconImage = existingIcon.GetComponent<Image>();
                            }
                            else
                            {
                                var iconGo = new GameObject("SkillIcon", typeof(RectTransform), typeof(Image));
                                iconGo.transform.SetParent(window, false);
                                var icRt = iconGo.GetComponent<RectTransform>();
                                icRt.anchorMin = new Vector2(0.4f, 0.88f); icRt.anchorMax = new Vector2(0.6f, 0.98f);
                                icRt.offsetMin = icRt.offsetMax = Vector2.zero;
                                var img = iconGo.GetComponent<Image>();
                                img.preserveAspect = true;
                                branchModal.skillIconImage = img;
                                
                                if (branchModal.skillNameText != null)
                                {
                                    var rt = branchModal.skillNameText.GetComponent<RectTransform>();
                                    rt.anchorMin = new Vector2(0.1f, 0.80f); rt.anchorMax = new Vector2(0.9f, 0.88f);
                                }
                                if (branchModal.skillDescText != null)
                                {
                                    var rt = branchModal.skillDescText.GetComponent<RectTransform>();
                                    rt.anchorMin = new Vector2(0.1f, 0.65f); rt.anchorMax = new Vector2(0.9f, 0.80f);
                                }
                            }
                        }
                    }
                }

                // Transformar antigo InfoPanel ou criar ScrollView
                Transform infoPanelTrans = constelPanel.transform.Find("InfoPanel");
                if (infoPanelTrans != null && constelPanel.skillListContainer == null)
                {
                    // Desativar textos antigos
                    var oldName = infoPanelTrans.Find("SkillNameText");
                    var oldDesc = infoPanelTrans.Find("SkillDescText");
                    if (oldName != null) oldName.gameObject.SetActive(false);
                    if (oldDesc != null) oldDesc.gameObject.SetActive(false);

                    infoPanelTrans.gameObject.SetActive(true);

                    // Adicionar ScrollRect
                    var scrollRect = infoPanelTrans.gameObject.GetComponent<ScrollRect>();
                    if (scrollRect == null) scrollRect = infoPanelTrans.gameObject.AddComponent<ScrollRect>();

                    // Criar Viewport
                    var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
                    viewportGo.transform.SetParent(infoPanelTrans, false);
                    var vRt = viewportGo.GetComponent<RectTransform>();
                    vRt.anchorMin = Vector2.zero; vRt.anchorMax = Vector2.one;
                    vRt.offsetMin = Vector2.zero; vRt.offsetMax = Vector2.zero;
                    viewportGo.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);
                    viewportGo.GetComponent<Mask>().showMaskGraphic = false;

                    // Criar Content
                    var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                    contentGo.transform.SetParent(viewportGo.transform, false);
                    var cRt = contentGo.GetComponent<RectTransform>();
                    cRt.anchorMin = new Vector2(0, 1); cRt.anchorMax = new Vector2(1, 1);
                    cRt.pivot = new Vector2(0.5f, 1);
                    cRt.sizeDelta = new Vector2(0, 0);

                    var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
                    vlg.childAlignment = TextAnchor.UpperCenter;
                    vlg.childControlHeight = false; vlg.childControlWidth = true;
                    vlg.spacing = 10; vlg.padding = new RectOffset(10, 10, 10, 10);

                    var csf = contentGo.GetComponent<ContentSizeFitter>();
                    csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    scrollRect.viewport = vRt;
                    scrollRect.content = cRt;
                    scrollRect.horizontal = false;
                    scrollRect.vertical = true;

                    constelPanel.skillListContainer = cRt;

                    // Prefab do item
                    var itemGo = new GameObject("SkillItemPrefab", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
                    itemGo.transform.SetParent(cRt, false);
                    var iRt = itemGo.GetComponent<RectTransform>();
                    iRt.sizeDelta = new Vector2(0, 80);
                    
                    var itemImg = itemGo.GetComponent<Image>();
                    itemImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                    
                    var itemBtn = itemGo.GetComponent<Button>();
                    itemBtn.targetGraphic = itemImg;

                    var itemLE = itemGo.GetComponent<LayoutElement>();
                    itemLE.minHeight = 80;

                    var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                    iconGo.transform.SetParent(itemGo.transform, false);
                    var icRt = iconGo.GetComponent<RectTransform>();
                    icRt.anchorMin = new Vector2(0, 0.5f); icRt.anchorMax = new Vector2(0, 0.5f);
                    icRt.pivot = new Vector2(0, 0.5f);
                    icRt.anchoredPosition = new Vector2(10, 0);
                    icRt.sizeDelta = new Vector2(60, 60);

                    var textGo = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
                    textGo.transform.SetParent(itemGo.transform, false);
                    var tRt = textGo.GetComponent<RectTransform>();
                    tRt.anchorMin = new Vector2(0, 0); tRt.anchorMax = new Vector2(1, 1);
                    tRt.offsetMin = new Vector2(80, 0); tRt.offsetMax = new Vector2(0, 0);
                    var tmp = textGo.GetComponent<TextMeshProUGUI>();
                    tmp.alignment = TextAlignmentOptions.Left; tmp.fontSize = 20;

                    itemGo.SetActive(false);
                    constelPanel.skillListItemPrefab = itemGo;
                }

                // Criar botão Detalhes
                if (constelPanel.detailsButton == null)
                {
                    var btn = CreateBtn(constelPanel.transform, "Btn_Details", "Detalhes Constelação", new Vector2(0, -350), new Vector2(300, 60));
                    constelPanel.detailsButton = btn;
                }

                // Criar ConstellationDetailsModal
                if (constelPanel.detailsModal == null)
                {
                    var modalGo = new GameObject("ConstellationDetailsModal", typeof(RectTransform), typeof(Image), typeof(ConstellationDetailsModal));
                    modalGo.transform.SetParent(constelPanel.transform.parent, false);
                    var mRt = modalGo.GetComponent<RectTransform>();
                    mRt.anchorMin = Vector2.zero; mRt.anchorMax = Vector2.one;
                    mRt.offsetMin = Vector2.zero; mRt.offsetMax = Vector2.zero;
                    modalGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.95f);

                    var modalComp = modalGo.GetComponent<ConstellationDetailsModal>();

                    // Clonar as estrelas do ConstelPanel para o modal
                    var constelContainer = new GameObject("ConstellationContainer", typeof(RectTransform));
                    constelContainer.transform.SetParent(modalGo.transform, false);
                    var ccRt = constelContainer.GetComponent<RectTransform>();
                    ccRt.anchorMin = new Vector2(0, 0); ccRt.anchorMax = new Vector2(0.5f, 1);
                    ccRt.offsetMin = Vector2.zero; ccRt.offsetMax = Vector2.zero;
                    modalComp.constellationContainer = ccRt;

                    for (int i = 0; i < 6; i++)
                    {
                        var star = new GameObject($"Star_{i}", typeof(RectTransform), typeof(Image));
                        star.transform.SetParent(constelContainer.transform, false);
                        var sRt = star.GetComponent<RectTransform>();
                        sRt.sizeDelta = new Vector2(100, 100);
                        modalComp.starIcons[i] = star.GetComponent<Image>();
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        var line = new GameObject($"Line_{i}", typeof(RectTransform), typeof(Image));
                        line.transform.SetParent(constelContainer.transform, false);
                        modalComp.connectionLines[i] = line.GetComponent<Image>();
                    }

                    // List Container no Modal
                    var listContainer = new GameObject("ListContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
                    listContainer.transform.SetParent(modalGo.transform, false);
                    var lRt = listContainer.GetComponent<RectTransform>();
                    lRt.anchorMin = new Vector2(0.5f, 0); lRt.anchorMax = new Vector2(1, 1);
                    lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;
                    var vlg = listContainer.GetComponent<VerticalLayoutGroup>();
                    vlg.childAlignment = TextAnchor.UpperCenter; vlg.childControlWidth = true; vlg.spacing = 15;
                    modalComp.listContainer = lRt;

                    if (constelPanel.skillListItemPrefab != null)
                    {
                        modalComp.listItemPrefab = Instantiate(constelPanel.skillListItemPrefab, listContainer.transform);
                        modalComp.listItemPrefab.SetActive(false);
                    }

                    modalComp.closeButton = CreateBtn(modalGo.transform, "Btn_Close", "X", new Vector2(200, 400), new Vector2(50, 50));
                    modalComp.closeButton.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
                    modalComp.closeButton.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

                    constelPanel.detailsModal = modalComp;
                    modalGo.SetActive(false);
                }

                EditorUtility.SetDirty(constelPanel);
                Debug.Log("[MasterUpdater] Constellation Panel atualizado com List, Scroll e DetailsModal.");
            }

            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }

            Debug.Log("[MasterUpdater] Tudo atualizado com sucesso!");
        }

        private static Button CreateBtn(Transform parent, string name, string text, Vector2 pos, Vector2 size = default)
        {
            if (size == default) size = new Vector2(300, 60);

            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos; rt.sizeDelta = size;

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(go.transform, false);
            var tRt = txtGo.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.one;
            tRt.offsetMin = Vector2.zero; tRt.offsetMax = Vector2.zero;

            var txt = txtGo.GetComponent<TextMeshProUGUI>();
            txt.text = text; txt.alignment = TextAlignmentOptions.Center;
            txt.fontSize = 24; txt.color = Color.black;

            return go.GetComponent<Button>();
        }
    }
}
