using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Giulia_UI;

namespace CelestialCross.Editor
{
    public class UIBuilder_PetManagementUI : EditorWindow
    {
        [MenuItem("Celestial Cross/UI Builders/Update Inventory for Pet Management")]
        public static void ShowWindow()
        {
            var window = GetWindow<UIBuilder_PetManagementUI>("Pet UI Builder");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Pet Management UI Builder", EditorStyles.boldLabel);
            
            if (GUILayout.Button("1. Add Manage Pet Button & Modal"))
            {
                InjectPetManagementUI();
            }
        }

        private void InjectPetManagementUI()
        {
            var inventoryUI = FindObjectOfType<InventoryUI>(true);
            if (inventoryUI == null)
            {
                Debug.LogError("InventoryUI not found in scene!");
                return;
            }

            // 1. Add Manage Pet Button to the Pets Top Panel (if found) or directly as child of InventoryUI
            Button btnManagePet = inventoryUI.managePetButton;
            if (btnManagePet == null)
            {
                GameObject btnGo = new GameObject("Btn_ManagePet", typeof(RectTransform), typeof(Image), typeof(Button));
                Undo.RegisterCreatedObjectUndo(btnGo, "Create Manage Pet Button");

                // Assuming Tab 1 is Pets
                Transform petsTopPanel = inventoryUI.topPanels != null && inventoryUI.topPanels.Length > 1 ? inventoryUI.topPanels[1] : inventoryUI.transform;
                btnGo.transform.SetParent(petsTopPanel, false);

                var rt = btnGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1, 0);
                rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(1, 0);
                rt.anchoredPosition = new Vector2(-20, 20); // bottom right of top panel
                rt.sizeDelta = new Vector2(140, 40);

                var img = btnGo.GetComponent<Image>();
                img.color = new Color(0.9f, 0.4f, 0.1f, 1f);

                GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtGo.transform.SetParent(btnGo.transform, false);
                var trt = txtGo.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                trt.offsetMin = trt.offsetMax = Vector2.zero;

                var text = txtGo.GetComponent<TextMeshProUGUI>();
                text.text = "Gerenciar";
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 18;
                text.color = Color.white;

                btnManagePet = btnGo.GetComponent<Button>();
                inventoryUI.managePetButton = btnManagePet;
            }

            // 2. Add PetManageModal
            PetManageModal modal = inventoryUI.petManageModal;
            if (modal == null)
            {
                GameObject modalGo = new GameObject("PetManageModal", typeof(RectTransform), typeof(Image));
                Undo.RegisterCreatedObjectUndo(modalGo, "Create PetManageModal");
                modalGo.transform.SetParent(inventoryUI.transform, false);
                
                var rt = modalGo.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                modalGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

                // Modal Window
                GameObject windowGo = new GameObject("Window", typeof(RectTransform), typeof(Image));
                windowGo.transform.SetParent(modalGo.transform, false);
                var wrt = windowGo.GetComponent<RectTransform>();
                wrt.sizeDelta = new Vector2(400, 350);
                windowGo.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

                // Title
                GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
                titleGo.transform.SetParent(windowGo.transform, false);
                var ttrt = titleGo.GetComponent<RectTransform>();
                ttrt.anchorMin = new Vector2(0, 1); ttrt.anchorMax = new Vector2(1, 1);
                ttrt.anchoredPosition = new Vector2(0, -30);
                ttrt.sizeDelta = new Vector2(0, 40);
                var titleTxt = titleGo.GetComponent<TextMeshProUGUI>();
                titleTxt.text = "Detalhes do Pet";
                titleTxt.alignment = TextAlignmentOptions.Center;
                titleTxt.fontSize = 24;

                // Info Details
                GameObject infoGo = new GameObject("Info", typeof(RectTransform), typeof(TextMeshProUGUI));
                infoGo.transform.SetParent(windowGo.transform, false);
                var irt = infoGo.GetComponent<RectTransform>();
                irt.anchorMin = new Vector2(0, 0); irt.anchorMax = new Vector2(1, 1);
                irt.offsetMin = new Vector2(20, 100); irt.offsetMax = new Vector2(-20, -70);
                var infoTxt = infoGo.GetComponent<TextMeshProUGUI>();
                infoTxt.text = "Info...";
                infoTxt.fontSize = 18;
                infoTxt.richText = true;

                // Release Button
                GameObject relBtnGo = new GameObject("Btn_Release", typeof(RectTransform), typeof(Image), typeof(Button));
                relBtnGo.transform.SetParent(windowGo.transform, false);
                var rbrt = relBtnGo.GetComponent<RectTransform>();
                rbrt.anchorMin = new Vector2(0.5f, 0); rbrt.anchorMax = new Vector2(0.5f, 0);
                rbrt.anchoredPosition = new Vector2(0, 50);
                rbrt.sizeDelta = new Vector2(200, 60);
                relBtnGo.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);

                GameObject relTxtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                relTxtGo.transform.SetParent(relBtnGo.transform, false);
                var rtlrt = relTxtGo.GetComponent<RectTransform>();
                rtlrt.anchorMin = Vector2.zero; rtlrt.anchorMax = Vector2.one;
                rtlrt.offsetMin = rtlrt.offsetMax = Vector2.zero;
                var relTxt = relTxtGo.GetComponent<TextMeshProUGUI>();
                relTxt.text = "LIBERTAR";
                relTxt.alignment = TextAlignmentOptions.Center;
                relTxt.fontSize = 16;
                relTxt.color = Color.white;

                // Close Button
                GameObject closeBtnGo = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
                closeBtnGo.transform.SetParent(windowGo.transform, false);
                var cbrt = closeBtnGo.GetComponent<RectTransform>();
                cbrt.anchorMin = new Vector2(1, 1); cbrt.anchorMax = new Vector2(1, 1);
                cbrt.anchoredPosition = new Vector2(-30, -30);
                cbrt.sizeDelta = new Vector2(40, 40);
                closeBtnGo.GetComponent<Image>().color = Color.black;

                modal = modalGo.AddComponent<PetManageModal>();
                
                // Use reflection or SerializedObject to set private fields!
                var so = new SerializedObject(modal);
                so.FindProperty("titleText").objectReferenceValue = titleTxt;
                so.FindProperty("detailsText").objectReferenceValue = infoTxt;
                so.FindProperty("releaseButton").objectReferenceValue = relBtnGo.GetComponent<Button>();
                so.FindProperty("releaseYieldText").objectReferenceValue = relTxt;
                so.FindProperty("closeButton").objectReferenceValue = closeBtnGo.GetComponent<Button>();
                so.ApplyModifiedProperties();

                inventoryUI.petManageModal = modal;
                modalGo.SetActive(false);
            }

            EditorUtility.SetDirty(inventoryUI);
            Debug.Log("Pet Management UI successfully injected in " + inventoryUI.gameObject.name);
        }
    }
}