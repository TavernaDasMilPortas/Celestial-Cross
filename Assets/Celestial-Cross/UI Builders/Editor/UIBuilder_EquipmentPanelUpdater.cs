using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Scenes.Unit;

namespace CelestialCross.UIBuilders.Editor
{
    public class UIBuilder_EquipmentPanelUpdater : UnityEditor.Editor
    {
        [MenuItem("Celestial Cross/UI Builders/Updaters/Update Equipment Panel")]
        public static void UpdateEquipmentPanel()
        {
            var panel = Object.FindObjectOfType<UnitDetailPanel_Equipment>(true);
            if (panel == null)
            {
                Debug.LogError("UnitDetailPanel_Equipment não encontrado na cena.");
                return;
            }

            // 1. Add TextMeshProUGUI to slot buttons
            string[] slotNames = { "Capacete", "Peitoral", "Luvas", "Botas", "Colar", "Anel" };
            for (int i = 0; i < 6; i++)
            {
                if (panel.artifactSlotButtons[i] != null)
                {
                    var slotT = panel.artifactSlotButtons[i].transform;
                    var txtName = slotT.Find("SlotNameText");
                    if (txtName == null)
                    {
                        var go = new GameObject("SlotNameText", typeof(RectTransform), typeof(TextMeshProUGUI));
                        go.transform.SetParent(slotT, false);
                        var rt = go.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0, 0);
                        rt.anchorMax = new Vector2(1, 0.25f);
                        rt.offsetMin = rt.offsetMax = Vector2.zero;
                        
                        var tmp = go.GetComponent<TextMeshProUGUI>();
                        tmp.text = slotNames[i];
                        tmp.fontSize = 18;
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.color = new Color(1,1,1, 0.7f);
                    }
                }
            }

            // 2. Adjust Grid layout to make room for Set List
            var grid = panel.transform.Find("SlotsGrid");
            if (grid != null)
            {
                var gRT = grid.GetComponent<RectTransform>();
                // Original era 0.05 a 0.95, vamos subir um pouco o painel para caber a lista em baixo
                gRT.anchorMin = new Vector2(0.05f, 0.45f); 
            }

            // 3. Create Set List Container
            var setListContainer = panel.transform.Find("SetListContainer");
            if (setListContainer == null)
            {
                var go = new GameObject("SetListContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
                go.transform.SetParent(panel.transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.15f);
                rt.anchorMax = new Vector2(0.95f, 0.4f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;

                var vlg = go.GetComponent<VerticalLayoutGroup>();
                vlg.childControlWidth = true;
                vlg.childForceExpandWidth = true;
                vlg.spacing = 10;
                vlg.padding = new RectOffset(10, 10, 10, 10);

                panel.setListContainer = rt;
                setListContainer = go.transform;
            }
            else
            {
                panel.setListContainer = setListContainer.GetComponent<RectTransform>();
            }

            // Prefab for Set List
            var setItemPrefab = panel.transform.Find("SetListItemPrefab");
            if (setItemPrefab == null)
            {
                var go = new GameObject("SetListItemPrefab", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasGroup));
                go.transform.SetParent(panel.transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(0, 50);
                go.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

                var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtGo.transform.SetParent(go.transform, false);
                var txtRt = txtGo.GetComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
                txtRt.offsetMin = new Vector2(10, 0); txtRt.offsetMax = new Vector2(-10, 0);
                
                var tmp = txtGo.GetComponent<TextMeshProUGUI>();
                tmp.text = "Set Name x2";
                tmp.fontSize = 20;
                tmp.alignment = TextAlignmentOptions.Left;

                go.SetActive(false);
                panel.setListItemPrefab = go;
            }

            // 4. Create Modal
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas não encontrado.");
                return;
            }

            var modalsContainer = canvas.transform.Find("Modals_Container");
            if (modalsContainer == null) modalsContainer = canvas.transform;

            var modalObj = modalsContainer.Find("ArtifactSetBonusModal");
            if (modalObj == null)
            {
                var go = new GameObject("ArtifactSetBonusModal", typeof(RectTransform), typeof(Image), typeof(ArtifactSetBonusModal));
                go.transform.SetParent(modalsContainer, false);
                SetFullscreen(go.GetComponent<RectTransform>());
                go.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
                var comp = go.GetComponent<ArtifactSetBonusModal>();
                comp.modalRoot = go;

                var window = new GameObject("Window", typeof(RectTransform), typeof(Image));
                window.transform.SetParent(go.transform, false);
                var wRT = window.GetComponent<RectTransform>();
                wRT.sizeDelta = new Vector2(700, 900);
                window.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

                comp.titleText = CreateText(window.transform, "Title", new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.95f), "Bônus de Conjunto", 30);
                comp.titleText.alignment = TextAlignmentOptions.Center;

                var scroll = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
                scroll.transform.SetParent(window.transform, false);
                var sRT = scroll.GetComponent<RectTransform>();
                sRT.anchorMin = new Vector2(0.05f, 0.15f); sRT.anchorMax = new Vector2(0.95f, 0.85f);
                sRT.offsetMin = sRT.offsetMax = Vector2.zero;

                var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
                viewport.transform.SetParent(scroll.transform, false);
                SetFullscreen(viewport.GetComponent<RectTransform>());
                viewport.GetComponent<Mask>().showMaskGraphic = false;

                var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
                content.transform.SetParent(viewport.transform, false);
                var cRT = content.GetComponent<RectTransform>();
                cRT.anchorMin = new Vector2(0, 1); cRT.anchorMax = new Vector2(1, 1);
                cRT.pivot = new Vector2(0.5f, 1);
                cRT.sizeDelta = new Vector2(0, 600);

                var vlg = content.GetComponent<VerticalLayoutGroup>();
                vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
                vlg.childControlHeight = true; vlg.childForceExpandHeight = false;
                vlg.spacing = 15;
                vlg.padding = new RectOffset(10, 10, 10, 10);

                scroll.GetComponent<ScrollRect>().content = cRT;
                scroll.GetComponent<ScrollRect>().viewport = viewport.GetComponent<RectTransform>();
                comp.bonusesContainer = cRT;

                // Bonus Prefab
                var bonusPrefab = new GameObject("BonusItemPrefab", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                bonusPrefab.transform.SetParent(window.transform, false);
                bonusPrefab.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);
                var bpRT = bonusPrefab.GetComponent<RectTransform>();
                bpRT.sizeDelta = new Vector2(0, 100);

                var bTxt = CreateText(bonusPrefab.transform, "DescText", Vector2.zero, Vector2.one, "[2 Peças] ...", 18);
                bTxt.alignment = TextAlignmentOptions.TopLeft;
                bTxt.rectTransform.offsetMin = new Vector2(10, 10);
                bTxt.rectTransform.offsetMax = new Vector2(-10, -10);

                bonusPrefab.SetActive(false);
                comp.bonusItemPrefab = bonusPrefab;

                // Close Button
                var closeBtnObj = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
                closeBtnObj.transform.SetParent(window.transform, false);
                var clRT = closeBtnObj.GetComponent<RectTransform>();
                clRT.anchorMin = new Vector2(0.3f, 0.03f); clRT.anchorMax = new Vector2(0.7f, 0.12f);
                clRT.offsetMin = clRT.offsetMax = Vector2.zero;
                closeBtnObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
                CreateText(closeBtnObj.transform, "Text", Vector2.zero, Vector2.one, "Fechar", 24).alignment = TextAlignmentOptions.Center;
                comp.closeButton = closeBtnObj.GetComponent<Button>();

                go.SetActive(false);
                panel.setBonusModal = comp;
            }
            else
            {
                panel.setBonusModal = modalObj.GetComponent<ArtifactSetBonusModal>();
            }

            EditorUtility.SetDirty(panel);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(panel.gameObject.scene);

            Debug.Log("Panel_Equipment atualizado com sucesso!");
        }

        private static void SetFullscreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string defaultText, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var txt = go.GetComponent<TextMeshProUGUI>();
            txt.text = defaultText; txt.fontSize = fontSize; txt.color = Color.white;
            return txt;
        }
    }
}
