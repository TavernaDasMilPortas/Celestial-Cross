using UnityEngine;

[RequireComponent(typeof(Health))]
public class FloatingTextSpawner : MonoBehaviour
{
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color critColor = Color.red;

    private Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        if (health != null)
            health.OnDamageTaken += SpawnText;
    }

    void OnDisable()
    {
        if (health != null)
            health.OnDamageTaken -= SpawnText;
    }

    private void SpawnText(int amount, bool isCritical)
    {
        if (floatingTextPrefab == null) return;

        GameObject go = Instantiate(floatingTextPrefab, transform.position + offset, Quaternion.identity);
        FloatingText ft = go.GetComponent<FloatingText>();
        
        if (ft != null)
        {
            string text = isCritical ? $"-{amount} CRIT!" : $"-{amount}";
            ft.Setup(text, isCritical ? critColor : normalColor);
        }
    }
}
