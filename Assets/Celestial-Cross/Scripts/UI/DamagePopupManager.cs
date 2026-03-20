using UnityEngine;
using System.Collections.Generic;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 1.5f, 0);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(Health health)
    {
        if (health == null) return;
        
        health.OnDamageTaken += (amount, isCritical) => SpawnPopup(health.transform.position, amount, isCritical, false);
        health.OnHealed += (amount) => SpawnPopup(health.transform.position, amount, false, true);
    }

    public void Unregister(Health health)
    {
        // Nota: As assinaturas anônimas acima são difíceis de remover individualmente sem referências salvas.
        // No entanto, como o Health é destruído, os eventos morrem com ele. 
        // Em um sistema mais complexo, usaríamos métodos nomeados.
    }

    private void SpawnPopup(Vector3 position, int amount, bool isCritical, bool isHeal)
    {
        if (damageNumberPrefab == null)
        {
            Debug.LogWarning("[DamagePopupManager] Prefab não atribuído no Inspector!");
            return;
        }

        GameObject obj = Instantiate(damageNumberPrefab, position + spawnOffset, Quaternion.identity);
        DamageNumberUI ui = obj.GetComponent<DamageNumberUI>();
        
        if (ui != null)
        {
            if (isHeal)
            {
                ui.Setup(amount, Color.green, "+");
            }
            else
            {
                Color color = isCritical ? Color.yellow : Color.red;
                ui.Setup(amount, color, "-");
            }
        }
    }
}
