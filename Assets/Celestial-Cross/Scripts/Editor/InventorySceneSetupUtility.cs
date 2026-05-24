using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.UI.Skills;

namespace CelestialCross.EditorArea
{
    public class InventorySceneSetupUtility : EditorWindow
    {
        [MenuItem("Celestial Cross/UI Builders/Skills/Setup Inventory Skill UI")]
        public static void SetupSkillUI()
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in scene.");
                return;
            }

            // Create or find SkillTabUI
            var skillTabGo = new GameObject("SkillTabUI", typeof(RectTransform), typeof(SkillTabUI));
            skillTabGo.transform.SetParent(canvas.transform, false);
            var skillTab = skillTabGo.GetComponent<SkillTabUI>();
            var skillTabRt = (RectTransform)skillTabGo.transform;
            skillTabRt.anchorMin = new Vector2(0, 0);
            skillTabRt.anchorMax = new Vector2(1, 0.45f);
            skillTabRt.offsetMin = Vector2.zero;
            skillTabRt.offsetMax = Vector2.zero;
            
            // Layout Group
            var layout = skillTabGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 16f;

            // Slots Container
            var slotsGo = new GameObject("SlotsContainer", typeof(RectTransform), typeof(GridLayoutGroup));
            slotsGo.transform.SetParent(skillTabGo.transform, false);
            var grid = slotsGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(200, 80);
            grid.spacing = new Vector2(16, 16);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            skillTab.basicSkillButton = CreateSlotButton(slotsGo.transform, "BasicSlot", out skillTab.basicSkillText);
            skillTab.movementSkillButton = CreateSlotButton(slotsGo.transform, "MovementSlot", out skillTab.movementSkillText);
            skillTab.slot1SkillButton = CreateSlotButton(slotsGo.transform, "Slot1", out skillTab.slot1SkillText);
            skillTab.slot2SkillButton = CreateSlotButton(slotsGo.transform, "Slot2", out skillTab.slot2SkillText);

            // Create Modals
            skillTab.selectionModal = CreateSelectionModal(canvas.transform);
            skillTab.branchModal = CreateBranchModal(canvas.transform);

            EditorUtility.SetDirty(skillTab);
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("Inventory Skill UI Setup Complete!");
        }

        private static Button CreateSlotButton(Transform parent, string name, out TextMeshProUGUI textComp)
        {
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

            return go.GetComponent<Button>();
        }

        private static SkillSelectionModal CreateSelectionModal(Transform parent)
        {
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
            bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            var optionsGo = new GameObject("OptionsContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            optionsGo.transform.SetParent(modalGo.transform, false);
            var optRt = (RectTransform)optionsGo.transform;
            optRt.anchorMin = new Vector2(0.1f, 0.1f);
            optRt.anchorMax = new Vector2(0.9f, 0.9f);
            optRt.offsetMin = optRt.offsetMax = Vector2.zero;

            var optionPrefab = CreateSlotButton(modalGo.transform, "OptionPrefab", out _).gameObject;
            optionPrefab.SetActive(false); // hide prefab

            var closeBtn = CreateSlotButton(modalGo.transform, "CloseButton", out _);
            closeBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Fechar";
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

        private static SkillBranchModal CreateBranchModal(Transform parent)
        {
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
            bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            var tiersGo = new GameObject("TiersContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            tiersGo.transform.SetParent(modalGo.transform, false);
            var optRt = (RectTransform)tiersGo.transform;
            optRt.anchorMin = new Vector2(0.1f, 0.1f);
            optRt.anchorMax = new Vector2(0.9f, 0.9f);
            optRt.offsetMin = optRt.offsetMax = Vector2.zero;

            // Tier Prefab
            var tierPrefab = new GameObject("TierPrefab", typeof(RectTransform), typeof(VerticalLayoutGroup));
            tierPrefab.transform.SetParent(modalGo.transform, false);
            var tierTitle = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            tierTitle.transform.SetParent(tierPrefab.transform, false);
            tierTitle.GetComponent<TextMeshProUGUI>().text = "Tier";
            
            var tierOptions = new GameObject("OptionsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tierOptions.transform.SetParent(tierPrefab.transform, false);
            tierPrefab.SetActive(false); // hide prefab

            var optionPrefab = CreateSlotButton(modalGo.transform, "OptionPrefab", out _).gameObject;
            optionPrefab.SetActive(false); // hide prefab

            var closeBtn = CreateSlotButton(modalGo.transform, "CloseButton", out _);
            closeBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Fechar";
            ((RectTransform)closeBtn.transform).anchorMin = new Vector2(0.8f, 0.9f);
            ((RectTransform)closeBtn.transform).anchorMax = new Vector2(0.95f, 0.98f);
            ((RectTransform)closeBtn.transform).offsetMin = ((RectTransform)closeBtn.transform).offsetMax = Vector2.zero;

            var comp = modalGo.GetComponent<SkillBranchModal>();
            comp.modalRoot = modalGo;
            comp.tiersContainer = optRt;
            comp.tierPrefab = tierPrefab;
            comp.optionPrefab = optionPrefab;
            comp.closeButton = closeBtn;

            modalGo.SetActive(false);
            return comp;
        }
    }
}
