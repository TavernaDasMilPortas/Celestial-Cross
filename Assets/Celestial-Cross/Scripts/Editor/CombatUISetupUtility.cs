using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.UI.Skills;

namespace CelestialCross.EditorArea
{
    public class CombatUISetupUtility : EditorWindow
    {
        [MenuItem("Celestial Cross/3. UI Builders/5. Skills/Setup Combat Passives UI")]
        public static void SetupPassivesUI()
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in scene.");
                return;
            }

            // Remover modal antigo se existir
            var oldModal = canvas.transform.Find("PassiveListModal");
            if (oldModal != null)
            {
                Object.DestroyImmediate(oldModal.gameObject);
            }

            // Create PassiveListModal
            var modalGo = new GameObject("PassiveListModal", typeof(RectTransform), typeof(PassiveListModal));
            modalGo.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)modalGo.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            // Fundo escuro Translúcido
            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(modalGo.transform, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);

            // Painel Principal Centralizado
            var mainPanel = new GameObject("MainPanel", typeof(RectTransform), typeof(Image));
            mainPanel.transform.SetParent(modalGo.transform, false);
            var mpRt = (RectTransform)mainPanel.transform;
            mpRt.anchorMin = new Vector2(0.1f, 0.1f);
            mpRt.anchorMax = new Vector2(0.9f, 0.9f);
            mpRt.offsetMin = mpRt.offsetMax = Vector2.zero;
            mainPanel.GetComponent<Image>().color = new Color(0.12f, 0.10f, 0.15f, 0.95f); // Violeta escuro harmônico
            var outline = mainPanel.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0.35f, 0.3f, 0.5f, 0.6f);
            outline.effectDistance = new Vector2(2f, 2f);

            // Título Principal
            var mainTitle = new GameObject("MainTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            mainTitle.transform.SetParent(mpRt, false);
            var mtRt = (RectTransform)mainTitle.transform;
            mtRt.anchorMin = new Vector2(0f, 0.92f);
            mtRt.anchorMax = new Vector2(1f, 0.98f);
            mtRt.offsetMin = mtRt.offsetMax = Vector2.zero;
            var mtText = mainTitle.GetComponent<TextMeshProUGUI>();
            mtText.text = "<b>STATUS DE COMBATE</b>";
            mtText.fontSize = 28;
            mtText.color = Color.white;
            mtText.alignment = TextAlignmentOptions.Center;

            // --- SEÇÃO 1: Efeitos Temporários (Topo, Largura Completa, Sem Imagem de Fundo) ---
            var sec1Root = new GameObject("Section1_TemporaryConditions", typeof(RectTransform));
            sec1Root.transform.SetParent(mpRt, false);
            var s1Rt = (RectTransform)sec1Root.transform;
            s1Rt.anchorMin = new Vector2(0.05f, 0.74f);
            s1Rt.anchorMax = new Vector2(0.95f, 0.90f);
            s1Rt.offsetMin = s1Rt.offsetMax = Vector2.zero;

            var s1Title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            s1Title.transform.SetParent(s1Rt, false);
            var s1tRt = (RectTransform)s1Title.transform;
            s1tRt.anchorMin = new Vector2(0f, 0.70f);
            s1tRt.anchorMax = new Vector2(1f, 0.95f);
            s1tRt.offsetMin = s1tRt.offsetMax = Vector2.zero;
            var s1tText = s1Title.GetComponent<TextMeshProUGUI>();
            s1tText.text = "<b>Efeitos Temporários (Buffs / Debuffs)</b>";
            s1tText.fontSize = 16;
            s1tText.color = new Color(0.85f, 0.85f, 0.95f, 1f);
            s1tText.alignment = TextAlignmentOptions.Left;

            var condGrid = new GameObject("ConditionsGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            condGrid.transform.SetParent(s1Rt, false);
            var cgRt = (RectTransform)condGrid.transform;
            cgRt.anchorMin = new Vector2(0f, 0.05f);
            cgRt.anchorMax = new Vector2(1f, 0.65f);
            cgRt.offsetMin = cgRt.offsetMax = Vector2.zero;
            var grid = condGrid.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(50f, 50f);
            grid.spacing = new Vector2(10f, 10f);
            grid.childAlignment = TextAnchor.MiddleLeft;

            // --- SEÇÃO 2: Modificadores de Atributos (Meio, Largura Completa com 2 Subcolunas, Sem Imagem de Fundo) ---
            var sec2Root = new GameObject("Section2_AttributeModifiers", typeof(RectTransform));
            sec2Root.transform.SetParent(mpRt, false);
            var s2Rt = (RectTransform)sec2Root.transform;
            s2Rt.anchorMin = new Vector2(0.05f, 0.40f);
            s2Rt.anchorMax = new Vector2(0.95f, 0.71f);
            s2Rt.offsetMin = s2Rt.offsetMax = Vector2.zero;

            // Área 2A: Positivos (Lado Esquerdo)
            var posRoot = new GameObject("PositiveModifiersArea", typeof(RectTransform));
            posRoot.transform.SetParent(s2Rt, false);
            var prRt = (RectTransform)posRoot.transform;
            prRt.anchorMin = new Vector2(0f, 0f);
            prRt.anchorMax = new Vector2(0.48f, 1f);
            prRt.offsetMin = prRt.offsetMax = Vector2.zero;

            var prTitle = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            prTitle.transform.SetParent(prRt, false);
            var prtRt = (RectTransform)prTitle.transform;
            prtRt.anchorMin = new Vector2(0f, 0.88f);
            prtRt.anchorMax = new Vector2(1f, 1f);
            prtRt.offsetMin = prtRt.offsetMax = Vector2.zero;
            var prtText = prTitle.GetComponent<TextMeshProUGUI>();
            prtText.text = "<b><color=#4f4>Bônus Ativos (Positivos)</color></b>";
            prtText.fontSize = 15;
            prtText.alignment = TextAlignmentOptions.Left;

            var posScroll = CreateScrollContainer(posRoot.transform, "PosScroll", out RectTransform posContainer);
            posScroll.anchorMin = new Vector2(0f, 0f);
            posScroll.anchorMax = new Vector2(1f, 0.86f);
            posScroll.offsetMin = posScroll.offsetMax = Vector2.zero;

            // Área 2B: Negativos (Lado Direito)
            var negRoot = new GameObject("NegativeModifiersArea", typeof(RectTransform));
            negRoot.transform.SetParent(s2Rt, false);
            var nrRt = (RectTransform)negRoot.transform;
            nrRt.anchorMin = new Vector2(0.52f, 0f);
            nrRt.anchorMax = new Vector2(1f, 1f);
            nrRt.offsetMin = nrRt.offsetMax = Vector2.zero;

            var nrTitle = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            nrTitle.transform.SetParent(nrRt, false);
            var nrtRt = (RectTransform)nrTitle.transform;
            nrtRt.anchorMin = new Vector2(0f, 0.88f);
            nrtRt.anchorMax = new Vector2(1f, 1f);
            nrtRt.offsetMin = nrtRt.offsetMax = Vector2.zero;
            var nrtText = nrTitle.GetComponent<TextMeshProUGUI>();
            nrtText.text = "<b><color=#f44>Penalidades Ativas (Negativos)</color></b>";
            nrtText.fontSize = 15;
            nrtText.alignment = TextAlignmentOptions.Left;

            var negScroll = CreateScrollContainer(negRoot.transform, "NegScroll", out RectTransform negContainer);
            negScroll.anchorMin = new Vector2(0f, 0f);
            negScroll.anchorMax = new Vector2(1f, 0.86f);
            negScroll.offsetMin = negScroll.offsetMax = Vector2.zero;

            // --- SEÇÃO 3: Passivas Nativas (Base, Largura Completa, Sem Imagem de Fundo) ---
            var sec3Root = new GameObject("Section3_CharacterPassives", typeof(RectTransform));
            sec3Root.transform.SetParent(mpRt, false);
            var s3Rt = (RectTransform)sec3Root.transform;
            s3Rt.anchorMin = new Vector2(0.05f, 0.05f);
            s3Rt.anchorMax = new Vector2(0.95f, 0.37f);
            s3Rt.offsetMin = s3Rt.offsetMax = Vector2.zero;

            var s3Title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            s3Title.transform.SetParent(s3Rt, false);
            var s3tRt = (RectTransform)s3Title.transform;
            s3tRt.anchorMin = new Vector2(0f, 0.88f);
            s3tRt.anchorMax = new Vector2(1f, 1f);
            s3tRt.offsetMin = s3tRt.offsetMax = Vector2.zero;
            var s3tText = s3Title.GetComponent<TextMeshProUGUI>();
            s3tText.text = "<b>Habilidades Passivas do Personagem</b>";
            s3tText.fontSize = 15;
            s3tText.color = new Color(0.85f, 0.85f, 0.95f, 1f);
            s3tText.alignment = TextAlignmentOptions.Left;

            var passScroll = CreateScrollContainer(sec3Root.transform, "PassivesScroll", out RectTransform passContainer);
            passScroll.anchorMin = new Vector2(0f, 0f);
            passScroll.anchorMax = new Vector2(1f, 0.86f);
            passScroll.offsetMin = passScroll.offsetMax = Vector2.zero;

            // Botão Fechar Modal Principal
            var closeBtnGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnGo.transform.SetParent(mpRt, false);
            closeBtnGo.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
            var cText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            cText.transform.SetParent(closeBtnGo.transform, false);
            var cTextRt = (RectTransform)cText.transform;
            cTextRt.anchorMin = Vector2.zero; cTextRt.anchorMax = Vector2.one;
            cTextRt.offsetMin = cTextRt.offsetMax = Vector2.zero;
            var cTextTmp = cText.GetComponent<TextMeshProUGUI>();
            cTextTmp.text = "Fechar";
            cTextTmp.alignment = TextAlignmentOptions.Center;
            cTextTmp.fontSize = 16;
            cTextTmp.color = Color.white;
            var cBtnRt = (RectTransform)closeBtnGo.transform;
            cBtnRt.anchorMin = new Vector2(0.82f, 0.92f);
            cBtnRt.anchorMax = new Vector2(0.98f, 0.98f);
            cBtnRt.offsetMin = cBtnRt.offsetMax = Vector2.zero;

            // --- PREFABS (Ocultos no modalGo) ---
            var prefabsGroup = new GameObject("PrefabsGroup", typeof(RectTransform));
            prefabsGroup.transform.SetParent(modalGo.transform, false);
            prefabsGroup.SetActive(false);

            // 1. ConditionIconPrefab (Ícone de Buff/Debuff)
            var condIconPrefab = new GameObject("ConditionIconPrefab", typeof(RectTransform), typeof(Image), typeof(Button));
            condIconPrefab.transform.SetParent(prefabsGroup.transform, false);
            ((RectTransform)condIconPrefab.transform).sizeDelta = new Vector2(50f, 50f);
            condIconPrefab.GetComponent<Image>().color = Color.white;
            
            var turnTextGo = new GameObject("TurnText", typeof(RectTransform), typeof(TextMeshProUGUI));
            turnTextGo.transform.SetParent(condIconPrefab.transform, false);
            var ttRt = (RectTransform)turnTextGo.transform;
            ttRt.anchorMin = new Vector2(0.4f, 0f);
            ttRt.anchorMax = new Vector2(1f, 0.4f);
            ttRt.offsetMin = ttRt.offsetMax = Vector2.zero;
            var ttTmp = turnTextGo.GetComponent<TextMeshProUGUI>();
            ttTmp.text = "1t";
            ttTmp.fontSize = 11;
            ttTmp.color = Color.yellow;
            ttTmp.alignment = TextAlignmentOptions.BottomRight;
            var ttOutline = turnTextGo.AddComponent<UnityEngine.UI.Outline>();
            ttOutline.effectColor = Color.black;
            ttOutline.effectDistance = new Vector2(1f, 1f);

            // 2. ModifierItemPrefab (Modificadores com Ícones e Layout Horizontal)
            var modItemPrefab = new GameObject("ModifierItemPrefab", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            modItemPrefab.transform.SetParent(prefabsGroup.transform, false);
            var mipRt = (RectTransform)modItemPrefab.transform;
            mipRt.sizeDelta = new Vector2(600f, 35f);
            var hlgMod = modItemPrefab.GetComponent<HorizontalLayoutGroup>();
            hlgMod.spacing = 10f;
            hlgMod.childAlignment = TextAnchor.MiddleLeft;
            hlgMod.childControlWidth = false; hlgMod.childControlHeight = false;
            hlgMod.childForceExpandWidth = false; hlgMod.childForceExpandHeight = false;

            var modIcon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            modIcon.transform.SetParent(mipRt, false);
            var miRt = (RectTransform)modIcon.transform;
            miRt.sizeDelta = new Vector2(30f, 30f);
            var miImg = modIcon.GetComponent<Image>();
            miImg.preserveAspect = true;
            miImg.raycastTarget = false;

            var modText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            modText.transform.SetParent(mipRt, false);
            var mtTextRt = (RectTransform)modText.transform;
            mtTextRt.sizeDelta = new Vector2(550f, 30f);
            var mtTmp = modText.GetComponent<TextMeshProUGUI>();
            mtTmp.fontSize = 13;
            mtTmp.color = Color.white;
            mtTmp.enableWordWrapping = true;
            mtTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // 3. PassiveItemPrefab (Passivas com Ícones e Layout Horizontal)
            var passItemPrefab = new GameObject("PassiveItemPrefab", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            passItemPrefab.transform.SetParent(prefabsGroup.transform, false);
            var pipRt = (RectTransform)passItemPrefab.transform;
            pipRt.sizeDelta = new Vector2(600f, 55f);
            var hlgPass = passItemPrefab.GetComponent<HorizontalLayoutGroup>();
            hlgPass.spacing = 12f;
            hlgPass.childAlignment = TextAnchor.MiddleLeft;
            hlgPass.childControlWidth = false; hlgPass.childControlHeight = false;
            hlgPass.childForceExpandWidth = false; hlgPass.childForceExpandHeight = false;

            var passIcon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            passIcon.transform.SetParent(pipRt, false);
            var piRt = (RectTransform)passIcon.transform;
            piRt.sizeDelta = new Vector2(35f, 35f);
            var piImg = passIcon.GetComponent<Image>();
            piImg.preserveAspect = true;
            piImg.raycastTarget = false;

            var passText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            passText.transform.SetParent(pipRt, false);
            var ptTextRt = (RectTransform)passText.transform;
            ptTextRt.sizeDelta = new Vector2(540f, 50f);
            var piTmp = passText.GetComponent<TextMeshProUGUI>();
            piTmp.fontSize = 13;
            piTmp.color = Color.white;
            piTmp.enableWordWrapping = true;
            piTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // --- SUBMODAL DE DETALHES ---
            var detailModalGo = new GameObject("PassiveDetailModal", typeof(RectTransform), typeof(PassiveDetailModal));
            detailModalGo.transform.SetParent(modalGo.transform, false);
            var dmRt = (RectTransform)detailModalGo.transform;
            dmRt.anchorMin = Vector2.zero; dmRt.anchorMax = Vector2.one;
            dmRt.offsetMin = dmRt.offsetMax = Vector2.zero;

            var dmBg = new GameObject("BG", typeof(RectTransform), typeof(Image), typeof(Button));
            dmBg.transform.SetParent(detailModalGo.transform, false);
            var dmbgRt = (RectTransform)dmBg.transform;
            dmbgRt.anchorMin = Vector2.zero; dmbgRt.anchorMax = Vector2.one;
            dmbgRt.offsetMin = dmbgRt.offsetMax = Vector2.zero;
            dmBg.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);

            var detailPanel = new GameObject("DetailPanel", typeof(RectTransform), typeof(Image));
            detailPanel.transform.SetParent(detailModalGo.transform, false);
            var dpRt = (RectTransform)detailPanel.transform;
            dpRt.anchorMin = new Vector2(0.25f, 0.35f);
            dpRt.anchorMax = new Vector2(0.75f, 0.65f);
            dpRt.offsetMin = dpRt.offsetMax = Vector2.zero;
            detailPanel.GetComponent<Image>().color = new Color(0.18f, 0.16f, 0.22f, 0.98f);
            var dpOutline = detailPanel.AddComponent<UnityEngine.UI.Outline>();
            dpOutline.effectColor = new Color(0.4f, 0.3f, 0.5f, 0.8f);
            dpOutline.effectDistance = new Vector2(1.5f, 1.5f);

            var detIcon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            detIcon.transform.SetParent(dpRt, false);
            var diRt = (RectTransform)detIcon.transform;
            diRt.anchorMin = new Vector2(0.4f, 0.65f);
            diRt.anchorMax = new Vector2(0.6f, 0.92f);
            diRt.offsetMin = diRt.offsetMax = Vector2.zero;
            detIcon.GetComponent<Image>().preserveAspect = true;

            var detTitle = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            detTitle.transform.SetParent(dpRt, false);
            var dtTitleRt = (RectTransform)detTitle.transform;
            dtTitleRt.anchorMin = new Vector2(0.05f, 0.40f);
            dtTitleRt.anchorMax = new Vector2(0.95f, 0.60f);
            dtTitleRt.offsetMin = dtTitleRt.offsetMax = Vector2.zero;
            var dtTitleTmp = detTitle.GetComponent<TextMeshProUGUI>();
            dtTitleTmp.fontSize = 16;
            dtTitleTmp.color = Color.white;
            dtTitleTmp.alignment = TextAlignmentOptions.Center;

            var detDesc = new GameObject("DescText", typeof(RectTransform), typeof(TextMeshProUGUI));
            detDesc.transform.SetParent(dpRt, false);
            var dtDescRt = (RectTransform)detDesc.transform;
            dtDescRt.anchorMin = new Vector2(0.05f, 0.05f);
            dtDescRt.anchorMax = new Vector2(0.95f, 0.38f);
            dtDescRt.offsetMin = dtDescRt.offsetMax = Vector2.zero;
            var dtDescTmp = detDesc.GetComponent<TextMeshProUGUI>();
            dtDescTmp.fontSize = 13;
            dtDescTmp.color = new Color(0.85f, 0.85f, 0.9f, 1f);
            dtDescTmp.enableWordWrapping = true;
            dtDescTmp.alignment = TextAlignmentOptions.Center;

            var detailCloseBtn = dmBg.GetComponent<Button>(); // Clicar no BG fecha o submodal
            var detailComp = detailModalGo.GetComponent<PassiveDetailModal>();
            detailComp.modalRoot = detailModalGo;
            detailComp.iconImage = detIcon.GetComponent<Image>();
            detailComp.titleText = dtTitleTmp;
            detailComp.descText = dtDescTmp;
            detailComp.closeButton = detailCloseBtn;
            detailModalGo.SetActive(false);

            // --- LIGAR COMPONENTES NO MODAL PRINCIPAL ---
            var comp = modalGo.GetComponent<PassiveListModal>();
            comp.modalRoot = modalGo;
            comp.conditionsGrid = cgRt;
            comp.conditionIconPrefab = condIconPrefab;
            comp.detailModal = detailComp;
            comp.positiveModifiersContainer = posContainer;
            comp.negativeModifiersContainer = negContainer;
            comp.modifierItemPrefab = modItemPrefab;
            comp.allPassivesContainer = passContainer;
            comp.passiveItemPrefab = passItemPrefab;
            comp.closeButton = closeBtnGo.GetComponent<Button>();

            modalGo.SetActive(false);

            // --- FIAÇÃO COM ACTIONBARUI ---
            var actionBar = Object.FindObjectOfType<ActionBarUI>();
            if (actionBar != null)
            {
                actionBar.passiveListModal = comp;
                
                // Se já existir o botão de passivas, vinculamos o clique dele
                if (actionBar.passivesButton != null)
                {
                    actionBar.passivesButton.onClick.RemoveAllListeners();
                    actionBar.passivesButton.onClick.AddListener(() => {
                        var current = TurnManager.Instance != null ? TurnManager.Instance.CurrentUnit : null;
                        if (current != null) comp.Open(current);
                    });
                }
            }

            EditorUtility.SetDirty(modalGo);
            if (actionBar != null) EditorUtility.SetDirty(actionBar);
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("Combat Passives UI Setup Complete!");
        }

        private static RectTransform CreateScrollContainer(Transform parent, string name, out RectTransform content)
        {
            var scrollGo = new GameObject(name, typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
            scrollGo.transform.SetParent(parent, false);
            var scrollImage = scrollGo.GetComponent<Image>();
            scrollImage.color = new Color(1f, 1f, 1f, 0.01f);
            var mask = scrollGo.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var vRt = (RectTransform)viewportGo.transform;
            vRt.anchorMin = Vector2.zero; vRt.anchorMax = Vector2.one;
            vRt.offsetMin = vRt.offsetMax = Vector2.zero;
            viewportGo.GetComponent<Mask>().showMaskGraphic = false;
            viewportGo.GetComponent<Image>().color = new Color(1, 1, 1, 0.02f);

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewportGo.transform, false);
            content = (RectTransform)contentGo.transform;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = content.offsetMax = Vector2.zero;

            var layout = contentGo.GetComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 6f;
            layout.padding = new RectOffset(8, 8, 8, 8);

            var fitter = contentGo.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = vRt;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 20f;

            return (RectTransform)scrollGo.transform;
        }
    }
}
