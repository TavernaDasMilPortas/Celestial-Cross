using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

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
    private List<GameObject> portraitPool = new();

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

        // Garante que a gaveta inicie na posição correta, evitando que ela "viaje" torta na primeira vez
        if (drawerPanel != null && openPositionRef != null && closedPositionRef != null)
        {
            drawerPanel.position = isDrawerOpen ? openPositionRef.position : closedPositionRef.position;
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
        // Movimento substituído por DOTween no ToggleDrawer
    }

    public void ToggleDrawer()
    {
        isDrawerOpen = !isDrawerOpen;
        if (drawerPanel != null && openPositionRef != null && closedPositionRef != null)
        {
            Vector3 targetPos = isDrawerOpen ? openPositionRef.position : closedPositionRef.position;
            drawerPanel.DOKill();
            // Acentuado o bounce usando DOMove para garantir o alinhamento mundial com o marcador
            drawerPanel.DOMove(targetPos, 0.5f).SetEase(Ease.OutBack, 2.5f);
        }
    }

    public void UpdateTimeline(IEnumerable<Unit> turnQueue)
    {
        ClearTimeline();

        if (turnQueue == null || portraitPrefab == null || container == null)
            return;

        int turnIndex = 1;

        // Garante que a unidade atual sempre seja mostrada primeiro, se ela existir.
        List<Unit> orderedQueue = new List<Unit>();
        if (TurnManager.Instance != null && TurnManager.Instance.CurrentUnit != null)
        {
            orderedQueue.Add(TurnManager.Instance.CurrentUnit);
        }

        // Adiciona as unidades da fila (exceto a atual, que já foi adicionada, caso esteja na fila por algum motivo)
        foreach (var unit in turnQueue)
        {
            if (TurnManager.Instance != null && unit == TurnManager.Instance.CurrentUnit)
                continue;
                
            orderedQueue.Add(unit);
        }

        foreach (var unit in orderedQueue)
        {
            GameObject portraitObj = GetPortraitFromPool();
            TurnPortraitUI pUI = portraitObj.GetComponent<TurnPortraitUI>();
            if (pUI != null) 
            {
                pUI.Setup(unit, turnIndex);
                turnIndex++;
            }
        }
    }

    private GameObject GetPortraitFromPool()
    {
        GameObject portraitObj;
        if (portraitPool.Count > 0)
        {
            portraitObj = portraitPool[0];
            portraitPool.RemoveAt(0);
            portraitObj.SetActive(true);
        }
        else
        {
            portraitObj = Instantiate(portraitPrefab, container);
        }
        activePortraits.Add(portraitObj);
        
        // Garante que a ordem na hierarquia está correta (o mais recente vai pro final)
        portraitObj.transform.SetAsLastSibling();
        
        return portraitObj;
    }

    private void ClearTimeline()
    {
        foreach (var p in activePortraits)
        {
            if (p != null)
            {
                p.SetActive(false);
                portraitPool.Add(p);
            }
        }
        activePortraits.Clear();
    }
}
