using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using MoreMountains.Feedbacks;
using CelestialCross.Scenes.Unit;
using UnityEditor.SceneManagement;

namespace CelestialCross.UIBuilders.Editor
{
    public class UIBuilder_UnitSceneJuice : UnityEditor.Editor
    {
        [MenuItem("Celestial Cross/UI Builders/Updaters/Unit Scene Juice")]
        public static void UpdateUnitSceneJuice()
        {
            UnitSceneController controller = Object.FindAnyObjectByType<UnitSceneController>(FindObjectsInactive.Include);
            if (controller == null)
            {
                Debug.LogWarning("UnitSceneController não encontrado na cena atual.");
                return;
            }

            Undo.RecordObject(controller, "Update UnitSceneController Juice");

            // 1. Modals Overlay
            if (controller.modalOverlay == null)
            {
                // Criar Overlay como último filho do canvas principal, antes dos modais
                Canvas mainCanvas = Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
                if (mainCanvas != null)
                {
                    GameObject overlayGo = new GameObject("ModalOverlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
                    overlayGo.transform.SetParent(mainCanvas.transform, false);
                    overlayGo.transform.SetSiblingIndex(mainCanvas.transform.childCount - 1); // Colocar atrás dos modais
                    
                    RectTransform rt = overlayGo.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;

                    Image img = overlayGo.GetComponent<Image>();
                    img.color = new Color(0, 0, 0, 0.7f); // Preto translúcido
                    img.raycastTarget = true; // Bloqueia clicks atrás

                    CanvasGroup cg = overlayGo.GetComponent<CanvasGroup>();
                    cg.alpha = 0f;

                    overlayGo.SetActive(false);

                    controller.modalOverlay = img;
                    Undo.RegisterCreatedObjectUndo(overlayGo, "Add Modal Overlay");
                }
            }

            // 2. Feedbacks no Controller
            Transform feedbacksContainer = controller.transform.Find("JuiceFeedbacks");
            if (feedbacksContainer == null)
            {
                GameObject containerGo = new GameObject("JuiceFeedbacks");
                containerGo.transform.SetParent(controller.transform, false);
                feedbacksContainer = containerGo.transform;
                Undo.RegisterCreatedObjectUndo(containerGo, "Add Feedbacks Container");
            }

            CreatePlayer(feedbacksContainer, "PanelTransitionFeedback");
            CreatePlayer(feedbacksContainer, "ModalOpenFeedback");
            CreatePlayer(feedbacksContainer, "ModalCloseFeedback");

            // 3. CanvasGroups nos Detail Panels
            AddCanvasGroupIfMissing(controller.attributesDetailPanel);
            AddCanvasGroupIfMissing(controller.petDetailPanel);
            AddCanvasGroupIfMissing(controller.equipmentDetailPanel);
            AddCanvasGroupIfMissing(controller.constellationDetailPanel);
            AddCanvasGroupIfMissing(controller.abilitiesDetailPanel);

            // 4. Modais CanvasGroups
            var actionModal = Object.FindAnyObjectByType<ArtifactActionModal>(FindObjectsInactive.Include);
            if (actionModal) AddCanvasGroupIfMissing(actionModal.gameObject);

            var miniModal = Object.FindAnyObjectByType<ArtifactMiniInfoModal>(FindObjectsInactive.Include);
            if (miniModal) AddCanvasGroupIfMissing(miniModal.gameObject);

            var setBonusModal = Object.FindAnyObjectByType<ArtifactSetBonusModal>(FindObjectsInactive.Include);
            if (setBonusModal) AddCanvasGroupIfMissing(setBonusModal.gameObject);

            var constModal = Object.FindAnyObjectByType<ConstellationDetailsModal>(FindObjectsInactive.Include);
            if (constModal) AddCanvasGroupIfMissing(constModal.gameObject);

            var artSelModal = Object.FindAnyObjectByType<UnitArtifactSelectModal>(FindObjectsInactive.Include);
            if (artSelModal) AddCanvasGroupIfMissing(artSelModal.gameObject);

            var petSelModal = Object.FindAnyObjectByType<UnitPetSelectModal>(FindObjectsInactive.Include);
            if (petSelModal) AddCanvasGroupIfMissing(petSelModal.gameObject);

            // 5. Main Panel Feedbacks
            UnitMainPanel mainPanel = Object.FindAnyObjectByType<UnitMainPanel>(FindObjectsInactive.Include);
            if (mainPanel != null)
            {
                Undo.RecordObject(mainPanel, "Update UnitMainPanel Juice");
                Transform mainFeedbacks = mainPanel.transform.Find("JuiceFeedbacks");
                if (mainFeedbacks == null)
                {
                    GameObject containerGo = new GameObject("JuiceFeedbacks");
                    containerGo.transform.SetParent(mainPanel.transform, false);
                    mainFeedbacks = containerGo.transform;
                    Undo.RegisterCreatedObjectUndo(containerGo, "Add MainPanel Feedbacks");
                }

                CreatePlayer(mainFeedbacks, "TabSwitchFeedback");
                CreatePlayer(mainFeedbacks, "UnitLoadFeedback");

                EditorUtility.SetDirty(mainPanel);
            }

            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);

            Debug.Log("<color=green>Unit Scene Juice (Estrutura) construído com sucesso!</color>");
        }

        private static MMF_Player CreatePlayer(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name);
            if (existing == null)
            {
                go.transform.SetParent(parent, false);
                Undo.RegisterCreatedObjectUndo(go, $"Add {name}");
            }
            
            var player = go.GetComponent<MMF_Player>();
            if (player == null) player = go.AddComponent<MMF_Player>();
            return player;
        }

        private static void AddCanvasGroupIfMissing(GameObject obj)
        {
            if (obj == null) return;
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                Undo.AddComponent<CanvasGroup>(obj);
            }
        }
    }
}
