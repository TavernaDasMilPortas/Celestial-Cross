using UnityEngine;
using UnityEngine.UI;

using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Gerenciador principal da cena de descanso (Rest Scene).
/// Controla a abertura/fechamento dos painéis de Missões e Inventário.
/// Configura o Canvas para resolução 16:9 portrait (1080×1920).
/// </summary>
public class RestSceneManager : MonoBehaviour
{
    public static RestSceneManager Instance { get; private set; }

    [Header("Painéis")]
    [Tooltip("Referência ao painel de missões")]
    public MissionsPanel missionsPanel;

    [Tooltip("Referência ao painel de inventário")]
    public GameObject inventoryPanel;

    [Header("Canvas")]
    [Tooltip("Canvas principal da cena")]
    public Canvas mainCanvas;

    [Header("Background")]
    [Tooltip("Imagem de fundo (placeholder)")]
    public Image backgroundImage;

    [Tooltip("Cor do fundo placeholder")]
    public Color backgroundColor = new Color(0.12f, 0.10f, 0.15f, 1f);

    [Header("Transition")]
    [Tooltip("Nome da cena do Hub (Menu principal)")]
    public string hubSceneName = "HubScene";

    void Awake()
    {
        // Singleton simples
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ConfigureCanvas();
        ConfigureBackground();
    }

    void Start()
    {
        // Garantir que a cena abra com o inventário aberto e outras coisas ocultas
        if (missionsPanel != null)
            missionsPanel.gameObject.SetActive(false);

        if (inventoryPanel != null)
            inventoryPanel.SetActive(true);

        EnsureBackToHubButton();
    }

    private void EnsureBackToHubButton()
    {
        if (mainCanvas == null) return;

        var go = new GameObject("Btn_BackToHub", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(mainCanvas.transform, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -100); // Top Left corner
        rt.sizeDelta = new Vector2(180, 60);

        go.GetComponent<Image>().color = new Color(0.8f, 0.4f, 0.2f, 1f);
        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(GoToHubScene);

        var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TMP_Text));
        txtGo.transform.SetParent(go.transform, false);
        var txtRt = (RectTransform)txtGo.transform;
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;

        var tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "Voltar (Hub)";
        tmp.color = Color.white;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    public void GoToHubScene()
    {
        if (!string.IsNullOrEmpty(hubSceneName))
        {
            SceneManager.LoadScene(hubSceneName);
        }
    }

    // =============================
    // CONFIGURAÇÃO
    // =============================

    void ConfigureCanvas()
    {
        if (mainCanvas == null) return;

        CanvasScaler scaler = mainCanvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode            = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution    = new Vector2(1080f, 1920f); // 16:9 portrait
        scaler.screenMatchMode        = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight     = 0.5f;
    }

    void ConfigureBackground()
    {
        if (backgroundImage != null)
            backgroundImage.color = backgroundColor;
    }

    // =============================
    // PUBLIC API — MISSÕES
    // =============================

    public void OpenMissionsPanel()
    {
        if (missionsPanel != null)
            missionsPanel.Open();
    }

    public void CloseMissionsPanel()
    {
        if (missionsPanel != null)
            missionsPanel.Close();
    }

    // =============================
    // PUBLIC API — INVENTÁRIO
    // =============================

    public void OpenInventoryPanel()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(true);
    }

    public void CloseInventoryPanel()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    public void ToggleInventoryPanel()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }
}
