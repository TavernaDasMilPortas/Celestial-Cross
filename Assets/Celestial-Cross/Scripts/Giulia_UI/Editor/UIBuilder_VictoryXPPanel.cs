using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Giulia_UI;

namespace CelestialCross.EditorArea
{
    public static class UIBuilder_VictoryXPPanel
    {
        [MenuItem("Celestial Cross/UI Builders/Generate Victory XP Panel")]
        public static void GenerateXPPanel()
        {
            VictoryRewardUI victoryUI = Object.FindObjectOfType<VictoryRewardUI>();
            if (victoryUI == null)
            {
                Debug.LogError("VictoryRewardUI não encontrado na cena!");
                return;
            }

            Undo.RecordObject(victoryUI, "Generate XP Panel");
            GenerateXPPanelInTarget(victoryUI.transform.Find("RootContainer") ?? victoryUI.transform);
        }

        public static void GenerateXPPanelInTarget(Transform targetParent)
        {
            VictoryRewardUI victoryUI = Object.FindObjectOfType<VictoryRewardUI>();
            
            // 1. Container para o Painel de XP
            GameObject panelGO = new GameObject("XP_Panel", typeof(RectTransform), typeof(VerticalLayoutGroup));
            panelGO.transform.SetParent(targetParent, false);
            
            RectTransform panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0, 0);
            panelRT.anchorMax = new Vector2(1, 0);
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = panelGO.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 2. Prefab do Slot
            GameObject slotTemplate = CreateSlotTemplate(panelGO.transform);
            
            if (victoryUI != null)
            {
                var serializedUI = new SerializedObject(victoryUI);
                serializedUI.FindProperty("xpSlotsPanel").objectReferenceValue = panelRT;
                serializedUI.FindProperty("xpSlotPrefab").objectReferenceValue = slotTemplate;
                serializedUI.ApplyModifiedProperties();
            }

            slotTemplate.SetActive(false);
            Debug.Log("Victory XP Panel gerado e vinculado!");
        }

        private static GameObject CreateSlotTemplate(Transform parent)
        {
            GameObject slot = new GameObject("XP_Slot_Template", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            slot.transform.SetParent(parent, false);
            
            HorizontalLayoutGroup hlg = slot.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 30;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = true;
            hlg.padding = new RectOffset(20, 20, 10, 10);

            // 1. Coluna 1: Ícone
            GameObject iconGO = new GameObject("UnitIcon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(slot.transform, false);
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(120, 120);
            iconGO.GetComponent<Image>().preserveAspect = true;

            // 2. Coluna 2: Informações (Vertical)
            GameObject infoGO = new GameObject("InfoColumn", typeof(RectTransform), typeof(VerticalLayoutGroup));
            infoGO.transform.SetParent(slot.transform, false);
            var vlg = infoGO.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;

            // Nível + XP (Top Row)
            GameObject levelGO = new GameObject("LevelText", typeof(RectTransform), typeof(TextMeshProUGUI));
            levelGO.transform.SetParent(infoGO.transform, false);
            var levelTxt = levelGO.GetComponent<TextMeshProUGUI>();
            levelTxt.fontSize = 36;
            levelTxt.text = "Lv. 1 (0/100) +10 XP";
            levelTxt.alignment = TextAlignmentOptions.Left;

            // Barra de XP (Bottom Row)
            GameObject barGO = new GameObject("XP_Bar_BG", typeof(RectTransform), typeof(Image));
            barGO.transform.SetParent(infoGO.transform, false);
            barGO.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 24);
            barGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);

            GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(barGO.transform, false);
            RectTransform fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.5f, 1);
            fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
            fillGO.GetComponent<Image>().color = new Color(0, 0.8f, 1f, 1f);

            // GainText (Oculto ou usado dentro do LevelText)
            GameObject gainGO = new GameObject("GainText", typeof(RectTransform), typeof(TextMeshProUGUI));
            gainGO.transform.SetParent(slot.transform, false);
            gainGO.SetActive(false); // Não usamos mais como objeto separado, mas mantemos a ref pro script se precisar

            return slot;
        }
    }
}
