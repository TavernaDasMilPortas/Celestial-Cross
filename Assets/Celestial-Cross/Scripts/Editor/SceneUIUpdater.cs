#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using CelestialCross.System;
using CelestialCross.Progression;
using CelestialCross.Scenes.Hub;

namespace CelestialCross.EditorTools
{
    public class SceneUIUpdater : EditorWindow
    {
        [MenuItem("Celestial Cross/4. Tools/Update Scene UI for Progression")]
        public static void UpdateScene()
        {
            // 1. Ensure ProgressionService & StoryFlowController exist
            var flowManager = FindObjectOfType<GameFlowManager>();
            if (flowManager != null)
            {
                if (flowManager.GetComponent<ProgressionService>() == null)
                    flowManager.gameObject.AddComponent<ProgressionService>();

                if (flowManager.GetComponent<StoryFlowController>() == null)
                    flowManager.gameObject.AddComponent<StoryFlowController>();
                
                Debug.Log("Adicionado ProgressionService e StoryFlowController ao GameFlowManager.");
            }
            else
            {
                Debug.LogWarning("GameFlowManager não encontrado na cena ativa.");
            }

            // 2. InviteUnlockModal
            var inviteModal = FindObjectOfType<InviteUnlockModal>(true);
            if (inviteModal == null)
            {
                InviteUnlockModalBuilder.BuildUI();
                Debug.Log("InviteUnlockModal gerado.");
            }

            // 3. ChapterMenuUI Setup
            var hubController = FindObjectOfType<HubSceneController>(true);
            if (hubController != null)
            {
                var chapterMenu = FindObjectOfType<ChapterMenuUI>(true);
                if (chapterMenu == null)
                {
                    GameObject chapterObj = new GameObject("ChapterMenuUI");
                    chapterObj.transform.SetParent(hubController.transform, false);
                    chapterMenu = chapterObj.AddComponent<ChapterMenuUI>();
                    
                    // Creates a simple container
                    GameObject container = new GameObject("Container");
                    container.transform.SetParent(chapterObj.transform, false);
                    
                    GameObject btnObj = new GameObject("NodeButtonPrefab");
                    btnObj.transform.SetParent(chapterObj.transform, false);
                    btnObj.AddComponent<Image>();
                    btnObj.AddComponent<Button>();
                    
                    // O usuário terá que preencher as referencias serilizadas manualmente ou fazemos aproximações
                    Debug.Log("ChapterMenuUI instanciado. Preencha as referências no Inspector.");
                }
            }

            if (flowManager != null) EditorUtility.SetDirty(flowManager.gameObject);
            if (hubController != null) EditorUtility.SetDirty(hubController.gameObject);
            
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("Atualização de UI concluída.");
        }
    }
}
#endif
