using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;

namespace CelestialCross.UI.Skills
{
    public class PassiveListModal : MonoBehaviour
    {
        public GameObject modalRoot;
        
        [Header("Animações (DOTween & Feel)")]
        public RectTransform paperBackground; // O fundo que representa a folha de caderno
        public CanvasGroup modalCanvasGroup;
        public float animationSpeed = 1f;
        public MMF_Player openPaperFeedback;
        public MMF_Player closePaperFeedback;
        public MMF_Player stickerPopFeedback;

        private Sequence currentAnimSeq;

        [Header("Seção 1: Condições Temporárias")]
        public RectTransform conditionsGrid;
        public GameObject conditionIconPrefab;
        public PassiveDetailModal detailModal;
        
        [Header("Seção 2: Efeitos Ativos (Status)")]
        public RectTransform positiveModifiersContainer;
        public RectTransform negativeModifiersContainer;
        public GameObject modifierItemPrefab;
        
        [Header("Seção 3: Todas as Passivas Nativas")]
        public RectTransform allPassivesContainer;
        public GameObject passiveItemPrefab;
        
        public Button closeButton;

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        public void Open(global::Unit unit)
        {
            if (unit == null) return;
            if (modalRoot != null) modalRoot.SetActive(true);
            
            Populate(unit);
            PlayOpenAnimation();
        }

        public void Close()
        {
            if (detailModal != null) detailModal.Close();
            PlayCloseAnimation();
        }

        private void PlayCloseAnimation()
        {
            if (currentAnimSeq != null && currentAnimSeq.IsActive())
            {
                currentAnimSeq.Kill();
            }

            currentAnimSeq = DOTween.Sequence();
            currentAnimSeq.timeScale = animationSpeed;

            List<Transform> allStickers = GetAllGeneratedItems(); 
            float delayPops = 0f;
            
            // "Descola" os adesivos rapidamente, de trás para frente
            for (int i = allStickers.Count - 1; i >= 0; i--)
            {
                Transform sticker = allStickers[i];
                currentAnimSeq.Insert(delayPops, sticker.DOScale(0f, 0.15f).SetEase(Ease.InBack));
                delayPops += 0.02f; // Extremamente rápido pra não travar muito o fechamento
            }

            float paperStartDelay = delayPops;

            // A Folha "puxada" da mesa
            if (paperBackground != null)
            {
                currentAnimSeq.Insert(paperStartDelay, paperBackground.DOAnchorPos(new Vector2(0, -800), 0.3f).SetEase(Ease.InBack));
                currentAnimSeq.Insert(paperStartDelay, paperBackground.DORotate(new Vector3(0, 0, -15f), 0.3f).SetEase(Ease.InBack));
                currentAnimSeq.Insert(paperStartDelay, paperBackground.DOScale(0.8f, 0.3f).SetEase(Ease.InBack));
                
                if (closePaperFeedback != null)
                {
                    currentAnimSeq.InsertCallback(paperStartDelay, () => closePaperFeedback.PlayFeedbacks());
                }
            }

            // Fade out geral de sombra do fundo
            if (modalCanvasGroup != null)
            {
                currentAnimSeq.Insert(paperStartDelay + 0.1f, modalCanvasGroup.DOFade(0f, 0.2f));
            }

            // Finalmente desativa a tela ao fim de tudo
            currentAnimSeq.OnComplete(() => {
                if (modalRoot != null) modalRoot.SetActive(false);
            });
        }

        private void PlayOpenAnimation()
        {
            if (currentAnimSeq != null && currentAnimSeq.IsActive())
            {
                currentAnimSeq.Kill();
            }

            if (modalCanvasGroup != null) modalCanvasGroup.alpha = 0f;
            
            if (paperBackground != null)
            {
                // Joga a folha pra fora da tela, pra baixo e inclinada
                paperBackground.anchoredPosition = new Vector2(0, -800); 
                paperBackground.localRotation = Quaternion.Euler(0, 0, -15f);
                paperBackground.localScale = Vector3.one * 0.8f;
            }

            List<Transform> allStickers = GetAllGeneratedItems(); 
            foreach (var sticker in allStickers)
            {
                sticker.localScale = Vector3.zero; // Encolhe para o efeito de "Pop"
            }

            currentAnimSeq = DOTween.Sequence();
            currentAnimSeq.timeScale = animationSpeed;

            // a) Fade in rápido geral do Modal
            if (modalCanvasGroup != null) 
                currentAnimSeq.Join(modalCanvasGroup.DOFade(1f, 0.15f));

            // b) A Folha "Bate" na mesa
            if (paperBackground != null)
            {
                currentAnimSeq.Join(paperBackground.DOAnchorPos(Vector2.zero, 0.35f).SetEase(Ease.OutBack, 1.2f));
                currentAnimSeq.Join(paperBackground.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack));
                currentAnimSeq.Join(paperBackground.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
                
                // Feedback tátil/sonoro de folha sendo manipulada
                if (openPaperFeedback != null)
                {
                    currentAnimSeq.InsertCallback(0.15f, () => openPaperFeedback.PlayFeedbacks());
                }
            }

            // c) Pipocar os adesivos e post-its em cascata (Stagger)
            float delayPops = 0f;
            foreach (var sticker in allStickers)
            {
                currentAnimSeq.Insert(0.25f + delayPops, sticker.DOScale(1f, 0.3f).SetEase(Ease.OutBack, 1.5f));
                
                // Feedback sonoro curtinho (tip, tip, tip)
                if (stickerPopFeedback != null)
                {
                    currentAnimSeq.InsertCallback(0.25f + delayPops, () => stickerPopFeedback.PlayFeedbacks());
                }
                delayPops += 0.04f; // Pequeno intervalo para criar o efeito cascata
            }
        }

        private List<Transform> GetAllGeneratedItems()
        {
            List<Transform> items = new List<Transform>();
            if (conditionsGrid != null) foreach(Transform child in conditionsGrid) items.Add(child);
            if (positiveModifiersContainer != null) foreach(Transform child in positiveModifiersContainer) items.Add(child);
            if (negativeModifiersContainer != null) foreach(Transform child in negativeModifiersContainer) items.Add(child);
            if (allPassivesContainer != null) foreach(Transform child in allPassivesContainer) items.Add(child);
            return items;
        }

        private void Populate(global::Unit unit)
        {
            // Limpar containers de forma segura
            ClearContainer(conditionsGrid);
            ClearContainer(positiveModifiersContainer);
            ClearContainer(negativeModifiersContainer);
            ClearContainer(allPassivesContainer);

            if (unit == null || unit.PassiveManager == null) return;

            // --- SEÇÃO 1: Condições Temporárias (Grid de Ícones) ---
            var allConditions = unit.PassiveManager.GetActiveConditionsInfo();
            if (allConditions != null && conditionsGrid != null && conditionIconPrefab != null)
            {
                foreach (var c in allConditions)
                {
                    if (c.isPersistent) continue; // Pular permanentes/passivas estáticas nesta seção

                    var go = Instantiate(conditionIconPrefab, conditionsGrid);
                    go.SetActive(true);

                    // Configurar imagem do botão (ícone da condição)
                    var btn = go.GetComponent<Button>();
                    
                    // Primeiro tenta encontrar o objeto filho chamado "Icon" (para não pegar a borda)
                    var imgTransform = go.transform.Find("Icon");
                    var img = imgTransform != null ? imgTransform.GetComponent<Image>() : go.GetComponent<Image>();
                    
                    if (img != null)
                    {
                        img.sprite = c.icon;
                    }

                    // Exibir turnos restantes no texto (ex: "3t")
                    var turnTxt = go.GetComponentInChildren<TextMeshProUGUI>();
                    if (turnTxt != null)
                    {
                        turnTxt.text = $"{c.remainingTurns}t";
                    }

                    // Ação de clique para abrir o submodal de detalhes
                    if (btn != null)
                    {
                        var capturedCond = c;
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => {
                            if (detailModal != null)
                            {
                                string durStr = $"{capturedCond.remainingTurns} Turnos Restantes" + (capturedCond.stacks > 1 ? $" (x{capturedCond.stacks} Acúmulos)" : "");
                                detailModal.Open(capturedCond.icon, capturedCond.name, durStr, capturedCond.description);
                            }
                        });
                    }
                }
            }

            // --- SEÇÃO 2: Modificadores de Status Ativos (Positivos e Negativos) ---
            var activeModifiers = unit.PassiveManager.GetActiveStatModifiers();
            if (activeModifiers != null && modifierItemPrefab != null)
            {
                foreach (var mod in activeModifiers)
                {
                    var container = mod.isPositive ? positiveModifiersContainer : negativeModifiersContainer;
                    if (container != null)
                    {
                        var go = Instantiate(modifierItemPrefab, container);
                        go.SetActive(true);

                        // Icone do Modificador
                        var img = go.transform.Find("Icon")?.GetComponent<Image>();
                        if (img != null)
                        {
                            img.sprite = mod.icon;
                            img.gameObject.SetActive(mod.icon != null);
                        }

                        // Texto do Modificador
                        var txt = go.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                        if (txt != null)
                        {
                            string colorTag = mod.isPositive ? "#4f4" : "#f44";
                            txt.text = $"<b><color={colorTag}>{mod.statText}</color></b> (<size=90%>{mod.remaining}</size>)";
                        }
                    }
                }
            }

            // --- SEÇÃO 3: Todas as Passivas Nativas ---
            var staticPassives = unit.PassiveManager.GetStaticPassives();
            if (staticPassives != null && allPassivesContainer != null && passiveItemPrefab != null)
            {
                foreach (var sp in staticPassives)
                {
                    var go = Instantiate(passiveItemPrefab, allPassivesContainer);
                    go.SetActive(true);

                    // Icone da Passiva
                    var img = go.transform.Find("Icon")?.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = sp.icon;
                        img.gameObject.SetActive(sp.icon != null);
                    }

                    // Texto e Descrição da Passiva
                    var txt = go.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                    if (txt != null)
                    {
                        string sourceStr = $"<color=#8bf>[{sp.source}]</color>";
                        string desc = string.IsNullOrEmpty(sp.description) ? "Sem descrição disponível." : sp.description;
                        txt.text = $"<b>{sp.name}</b> {sourceStr}\n<size=85%>{desc}</size>";
                    }
                }
            }
        }

        private void ClearContainer(RectTransform container)
        {
            if (container == null) return;
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
