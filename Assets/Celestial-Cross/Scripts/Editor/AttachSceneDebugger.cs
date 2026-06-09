#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CelestialCross.Scenes.Hub;

namespace CelestialCross.Editor
{
    public static class AttachSceneDebugger
    {
        private const string HubScenePath = "Assets/Celestial-Cross/Scenes/HubScene.unity";

        [MenuItem("Celestial Cross/5. Debug/Attach Scene Debugger to Hub")]
        public static void AttachDebugger()
        {
            // Salva cena aberta atualmente
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            // Abre a HubScene
            var scene = EditorSceneManager.OpenScene(HubScenePath, OpenSceneMode.Single);

            // Acha o HubSceneController ou cria um objeto Managers
            HubSceneController hubCtrl = Object.FindAnyObjectByType<HubSceneController>();
            GameObject targetGO = null;

            if (hubCtrl != null)
            {
                targetGO = hubCtrl.gameObject;
                Debug.Log($"[AttachSceneDebugger] Encontrou HubSceneController em: {targetGO.name}");
            }
            else
            {
                targetGO = GameObject.Find("Managers");
                if (targetGO == null)
                {
                    targetGO = new GameObject("Managers");
                }
                Debug.Log($"[AttachSceneDebugger] HubSceneController não encontrado. Usando/Criando GameObject: {targetGO.name}");
            }

            // Adiciona SceneDebugger se não existir
            if (targetGO.GetComponent<SceneDebugger>() == null)
            {
                targetGO.AddComponent<SceneDebugger>();
                Debug.Log("[AttachSceneDebugger] Adicionado componente SceneDebugger.");
            }
            else
            {
                Debug.Log("[AttachSceneDebugger] SceneDebugger já estava presente.");
            }

            // Marca a cena como modificada e salva
            EditorUtility.SetDirty(targetGO);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[AttachSceneDebugger] HubScene salva com o SceneDebugger!");
        }
    }
}
#endif
