using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.UI.Skills;

namespace CelestialCross.EditorArea
{
    public class CombatUISetupUtility : EditorWindow
    {
        [MenuItem("Celestial Cross/UI Builders/Skills/Setup Combat Passives UI")]
        public static void SetupPassivesUI()
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in scene.");
                return;
            }

            // Create PassiveListModal
            var modalGo = new GameObject("PassiveListModal", typeof(RectTransform), typeof(PassiveListModal));
            modalGo.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)modalGo.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(modalGo.transform, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            var passivesContainer = CreateContainer(modalGo.transform, "PassivesContainer", new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.9f));
            var conditionsContainer = CreateContainer(modalGo.transform, "ConditionsContainer", new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.55f));
            var buffsContainer = CreateContainer(modalGo.transform, "BuffsContainer", new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.3f));

            var itemPrefabGo = new GameObject("ListItemPrefab", typeof(RectTransform), typeof(Image));
            itemPrefabGo.transform.SetParent(modalGo.transform, false);
            itemPrefabGo.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.4f, 1f);
            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(itemPrefabGo.transform, false);
            textGo.GetComponent<TextMeshProUGUI>().enableWordWrapping = true;
            textGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            itemPrefabGo.SetActive(false); // Hide prefab

            var closeBtnGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnGo.transform.SetParent(modalGo.transform, false);
            closeBtnGo.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
            var cText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            cText.transform.SetParent(closeBtnGo.transform, false);
            cText.GetComponent<TextMeshProUGUI>().text = "Fechar";
            var cBtnRt = (RectTransform)closeBtnGo.transform;
            cBtnRt.anchorMin = new Vector2(0.8f, 0.9f);
            cBtnRt.anchorMax = new Vector2(0.95f, 0.98f);
            cBtnRt.offsetMin = cBtnRt.offsetMax = Vector2.zero;

            var comp = modalGo.GetComponent<PassiveListModal>();
            comp.modalRoot = modalGo;
            comp.passivesContainer = passivesContainer;
            comp.conditionsContainer = conditionsContainer;
            comp.buffsContainer = buffsContainer;
            comp.listItemPrefab = itemPrefabGo;
            comp.closeButton = closeBtnGo.GetComponent<Button>();

            modalGo.SetActive(false);

            // Need to link this to ActionBarUI if it exists
            var actionBar = Object.FindObjectOfType<ActionBarUI>();
            if (actionBar != null)
            {
                var passivesBtnGo = new GameObject("Btn_Passives", typeof(RectTransform), typeof(Image), typeof(Button));
                passivesBtnGo.transform.SetParent(actionBar.transform, false);
                passivesBtnGo.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.8f, 1f);
                var pText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                pText.transform.SetParent(passivesBtnGo.transform, false);
                pText.GetComponent<TextMeshProUGUI>().text = "Passivas";
                
                var passivesBtn = passivesBtnGo.GetComponent<Button>();
                
                actionBar.passivesButton = passivesBtn;
                actionBar.passiveListModal = comp;
            }

            EditorUtility.SetDirty(modalGo);
            if (actionBar != null) EditorUtility.SetDirty(actionBar);
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("Combat Passives UI Setup Complete!");
        }

        private static RectTransform CreateContainer(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 8f;

            return rt;
        }
    }
}
