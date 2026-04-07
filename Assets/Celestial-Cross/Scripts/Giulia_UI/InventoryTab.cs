using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Componente de cada aba do inventário (Poções, Armas, Suprimentos).
/// Gerencia o estado visual ativo/inativo e dispara evento ao ser clicado.
/// </summary>
[RequireComponent(typeof(Button))]
public class InventoryTab : MonoBehaviour
{
    [Header("Referências")]
    public TextMeshProUGUI titleText;

    [Header("Cores")]
    public Color activeColor   = new Color(0.9f, 0.85f, 0.7f, 1f);
    public Color inactiveColor = new Color(0.4f, 0.38f, 0.35f, 1f);
    public Color activeTextColor   = Color.black;
    public Color inactiveTextColor = new Color(0.75f, 0.75f, 0.75f, 1f);

    /// <summary>
    /// Índice da aba (0 = Poções, 1 = Armas, 2 = Suprimentos).
    /// Atribuído pelo InventoryUI na inicialização.
    /// </summary>
    [HideInInspector] public int tabIndex;

    /// <summary>Disparado quando o jogador clica nesta aba.</summary>
    public event Action<int> OnTabClicked;

    private Button button;
    private Image background;

    void Awake()
    {
        button = GetComponent<Button>();
        background = GetComponent<Image>();
        button.onClick.AddListener(() => OnTabClicked?.Invoke(tabIndex));
    }

    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }

    // =============================
    // PUBLIC API
    // =============================

    public void SetActive(bool isActive)
    {
        if (background != null)
            background.color = isActive ? activeColor : inactiveColor;

        if (titleText != null)
            titleText.color = isActive ? activeTextColor : inactiveTextColor;
    }
}
