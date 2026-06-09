using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class TurnTimelineUI : MonoBehaviour
{
    [Header("Timeline Setup")]
    [SerializeField] public GameObject portraitPrefab;
    [SerializeField] public Transform container;

    [Header("Drawer Settings (Gaveta)")]
    [Tooltip("O painel inteiro da Timeline que vai se mover")]
    [SerializeField] public RectTransform drawerPanel;
    [Tooltip("O botão usado para puxar ou empurrar a gaveta")]
    [SerializeField] public Button toggleDrawerButton;
    [Tooltip("Um RectTransform vazio (ou ponto) marcando onde é a posição ABERTA da timeline na tela")]
    [SerializeField] public RectTransform openPositionRef;
    [Tooltip("Um RectTransform vazio (ou ponto) marcando onde é a posição FECHADA da timeline")]
    [SerializeField] public RectTransform closedPositionRef;
    [SerializeField] public float slideSpeed = 15f;

    private bool isDrawerOpen = true;
    private List<GameObject> activePortraits = new();

    private void Awake()
    {
        TurnManager.OnRoundStarted += HandleRoundStarted;

        // Começa escondido
        if (drawerPanel != null)
        {
            drawerPanel.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        TurnManager.OnRoundStarted -= HandleRoundStarted;
    }

    private void Start()
    {
        if (toggleDrawerButton != null)
        {
            toggleDrawerButton.onClick.RemoveListener(ToggleDrawer);
            toggleDrawerButton.onClick.AddListener(ToggleDrawer);
        }
    }

    private void HandleRoundStarted(int round)
    {
        if (round == 1 && drawerPanel != null)
        {
            drawerPanel.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (drawerPanel != null && openPositionRef != null && closedPositionRef != null)
        {
            Vector3 targetPos = isDrawerOpen ? openPositionRef.position : closedPositionRef.position;
            drawerPanel.position = Vector3.Lerp(drawerPanel.position, targetPos, Time.deltaTime * slideSpeed);
        }
    }

    public void ToggleDrawer()
    {
        isDrawerOpen = !isDrawerOpen;
    }

    public void UpdateTimeline(IEnumerable<Unit> turnQueue)
    {
        ClearTimeline();

        if (turnQueue == null || portraitPrefab == null || container == null)
            return;

        int turnIndex = 1;
        foreach (var unit in turnQueue)
        {
            GameObject portraitObj = Instantiate(portraitPrefab, container);
            TurnPortraitUI pUI = portraitObj.GetComponent<TurnPortraitUI>();
            if (pUI != null) 
            {
                pUI.Setup(unit, turnIndex);
                turnIndex++;
            }
            
            activePortraits.Add(portraitObj);
        }
    }

    private void ClearTimeline()
    {
        foreach (var p in activePortraits)
            if (p != null) Destroy(p);
        activePortraits.Clear();
    }
}
