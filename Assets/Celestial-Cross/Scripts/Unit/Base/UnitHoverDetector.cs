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

    public void ManualMouseEnter()
    {
        outline?.SetHover(true);
        ForceHover(unit);
    }

    public void ManualMouseExit()
    {
        outline?.SetHover(false);
        ForceHoverEnd(unit);
    }

    public static void ForceHover(Unit u)
    {
        OnHoverStarted?.Invoke(u);
    }

    public static void ForceHoverEnd(Unit u)
    {
        OnHoverEnded?.Invoke(u);
    }
}
