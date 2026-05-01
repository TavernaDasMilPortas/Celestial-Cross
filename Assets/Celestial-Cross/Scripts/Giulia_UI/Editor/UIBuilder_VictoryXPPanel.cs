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

            // 1. Container para o Painel de XP
            GameObject panelGO = new GameObject("XP_Panel", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            panelGO.transform.SetParent(victoryUI.transform.Find("RootContainer") ?? victoryUI.transform, false);
            
            RectTransform panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.1f, 0.2f);
            panelRT.anchorMax = new Vector2(0.9f, 0.45f);
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

            Image panelImg = panelGO.GetComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.5f);

            HorizontalLayoutGroup hlg = panelGO.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(20, 20, 10, 10);

            // 2. Prefab do Slot (Vamos criar um GameObject temporário para servir de base se não houver prefab)
            GameObject slotTemplate = CreateSlotTemplate(panelGO.transform);
            
            // Linkar no VictoryUI
            var serializedUI = new SerializedObject(victoryUI);
            serializedUI.FindProperty("xpSlotsPanel").objectReferenceValue = panelRT;
            serializedUI.FindProperty("xpSlotPrefab").objectReferenceValue = slotTemplate;
            serializedUI.ApplyModifiedProperties();

            // Esconder o template (o VictoryUI vai instanciar em runtime)
            slotTemplate.SetActive(false);

            Debug.Log("Victory XP Panel gerado e vinculado com sucesso!");
        }

        private static GameObject CreateSlotTemplate(Transform parent)
        {
            GameObject slot = new GameObject("XP_Slot_Template", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            slot.transform.SetParent(parent, false);
            
            Image bg = slot.GetComponent<Image>();
            bg.color = new Color(1, 1, 1, 0.1f);

            VerticalLayoutGroup vlg = slot.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.padding = new RectOffset(10, 10, 10, 10);

            // Ícone
            GameObject iconGO = new GameObject("UnitIcon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(slot.transform, false);
            iconGO.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
            iconGO.GetComponent<Image>().preserveAspect = true;

            // Nome/Nível
            GameObject levelGO = new GameObject("LevelText", typeof(RectTransform), typeof(TextMeshProUGUI));
            levelGO.transform.SetParent(slot.transform, false);
            var levelTxt = levelGO.GetComponent<TextMeshProUGUI>();
            levelTxt.fontSize = 18;
            levelTxt.alignment = TextAlignmentOptions.Center;
            levelTxt.text = "Lv. 1";

            // Barra de XP
            GameObject barGO = new GameObject("XP_Bar_BG", typeof(RectTransform), typeof(Image));
            barGO.transform.SetParent(slot.transform, false);
            barGO.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 15);
            barGO.GetComponent<Image>().color = Color.black;

            GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(barGO.transform, false);
            RectTransform fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.5f, 1); // 50% inicial
            fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
            fillGO.GetComponent<Image>().color = Color.cyan;

            // Texto de Ganho (+XP)
            GameObject gainGO = new GameObject("GainText", typeof(RectTransform), typeof(TextMeshProUGUI));
            gainGO.transform.SetParent(slot.transform, false);
            var gainTxt = gainGO.GetComponent<TextMeshProUGUI>();
            gainTxt.fontSize = 14;
            gainTxt.color = Color.yellow;
            gainTxt.alignment = TextAlignmentOptions.Center;
            gainTxt.text = "+0 XP";

            return slot;
        }
    }
}
