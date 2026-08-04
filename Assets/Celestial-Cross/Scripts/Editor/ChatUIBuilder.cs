#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.UI;

namespace CelestialCross.EditorScripts
{
    public class ChatUIBuilder : EditorWindow
    {
        [MenuItem("Celestial Cross/Gerar UI do Chat na Cena Atual")]
        public static void BuildChatUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Nenhum Canvas encontrado na cena. Crie um Canvas primeiro (GameObject > UI > Canvas).");
                return;
            }

            // 1. Criar Botão Flutuante
            GameObject floatBtnObj = new GameObject("FloatingChatButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(FloatingChatButton));
            floatBtnObj.transform.SetParent(canvas.transform, false);
            RectTransform btnRect = floatBtnObj.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(80, 80);
            btnRect.anchorMin = new Vector2(1, 0.5f);
            btnRect.anchorMax = new Vector2(1, 0.5f);
            btnRect.anchoredPosition = new Vector2(-60, 0);
            
            Image btnImg = floatBtnObj.GetComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 1f, 1f);

            // 2. Criar Painel Principal do Chat
            GameObject chatPanelObj = new GameObject("ChatPanel", typeof(RectTransform), typeof(Image), typeof(ChatUIManager));
            chatPanelObj.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = chatPanelObj.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(600, 800);
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            
            Image panelImg = chatPanelObj.GetComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            chatPanelObj.SetActive(false);

            // 3. BARRA DE ABAS (Top)
            GameObject tabsAreaObj = new GameObject("TabsArea", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tabsAreaObj.transform.SetParent(chatPanelObj.transform, false);
            RectTransform tabsAreaRect = tabsAreaObj.GetComponent<RectTransform>();
            tabsAreaRect.anchorMin = new Vector2(0, 0.9f);
            tabsAreaRect.anchorMax = new Vector2(1, 1f);
            tabsAreaRect.offsetMin = new Vector2(10, 0);
            tabsAreaRect.offsetMax = new Vector2(-10, -10);

            var hlg = tabsAreaObj.GetComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = true;
            hlg.spacing = 10;

            // Aba Global
            GameObject globalTabObj = new GameObject("GlobalTab", typeof(RectTransform), typeof(Image), typeof(Button));
            globalTabObj.transform.SetParent(tabsAreaObj.transform, false);
            globalTabObj.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f); // Aba ativa
            GameObject globalTabTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            globalTabTxtObj.transform.SetParent(globalTabObj.transform, false);
            SetTextRect(globalTabTxtObj, "Global");

            // Aba Guilda
            GameObject guildTabObj = new GameObject("GuildTab", typeof(RectTransform), typeof(Image), typeof(Button));
            guildTabObj.transform.SetParent(tabsAreaObj.transform, false);
            guildTabObj.GetComponent<Image>().color = Color.black; // Aba inativa
            GameObject guildTabTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            guildTabTxtObj.transform.SetParent(guildTabObj.transform, false);
            SetTextRect(guildTabTxtObj, "Guilda");

            // 4. Área de Rolagem (ScrollView)
            GameObject scrollViewObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollViewObj.transform.SetParent(chatPanelObj.transform, false);
            RectTransform scrollRect = scrollViewObj.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0.1f);
            scrollRect.anchorMax = new Vector2(1, 0.9f); // Vai só até 0.9 agora por causa das abas
            scrollRect.offsetMin = new Vector2(10, 10);
            scrollRect.offsetMax = new Vector2(-10, -10);
            scrollViewObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewportObj.transform.SetParent(scrollViewObj.transform, false);
            RectTransform viewRect = viewportObj.GetComponent<RectTransform>();
            viewRect.anchorMin = Vector2.zero;
            viewRect.anchorMax = Vector2.one;
            viewRect.offsetMin = Vector2.zero;
            viewRect.offsetMax = Vector2.zero;
            viewportObj.GetComponent<Image>().color = Color.white;
            viewportObj.GetComponent<Mask>().showMaskGraphic = false;

            GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            var vlg = contentObj.GetComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 5;
            vlg.padding = new RectOffset(10, 10, 10, 10);

            var csf = contentObj.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect sr = scrollViewObj.GetComponent<ScrollRect>();
            sr.content = contentRect;
            sr.viewport = viewRect;
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 15f;

            // 5. Área de Input
            GameObject inputAreaObj = new GameObject("InputArea", typeof(RectTransform));
            inputAreaObj.transform.SetParent(chatPanelObj.transform, false);
            RectTransform inputAreaRect = inputAreaObj.GetComponent<RectTransform>();
            inputAreaRect.anchorMin = new Vector2(0, 0);
            inputAreaRect.anchorMax = new Vector2(1, 0.1f);
            inputAreaRect.offsetMin = new Vector2(10, 10);
            inputAreaRect.offsetMax = new Vector2(-10, -10);

            GameObject inputFieldObj = new GameObject("TMP_InputField", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            inputFieldObj.transform.SetParent(inputAreaObj.transform, false);
            RectTransform inputFieldRect = inputFieldObj.GetComponent<RectTransform>();
            inputFieldRect.anchorMin = new Vector2(0, 0);
            inputFieldRect.anchorMax = new Vector2(0.75f, 1);
            inputFieldRect.offsetMin = Vector2.zero;
            inputFieldRect.offsetMax = new Vector2(-5, 0);
            inputFieldObj.GetComponent<Image>().color = Color.white;

            GameObject textAreaObj = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            textAreaObj.transform.SetParent(inputFieldObj.transform, false);
            RectTransform textAreaRect = textAreaObj.GetComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 5);
            textAreaRect.offsetMax = new Vector2(-10, -5);

            GameObject textInputObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textInputObj.transform.SetParent(textAreaObj.transform, false);
            SetTextRect(textInputObj, "", Color.black, TextAlignmentOptions.MidlineLeft);
            
            TMP_InputField inputField = inputFieldObj.GetComponent<TMP_InputField>();
            inputField.textComponent = textInputObj.GetComponent<TextMeshProUGUI>();
            inputField.textViewport = textAreaRect;

            // Botão de Enviar
            GameObject sendBtnObj = new GameObject("SendButton", typeof(RectTransform), typeof(Image), typeof(Button));
            sendBtnObj.transform.SetParent(inputAreaObj.transform, false);
            RectTransform sendBtnRect = sendBtnObj.GetComponent<RectTransform>();
            sendBtnRect.anchorMin = new Vector2(0.75f, 0);
            sendBtnRect.anchorMax = new Vector2(1, 1);
            sendBtnRect.offsetMin = new Vector2(5, 0);
            sendBtnRect.offsetMax = Vector2.zero;
            sendBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f);

            GameObject sendTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            sendTxtObj.transform.SetParent(sendBtnObj.transform, false);
            SetTextRect(sendTxtObj, "Enviar");

            // 6. Botão de Fechar Chat
            GameObject closeBtnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(chatPanelObj.transform, false);
            RectTransform closeBtnRect = closeBtnObj.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(1, 1);
            closeBtnRect.anchorMax = new Vector2(1, 1);
            closeBtnRect.sizeDelta = new Vector2(40, 40);
            closeBtnRect.anchoredPosition = new Vector2(-25, -25);
            closeBtnObj.GetComponent<Image>().color = Color.red;
            
            GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
            SetTextRect(closeTxtObj, "X");

            // 7. Prefab da Mensagem de Texto (TMP)
            GameObject msgPrefabObj = new GameObject("MessagePrefab_TMP", typeof(RectTransform), typeof(TextMeshProUGUI));
            msgPrefabObj.transform.SetParent(chatPanelObj.transform, false);
            msgPrefabObj.SetActive(false); 
            TextMeshProUGUI msgText = msgPrefabObj.GetComponent<TextMeshProUGUI>();
            msgText.color = Color.white;
            msgText.fontSize = 22;
            msgText.enableWordWrapping = true;
            var csfMsg = msgPrefabObj.AddComponent<ContentSizeFitter>();
            csfMsg.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 8. Amarrar os Scripts
            ChatUIManager uiManager = chatPanelObj.GetComponent<ChatUIManager>();
            uiManager.globalTabBtn = globalTabObj.GetComponent<Button>();
            uiManager.guildTabBtn = guildTabObj.GetComponent<Button>();
            uiManager.inputField = inputField;
            uiManager.sendButton = sendBtnObj.GetComponent<Button>();
            uiManager.closeButton = closeBtnObj.GetComponent<Button>();
            uiManager.contentParent = contentRect;
            uiManager.messagePrefab = msgPrefabObj;

            FloatingChatButton floatingBtn = floatBtnObj.GetComponent<FloatingChatButton>();
            floatingBtn.chatPanelToToggle = chatPanelObj;

            Debug.Log("UI do Chat (com Canais e Histórico) gerada com sucesso!");
        }

        // Helper para alinhar textos
        private static void SetTextRect(GameObject obj, string textContent, Color? color = null, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI txt = obj.GetComponent<TextMeshProUGUI>();
            txt.text = textContent;
            txt.alignment = align;
            txt.color = color ?? Color.white;
            txt.fontSize = 24;
        }
    }
}
#endif
