using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Giulia_UI;

namespace CelestialCross.EditorArea
{
    public static class UIBuilder_InventoryRepair
    {
        [MenuItem("Celestial Cross/UI Builders/Repair & Force Link All Systems")]
        public static void RepairInventoryUI()
        {
            InventoryUI inventory = Object.FindObjectOfType<InventoryUI>();
            if (inventory == null) { Debug.LogError("InventoryUI não encontrado na cena!"); return; }

            SerializedObject soInv = new SerializedObject(inventory);
            
            // Tentar auto-preencher o catálogo se estiver vazio
            if (inventory.unitCatalog == null)
            {
                string[] catalogGuids = AssetDatabase.FindAssets("t:UnitCatalog");
                if (catalogGuids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(catalogGuids[0]);
                    var foundCatalog = AssetDatabase.LoadAssetAtPath<UnitCatalog>(path);
                    soInv.FindProperty("unitCatalog").objectReferenceValue = foundCatalog;
                    inventory.unitCatalog = foundCatalog;
                    Debug.Log($"[Repair] UnitCatalog encontrado e vinculado: {path}");
                }
            }

            // Debug: Listar conteúdo do catálogo para garantir que IDs batem
            if (inventory.unitCatalog != null)
            {
                var units = inventory.unitCatalog.GetAllUnitData();
                Debug.Log($"[Repair] Catálogo verificado: {units.Count} unidades encontradas.");
                foreach (var u in units)
                {
                    if (u != null) Debug.Log($"   -> [{u.displayName}] ID: {u.UnitID}");
                }
            }

            Transform canvasTr = inventory.GetComponentInParent<Canvas>().transform;

            // 1. Garantir que o TopPanel_0 exista e tenha o ProfileContainer
            if (inventory.topPanels == null || inventory.topPanels.Length == 0 || inventory.topPanels[0] == null)
            {
                Debug.LogError("TopPanel_0 não configurado corretamente no InventoryUI!"); return;
            }

            Transform unitPanel = inventory.topPanels[0];
            Transform profile = unitPanel.Find("ProfileContainer");
            if (profile == null) profile = unitPanel;

            // --- REPARAR XP / LEVEL ---
            Transform xpSec = profile.Find("Section_LevelXP");
            if (xpSec == null) xpSec = CreateXPSection(profile);
            
            soInv.FindProperty("unitLevelText").objectReferenceValue = xpSec.Find("Txt_Level")?.GetComponent<TextMeshProUGUI>();
            soInv.FindProperty("unitXPBar").objectReferenceValue = xpSec.Find("XPBar_BG/Fill")?.GetComponent<Image>();
            soInv.FindProperty("unitXPText").objectReferenceValue = xpSec.Find("Txt_XPValue")?.GetComponent<TextMeshProUGUI>();

            // --- REPARAR LINK DA CONSTELAÇÃO ---
            Transform constLink = unitPanel.Find("Section_Constellation_Link");
            if (constLink == null) constLink = CreateConstellationLink(unitPanel);
            
            soInv.FindProperty("constellationButton").objectReferenceValue = constLink.GetComponentInChildren<Button>();
            soInv.FindProperty("insigniaCountText").objectReferenceValue = constLink.Find("Txt_Insignia")?.GetComponent<TextMeshProUGUI>();

            // Forçar o listener do botão
            Button b = (Button)soInv.FindProperty("constellationButton").objectReferenceValue;
            if (b != null)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(b.onClick, inventory.OnConstellationUpgradeClicked);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(b.onClick, inventory.OnConstellationUpgradeClicked);
            }

            // --- REPARAR MODAL DE CONSTELAÇÃO ---
            ConstellationModal modal = Object.FindObjectOfType<ConstellationModal>(true);
            if (modal == null) modal = CreateConstellationModal(canvasTr);
            
            soInv.FindProperty("constellationModal").objectReferenceValue = modal;

            // --- FORÇAR LINKS INTERNOS DO MODAL ---
            RepairModalInternalLinks(modal);

            soInv.ApplyModifiedProperties();
            EditorUtility.SetDirty(inventory);
            
            // Garantir que os catálogos básicos estão preenchidos para não dar NullRef
            if (inventory.unitCatalog == null) Debug.LogWarning("InventoryUI: UnitCatalog não está preenchido! Preencha no Inspector.");
            
            Debug.Log("Inventory UI Repair: Todos os sistemas foram verificados e religados com sucesso!");
        }

        private static Transform CreateXPSection(Transform parent)
        {
            GameObject go = new GameObject("Section_LevelXP", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0.15f);
            rt.offsetMin = new Vector2(0, -55); rt.offsetMax = new Vector2(0, 0);
            
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10; hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false; hlg.childForceExpandWidth = false;

            var lv = CreateText(go.transform, "Txt_Level", "Lv. --", 18);
            lv.rectTransform.sizeDelta = new Vector2(70, 30);
            
            var bar = new GameObject("XPBar_BG", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            bar.SetParent(go.transform, false); bar.sizeDelta = new Vector2(120, 12);
            bar.GetComponent<Image>().color = new Color(0,0,0,0.5f);
            
            var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(bar, false);
            var fill = fillGO.GetComponent<Image>();
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = new Vector2(0.5f, 1);
            fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
            fill.color = Color.cyan;

            CreateText(go.transform, "Txt_XPValue", "-- / --", 12);
            return go.transform;
        }

        private static Transform CreateConstellationLink(Transform parent)
        {
            GameObject go = new GameObject("Section_Constellation_Link", typeof(RectTransform), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.6f, 0.75f); rt.anchorMax = new Vector2(0.95f, 0.95f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            
            var vlg = go.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 5; vlg.childAlignment = TextAnchor.UpperRight; vlg.childControlHeight = false;

            var btnGO = new GameObject("Btn_OpenConstellation", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(go.transform, false);
            var btn = btnGO.GetComponent<Button>();
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 40);
            btn.GetComponent<Image>().color = new Color(0.8f, 0.6f, 0.2f, 1f);
            var t = CreateText(btn.transform, "Text", "CONSTELAÇÃO", 14);
            t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one;
            t.rectTransform.offsetMin = t.rectTransform.offsetMax = Vector2.zero;
            t.alignment = TextAlignmentOptions.Center;

            CreateText(go.transform, "Txt_Insignia", "Insígnias: 0", 12).alignment = TextAlignmentOptions.Right;
            return go.transform;
        }

        private static ConstellationModal CreateConstellationModal(Transform canvas)
        {
            GameObject go = new GameObject("ConstellationModal", typeof(RectTransform), typeof(ConstellationModal));
            go.transform.SetParent(canvas, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            
            GameObject root = new GameObject("ModalRoot", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(go.transform, false);
            root.GetComponent<RectTransform>().anchorMin = Vector2.zero; root.GetComponent<RectTransform>().anchorMax = Vector2.one;
            root.GetComponent<RectTransform>().offsetMin = root.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0,0,0,0.9f);

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 850);
            panel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 1f);

            GameObject nodes = new GameObject("NodesContainer", typeof(RectTransform));
            nodes.transform.SetParent(panel.transform, false);
            nodes.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 600);

            GameObject close = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            close.transform.SetParent(panel.transform, false);
            var cRT = close.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(1,1); cRT.anchorMax = new Vector2(1,1);
            cRT.anchoredPosition = new Vector2(-40, -40);
            cRT.sizeDelta = new Vector2(50, 50);
            close.GetComponent<Image>().color = Color.red;

            return go.GetComponent<ConstellationModal>();
        }

        private static void RepairModalInternalLinks(ConstellationModal modal)
        {
            SerializedObject so = new SerializedObject(modal);
            Transform p = modal.transform.Find("ModalRoot/Panel");
            if (p == null) return;

            so.FindProperty("root").objectReferenceValue = modal.transform.Find("ModalRoot")?.gameObject;
            so.FindProperty("closeButton").objectReferenceValue = p.Find("Btn_Close")?.GetComponent<Button>();
            so.FindProperty("nodesContainer").objectReferenceValue = p.Find("NodesContainer")?.GetComponent<RectTransform>();

            var title = p.Find("Title") ?? CreateText(p, "Title", "NOME", 24).transform;
            so.FindProperty("unitNameText").objectReferenceValue = title.GetComponent<TextMeshProUGUI>();
            title.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            title.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50);

            var upgrade = p.Find("Btn_Upgrade") ?? CreateUpgradeButton(p);
            so.FindProperty("upgradeButton").objectReferenceValue = upgrade.GetComponent<Button>();

            var insign = p.Find("InsigniaCount") ?? CreateText(p, "InsigniaCount", "0", 16).transform;
            so.FindProperty("insigniaCountText").objectReferenceValue = insign.GetComponent<TextMeshProUGUI>();
            insign.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -300);

            var info = p.Find("InfoPanel") ?? CreateInfoPanel(p);
            so.FindProperty("infoPanel").objectReferenceValue = info.gameObject;
            so.FindProperty("skillNameText").objectReferenceValue = info.Find("SkillName")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("skillDescText").objectReferenceValue = info.Find("SkillDesc")?.GetComponent<TextMeshProUGUI>();

            Transform nodes = p.Find("NodesContainer");
            if (nodes != null)
            {
                var starsProp = so.FindProperty("starIcons");
                starsProp.arraySize = 6;
                for (int i = 0; i < 6; i++)
                {
                    Transform s = nodes.Find($"Star_{i}");
                    if (s == null) {
                        GameObject sGO = new GameObject($"Star_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                        sGO.transform.SetParent(nodes, false);
                        s = sGO.transform;
                        s.GetComponent<RectTransform>().sizeDelta = new Vector2(40,40);
                    }
                    starsProp.GetArrayElementAtIndex(i).objectReferenceValue = s.GetComponent<Image>();
                }

                var linesProp = so.FindProperty("connectionLines");
                linesProp.arraySize = 5;
                for (int i = 0; i < 5; i++)
                {
                    Transform l = nodes.Find($"Line_{i}");
                    if (l == null) {
                        GameObject lGO = new GameObject($"Line_{i}", typeof(RectTransform), typeof(Image));
                        lGO.transform.SetParent(nodes, false);
                        l = lGO.transform;
                        l.GetComponent<Image>().color = new Color(0.2f,0.2f,0.2f,0.5f);
                    }
                    linesProp.GetArrayElementAtIndex(i).objectReferenceValue = l.GetComponent<Image>();
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(modal);
        }

        private static Transform CreateUpgradeButton(Transform p)
        {
            GameObject go = new GameObject("Btn_Upgrade", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(p, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0); rt.anchorMax = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 100);
            rt.sizeDelta = new Vector2(250, 60);
            go.GetComponent<Image>().color = new Color(0, 0.6f, 0.2f, 1f);
            var t = CreateText(go.transform, "Text", "ATIVAR ESTRELA", 18);
            t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one;
            t.rectTransform.offsetMin = t.rectTransform.offsetMax = Vector2.zero;
            t.alignment = TextAlignmentOptions.Center;
            return go.transform;
        }

        private static Transform CreateInfoPanel(Transform p)
        {
            GameObject go = new GameObject("InfoPanel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(p, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f); rt.anchorMax = new Vector2(1, 0.5f);
            rt.anchoredPosition = new Vector2(220, 0);
            rt.sizeDelta = new Vector2(400, 250);
            go.GetComponent<Image>().color = new Color(0,0,0,0.85f);
            
            var name = CreateText(go.transform, "SkillName", "Habilidade", 20);
            name.color = Color.yellow;
            name.rectTransform.anchoredPosition = new Vector2(0, 80);
            
            var desc = CreateText(go.transform, "SkillDesc", "Descrição...", 14);
            desc.rectTransform.sizeDelta = new Vector2(360, 120);
            desc.rectTransform.anchoredPosition = new Vector2(0, -20);
            desc.enableWordWrapping = true;
            
            return go.transform;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string content, float size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = content; tmp.fontSize = size; tmp.color = Color.white;
            return tmp;
        }
    }
}
