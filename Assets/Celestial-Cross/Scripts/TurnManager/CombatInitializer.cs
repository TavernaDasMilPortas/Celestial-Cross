using UnityEngine;
using System.Collections.Generic;

public class CombatInitializer : MonoBehaviour
{
    [Header("Combat Setup")]
    [Tooltip("Inicia o combate automaticamente no Start")]
    public bool autoStart = true;

    [Tooltip("Units que participar�o do combate (opcional)")]
    public List<Unit> predefinedUnits = new();

    void Start()
    {
        if (autoStart)
            StartCombat();
    }

    // =============================
    // PUBLIC API
    // =============================

    public void StartCombat()
    {
        List<Unit> units = CollectUnits();

        if (units.Count == 0)
        {
            Debug.LogWarning("[CombatInitializer] Nenhuma Unit encontrada para iniciar combate.");
            return;
        }

        Debug.Log($"[CombatInitializer] Iniciando combate com {units.Count} units.");

        TurnManager.Instance.StartCombat(units);
    }

    // =============================
    // INTERNAL
    // =============================

    List<Unit> CollectUnits()
    {
        // Se houver units pr�-definidas, use elas
        if (predefinedUnits != null && predefinedUnits.Count > 0)
        {
            // remove nulls por seguran�a
            predefinedUnits.RemoveAll(u => u == null);
            return new List<Unit>(predefinedUnits);
        }

        // Caso contr�rio, encontra todas as Units na cena
        return new List<Unit>(Object.FindObjectsByType<Unit>(FindObjectsSortMode.None));
    }
}
