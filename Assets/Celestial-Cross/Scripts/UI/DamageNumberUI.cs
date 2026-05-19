using UnityEngine;
using TMPro;
using System.Collections;

public class DamageNumberUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textMesh;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1f, 1, 1.2f);

    private bool isWorldSpace = true;

    public void Setup(int amount, Color color, string prefix, bool worldSpace = true)
    {
        this.isWorldSpace = worldSpace;
        if (!isWorldSpace) transform.localScale = Vector3.one; // Reseta escala minúscula do mundo para UI
        
        if (textMesh == null) textMesh = GetComponentInChildren<TMP_Text>();
        
        if (textMesh != null)
        {
            textMesh.text = $"{prefix}{amount}";
            textMesh.color = color;
        }

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 initialScale = transform.localScale;
        Color startColor = textMesh != null ? textMesh.color : Color.white;
        
        float currentMoveSpeed = isWorldSpace ? moveSpeed : moveSpeed * 100f; // Compensa metros vs pixels

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Movimento para cima
            transform.position = startPos + Vector3.up * (elapsed * currentMoveSpeed);

            // Escala (pop-in respeitando a escala inicial do prefab)
            float scaleValue = scaleCurve.Evaluate(t);
            transform.localScale = initialScale * scaleValue;

            // Fade out
            if (textMesh != null)
            {
                Color c = startColor;
                c.a = 1f - t;
                textMesh.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void LateUpdate()
    {
        // Garante que o texto sempre encare a câmera (Billboard) apenas em World Space
        if (isWorldSpace && Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }
    }
}
