using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CelestialCross.Grid
{
    [ExecuteAlways]
    public class HighlightOverlayPool : MonoBehaviour
    {
        public static HighlightOverlayPool Instance { get; private set; }

        [SerializeField] private GameObject overlayPrefab;
        [SerializeField] private int initialSize = 50;

        private Stack<GameObject> pool = new Stack<GameObject>();
        [SerializeField, ReadOnly] private List<GameObject> activeOverlays = new List<GameObject>();

        void Awake() => Initialize();
        void OnEnable() => Initialize();

        private void Initialize()
        {
            Instance = this;
            
            // Garante que o prefab ou o fallback exista mesmo no Editor
            if (overlayPrefab == null)
            {
                Transform existing = transform.Find("HighlightOverlay_Prefab");
                if (existing != null)
                {
                    overlayPrefab = existing.gameObject;
                }
                else
                {
                    overlayPrefab = new GameObject("HighlightOverlay_Prefab");
                    overlayPrefab.AddComponent<SpriteRenderer>();
                    overlayPrefab.transform.rotation = Quaternion.Euler(90, 0, 0);
                    overlayPrefab.SetActive(false);
                    overlayPrefab.transform.SetParent(transform);
                }
            }
        }

        [Button(ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
        public void ShowIndexGuide()
        {
            Initialize(); // Força a instância no Editor
            Clear();
            
            var grid = GridMap.Instance;
            if (grid == null || grid.defaultHighlightConfig == null)
            {
                Debug.LogWarning("[HighlightPool] GridMap ou Config não configurado para o guia.");
                return;
            }

            for (int i = 0; i < 16; i++)
            {
                Vector3 pos = new Vector3(i * (grid.tileSize + 0.2f), 0, 0);
                string label = GetMaskLabel(i);
                
                Sprite s = grid.defaultHighlightConfig.GetSprite(i);
                var obj = Get(pos, s, grid.defaultHighlightConfig.GetColor(HighlightType.Special));
                if (obj != null) obj.name = $"{label} [{i}]";
            }
        }

        private string GetMaskLabel(int mask)
        {
            List<string> parts = new List<string>();
            if ((mask & 1) != 0) parts.Add("C");
            if ((mask & 2) != 0) parts.Add("D");
            if ((mask & 4) != 0) parts.Add("B");
            if ((mask & 8) != 0) parts.Add("E");
            return parts.Count == 0 ? "VAZIO" : string.Join("_", parts);
        }

        public void Clear()
        {
            Initialize();

            // Limpa nulos que podem ter ficado após domain reload
            activeOverlays.RemoveAll(item => item == null);

            foreach (var obj in activeOverlays)
            {
                obj.SetActive(false);
                pool.Push(obj);
            }
            activeOverlays.Clear();
        }

        public GameObject Get(Vector3 worldPos, Sprite sprite, Color color)
        {
            Initialize();

            if (overlayPrefab == null) return null;

            GameObject obj = (pool.Count > 0) ? pool.Pop() : null;

            if (obj == null)
            {
                obj = Instantiate(overlayPrefab, transform);
            }
            
            obj.transform.position = worldPos + new Vector3(0, 0.05f, 0); 
            obj.SetActive(true);
            
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = sprite;
                sr.color = color;
            }
            
            activeOverlays.Add(obj);
            return obj;
        }
    }
}
