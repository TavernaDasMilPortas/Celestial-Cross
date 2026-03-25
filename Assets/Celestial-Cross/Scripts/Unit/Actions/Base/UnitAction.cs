using UnityEngine;

public abstract class UnitAction : MonoBehaviour
{
    protected Unit unit;

    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public abstract bool CanExecute();
    public abstract void Execute(Vector2Int target);
}
