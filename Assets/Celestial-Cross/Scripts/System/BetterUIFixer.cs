using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace CelestialCross.System
{
    public class BetterUIFixer : MonoBehaviour
    {
        public static BetterUIFixer Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Subscreve ao evento de carregamento de cena para fazer a correção automática
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Aguarda alguns frames para que o Canvas termine o layout e o BetterUI inicialize,
            // e só então aplica a correção, garantindo que rode liso na primeira vez.
            StartCoroutine(CoRefreshDelayedGlobal(0.2f));
        }

        private global::System.Collections.IEnumerator CoRefreshDelayedGlobal(float delay)
        {
            yield return new WaitForSeconds(delay);
            RefreshAllCanvases();
        }

        public void RefreshAllCanvases()
        {
            var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                if (canvas != null && canvas.gameObject.activeInHierarchy)
                {
                    RefreshImages(canvas.gameObject);
                }
            }
            Debug.Log($"[BetterUIFixer] Refresh global automático executado em {canvases.Length} canvas(es).");
        }

        public void RefreshImages(GameObject root)
        {
            if (root == null) return;

            var images = root.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img == null) continue;

                if (img.GetType().Name.Contains("BetterImage"))
                {
                    // Apenas informa à engine que o componente precisa ser redesenhado na GPU
                    img.SetAllDirty();
                    img.SetMaterialDirty();
                }
            }

            // Força rebuild dos layouts
            var layouts = root.GetComponentsInChildren<LayoutGroup>(true);
            foreach (var layout in layouts)
            {
                if (layout != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
                }
            }
        }

        /// <summary>
        /// Agenda um refresh com delay para uso em callbacks de UI (ex: ao abrir modal, trocar tab).
        /// </summary>
        public void RefreshImagesDelayed(GameObject root, float delay = 0.1f)
        {
            if (root == null) return;
            StartCoroutine(CoRefreshDelayed(root, delay));
        }

        private global::System.Collections.IEnumerator CoRefreshDelayed(GameObject root, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (root != null)
            {
                RefreshImages(root);
            }
        }
    }
}
