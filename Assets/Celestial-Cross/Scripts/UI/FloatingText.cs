using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private float duration = 1f;
    [SerializeField] private Vector3 moveSpeed = new Vector3(0, 1, 0);
    [SerializeField] private AnimationCurve alphaCurve;

    private float timer;
    private Color startColor;

    public void Setup(string text, Color color)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            startColor = color;
        }
        timer = 0;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / duration;

        transform.position += moveSpeed * Time.deltaTime;

        if (textMesh != null)
        {
            float alpha = alphaCurve != null ? alphaCurve.Evaluate(progress) : 1f - progress;
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }

        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
