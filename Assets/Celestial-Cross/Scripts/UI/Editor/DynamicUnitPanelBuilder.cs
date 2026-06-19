#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.UI.Editor
{
    public class DynamicUnitPanelBuilder
    {
        [MenuItem("Celestial Cross/UI/Generate Split Screen UI")]
        public static void GenerateSplitScreenUI()
        {
            // 1. Create Root Object
            GameObject rootGo = new GameObject("SplitScreenUI", typeof(RectTransform));
            
            Canvas parentCanvas = null;
            if (Selection.activeTransform != null)
                parentCanvas = Selection.activeTransform.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
                parentCanvas = Object.FindObjectOfType<Canvas>();
            
            if (parentCanvas != null)
            {
                rootGo.transform.SetParent(parentCanvas.transform, false);
            }

            RectTransform rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;
            rootRect.anchoredPosition = Vector2.zero;

            SplitScreenUIManager manager = rootGo.AddComponent<SplitScreenUIManager>();

            // 2. Create Backgrounds
            GameObject leftBg = CreateImageObject("LeftBackground", rootGo.transform, new Color(0, 0.5f, 1f, 0.8f));
            RectTransform leftRect = leftBg.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0, 1);
            leftRect.anchorMax = new Vector2(0.5f, 1);
            leftRect.pivot = new Vector2(0.5f, 1);
            leftRect.sizeDelta = new Vector2(0, 150);
            leftRect.anchoredPosition = Vector2.zero;

            GameObject rightBg = CreateImageObject("RightBackground", rootGo.transform, new Color(1f, 0, 0.5f, 0.8f));
            RectTransform rightRect = rightBg.GetComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.5f, 1);
            rightRect.anchorMax = new Vector2(1f, 1f);
            rightRect.pivot = new Vector2(0.5f, 1);
            rightRect.sizeDelta = new Vector2(0, 150);
            rightRect.anchoredPosition = Vector2.zero;

            // 4. Create Modals
            UnitModalUI leftModal = CreateUnitModal("LeftModal", rootGo.transform);
            RectTransform leftModalRect = leftModal.GetComponent<RectTransform>();
            leftModalRect.anchorMin = new Vector2(0, 1);
            leftModalRect.anchorMax = new Vector2(0.5f, 1);
            leftModalRect.pivot = new Vector2(0.5f, 1);
            leftModalRect.sizeDelta = new Vector2(0, 150);
            leftModalRect.anchoredPosition = new Vector2(0, 0); // Center in left half
            
            UnitModalUI rightModal = CreateUnitModal("RightModal", rootGo.transform);
            RectTransform rightModalRect = rightModal.GetComponent<RectTransform>();
            rightModalRect.anchorMin = new Vector2(0.5f, 1);
            rightModalRect.anchorMax = new Vector2(1f, 1f);
            rightModalRect.pivot = new Vector2(0.5f, 1);
            rightModalRect.sizeDelta = new Vector2(0, 150);
            rightModalRect.anchoredPosition = new Vector2(0, 0); // Center in right half

            // 5. Create Target Buttons Container
            GameObject buttonsContainer = new GameObject("TargetButtonsContainer");
            buttonsContainer.transform.SetParent(rootGo.transform, false);
            HorizontalLayoutGroup hg = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            hg.childAlignment = TextAnchor.MiddleCenter;
            hg.spacing = 20;
            RectTransform btnRect = buttonsContainer.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0);
            btnRect.anchorMax = new Vector2(0.5f, 0);
            btnRect.anchoredPosition = new Vector2(0, 100);
            btnRect.sizeDelta = new Vector2(400, 80);

            // 6. Create Button Prefab Template (will be hidden)
            GameObject buttonPrefab = CreateTargetButtonPrefab(rootGo.transform);
            buttonPrefab.SetActive(false); // Hide the template

            // 7. Create Intro Modal outside SplitScreenUI (direct to Canvas)
            if (parentCanvas != null)
            {
                CreateIntroModal(parentCanvas.transform);
            }
            else
            {
                CreateIntroModal(rootGo.transform.parent); // Fallback
            }

            // 8. Wire Manager
            manager.leftBackground = leftRect;
            manager.rightBackground = rightRect;
            manager.leftModal = leftModal;
            manager.rightModal = rightModal;
            manager.targetButtonsContainer = buttonsContainer.transform;
            manager.targetButtonPrefab = buttonPrefab.GetComponent<TargetButtonUI>();

            Selection.activeGameObject = rootGo;
            Debug.Log("SplitScreenUI successfully generated!");
        }

        private static IntroModalUI CreateIntroModal(Transform parent)
        {
            GameObject introGo = new GameObject("IntroModal", typeof(RectTransform));
            introGo.transform.SetParent(parent, false);
            
            RectTransform introRect = introGo.GetComponent<RectTransform>();
            introRect.anchorMin = new Vector2(0, 1);
            introRect.anchorMax = new Vector2(1, 1);
            introRect.pivot = new Vector2(0.5f, 1);
            introRect.sizeDelta = new Vector2(0, 200);
            introRect.anchoredPosition = new Vector2(0, 300); // Offscreen top
            
            Image bg = introGo.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);

            GameObject chapterTextGo = new GameObject("ChapterText");
            chapterTextGo.transform.SetParent(introGo.transform, false);
            TextMeshProUGUI chapterText = chapterTextGo.AddComponent<TextMeshProUGUI>();
            chapterText.text = "Chapter 1";
            chapterText.fontSize = 40;
            chapterText.alignment = TextAlignmentOptions.Center;
            chapterText.color = Color.white;
            RectTransform chapRect = chapterText.GetComponent<RectTransform>();
            chapRect.anchorMin = Vector2.zero;
            chapRect.anchorMax = Vector2.one;
            chapRect.sizeDelta = Vector2.zero;

            GameObject stageTextGo = new GameObject("StageText");
            stageTextGo.transform.SetParent(introGo.transform, false);
            TextMeshProUGUI stageText = stageTextGo.AddComponent<TextMeshProUGUI>();
            stageText.text = "Stage 1";
            stageText.fontSize = 60;
            stageText.alignment = TextAlignmentOptions.Center;
            stageText.color = Color.red;
            RectTransform stageRect = stageText.GetComponent<RectTransform>();
            stageRect.anchorMin = Vector2.zero;
            stageRect.anchorMax = Vector2.one;
            stageRect.sizeDelta = Vector2.zero;
            
            IntroModalUI introModal = introGo.AddComponent<IntroModalUI>();
            introModal.panelTransform = introRect;
            introModal.chapterText = chapterText;
            introModal.stageText = stageText;
            
            return introModal;
        }

        private static GameObject CreateImageObject(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private static UnitModalUI CreateUnitModal(string name, Transform parent)
        {
            GameObject modalGo = new GameObject(name, typeof(RectTransform));
            modalGo.transform.SetParent(parent, false);
            UnitModalUI modalUI = modalGo.AddComponent<UnitModalUI>();

            // Setup Name Container
            GameObject nameOuter = CreateImageObject("NameOuterBorder", modalGo.transform, Color.gray);
            RectTransform nameOuterRect = nameOuter.GetComponent<RectTransform>();
            nameOuterRect.anchoredPosition = new Vector2(0, 50);
            nameOuterRect.sizeDelta = new Vector2(300, 60);

            GameObject nameInner = CreateImageObject("NameInnerBorder", nameOuter.transform, Color.white);
            RectTransform nameInnerRect = nameInner.GetComponent<RectTransform>();
            nameInnerRect.anchorMin = Vector2.zero;
            nameInnerRect.anchorMax = Vector2.one;
            nameInnerRect.sizeDelta = new Vector2(-10, -10);

            GameObject nameTextGo = new GameObject("NameText");
            nameTextGo.transform.SetParent(nameInner.transform, false);
            TextMeshProUGUI nameText = nameTextGo.AddComponent<TextMeshProUGUI>();
            nameText.color = Color.black;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.text = "Unit Name";
            nameText.fontSize = 24;
            RectTransform nameTextRect = nameText.GetComponent<RectTransform>();
            nameTextRect.anchorMin = Vector2.zero;
            nameTextRect.anchorMax = Vector2.one;
            nameTextRect.sizeDelta = Vector2.zero;

            // Setup HP Container
            GameObject hpOuter = CreateImageObject("HpOuterBorder", modalGo.transform, Color.white);
            RectTransform hpOuterRect = hpOuter.GetComponent<RectTransform>();
            hpOuterRect.anchoredPosition = new Vector2(0, -30);
            hpOuterRect.sizeDelta = new Vector2(300, 40);

            GameObject hpInner = CreateImageObject("HpInnerBorder", hpOuter.transform, Color.gray);
            RectTransform hpInnerRect = hpInner.GetComponent<RectTransform>();
            hpInnerRect.anchorMin = Vector2.zero;
            hpInnerRect.anchorMax = Vector2.one;
            hpInnerRect.sizeDelta = new Vector2(-6, -6);

            GameObject hpFill = CreateImageObject("HpFill", hpInner.transform, Color.red);
            Image fillImg = hpFill.GetComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1f;
            RectTransform fillRect = hpFill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            GameObject hpTextGo = new GameObject("HpValueText");
            hpTextGo.transform.SetParent(modalGo.transform, false);
            TextMeshProUGUI hpText = hpTextGo.AddComponent<TextMeshProUGUI>();
            hpText.color = Color.white;
            hpText.alignment = TextAlignmentOptions.Center;
            hpText.text = "100 / 100";
            hpText.fontSize = 20;
            RectTransform hpTextRect = hpText.GetComponent<RectTransform>();
            hpTextRect.anchoredPosition = new Vector2(0, -70);
            hpTextRect.sizeDelta = new Vector2(300, 30);

            // Setup Pet Icon
            GameObject petIcon = CreateImageObject("PetIcon", modalGo.transform, Color.white);
            RectTransform petIconRect = petIcon.GetComponent<RectTransform>();
            petIconRect.anchoredPosition = new Vector2(-150, 0);
            petIconRect.sizeDelta = new Vector2(80, 80);

            // Wire Modal
            modalUI.nameOuterBorder = nameOuter.GetComponent<Image>();
            modalUI.nameInnerBorder = nameInner.GetComponent<Image>();
            modalUI.nameText = nameText;
            modalUI.hpOuterBorder = hpOuter.GetComponent<Image>();
            modalUI.hpInnerBorder = hpInner.GetComponent<Image>();
            modalUI.hpFillImage = fillImg;
            modalUI.hpValueText = hpText;
            modalUI.petIconImage = petIcon.GetComponent<Image>();

            return modalUI;
        }

        private static GameObject CreateTargetButtonPrefab(Transform parent)
        {
            GameObject btnGo = new GameObject("TargetButtonTemplate");
            btnGo.transform.SetParent(parent, false);
            
            Image bg = btnGo.AddComponent<Image>();
            bg.color = Color.white;

            RectTransform rect = btnGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(60, 60);

            GameObject textGo = new GameObject("NumberText");
            textGo.transform.SetParent(btnGo.transform, false);
            TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
            text.text = "1";
            text.fontSize = 32;
            text.fontStyle = FontStyles.Bold;
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TargetButtonUI tb = btnGo.AddComponent<TargetButtonUI>();
            tb.backgroundImage = bg;
            tb.numberText = text;
            tb.defaultColor = Color.white;
            tb.selectedColor = Color.yellow;

            return btnGo;
        }
    }
}
#endif
