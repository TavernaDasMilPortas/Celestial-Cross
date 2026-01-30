using UnityEngine;

[RequireComponent(typeof(Outline))]
public class UnitOutlineController : MonoBehaviour
{
    [Header("Colors")]
    public Color hoverColor = Color.cyan;
    public Color selectedColor = Color.yellow;

    Outline outline;

    bool isHovered;
    bool isSelected;

    void Awake()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    // =========================
    // API PÚBLICA
    // =========================

    public void SetHover(bool value)
    {
        isHovered = value;
        Refresh();
    }

    public void SetSelected(bool value)
    {
        isSelected = value;
        Refresh();
    }

    public void ClearAll()
    {
        isHovered = false;
        isSelected = false;
        Refresh();
    }

    // =========================
    // LÓGICA DE PRIORIDADE
    // =========================

    void Refresh()
    {
        if (isSelected)
        {
            Apply(selectedColor);
        }
        else if (isHovered)
        {
            Apply(hoverColor);
        }
        else
        {
            outline.enabled = false;
        }
    }

    void Apply(Color color)
    {
        outline.enabled = true;
        outline.OutlineColor = color;
    }
}
