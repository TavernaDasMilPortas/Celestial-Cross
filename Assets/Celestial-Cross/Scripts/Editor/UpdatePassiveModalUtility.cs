using UnityEngine;
using UnityEditor;
using CelestialCross.UI.Skills;
using MoreMountains.Feedbacks;

namespace CelestialCross.EditorScripts
{
    public class UpdatePassiveModalUtility
    {
        [MenuItem("Celestial Cross/UI/Atualizar Passive List Modal (Persona 5)")]
        public static void UpdateModalInScene()
        {
            // 1. Encontra o modal na cena aberta
            PassiveListModal modal = Object.FindAnyObjectByType<PassiveListModal>(FindObjectsInactive.Include);
            
            if (modal == null)
            {
                Debug.LogWarning("Nenhum 'PassiveListModal' encontrado na cena atual. Abra a cena que contém o Canvas de Combate/UI.");
                return;
            }

            Undo.RecordObject(modal, "Atualizar Passive List Modal");

            // 2. Configurar CanvasGroup
            if (modal.modalCanvasGroup == null)
            {
                GameObject root = modal.modalRoot != null ? modal.modalRoot : modal.gameObject;
                CanvasGroup cg = root.GetComponent<CanvasGroup>();
                if (cg == null)
                {
                    cg = root.AddComponent<CanvasGroup>();
                    Undo.RegisterCreatedObjectUndo(cg, "Add CanvasGroup");
                }
                modal.modalCanvasGroup = cg;
            }

            // 3. Tentar descobrir qual é o fundo da folha (paperBackground)
            if (modal.paperBackground == null)
            {
                // Procuramos por nomes comuns que você possa ter dado à imagem de fundo
                Transform bg = modal.transform.Find("Background") ?? 
                               modal.transform.Find("Panel") ?? 
                               modal.transform.Find("Paper");

                // Se não achar por nome, tenta pegar o primeiro filho válido da raiz que não seja ele mesmo
                if (bg == null && modal.modalRoot != null && modal.modalRoot.transform.childCount > 0)
                {
                    bg = modal.modalRoot.transform.GetChild(0);
                }

                if (bg != null)
                {
                    modal.paperBackground = bg.GetComponent<RectTransform>();
                }
                else
                {
                    Debug.LogWarning("Não conseguimos identificar automaticamente o 'paperBackground'. Arraste o fundo do painel manualmente no Inspector.");
                }
            }

            // 4. Configurar os Feedbacks (Feel)
            Transform feedbacksContainer = modal.transform.Find("AnimFeedbacks");
            if (feedbacksContainer == null)
            {
                GameObject containerGo = new GameObject("AnimFeedbacks");
                containerGo.transform.SetParent(modal.transform, false);
                feedbacksContainer = containerGo.transform;
                Undo.RegisterCreatedObjectUndo(containerGo, "Add Feedbacks Container");
            }

            if (modal.openPaperFeedback == null)
            {
                Transform openFbTransform = feedbacksContainer.Find("OpenPaperFeedback");
                GameObject openFbGo = openFbTransform != null ? openFbTransform.gameObject : new GameObject("OpenPaperFeedback");
                if (openFbTransform == null)
                {
                    openFbGo.transform.SetParent(feedbacksContainer, false);
                    Undo.RegisterCreatedObjectUndo(openFbGo, "Add OpenPaperFeedback");
                }
                
                var player = openFbGo.GetComponent<MMF_Player>();
                if (player == null) player = openFbGo.AddComponent<MMF_Player>();
                modal.openPaperFeedback = player;
            }

            if (modal.closePaperFeedback == null)
            {
                Transform closeFbTransform = feedbacksContainer.Find("ClosePaperFeedback");
                GameObject closeFbGo = closeFbTransform != null ? closeFbTransform.gameObject : new GameObject("ClosePaperFeedback");
                if (closeFbTransform == null)
                {
                    closeFbGo.transform.SetParent(feedbacksContainer, false);
                    Undo.RegisterCreatedObjectUndo(closeFbGo, "Add ClosePaperFeedback");
                }
                
                var player = closeFbGo.GetComponent<MMF_Player>();
                if (player == null) player = closeFbGo.AddComponent<MMF_Player>();
                modal.closePaperFeedback = player;
            }

            if (modal.stickerPopFeedback == null)
            {
                Transform stickerFbTransform = feedbacksContainer.Find("StickerPopFeedback");
                GameObject stickerFbGo = stickerFbTransform != null ? stickerFbTransform.gameObject : new GameObject("StickerPopFeedback");
                if (stickerFbTransform == null)
                {
                    stickerFbGo.transform.SetParent(feedbacksContainer, false);
                    Undo.RegisterCreatedObjectUndo(stickerFbGo, "Add StickerPopFeedback");
                }
                
                var player = stickerFbGo.GetComponent<MMF_Player>();
                if (player == null) player = stickerFbGo.AddComponent<MMF_Player>();
                modal.stickerPopFeedback = player;
            }

            EditorUtility.SetDirty(modal);
            Debug.Log("<color=green>PassiveListModal atualizado com sucesso!</color> Verifique o objeto no Inspector.");
        }
    }
}
