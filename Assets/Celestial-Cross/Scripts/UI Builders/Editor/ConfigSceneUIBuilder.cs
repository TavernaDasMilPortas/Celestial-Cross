using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.UI;

namespace CelestialCross.EditorScripts
{
    public class ConfigSceneUIBuilder : EditorWindow
    {
        [MenuItem("Celestial Cross/UI/Build Config Scene (Gerar do Zero)")]
        public static void BuildConfigSceneUI()
        {
            Debug.Log("[ConfigSceneUIBuilder] Construindo a cena de configuração do zero...");

            Canvas canvas = UIBuilderHelper.GetOrCreateMainCanvas();

            // Limpa o canvas (pois esta cena é gerada do zero)
            for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(canvas.transform.GetChild(i).gameObject);
            }

            // Fundo
            RectTransform bgPanel = UIBuilderHelper.CreatePanel(canvas.transform, "Background", new Color(0.08f, 0.08f, 0.12f, 1f));
            
            // Container Central (Perfil)
            RectTransform profileBox = UIBuilderHelper.CreatePanel(bgPanel, "ProfileBox", new Color(0.12f, 0.15f, 0.22f, 1f));
            profileBox.anchorMin = new Vector2(0.2f, 0.2f);
            profileBox.anchorMax = new Vector2(0.8f, 0.8f);
            profileBox.offsetMin = Vector2.zero;
            profileBox.offsetMax = Vector2.zero;

            // Título
            TextMeshProUGUI title = UIBuilderHelper.CreateText(profileBox, "Title", "PERFIL E CONFIGURAÇÕES", 60, Color.white, TextAlignmentOptions.Center);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 0.9f);
            titleRect.anchorMax = new Vector2(0.5f, 0.9f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(800, 100);

            // Ícone do Avatar (Placeholder)
            RectTransform avatarImg = UIBuilderHelper.CreatePanel(profileBox, "AvatarIcon", new Color(0.4f, 0.4f, 0.4f, 1f));
            avatarImg.anchorMin = new Vector2(0.1f, 0.5f);
            avatarImg.anchorMax = new Vector2(0.1f, 0.5f);
            avatarImg.anchoredPosition = new Vector2(150, 0); // Ajuste fino no eixo X/Y
            avatarImg.sizeDelta = new Vector2(250, 250);

            // Nome de Usuário
            TextMeshProUGUI username = UIBuilderHelper.CreateText(profileBox, "UsernameText", "Nome de Jogador", 50, Color.white, TextAlignmentOptions.Left);
            RectTransform userRect = username.rectTransform;
            userRect.anchorMin = new Vector2(0.35f, 0.65f);
            userRect.anchorMax = new Vector2(0.35f, 0.65f);
            userRect.anchoredPosition = new Vector2(300, 0);
            userRect.sizeDelta = new Vector2(600, 80);

            // UID
            TextMeshProUGUI uid = UIBuilderHelper.CreateText(profileBox, "UIDText", "UID: -------", 30, new Color(0.7f, 0.7f, 0.7f, 1f), TextAlignmentOptions.Left);
            RectTransform uidRect = uid.rectTransform;
            uidRect.anchorMin = new Vector2(0.35f, 0.55f);
            uidRect.anchorMax = new Vector2(0.35f, 0.55f);
            uidRect.anchoredPosition = new Vector2(300, 0);
            uidRect.sizeDelta = new Vector2(600, 50);

            // Botão Voltar
            Button backBtn = UIBuilderHelper.CreateButton(profileBox, "BtnBack", "VOLTAR", new Color(0.6f, 0.2f, 0.2f, 1f));
            RectTransform backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.05f, 0.05f);
            backRect.anchorMax = new Vector2(0.05f, 0.05f);
            backRect.anchoredPosition = new Vector2(150, 50);
            backRect.sizeDelta = new Vector2(300, 80);

            // Botão Mudar Nome (Dummy)
            Button changeNameBtn = UIBuilderHelper.CreateButton(profileBox, "BtnChangeName", "Alterar Nome", new Color(0.2f, 0.4f, 0.6f, 1f));
            RectTransform cnRect = changeNameBtn.GetComponent<RectTransform>();
            cnRect.anchorMin = new Vector2(0.35f, 0.4f);
            cnRect.anchorMax = new Vector2(0.35f, 0.4f);
            cnRect.anchoredPosition = new Vector2(150, 0);
            cnRect.sizeDelta = new Vector2(300, 60);

            // Anexa Manager
            ConfigSceneManager manager = bgPanel.gameObject.AddComponent<ConfigSceneManager>();
            manager.usernameText = username;
            manager.uidText = uid;
            manager.backButton = backBtn;

            Debug.Log("[ConfigSceneUIBuilder] Cena de configurações gerada do zero com sucesso!");
        }
    }
}
