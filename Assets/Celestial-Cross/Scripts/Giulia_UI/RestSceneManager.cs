using UnityEngine;
using UnityEngine.UI;

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
        // Garantir que painéis comecem fechados
        if (missionsPanel != null)
            missionsPanel.gameObject.SetActive(false);

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
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
