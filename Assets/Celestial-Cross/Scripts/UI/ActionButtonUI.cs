using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class ActionButtonUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectionImage;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Selection Visuals")]
    [SerializeField] private Image starImage;
    [SerializeField] private Color selectedStarColor = new Color(1f, 0.4f, 0.7f, 1f); // Rosa
    [SerializeField] private Color unselectedStarColor = new Color(1f, 1f, 1f, 1f); // Branco ou Transparente

    public IUnitAction Action => action;
    private IUnitAction action;
    private int actionIndex;
    private bool isClickable = true;

    private float holdTime = 0.15f;
    private float timer;
    private bool isHolding;
    private bool modalShown;
    
    private Tween shakeTween;

    public void SetupForPlacement(UnitData unitData, System.Action onClickCallback)
    {
        this.action = null; // Not an action button in this context
        this.isClickable = true;

        if (iconImage != null && unitData != null && unitData.icon != null)
        {
            iconImage.sprite = unitData.icon;
        }

        if (selectionImage != null) selectionImage.gameObject.SetActive(false);

        if (nameText != null && unitData != null)
        {
            nameText.text = unitData.displayName;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickCallback?.Invoke());
            button.interactable = true;
        }
    }

    public void Setup(IUnitAction action, int index, bool clickable = true)
    {
        this.action = action;
        this.actionIndex = index;
        this.isClickable = clickable;
        
        if (iconImage != null && action.ActionIcon != null)
        {
            iconImage.sprite = action.ActionIcon;
        }

        if (nameText != null && action != null)
        {
            nameText.text = action.ActionName;
        }

        if (selectionImage != null) selectionImage.gameObject.SetActive(false);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
            button.interactable = isClickable; // Disable button component for non-clickable

            // Injeta EventTrigger para garantir detecção do Hold (contornando o Button)
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();
            
            trigger.triggers.Clear();
            
            AddTrigger(trigger, EventTriggerType.PointerDown, (data) => OnPointerDown((PointerEventData)data));
            AddTrigger(trigger, EventTriggerType.PointerUp, (data) => OnPointerUp((PointerEventData)data));
            AddTrigger(trigger, EventTriggerType.PointerExit, (data) => OnPointerExit((PointerEventData)data));
        }

        // Garante que o botão comece sem chacoalhar
        SetSelected(false);
    }

    void AddTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    public void SetSelected(bool selected)
    {
        // selectionImage.gameObject.SetActive(selected); // Desativado conforme pedido

        if (starImage != null)
        {
            starImage.color = selected ? selectedStarColor : unselectedStarColor;
        }
        
        if (shakeTween != null)
        {
            shakeTween.Kill();
            shakeTween = null;
        }

        if (selected)
        {
            // Chacoalho MUITO suave e lento (looping) no botão inteiro
            transform.localRotation = Quaternion.Euler(0, 0, -2f);
            shakeTween = transform.DOLocalRotate(new Vector3(0, 0, 2f), 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
        
        // Se a ação não pode ser mais usada (já agiu), também para de chacoalhar
        if (!interactable)
        {
            SetSelected(false);
        }
    }

    public void SetConfirmedVisual(bool confirmed)
    {
        if (iconImage != null)
        {
            iconImage.color = confirmed ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (action == null) return; // Disable hold for placement buttons
        isHolding = true;
        timer = 0f;
        modalShown = false;
        Debug.Log($"[ActionButtonUI] Segurando: {action?.ActionName}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelHold();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelHold();
    }

    private void CancelHold()
    {
        if (isHolding)
        {
            Debug.Log($"[ActionButtonUI] Soltou/Saiu de {action?.ActionName}");
        }
        
        isHolding = false;
        if (modalShown)
        {
            ActionModalUI.Instance?.Hide();
            modalShown = false;
        }
    }

    private void Update()
    {
        if (isHolding && !modalShown)
        {
            timer += Time.deltaTime;
            if (timer >= holdTime)
            {
                modalShown = true;
                if (ActionModalUI.Instance == null)
                {
                    Debug.LogError("[ActionButtonUI] ActionModalUI.Instance está NULO! O modal existe na cena?");
                }
                else
                {
                    Debug.Log($"[ActionButtonUI] DISPARANDO MODAL: {action?.ActionName}");
                    ActionModalUI.Instance.Show(action);
                }
            }
        }
    }

    private void OnClick()
    {
        if (modalShown || !isClickable) return;

        CelestialCross.Tutorial.TutorialManager.Instance?.NotifyActionSelected(actionIndex);
        PlayerController.Instance.SelectAction(actionIndex);
    }
}
