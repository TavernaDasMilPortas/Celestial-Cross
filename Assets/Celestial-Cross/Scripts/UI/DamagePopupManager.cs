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

    private List<GameObject> activePopups = new();

    public bool HasActivePopups
    {
        get
        {
            activePopups.RemoveAll(p => p == null);
            return activePopups.Count > 0;
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
            if (health != null) SpawnPopup(health.transform.position, amount, isCritical, false);
        };

        health.OnHealed += (amount) => {
            if (health != null) SpawnPopup(health.transform.position, amount, false, true);
        };
    }

    private void SpawnPopup(Vector3 position, int amount, bool isCritical, bool isHeal)
    {
        if (damageNumberPrefab == null)
            return;
        if (amount <= 0) return;

        Vector3 randomJitter = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.2f, 0.2f), 0);
        GameObject obj = null;
        bool isUI = false;
        Vector3 spawnPosition = Vector3.zero;

        // 1. Tenta converter usando o RenderTextureInputManager (caso esteja ativo)
        if (RenderTextureInputManager.Instance != null && RenderTextureInputManager.Instance.WorldToCanvasWorldPoint(position + spawnOffset, out Vector3 canvasWorldPos))
        {
            spawnPosition = canvasWorldPos;
            isUI = true;
        }
        // 2. Se não estiver ativo, tenta projetar usando a Câmera Principal diretamente no Canvas da UI
        else if (Camera.main != null && canvasParent != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(position + spawnOffset);
            if (screenPos.z >= 0)
            {
                RectTransform parentRect = canvasParent.GetComponent<RectTransform>();
                Canvas canvas = canvasParent.GetComponentInParent<Canvas>();
                Camera uiCamera = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
                
                if (parentRect != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, screenPos, uiCamera, out Vector3 worldPoint))
                {
                    spawnPosition = worldPoint;
                    isUI = true;
                }
            }
        }

        if (isUI)
        {
            Transform parent = canvasParent != null ? canvasParent : transform;
            obj = Instantiate(damageNumberPrefab, parent);
            
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.position = spawnPosition;
                rect.anchoredPosition += (Vector2)randomJitter * 100f; // Jitter no espaço da UI
            }
            else
            {
                obj.transform.position = spawnPosition;
            }
            
            obj.transform.localScale = uiScale;
            obj.layer = LayerMask.NameToLayer("UI");
        }
        else
        {
            // Fallback absoluto
            Transform parent = canvasParent != null ? canvasParent : transform;
            obj = Instantiate(damageNumberPrefab, parent);
            obj.transform.localScale = uiScale;
        }

        if (obj != null)
        {
            activePopups.Add(obj);
        }

        DamageNumberUI ui = obj.GetComponent<DamageNumberUI>();
        if (ui != null)
        {
            Color color = isHeal ? Color.green : (isCritical ? Color.yellow : Color.red);
            string prefix = isHeal ? "+" : "-";
            ui.Setup(amount, color, prefix, false); // Sempre falso, pois o prefab agora é sempre elemento UI do Canvas
        }
    }
}
