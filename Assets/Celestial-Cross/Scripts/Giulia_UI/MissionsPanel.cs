using UnityEngine;

/// <summary>
/// Painel de missões — abre/fecha com animação de slide.
/// O conteúdo de missões será adicionado futuramente.
/// </summary>
public class MissionsPanel : MonoBehaviour
{
    [Header("Animação")]
    [Tooltip("Duração do slide-in/out em segundos")]
    public float slideDuration = 0.35f;

    private RectTransform rectTransform;
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;
    private Coroutine slideCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Posição visível = centro da tela
        visiblePosition = Vector2.zero;
        // Posição escondida = fora da tela pela direita
        hiddenPosition = new Vector2(Screen.width * 1.2f, 0f);

        rectTransform.anchoredPosition = hiddenPosition;
        gameObject.SetActive(false);
    }

    // =============================
    // PUBLIC API
    // =============================

    public void Open()
    {
        gameObject.SetActive(true);
        StopSlide();
        slideCoroutine = StartCoroutine(SlideTo(visiblePosition));
    }

    public void Close()
    {
        StopSlide();
        slideCoroutine = StartCoroutine(SlideToAndHide(hiddenPosition));
    }

    // =============================
    // ANIMAÇÃO
    // =============================

    void StopSlide()
    {
        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
            slideCoroutine = null;
        }
    }

    System.Collections.IEnumerator SlideTo(Vector2 target)
    {
        Vector2 start = rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        rectTransform.anchoredPosition = target;
        slideCoroutine = null;
    }

    System.Collections.IEnumerator SlideToAndHide(Vector2 target)
    {
        yield return SlideTo(target);
        gameObject.SetActive(false);
    }
}
