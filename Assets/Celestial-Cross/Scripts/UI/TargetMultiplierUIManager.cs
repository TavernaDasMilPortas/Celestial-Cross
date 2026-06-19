using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace Celestial_Cross.Scripts.UI
{
    public class TargetMultiplierUIManager : MonoBehaviour
    {
        public static TargetMultiplierUIManager Instance { get; private set; }

        [Header("Prefabs & References")]
        [SerializeField] private GameObject multiplierTextPrefab; // Prefab com TargetMultiplierUI
        [SerializeField] private TextMeshProUGUI remainingTargetsText; // Texto UI principal na tela
        [Tooltip("O pai onde os multiplicadores serão instanciados. Se vazio, usará o Canvas principal.")]
        [SerializeField] private Transform multiplierParent; 
        [SerializeField] private Canvas mainScreenCanvas; // Onde os textos de multiplier (se screen space) ou o remaining vão

        [Header("Offsets")]
        [Tooltip("Deslocamento do texto '2x, 3x' em relação ao centro da unidade")]
        [SerializeField] private Vector3 unitOffset = new Vector3(0, 2f, 0);
        [Tooltip("Deslocamento do texto '2x, 3x' em relação ao centro do tile (chão)")]
        [SerializeField] private Vector3 tileOffset = new Vector3(0, 1f, 0);

        private List<TargetMultiplierUI> activeMultipliers = new List<TargetMultiplierUI>();
        private Queue<TargetMultiplierUI> multiplierPool = new Queue<TargetMultiplierUI>();

        private TargetSelector targetSelector;
        private TargetingRuleData currentRule;
        private int currentTargetCount = 0;
        private bool isSelecting = false;

        void Awake()
        {
            Instance = this;
            if (remainingTargetsText != null) remainingTargetsText.gameObject.SetActive(false);
        }

        void Start()
        {
            targetSelector = FindFirstObjectByType<TargetSelector>();
            if (targetSelector != null)
            {
                targetSelector.OnSelectedTargetsChanged += HandleSelectionChanged;
                targetSelector.OnCanceled += HandleSelectionEnded;
                targetSelector.OnTargetsConfirmed += HandleTargetsConfirmed;
            }
        }

        void OnDestroy()
        {
            if (targetSelector != null)
            {
                targetSelector.OnSelectedTargetsChanged -= HandleSelectionChanged;
                targetSelector.OnCanceled -= HandleSelectionEnded;
                targetSelector.OnTargetsConfirmed -= HandleTargetsConfirmed;
            }
        }

        public void RegisterTargetSelector(TargetSelector selector)
        {
            if (targetSelector != null)
            {
                targetSelector.OnSelectedTargetsChanged -= HandleSelectionChanged;
                targetSelector.OnCanceled -= HandleSelectionEnded;
                targetSelector.OnTargetsConfirmed -= HandleTargetsConfirmed;
            }
            targetSelector = selector;
            if (targetSelector != null)
            {
                targetSelector.OnSelectedTargetsChanged += HandleSelectionChanged;
                targetSelector.OnCanceled += HandleSelectionEnded;
                targetSelector.OnTargetsConfirmed += HandleTargetsConfirmed;
            }
        }

        public void BeginSelection(TargetingRuleData rule)
        {
            currentRule = rule;
            currentTargetCount = 0;
            ClearMultipliers(); // Limpa alvos anteriores ao clicar em nova habilidade
            
            // Ativa o texto de "restantes" apenas se tiver múltiplos alvos e a origem for Unit ou Point com repetição
            if (rule.maxTargets > 1)
            {
                isSelecting = true;
                UpdateRemainingText(0);
                if (remainingTargetsText != null) remainingTargetsText.gameObject.SetActive(true);
            }
        }

        private void HandleSelectionChanged(List<Unit> resolvedTargets)
        {
            if (!isSelecting || currentRule == null) return;

            // TargetSelector só expõe List<Unit> e os points. 
            // Para UI, precisamos agrupar para criar os multiplicadores X2, X3.
            // Para as Units:
            Dictionary<Unit, int> unitCounts = new Dictionary<Unit, int>();
            foreach (var u in resolvedTargets)
            {
                if (u == null) continue;
                if (!unitCounts.ContainsKey(u)) unitCounts[u] = 0;
                unitCounts[u]++;
            }

            // Precisamos do TargetSelector atual para ler selectedPoints ou targets brutos para contar corretamente o "Remaining"
            int currentSelections = 0;
            if (targetSelector != null)
            {
                // Se origin for Point, a seleção manual é baseada em Pontos (selectedPoints não está acessível publicamente direto além de SelectedPoints property)
                if (currentRule.origin == TargetOrigin.Point)
                {
                    currentSelections = targetSelector.SelectedPoints.Count;
                    
                    // Se for ponto, desenhamos o X2 no tile
                    Dictionary<Vector2Int, int> pointCounts = new Dictionary<Vector2Int, int>();
                    foreach (var p in targetSelector.SelectedPoints)
                    {
                        if (!pointCounts.ContainsKey(p)) pointCounts[p] = 0;
                        pointCounts[p]++;
                    }
                    UpdatePointMultipliers(pointCounts);
                }
                else
                {
                    // Como não expusemos selectedTargets públicos (apenas o evento manda o resolved),
                    // O resolvedTargets JÁ possui a duplicação se allowSameTargetMultipleTimes estiver on.
                    currentSelections = resolvedTargets.Count;
                    UpdateUnitMultipliers(unitCounts);
                }
            }

            currentTargetCount = currentSelections;
            UpdateRemainingText(currentSelections);
        }

        private void UpdateRemainingText(int currentCount)
        {
            if (remainingTargetsText != null && currentRule != null)
            {
                int remaining = Mathf.Max(0, currentRule.maxTargets - currentCount);
                remainingTargetsText.text = $"Alvos restantes: {remaining}";
            }
        }

        private void UpdateUnitMultipliers(Dictionary<Unit, int> counts)
        {
            ClearMultipliers();
            foreach (var kvp in counts)
            {
                if (kvp.Value > 1)
                {
                    var ui = GetMultiplierUI();
                    ui.Setup(kvp.Key.transform.position + unitOffset, kvp.Value);
                    activeMultipliers.Add(ui);
                }
            }
        }

        private void UpdatePointMultipliers(Dictionary<Vector2Int, int> counts)
        {
            ClearMultipliers();
            if (GridMap.Instance == null) return;

            foreach (var kvp in counts)
            {
                if (kvp.Value > 1)
                {
                    var tile = GridMap.Instance.GetTile(kvp.Key);
                    if (tile != null)
                    {
                        var ui = GetMultiplierUI();
                        ui.Setup(tile.transform.position + tileOffset, kvp.Value);
                        activeMultipliers.Add(ui);
                    }
                }
            }
        }

        private void HandleSelectionEnded()
        {
            EndSelection();
        }

        private void HandleTargetsConfirmed(List<Unit> targets)
        {
            EndSelection();
        }

        private void EndSelection()
        {
            isSelecting = false;
            currentRule = null;
            if (remainingTargetsText != null) remainingTargetsText.gameObject.SetActive(false);
            ClearMultipliers();
        }

        private void ClearMultipliers()
        {
            foreach (var ui in activeMultipliers)
            {
                ui.Hide();
                multiplierPool.Enqueue(ui);
            }
            activeMultipliers.Clear();
        }

        private TargetMultiplierUI GetMultiplierUI()
        {
            while (multiplierPool.Count > 0)
            {
                var ui = multiplierPool.Dequeue();
                if (ui != null && ui.gameObject != null)
                {
                    ui.gameObject.SetActive(true);
                    return ui;
                }
            }

            Transform parentTransform = multiplierParent != null ? multiplierParent : (mainScreenCanvas != null ? mainScreenCanvas.transform : transform);

            if (multiplierTextPrefab != null)
            {
                GameObject go = Instantiate(multiplierTextPrefab, parentTransform);
                go.SetActive(true); // Ensures Awake is called even if prefab was disabled
                
                // Previne conflitos se o usuário esqueceu o script no objeto filho (Texto) em vez da Raiz (Imagem de Fundo)
                var childUIs = go.GetComponentsInChildren<TargetMultiplierUI>();
                foreach (var oldUi in childUIs)
                {
                    if (oldUi.gameObject != go) Destroy(oldUi);
                }

                var ui = go.GetComponent<TargetMultiplierUI>();
                if (ui == null) ui = go.AddComponent<TargetMultiplierUI>();
                return ui;
            }

            // Fallback (se não setado no inspector, cria um gameObject simples vazio, só pra não dar nullref)
            GameObject emptyGo = new GameObject("MultiplierFallback");
            emptyGo.transform.SetParent(parentTransform, false);
            return emptyGo.AddComponent<TargetMultiplierUI>();
        }
    }
}
