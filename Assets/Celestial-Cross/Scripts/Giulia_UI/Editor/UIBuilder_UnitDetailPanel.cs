using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.EditorArea
{
    public static class UIBuilder_UnitDetailPanel
    {
        [MenuItem("Celestial Cross/UI Builders/Overhaul Unit Detail Panel (Level & Constellation)")]
        public static void OverhaulDetailPanel()
        {
            InventoryUI inventory = Object.FindObjectOfType<InventoryUI>();
            if (inventory == null)
            {
                Debug.LogError("InventoryUI não encontrado!");
                return;
            }

            Undo.RecordObject(inventory, "Overhaul Detail Panel");

            // Procurar o TopPanel de Unidades (Index 0)
            if (inventory.topPanels == null || inventory.topPanels.Length == 0 || inventory.topPanels[0] == null)
            {
                Debug.LogError("TopPanel_0 não encontrado no InventoryUI!");
                return;
            }

            Transform unitPanel = inventory.topPanels[0];
            Transform profileContainer = unitPanel.Find("ProfileContainer");
            if (profileContainer == null)
            {
                Debug.LogError("ProfileContainer não encontrado no TopPanel_0!");
                return;
            }

            // 1. Seção de Level e XP (Abaixo das habilidades ou ao lado dos stats)
            // Vamos criar um container horizontal para Level/XP
            GameObject levelSection = new GameObject("Section_LevelXP", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            levelSection.transform.SetParent(profileContainer, false);
            RectTransform levelRT = levelSection.GetComponent<RectTransform>();
            levelRT.anchorMin = new Vector2(0, 0);
            levelRT.anchorMax = new Vector2(1, 0.15f);
            levelRT.offsetMin = new Vector2(0, -60); // Puxa pra baixo
            levelRT.offsetMax = new Vector2(0, 0);

            var hlg = levelSection.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childForceExpandWidth = false;

            // Texto Lv. X
            GameObject lvGO = new GameObject("Txt_Level", typeof(RectTransform), typeof(TextMeshProUGUI));
            lvGO.transform.SetParent(levelSection.transform, false);
            var lvTxt = lvGO.GetComponent<TextMeshProUGUI>();
            lvTxt.fontSize = 20;
            lvTxt.text = "Lv. 60";
            lvTxt.rectTransform.sizeDelta = new Vector2(80, 30);

            // Barra de XP
            GameObject barGO = new GameObject("XPBar_BG", typeof(RectTransform), typeof(Image));
            barGO.transform.SetParent(levelSection.transform, false);
            barGO.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 12);
            barGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(barGO.transform, false);
            RectTransform fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.7f, 1);
            fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
            fillGO.GetComponent<Image>().color = new Color(0, 0.8f, 1f, 1f);

            // Texto XP
            GameObject xpValGO = new GameObject("Txt_XPValue", typeof(RectTransform), typeof(TextMeshProUGUI));
            xpValGO.transform.SetParent(levelSection.transform, false);
            var xpTxt = xpValGO.GetComponent<TextMeshProUGUI>();
            xpTxt.fontSize = 14;
            xpTxt.text = "1200 / 5000";

            // 2. Seção de Constelação (Lado direito, acima dos equipamentos ou integrado)
            // Vamos criar um novo painel lateral para Constelação
            GameObject constSection = new GameObject("Section_Constellation", typeof(RectTransform), typeof(VerticalLayoutGroup));
            constSection.transform.SetParent(unitPanel, false);
            RectTransform constRT = constSection.GetComponent<RectTransform>();
            constRT.anchorMin = new Vector2(0.4f, 0.7f); // Canto superior direito do painel de detalhes
            constRT.anchorMax = new Vector2(1, 1);
            constRT.offsetMin = new Vector2(16, 8);
            constRT.offsetMax = new Vector2(-16, -8);

            var vlg = constSection.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.UpperRight;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;

            // Grid de 6 estrelas
            GameObject starsGO = new GameObject("StarsGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            starsGO.transform.SetParent(constSection.transform, false);
            var grid = starsGO.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(25, 25);
            grid.spacing = new Vector2(5, 5);
            grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            grid.constraintCount = 1;

            Image[] stars = new Image[6];
            for (int i = 0; i < 6; i++)
            {
                GameObject starGO = new GameObject($"Star_{i}", typeof(RectTransform), typeof(Image));
                starGO.transform.SetParent(starsGO.transform, false);
                stars[i] = starGO.GetComponent<Image>();
                stars[i].color = Color.gray; // Inativo por padrão
            }

            // Botão Ascender
            GameObject btnGO = new GameObject("Btn_Ascend", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(constSection.transform, false);
            btnGO.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 40);
            btnGO.GetComponent<Image>().color = new Color(0.8f, 0.6f, 0, 1f);

            GameObject btnTxtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTxtGO.transform.SetParent(btnGO.transform, false);
            var btnTxt = btnTxtGO.GetComponent<TextMeshProUGUI>();
            btnTxt.fontSize = 16;
            btnTxt.text = "ASCENDER";
            btnTxt.alignment = TextAlignmentOptions.Center;
            btnTxt.rectTransform.anchorMin = Vector2.zero; btnTxt.rectTransform.anchorMax = Vector2.one;
            btnTxt.rectTransform.offsetMin = btnTxt.rectTransform.offsetMax = Vector2.zero;

            // Texto Insígnias
            GameObject insignGO = new GameObject("Txt_InsigniaCount", typeof(RectTransform), typeof(TextMeshProUGUI));
            insignGO.transform.SetParent(constSection.transform, false);
            var insignTxt = insignGO.GetComponent<TextMeshProUGUI>();
            insignTxt.fontSize = 12;
            insignTxt.text = "Insígnias: 0";
            insignTxt.alignment = TextAlignmentOptions.Right;

            // --- LINKAR REFERÊNCIAS ---
            var serializedInv = new SerializedObject(inventory);
            serializedInv.FindProperty("unitLevelText").objectReferenceValue = lvTxt;
            serializedInv.FindProperty("unitXPBar").objectReferenceValue = fillGO.GetComponent<Image>();
            serializedInv.FindProperty("unitXPText").objectReferenceValue = xpTxt;

            var constellationStarsProp = serializedInv.FindProperty("constellationStars");
            for (int i = 0; i < 6; i++)
                constellationStarsProp.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];

            serializedInv.FindProperty("constellationButton").objectReferenceValue = btnGO.GetComponent<Button>();
            serializedInv.FindProperty("insigniaCountText").objectReferenceValue = insignTxt;
            
            serializedInv.ApplyModifiedProperties();

            Debug.Log("Unit Detail Panel Overhaul concluído com sucesso!");
        }
    }
}
