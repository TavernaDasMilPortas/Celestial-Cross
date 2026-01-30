using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UnitHoverDetector : MonoBehaviour
{
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
        UnitInfoUI.Instance.Show(unit);
    }

    void OnMouseExit()
    {
        outline?.SetHover(false);
        UnitInfoUI.Instance.Hide(unit);
    }
}
