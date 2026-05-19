using UnityEngine;
using System.Collections.Generic;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Transform canvasParent;
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

        // Tenta converter posição do mundo para posição de tela se estivermos usando RawImage (RenderTexture)
        if (RenderTextureInputManager.Instance != null && RenderTextureInputManager.Instance.WorldToScreenPoint(position + spawnOffset, out Vector2 screenPos))
        {
            Transform parent = canvasParent != null ? canvasParent : transform;
            obj = Instantiate(damageNumberPrefab, parent);
            
            // Configura posição na UI
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.position = new Vector3(screenPos.x, screenPos.y, 0);
                rect.anchoredPosition += (Vector2)randomJitter * 50f; // Jitter no espaço da UI
            }
            else
            {
                obj.transform.position = new Vector3(screenPos.x, screenPos.y, 0);
            }
            
            obj.layer = LayerMask.NameToLayer("UI");
            isUI = true;
        }
        else
        {
            // Fallback para World Space (comportamento padrão)
            Transform parent = canvasParent != null ? canvasParent : transform;
            obj = Instantiate(damageNumberPrefab, position + spawnOffset + randomJitter, Quaternion.identity, parent);
        }

        DamageNumberUI ui = obj.GetComponent<DamageNumberUI>();
        if (ui != null)
        {
            Color color = isHeal ? Color.green : (isCritical ? Color.yellow : Color.red);
            string prefix = isHeal ? "+" : "-";
            ui.Setup(amount, color, prefix, !isUI);
        }
    }
}
