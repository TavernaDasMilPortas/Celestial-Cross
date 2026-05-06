using UnityEngine;
using TMPro;
using System.Collections;

public class ActionModalUI : MonoBehaviour
{
    public static ActionModalUI Instance 
    { 
        get 
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ActionModalUI>(true);
            }
            return _instance;
        }
    }
    private static ActionModalUI _instance;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject visualRoot;

    [Header("Animation Settings")]
    [SerializeField] private RectTransform animatedObject;
    [Tooltip("Opcional: Um objeto pai que contém todos os textos para ativá-los/desativá-los juntos.")]
    [SerializeField] private GameObject textContainer;
    [Tooltip("Opcional: Para fazer o efeito de fade no texto.")]
    [SerializeField] private CanvasGroup textCanvasGroup;
    [SerializeField] private Vector2 hiddenPosition;
    [SerializeField] private Vector2 visiblePosition;
    [SerializeField] private float animationDuration = 0.25f;

    private Coroutine currentAnim;

    private void Awake()
    {
        _instance = this;
        Debug.Log("[ActionModalUI] Instance inicializada com sucesso.");
        if (visualRoot != null) visualRoot.SetActive(false);
    }

    public void Show(IUnitAction action)
    {
        if (action == null) return;
        
        if (visualRoot == null)
        {
            Debug.LogError("[ActionModalUI] visualRoot não atribuído no Inspector!");
            return;
        }

        if (nameText != null) nameText.text = action.ActionName;
        if (statsText != null) statsText.text = action.GetDetailStats();
        if (descriptionText != null) descriptionText.text = action.ActionDescription;

        visualRoot.SetActive(true);
        SetTextsActive(false);

        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(AnimateShow());
    }

    public void Hide()
    {
        if (visualRoot == null || !visualRoot.activeSelf) return;

        if (currentAnim != null) StopCoroutine(currentAnim);
        
        if (gameObject.activeInHierarchy)
        {
            currentAnim = StartCoroutine(AnimateHide());
        }
        else
        {
            visualRoot.SetActive(false);
        }
    }

    private void SetTextsActive(bool state)
    {
        if (textContainer != null)
        {
            textContainer.SetActive(state);
        }
        else
        {
            if (nameText != null) nameText.gameObject.SetActive(state);
            if (statsText != null) statsText.gameObject.SetActive(state);
            if (descriptionText != null) descriptionText.gameObject.SetActive(state);
        }
    }

    private IEnumerator AnimateShow()
    {
        if (textCanvasGroup != null) textCanvasGroup.alpha = 0f;

        if (animatedObject != null)
        {
            float elapsed = 0f;
            animatedObject.anchoredPosition = hiddenPosition;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                // Ease out cubic
                t = 1f - Mathf.Pow(1f - t, 3f);
                animatedObject.anchoredPosition = Vector2.LerpUnclamped(hiddenPosition, visiblePosition, t);
                yield return null;
            }
            animatedObject.anchoredPosition = visiblePosition;
        }
        
        SetTextsActive(true);

        if (textCanvasGroup != null)
        {
            float elapsed = 0f;
            float fadeDuration = 0.2f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                textCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            textCanvasGroup.alpha = 1f;
        }
    }

    private IEnumerator AnimateHide()
    {
        if (textCanvasGroup != null)
        {
            float elapsed = 0f;
            float fadeDuration = 0.1f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                textCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            textCanvasGroup.alpha = 0f;
        }

        SetTextsActive(false);
        
        if (animatedObject != null)
        {
            float elapsed = 0f;
            Vector2 startPos = animatedObject.anchoredPosition;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                // Ease in cubic
                t = t * t * t;
                animatedObject.anchoredPosition = Vector2.LerpUnclamped(startPos, hiddenPosition, t);
                yield return null;
            }
            animatedObject.anchoredPosition = hiddenPosition;
        }
        
        visualRoot.SetActive(false);
    }
}
