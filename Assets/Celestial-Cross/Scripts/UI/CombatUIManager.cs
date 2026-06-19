using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using CelestialCross.UI;

public class CombatUIManager : MonoBehaviour
{
    public static CombatUIManager Instance { get; private set; }

    [SerializeField] private SplitScreenUIManager splitScreenUI;
    [SerializeField] private ActionBarUI actionBarUI;
    [SerializeField] private CombatForecastUI combatForecastUI;
    [SerializeField] private TurnTimelineUI turnTimelineUI;

    [Header("Turn Display")]
    [SerializeField] private RectTransform turnDisplayContainer;
    [SerializeField] private TextMeshProUGUI currentTurnUnitNameText;
    [SerializeField] private Image turnBackgroundImage;
    [SerializeField] private Color allyColor = Color.cyan;
    [SerializeField] private Color enemyColor = Color.red;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Vector2 exitOffset = new Vector2(0f, 50f);

    private CanvasGroup displayCanvasGroup;
    private SoftShadow backgroundShadow;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (turnDisplayContainer != null)
        {
            displayCanvasGroup = turnDisplayContainer.GetComponent<CanvasGroup>();
            if (displayCanvasGroup == null)
                displayCanvasGroup = turnDisplayContainer.gameObject.AddComponent<CanvasGroup>();
        }

        if (turnBackgroundImage != null)
            backgroundShadow = turnBackgroundImage.GetComponent<SoftShadow>();
    }

    private TargetSelector targetSelector;

    private void Start()
    {
        TurnManager.OnTurnStarted += HandleTurnStarted;
        TurnManager.OnTurnEnded += HandleTurnEnded;
        TurnManager.OnQueueChanged += HandleQueueChanged;

        targetSelector = FindFirstObjectByType<TargetSelector>(FindObjectsInactive.Include);
        if (targetSelector != null)
        {
            targetSelector.OnSelectedTargetsChanged += HandleSelectedTargetsChanged;
            targetSelector.OnCanceled += HandleTargetingCanceled;
        }

        // Sync initial state if combat already started
        if (TurnManager.Instance != null && TurnManager.Instance.CurrentUnit != null)
        {
            HandleQueueChanged(TurnManager.Instance.GetTurnQueue());
            HandleTurnStarted(TurnManager.Instance.CurrentUnit);
        }
    }

    private Unit currentActiveUnit;

    private void OnDestroy()
    {
        TurnManager.OnTurnStarted -= HandleTurnStarted;
        TurnManager.OnTurnEnded -= HandleTurnEnded;
        TurnManager.OnQueueChanged -= HandleQueueChanged;
        
        if (targetSelector != null)
        {
            targetSelector.OnSelectedTargetsChanged -= HandleSelectedTargetsChanged;
            targetSelector.OnCanceled -= HandleTargetingCanceled;
        }

        if (currentActiveUnit != null)
            currentActiveUnit.OnActionChanged -= HandleActionChanged;
    }

    private void HandleSelectedTargetsChanged(List<Unit> targets)
    {
        Debug.Log($"[CombatUIManager] HandleSelectedTargetsChanged called. Targets count: {(targets != null ? targets.Count : 0)}");
        
        if (splitScreenUI != null && currentActiveUnit != null && targets != null)
        {
            List<Unit> oppositeFactionTargets = new List<Unit>();
            foreach(var t in targets)
            {
                if (t != null && t.Team != currentActiveUnit.Team)
                    oppositeFactionTargets.Add(t);
            }

            Debug.Log($"[CombatUIManager] Opposite faction targets: {oppositeFactionTargets.Count}");

            if (oppositeFactionTargets.Count > 0)
            {
                splitScreenUI.ShowSplit(currentActiveUnit, oppositeFactionTargets);
            }
            else
            {
                splitScreenUI.ShowFullScreenTurn(currentActiveUnit);
            }
        }
        else
        {
            Debug.LogWarning($"[CombatUIManager] Failed to handle targets. SplitScreenUI: {splitScreenUI != null}, ActiveUnit: {currentActiveUnit != null}");
        }
    }

    private void HandleTargetingCanceled()
    {
        if (splitScreenUI != null && currentActiveUnit != null)
        {
            splitScreenUI.ShowFullScreenTurn(currentActiveUnit);
        }
    }

    private void HandleTurnEnded()
    {
        if (actionBarUI != null) actionBarUI.ClearButtons();
    }

    private void HandleTurnStarted(Unit unit)
    {
        if (currentActiveUnit != null)
            currentActiveUnit.OnActionChanged -= HandleActionChanged;

        currentActiveUnit = unit;
        
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);
        
        transitionCoroutine = StartCoroutine(TransitionTurnUI(unit));

        if (currentActiveUnit != null)
            currentActiveUnit.OnActionChanged += HandleActionChanged;

        // Update player UI if it's player's turn
        bool isPlayerTurn = unit is Pet || unit.CompareTag("Player");
        
        if (isPlayerTurn)
        {
            if (splitScreenUI != null) splitScreenUI.ShowFullScreenTurn(unit);
            if (actionBarUI != null) actionBarUI.GenerateButtons(unit);
        }
        else
        {
            if (splitScreenUI != null) splitScreenUI.ShowFullScreenTurn(unit);
            if (actionBarUI != null) actionBarUI.ClearButtons();
            if (combatForecastUI != null) combatForecastUI.Hide();
        }
    }

    private IEnumerator TransitionTurnUI(Unit unit)
    {
        if (turnDisplayContainer == null) yield break;

        Vector2 originalPos = turnDisplayContainer.anchoredPosition;
        Vector2 targetExitPos = originalPos + exitOffset; // Alvo para cima

        // --- SAIDA (Sobe como se tirasse o adesivo) ---
        float elapsed = 0;
        while (elapsed < transitionDuration / 2f)
        {
            float t = elapsed / (transitionDuration / 2f);
            float curve = Mathf.SmoothStep(0, 1, t);
            
            turnDisplayContainer.anchoredPosition = Vector2.Lerp(originalPos, targetExitPos, curve);
            if (displayCanvasGroup != null) displayCanvasGroup.alpha = 1f - curve;
            
            // Sombra aumenta para parecer que está descolando
            if (backgroundShadow != null)
                backgroundShadow.effectDistance = new Vector2(5f + curve * 15f, -5f - curve * 15f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- TROCA DE DADOS (Enquanto está invisível no topo) ---
        if (currentTurnUnitNameText != null)
        {
            currentTurnUnitNameText.text = unit != null ? unit.DisplayName : "";
            if (unit != null)
            {
                bool isAlly = unit.Team == Team.Player;
                currentTurnUnitNameText.color = isAlly ? allyColor : enemyColor;
            }
        }

        // --- ENTRADA (Vem de cima para baixo como se colasse um novo) ---
        elapsed = 0;
        while (elapsed < transitionDuration / 2f)
        {
            float t = elapsed / (transitionDuration / 2f);
            float curve = Mathf.SmoothStep(0, 1, t);
            
            // Agora interpolamos do TOPO (targetExitPos) de volta para o ORIGINAL (originalPos)
            turnDisplayContainer.anchoredPosition = Vector2.Lerp(targetExitPos, originalPos, curve);
            if (displayCanvasGroup != null) displayCanvasGroup.alpha = curve;
            
            // Sombra diminui para parecer que está sendo colado de volta
            if (backgroundShadow != null)
                backgroundShadow.effectDistance = new Vector2(20f - curve * 15f, -20f + curve * 15f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        turnDisplayContainer.anchoredPosition = originalPos;
        if (displayCanvasGroup != null) displayCanvasGroup.alpha = 1f;
        if (backgroundShadow != null)
            backgroundShadow.effectDistance = new Vector2(5f, -5f);
    }

    public void RegisterTargetSelector(TargetSelector selector)
    {
        if (selector == null) return;
        
        if (targetSelector != null)
        {
            targetSelector.OnSelectedTargetsChanged -= HandleSelectedTargetsChanged;
            targetSelector.OnCanceled -= HandleTargetingCanceled;
        }

        targetSelector = selector;
        targetSelector.OnSelectedTargetsChanged += HandleSelectedTargetsChanged;
        targetSelector.OnCanceled += HandleTargetingCanceled;
        
        Debug.Log($"[CombatUIManager] Novo TargetSelector registrado com sucesso!");
    }

    private void HandleActionChanged(IUnitAction newAction)
    {
        if (newAction == null)
        {
            if (splitScreenUI != null && currentActiveUnit != null)
            {
                splitScreenUI.ShowFullScreenTurn(currentActiveUnit);
            }
        }

        if (combatForecastUI != null)
            combatForecastUI.SetAction(newAction);
    }

    private void HandleQueueChanged(IEnumerable<Unit> queue)
    {
        if (turnTimelineUI != null)
            turnTimelineUI.UpdateTimeline(queue);
    }
}
// refresh
