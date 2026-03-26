using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI de inventário com 3 abas trocáveis (Poções, Armas, Suprimentos).
/// Cada aba possui um grid 6×6 de slots criados dinamicamente.
/// Suporta troca por toque nas abas ou swipe horizontal.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Abas")]
    [Tooltip("Arrastar as 3 InventoryTab (Poções, Armas, Suprimentos) na ordem")]
    public InventoryTab[] tabs;

    [Header("Containers dos Grids")]
    [Tooltip("Um RectTransform com GridLayoutGroup para cada aba, na mesma ordem das tabs")]
    public RectTransform[] gridContainers;

    [Header("Slot Prefab")]
    [Tooltip("Prefab de cada slot do inventário (uma Image simples)")]
    public GameObject slotPrefab;

    [Header("Grid Config")]
    public int columns = 6;
    public int rows    = 6;
    public Vector2 cellSize    = new Vector2(120f, 120f);
    public Vector2 cellSpacing = new Vector2(12f, 12f);

    [Header("Swipe")]
    [Tooltip("Referência ao SwipeDetector (pode estar no mesmo GameObject)")]
    public SwipeDetector swipeDetector;

    private int currentTabIndex = 0;

    // =============================
    // LIFECYCLE
    // =============================

    void Start()
    {
        InitializeGrids();
        InitializeTabs();
        RegisterSwipe();
        SwitchToTab(0);
    }

    void OnDestroy()
    {
        UnregisterSwipe();

        if (tabs != null)
        {
            foreach (var tab in tabs)
            {
                if (tab != null)
                    tab.OnTabClicked -= SwitchToTab;
            }
        }
    }

    // =============================
    // INICIALIZAÇÃO
    // =============================

    void InitializeGrids()
    {
        int totalSlots = columns * rows; // 36

        for (int i = 0; i < gridContainers.Length; i++)
        {
            // Configurar GridLayoutGroup
            GridLayoutGroup grid = gridContainers[i].GetComponent<GridLayoutGroup>();
            if (grid == null)
                grid = gridContainers[i].gameObject.AddComponent<GridLayoutGroup>();

            grid.cellSize         = cellSize;
            grid.spacing          = cellSpacing;
            grid.constraint       = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount  = columns;
            grid.childAlignment   = TextAnchor.UpperCenter;
            grid.padding          = new RectOffset(16, 16, 16, 16);

            // Criar slots
            for (int s = 0; s < totalSlots; s++)
            {
                GameObject slot;
                if (slotPrefab != null)
                {
                    slot = Instantiate(slotPrefab, gridContainers[i]);
                }
                else
                {
                    // Slot padrão (placeholder)
                    slot = new GameObject($"Slot_{s}", typeof(RectTransform), typeof(Image));
                    slot.transform.SetParent(gridContainers[i], false);

                    Image img = slot.GetComponent<Image>();
                    img.color = new Color(0.25f, 0.23f, 0.2f, 0.85f);
                }

                slot.name = $"Slot_{s}";
            }
        }
    }

    void InitializeTabs()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].tabIndex = i;
            tabs[i].OnTabClicked += SwitchToTab;
        }
    }

    void RegisterSwipe()
    {
        if (swipeDetector == null) return;
        swipeDetector.OnSwipeLeft  += OnSwipeLeft;
        swipeDetector.OnSwipeRight += OnSwipeRight;
    }

    void UnregisterSwipe()
    {
        if (swipeDetector == null) return;
        swipeDetector.OnSwipeLeft  -= OnSwipeLeft;
        swipeDetector.OnSwipeRight -= OnSwipeRight;
    }

    // =============================
    // TROCA DE ABAS
    // =============================

    public void SwitchToTab(int index)
    {
        if (index < 0 || index >= tabs.Length) return;

        currentTabIndex = index;

        // Ativar/desativar grids
        for (int i = 0; i < gridContainers.Length; i++)
            gridContainers[i].gameObject.SetActive(i == index);

        // Atualizar visual das abas
        for (int i = 0; i < tabs.Length; i++)
            tabs[i].SetActive(i == index);
    }

    // =============================
    // SWIPE HANDLERS
    // =============================

    void OnSwipeLeft()
    {
        // Swipe para esquerda → próxima aba
        int next = currentTabIndex + 1;
        if (next < tabs.Length)
            SwitchToTab(next);
    }

    void OnSwipeRight()
    {
        // Swipe para direita → aba anterior
        int prev = currentTabIndex - 1;
        if (prev >= 0)
            SwitchToTab(prev);
    }
}
