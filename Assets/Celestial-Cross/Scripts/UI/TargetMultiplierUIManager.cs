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
        [SerializeField] private Canvas mainScreenCanvas; // Onde os textos de multiplier (se screen space) ou o remaining vão

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
                    ui.Setup(kvp.Key.transform.position + Vector3.up * 2f, kvp.Value); // Offset acima da unidade
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
                        ui.Setup(tile.transform.position + Vector3.up * 1f, kvp.Value); // Offset acima do tile
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
            if (multiplierPool.Count > 0)
            {
                var ui = multiplierPool.Dequeue();
                ui.gameObject.SetActive(true);
                return ui;
            }

            if (multiplierTextPrefab != null && mainScreenCanvas != null)
            {
                GameObject go = Instantiate(multiplierTextPrefab, mainScreenCanvas.transform);
                var ui = go.GetComponent<TargetMultiplierUI>();
                if (ui == null) ui = go.AddComponent<TargetMultiplierUI>();
                return ui;
            }

            // Fallback (se não setado no inspector, cria um gameObject simples vazio, só pra não dar nullref)
            GameObject emptyGo = new GameObject("MultiplierFallback");
            return emptyGo.AddComponent<TargetMultiplierUI>();
        }
    }
}
