using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Giulia_UI.Editor
{
    [CustomEditor(typeof(ItemsInventoryUI))]
    public class UIBuilder_ItemsInventoryUI : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ItemsInventoryUI script = (ItemsInventoryUI)target;

            GUILayout.Space(20);
            if (GUILayout.Button("BUI: Auto-Gerar Hierarquia de Itens", GUILayout.Height(40)))
            {
                BuildHierarchy(script);
            }
        }

        private void BuildHierarchy(ItemsInventoryUI rootScript)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found!");
                return;
            }

            GameObject mainPanel = new GameObject("ItemsInventory_Panel", typeof(RectTransform), typeof(Image));
            mainPanel.transform.SetParent(canvas.transform, false);
            mainPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            
            RectTransform mainRt = mainPanel.GetComponent<RectTransform>();
            mainRt.anchorMin = Vector2.zero;
            mainRt.anchorMax = Vector2.one;
            mainRt.sizeDelta = Vector2.zero;

            // --- Top Panel (Menor) ---
            GameObject topPanel = new GameObject("TopDetailsPanel", typeof(RectTransform), typeof(Image));
            topPanel.transform.SetParent(mainPanel.transform, false);
            topPanel.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

            RectTransform topRt = topPanel.GetComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0, 0.7f); // 30% da tela no topo
            topRt.anchorMax = new Vector2(1, 1);
            topRt.offsetMin = Vector2.zero;
            topRt.offsetMax = Vector2.zero;

            GameObject txtItemName = CreateText(topPanel.transform, "ItemNameTXT", "Nome do Item", 36, TextAlignmentOptions.TopLeft);
            RectTransform txtNameRt = txtItemName.GetComponent<RectTransform>();
            txtNameRt.anchorMin = new Vector2(0.25f, 0.6f); txtNameRt.anchorMax = new Vector2(0.95f, 0.9f);
            rootScript.selectedItemNameText = txtItemName.GetComponent<TMP_Text>();

            GameObject txtItemQuantity = CreateText(topPanel.transform, "ItemQtdTXT", "Quantidade: 0", 24, TextAlignmentOptions.MidlineLeft);
            RectTransform txtQtdRt = txtItemQuantity.GetComponent<RectTransform>();
            txtQtdRt.anchorMin = new Vector2(0.25f, 0.4f); txtQtdRt.anchorMax = new Vector2(0.95f, 0.6f);
            rootScript.selectedItemQuantityText = txtItemQuantity.GetComponent<TMP_Text>();
            
            GameObject txtItemDesc = CreateText(topPanel.transform, "ItemDescTXT", "Descrição...\n\nPense nas possibilidades", 20, TextAlignmentOptions.TopLeft);
            RectTransform txtDescRt = txtItemDesc.GetComponent<RectTransform>();
            txtDescRt.anchorMin = new Vector2(0.25f, 0.05f); txtDescRt.anchorMax = new Vector2(0.95f, 0.4f);
            rootScript.selectedItemDescriptionText = txtItemDesc.GetComponent<TMP_Text>();

            GameObject iconTemplate = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconTemplate.transform.SetParent(topPanel.transform, false);
            RectTransform iconRt = iconTemplate.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.05f, 0.2f); iconRt.anchorMax = new Vector2(0.2f, 0.8f);
            iconRt.offsetMin = iconRt.offsetMax = Vector2.zero;
            rootScript.selectedItemIcon = iconTemplate.GetComponent<Image>();

            // --- Middle (Filters) ---
            GameObject filtersPanel = new GameObject("FiltersPanel", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            filtersPanel.transform.SetParent(mainPanel.transform, false);
            RectTransform filtersRt = filtersPanel.GetComponent<RectTransform>();
            filtersRt.anchorMin = new Vector2(0, 0.62f);
            filtersRt.anchorMax = new Vector2(1, 0.7f); // 8% da tela
            filtersRt.offsetMin = filtersRt.offsetMax = Vector2.zero;

            HorizontalLayoutGroup hg = filtersPanel.GetComponent<HorizontalLayoutGroup>();
            hg.childControlWidth = true; hg.childControlHeight = true;
            hg.childForceExpandWidth = true; hg.childForceExpandHeight = true;
            hg.spacing = 10;
            hg.padding = new RectOffset(20, 20, 10, 10);

            rootScript.btnFilterAll = CreateFilterBtn(filtersPanel.transform, "All").GetComponent<Button>();
            rootScript.btnFilterPetSouls = CreateFilterBtn(filtersPanel.transform, "Pet Souls").GetComponent<Button>();

            // --- Bottom (Grid Container) ---
            GameObject bottomPanel = new GameObject("BottomGridPanel", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            bottomPanel.transform.SetParent(mainPanel.transform, false);
            bottomPanel.GetComponent<Image>().color = new Color(0,0,0,0);

            RectTransform botRt = bottomPanel.GetComponent<RectTransform>();
            botRt.anchorMin = new Vector2(0, 0); // O resto da tela
            botRt.anchorMax = new Vector2(1, 0.62f);
            botRt.offsetMin = Vector2.zero;
            botRt.offsetMax = Vector2.zero;

            GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
            contentObj.transform.SetParent(bottomPanel.transform, false);
            RectTransform contentRt = contentObj.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0,1); contentRt.anchorMax = new Vector2(1,1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = new Vector2(0, 1500); // Exemplo height fixo por hora

            GridLayoutGroup grid = contentObj.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(180, 180);
            grid.spacing = new Vector2(30, 30);
            grid.padding = new RectOffset(40, 40, 40, 40);

            ScrollRect scroll = bottomPanel.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.horizontal = false;
            scroll.vertical = true;

            rootScript.gridContainer = contentRt;

            // --- Prefab de Teste (Slot) ---
            GameObject prefabGo = new GameObject("ItemSlotPrefab_TEMP", typeof(RectTransform), typeof(Image), typeof(Button));
            prefabGo.GetComponent<Image>().color = Color.white;
            GameObject prefabTxt = CreateText(prefabGo.transform, "TXT", "x1", 24, TextAlignmentOptions.BottomRight);
            RectTransform pTxtRt = prefabTxt.GetComponent<RectTransform>();
            pTxtRt.anchorMin = Vector2.zero; pTxtRt.anchorMax = Vector2.one;
            rootScript.itemSlotPrefab = prefabGo;
            prefabGo.SetActive(false); // Oculta na cena porque é só de molde
            prefabGo.transform.SetParent(mainPanel.transform, false);

            Undo.RegisterCreatedObjectUndo(mainPanel, "BUI Create Items Inventory");
            EditorUtility.SetDirty(rootScript);
            
            Debug.Log("Hierarquia de Itens criada com sucesso!");
        }

        private GameObject CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions align)
        {
            GameObject txtObj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(parent, false);

            RectTransform rt = txtObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;

            return txtObj;
        }

        private GameObject CreateFilterBtn(Transform parent, string label)
        {
            GameObject btnGo = new GameObject($"BtnFilter_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);
            btnGo.GetComponent<Image>().color = new Color(0.2f,0.3f,0.4f, 1f);

            CreateText(btnGo.transform, "TXT", label, 28, TextAlignmentOptions.Center);
            return btnGo;
        }
    }
}