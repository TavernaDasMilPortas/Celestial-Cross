using UnityEngine;
using System.Collections.Generic;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Transform canvasParent;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private Vector3 uiScale = Vector3.one;

    private Queue<GameObject> popupPool = new Queue<GameObject>();
    private int activePopupCount = 0;

    public bool HasActivePopups => activePopupCount > 0;

    public void ReturnToPool(GameObject popup)
    {
        if (popup != null)
        {
            popup.SetActive(false);
            popupPool.Enqueue(popup);
            activePopupCount--;
            if (activePopupCount < 0) activePopupCount = 0;
        }
    }

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

        // Nota: Como estamos usando lambdas que capturam 'health', a desinscrição simples 
        // por referência de método não funciona aqui. No entanto, para o fluxo atual de batalha,
        // isso garante que o popup sempre use a posição atualizada da unidade.
        
        health.OnDamageTaken += (amount, isCritical) => {
            if (health != null) SpawnPopup(health.transform, amount, isCritical, false);
        };

        health.OnHealed += (amount) => {
            if (health != null) SpawnPopup(health.transform, amount, false, true);
        };
    }

    private void SpawnPopup(Transform target, int amount, bool isCritical, bool isHeal)
    {
        if (damageNumberPrefab == null)
            return;
        if (amount <= 0) return;
        if (target == null) return;

        Vector3 randomJitter = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.2f, 0.2f), 0);
        GameObject obj = null;

        if (popupPool.Count > 0)
        {
            obj = popupPool.Dequeue();
            obj.SetActive(true);
            obj.transform.SetAsLastSibling();
        }
        else
        {
            Transform parent = canvasParent != null ? canvasParent : transform;
            obj = Instantiate(damageNumberPrefab, parent);
            if (obj != null)
            {
                obj.transform.localScale = uiScale;
                obj.layer = LayerMask.NameToLayer("UI");
            }
        }

        if (obj != null)
        {
            activePopupCount++;
        }

        DamageNumberUI ui = obj.GetComponent<DamageNumberUI>();
        if (ui != null)
        {
            Color color = isHeal ? Color.green : (isCritical ? Color.yellow : Color.red);
            string prefix = isHeal ? "+" : "-";
            ui.Setup(target, spawnOffset, randomJitter, amount, color, prefix);
        }
    }
}
