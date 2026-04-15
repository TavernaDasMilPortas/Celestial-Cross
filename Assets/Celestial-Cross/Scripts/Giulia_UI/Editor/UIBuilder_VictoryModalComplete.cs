using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Giulia_UI.Editor
{
    public class UIBuilder_VictoryModalComplete
    {
        [MenuItem("Celestial Cross/UI Builders/Generate Victory Modal Complete")]
        public static void BuildModal()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[UI Builder] Nenhum Canvas encontrado na cena. Abra uma cena com Canvas.");
                return;
            }

            // ==========================================
            // 1. Root do Modal (Fundo Escuro)
            // ==========================================
            GameObject modalRoot = new GameObject("RewardDetailsModal_Complete", typeof(RectTransform), typeof(Image));
            modalRoot.transform.SetParent(canvas.transform, false);
            modalRoot.transform.SetAsLastSibling();

            RectTransform modalRt = modalRoot.GetComponent<RectTransform>();
            modalRt.anchorMin = new Vector2(0.2f, 0.2f);
            modalRt.anchorMax = new Vector2(0.8f, 0.8f);
            modalRt.offsetMin = Vector2.zero;
            modalRt.offsetMax = Vector2.zero;
            modalRoot.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f, 0.98f); // Cinza bem escuro quase opaco

            // ==========================================
            // 2. Ícone
            // ==========================================
            GameObject mIconGo = new GameObject("ModalIcon", typeof(RectTransform), typeof(Image));
            mIconGo.transform.SetParent(modalRoot.transform, false);
            RectTransform mIconRt = mIconGo.GetComponent<RectTransform>();
            mIconRt.anchorMin = new Vector2(0.25f, 0.5f);
            mIconRt.anchorMax = new Vector2(0.75f, 0.95f);
            mIconRt.offsetMin = mIconRt.offsetMax = Vector2.zero;
            mIconGo.GetComponent<Image>().preserveAspect = true;

            // ==========================================
            // 3. Título e Descrição
            // ==========================================
            var titleGo = CreateText(modalRoot.transform, "ModalTitle", 36, new Vector2(0.05f, 0.4f), new Vector2(0.95f, 0.48f), Color.yellow);
            var titleTxt = titleGo.GetComponent<TextMeshProUGUI>();
            titleTxt.text = "Nome do Item Grande";
            titleTxt.fontStyle = FontStyles.Bold;

            var descGo = CreateText(modalRoot.transform, "ModalDesc", 20, new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.38f), Color.white);
            var descTxt = descGo.GetComponent<TextMeshProUGUI>();
            descTxt.text = "Especificações detalhadas do item.\nStatus 1...\nStatus 2...";
            descTxt.alignment = TextAlignmentOptions.Top;

            // ==========================================
            // 4. Botões de Ação
            // ==========================================
            // Vender Btn
            GameObject sBtnGo = new GameObject("Btn_Sell", typeof(RectTransform), typeof(Image), typeof(Button));
            sBtnGo.transform.SetParent(modalRoot.transform, false);
            RectTransform sRt = sBtnGo.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.1f, 0.05f); sRt.anchorMax = new Vector2(0.45f, 0.15f);
            sRt.offsetMin = Vector2.zero; sRt.offsetMax = Vector2.zero;
            sBtnGo.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f); // Vermelho
            var sellTxtGo = CreateText(sBtnGo.transform, "Text", 22, Vector2.zero, Vector2.one, Color.white);
            var sellTxt = sellTxtGo.GetComponent<TextMeshProUGUI>();
            sellTxt.text = "Vender";

            // Fechar Btn
            GameObject cBtnGo = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            cBtnGo.transform.SetParent(modalRoot.transform, false);
            RectTransform cRt = cBtnGo.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.55f, 0.05f); cRt.anchorMax = new Vector2(0.9f, 0.15f);
            cRt.offsetMin = Vector2.zero; cRt.offsetMax = Vector2.zero;
            cBtnGo.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 1f); // Cinza
            var closeTxtGo = CreateText(cBtnGo.transform, "Text", 22, Vector2.zero, Vector2.one, Color.white);
            closeTxtGo.GetComponent<TextMeshProUGUI>().text = "Fechar";

            // ==========================================
            // 5. Vincular ao VictoryRewardUI (se existir)
            // ==========================================
            var victoryUI = Object.FindFirstObjectByType<VictoryRewardUI>();
            if (victoryUI != null)
            {
                SerializedObject so = new SerializedObject(victoryUI);
                so.Update();
                so.FindProperty("detailsModal").objectReferenceValue = modalRoot;
                so.FindProperty("modalTitle").objectReferenceValue = titleTxt;
                so.FindProperty("modalDesc").objectReferenceValue = descTxt;
                so.FindProperty("modalSellBtn").objectReferenceValue = sBtnGo.GetComponent<Button>();
                so.FindProperty("modalSellTxt").objectReferenceValue = sellTxt;
                so.FindProperty("modalCloseBtn").objectReferenceValue = cBtnGo.GetComponent<Button>();
                so.ApplyModifiedProperties();
                
                Debug.Log("[UI Builder] Modal gerado e associado automaticamente ao VictoryRewardUI na cena!");
            }
            else
            {
                Debug.Log("[UI Builder] Modal gerado! Mas não encontrei nenhum VictoryRewardUI na cena para ligar as referências automaticamente.");
            }

            modalRoot.SetActive(false); // Oculta por padrão para não atrapalhar
            Undo.RegisterCreatedObjectUndo(modalRoot, "Create Complete Victory Modal");
            Selection.activeGameObject = modalRoot;
        }

        private static GameObject CreateText(Transform parent, string name, int fontSize, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject txtGo = new GameObject(name, typeof(RectTransform));
            txtGo.transform.SetParent(parent, false);
            RectTransform rt = txtGo.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var txt = txtGo.AddComponent<TextMeshProUGUI>();
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = TextAlignmentOptions.Center;
            txt.enableWordWrapping = true;

            return txtGo;
        }
    }
}