using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.EditorArea;


namespace CelestialCross.Giulia_UI.Editor
{
    public class UIBuilder_VictoryModalComplete
    {
        [MenuItem("Celestial Cross/UI Builders/Generate Victory Modal Complete")]
        public static void BuildModal()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[UI Builder] Nenhum Canvas encontrado na cena.");
                return;
            }

            // 1. Root do Modal (Fundo Escuro)
            GameObject modalRoot = new GameObject("RewardDetailsModal_Complete", typeof(RectTransform), typeof(Image));
            modalRoot.transform.SetParent(canvas.transform, false);
            modalRoot.transform.SetAsLastSibling();
            RectTransform modalRt = modalRoot.GetComponent<RectTransform>();
            modalRt.anchorMin = Vector2.zero; modalRt.anchorMax = Vector2.one;
            modalRt.offsetMin = modalRt.offsetMax = Vector2.zero;
            modalRoot.GetComponent<Image>().color = new Color(0, 0, 0, 0.85f);

            // 2. ScrollView Principal
            GameObject scrollGO = new GameObject("MainScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollGO.transform.SetParent(modalRoot.transform, false);
            RectTransform scrollRT = scrollGO.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0f, 0.15f); scrollRT.anchorMax = new Vector2(1f, 1f);
            scrollRT.offsetMin = scrollRT.offsetMax = Vector2.zero;

            // Viewport
            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGO.transform, false);
            RectTransform viewRT = viewport.GetComponent<RectTransform>();
            viewRT.anchorMin = Vector2.zero; viewRT.anchorMax = Vector2.one;
            viewRT.offsetMin = viewRT.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.05f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            // Content
            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.offsetMin = contentRT.offsetMax = Vector2.zero;

            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 80, 100);
            vlg.spacing = 60;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = scrollGO.GetComponent<ScrollRect>();
            scrollRect.content = contentRT;
            scrollRect.viewport = viewRT;
            scrollRect.horizontal = false;

            // --- SEÇÃO 1: TÍTULO ---
            var titleGo = CreateText(content.transform, "ModalTitle", 80, Vector2.zero, Vector2.zero, Color.yellow);
            titleGo.GetComponent<TextMeshProUGUI>().text = "VITÓRIA!";
            titleGo.AddComponent<LayoutElement>().preferredHeight = 100;

            // --- SEÇÃO 2: RECURSOS ---
            GameObject resGO = new GameObject("Section_Resources", typeof(RectTransform), typeof(VerticalLayoutGroup));
            resGO.transform.SetParent(content.transform, false);
            var resVlg = resGO.GetComponent<VerticalLayoutGroup>();
            resVlg.spacing = 5;
            resVlg.childAlignment = TextAnchor.UpperLeft;
            resVlg.childControlHeight = true;
            resVlg.childForceExpandHeight = false;
            resVlg.childControlWidth = true;
            resVlg.childForceExpandWidth = true;

            var resHeader = CreateText(resGO.transform, "Header", 40, Vector2.zero, Vector2.zero, Color.gray);
            resHeader.GetComponent<TextMeshProUGUI>().text = "Recursos adquiridos:";
            resHeader.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            var resText = CreateText(resGO.transform, "ResourceList", 36, Vector2.zero, Vector2.zero, Color.white);
            resText.GetComponent<TextMeshProUGUI>().text = "- Dinheiro: +100\n- Energia: +10";
            resText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            // --- SEÇÃO 3: EXP (Será preenchida pelo VictoryXPPanel) ---
            GameObject expGO = new GameObject("Section_EXP", typeof(RectTransform), typeof(VerticalLayoutGroup));
            expGO.transform.SetParent(content.transform, false);
            var expVlg = expGO.GetComponent<VerticalLayoutGroup>();
            expVlg.spacing = 10;
            expVlg.childAlignment = TextAnchor.UpperCenter;
            expVlg.childControlHeight = true; expVlg.childForceExpandHeight = false;
            expVlg.childControlWidth = true; expVlg.childForceExpandWidth = true;


            var expHeader = CreateText(expGO.transform, "Header", 40, Vector2.zero, Vector2.zero, Color.gray);
            expHeader.GetComponent<TextMeshProUGUI>().text = "Experiência:";
            expHeader.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            // Chamar o Builder de XP aqui para preencher o expGO
            UIBuilder_VictoryXPPanel.GenerateXPPanelInTarget(expGO.transform);

            // --- SEÇÃO 4: LOOT ---
            GameObject lootGO = new GameObject("Section_Loot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            lootGO.transform.SetParent(content.transform, false);
            var lootVlg = lootGO.GetComponent<VerticalLayoutGroup>();
            lootVlg.spacing = 10;
            lootVlg.childControlHeight = true; lootVlg.childForceExpandHeight = false;
            lootVlg.childControlWidth = true; lootVlg.childForceExpandWidth = true;


            var lootHeader = CreateText(lootGO.transform, "Header", 40, Vector2.zero, Vector2.zero, Color.gray);
            lootHeader.GetComponent<TextMeshProUGUI>().text = "Drops:";
            lootHeader.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            // ScrollView interna para Loot
            GameObject lootScrollGO = new GameObject("LootScrollView", typeof(RectTransform), typeof(ScrollRect));
            lootScrollGO.transform.SetParent(lootGO.transform, false);
            lootScrollGO.AddComponent<LayoutElement>().preferredHeight = 600;
            
            GameObject lootViewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            lootViewport.transform.SetParent(lootScrollGO.transform, false);
            RectTransform lViewRT = lootViewport.GetComponent<RectTransform>();
            lViewRT.anchorMin = Vector2.zero; lViewRT.anchorMax = Vector2.one;
            lViewRT.offsetMin = lViewRT.offsetMax = Vector2.zero;
            lootViewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject lootContent = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            lootContent.transform.SetParent(lootViewport.transform, false);
            RectTransform lContentRT = lootContent.GetComponent<RectTransform>();
            lContentRT.anchorMin = new Vector2(0, 1); lContentRT.anchorMax = new Vector2(1, 1);
            lContentRT.pivot = new Vector2(0.5f, 1);
            lContentRT.offsetMin = lContentRT.offsetMax = Vector2.zero;
            
            var glg = lootContent.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(180, 180); glg.spacing = new Vector2(20, 20);
            glg.padding = new RectOffset(20, 20, 20, 20);
            lootContent.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            lootScrollGO.GetComponent<ScrollRect>().content = lContentRT;
            lootScrollGO.GetComponent<ScrollRect>().viewport = lViewRT;

            // --- BOTÕES (Fixos no fundo) ---
            GameObject closeBtnGo = new GameObject("Btn_Continue", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnGo.transform.SetParent(modalRoot.transform, false);
            RectTransform cRt = closeBtnGo.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0); cRt.anchorMax = new Vector2(0.5f, 0);
            cRt.sizeDelta = new Vector2(600, 120);
            cRt.anchoredPosition = new Vector2(0, 80);
            closeBtnGo.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f, 1f);
            var btnText = CreateText(closeBtnGo.transform, "Text", 48, Vector2.zero, Vector2.one, Color.white);
            btnText.GetComponent<TextMeshProUGUI>().text = "CONTINUAR";

            // --- NOVO: REWARD DETAILS MODAL (Pop-up de detalhes) ---
            GameObject detailModalGO = new GameObject("DetailsModal_Popup", typeof(RectTransform), typeof(Image));
            detailModalGO.transform.SetParent(modalRoot.transform, false);
            RectTransform detailRT = detailModalGO.GetComponent<RectTransform>();
            detailRT.anchorMin = new Vector2(0.1f, 0.2f); detailRT.anchorMax = new Vector2(0.9f, 0.8f);
            detailRT.offsetMin = detailRT.offsetMax = Vector2.zero;
            detailModalGO.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // Título do Detalhe
            var dTitle = CreateText(detailModalGO.transform, "ModalTitle", 40, new Vector2(0, 0.85f), new Vector2(1, 1), Color.white);
            // Descrição do Detalhe
            var dDesc = CreateText(detailModalGO.transform, "ModalDesc", 30, new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.8f), Color.white);
            dDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
            // Ícone do Detalhe
            GameObject dIcon = new GameObject("ModalIcon", typeof(RectTransform), typeof(Image));
            dIcon.transform.SetParent(detailModalGO.transform, false);
            RectTransform diRT = dIcon.GetComponent<RectTransform>();
            diRT.anchorMin = new Vector2(0.5f, 0.6f); diRT.anchorMax = new Vector2(0.5f, 0.6f);
            diRT.sizeDelta = new Vector2(150, 150);

            // Botão Vender/Soltar
            GameObject sellBtnGo = new GameObject("Generated_Btn_Sell", typeof(RectTransform), typeof(Image), typeof(Button));
            sellBtnGo.transform.SetParent(detailModalGO.transform, false);
            RectTransform sRt = sellBtnGo.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.1f, 0.05f); sRt.anchorMax = new Vector2(0.45f, 0.2f);
            sRt.offsetMin = sRt.offsetMax = Vector2.zero;
            sellBtnGo.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f, 1f);
            var sellText = CreateText(sellBtnGo.transform, "Text", 28, Vector2.zero, Vector2.one, Color.white);

            // Botão Fechar Detalhe
            GameObject dCloseBtnGo = new GameObject("Generated_Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            dCloseBtnGo.transform.SetParent(detailModalGO.transform, false);
            RectTransform dcRt = dCloseBtnGo.GetComponent<RectTransform>();
            dcRt.anchorMin = new Vector2(0.55f, 0.05f); dcRt.anchorMax = new Vector2(0.9f, 0.2f);
            dcRt.offsetMin = dcRt.offsetMax = Vector2.zero;
            dCloseBtnGo.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 1f);
            var closeDText = CreateText(dCloseBtnGo.transform, "Text", 28, Vector2.zero, Vector2.one, Color.white);
            closeDText.GetComponent<TextMeshProUGUI>().text = "FECHAR";

            detailModalGO.SetActive(false);

            // --- VINCULAR ---
            var victoryUI = Object.FindFirstObjectByType<VictoryRewardUI>();
            if (victoryUI == null)
            {
                Debug.LogWarning("[UI Builder] VictoryRewardUI não encontrado na cena! Criando um novo...");
                GameObject uiObj = new GameObject("VictoryRewardUI");
                uiObj.transform.SetParent(canvas.transform, false);
                victoryUI = uiObj.AddComponent<VictoryRewardUI>();
            }

            SerializedObject so = new SerializedObject(victoryUI);
            so.Update();
            so.FindProperty("rootContainer").objectReferenceValue = modalRoot;
            so.FindProperty("moneyAndEnergyText").objectReferenceValue = resText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("itemsContent").objectReferenceValue = lContentRT;
            so.FindProperty("continueButton").objectReferenceValue = closeBtnGo.GetComponent<Button>();
            
            // Vincular Modal de Detalhes
            so.FindProperty("detailsModal").objectReferenceValue = detailModalGO;
            so.FindProperty("modalTitle").objectReferenceValue = dTitle.GetComponent<TextMeshProUGUI>();
            so.FindProperty("modalDesc").objectReferenceValue = dDesc.GetComponent<TextMeshProUGUI>();
            so.FindProperty("modalSellBtn").objectReferenceValue = sellBtnGo.GetComponent<Button>();
            so.FindProperty("modalSellTxt").objectReferenceValue = sellText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("modalCloseBtn").objectReferenceValue = dCloseBtnGo.GetComponent<Button>();

            // --- AUTO-LINK CATALOGS AND CONFIGS ---
            LinkAssetIfNull(so, "artifactSetCatalog", "t:ArtifactSetCatalog");
            LinkAssetIfNull(so, "petCatalog", "t:PetCatalog");
            LinkAssetIfNull(so, "unitCatalog", "t:UnitCatalog");
            LinkAssetIfNull(so, "levelingConfig", "t:LevelingConfig");
            
            // Linkar o prefab de item de loot
            LinkAssetIfNull(so, "artifactItemPrefab", "ArtifactPet_Button_Template t:Prefab");

            so.ApplyModifiedProperties();
            Debug.Log("[UI Builder] Modal gerado e associado ao VictoryRewardUI!");

            modalRoot.SetActive(false);
            Undo.RegisterCreatedObjectUndo(modalRoot, "Create Scrollable Victory Modal");
            Selection.activeGameObject = modalRoot;
        }

        private static GameObject CreateText(Transform parent, string name, int fontSize, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject txtGo = new GameObject(name, typeof(RectTransform));
            txtGo.transform.SetParent(parent, false);
            RectTransform rt = txtGo.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var txt = txtGo.AddComponent<TextMeshProUGUI>();
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = TextAlignmentOptions.Center;
            txt.enableWordWrapping = true;

            return txtGo;
        }

        private static void LinkAssetIfNull(SerializedObject so, string propertyName, string searchFilter)
        {
            var prop = so.FindProperty(propertyName);
            if (prop != null && prop.objectReferenceValue == null)
            {
                string[] guids = AssetDatabase.FindAssets(searchFilter);
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    prop.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Object>(path);
                    Debug.Log($"[UI Builder] Vinculado automaticamente: {propertyName} -> {path}");
                }
            }
        }
    }
}