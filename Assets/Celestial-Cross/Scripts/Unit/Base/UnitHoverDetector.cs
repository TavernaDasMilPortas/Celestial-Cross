using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UnitHoverDetector : MonoBehaviour
{
    public static event System.Action<Unit> OnHoverStarted;
    public static event System.Action<Unit> OnHoverEnded;

    Unit unit;
    UnitOutlineController outline;

    void Awake()
    {
        unit = GetComponent<Unit>();
        outline = GetComponent<UnitOutlineController>();
    }

    void OnMouseEnter()
    {
        outline?.SetHover(true);
        OnHoverStarted?.Invoke(unit);
    }

    void OnMouseExit()
    {
        outline?.SetHover(false);
        OnHoverEnded?.Invoke(unit);
    }
}
