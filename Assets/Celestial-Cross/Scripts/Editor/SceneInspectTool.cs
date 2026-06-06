#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using CelestialCross.Scenes.Inventory;

namespace CelestialCross.Editor
{
    public static class SceneInspectTool
    {
        private const string ScenePath = "Assets/Celestial-Cross/Scenes/InventoryScene.unity";

        [MenuItem("Celestial Cross/5. Debug/Inspect Inventory Scene")]
        public static void InspectScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            bool wasOpen = activeScene.path == ScenePath;

            if (!wasOpen)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                activeScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\n=== [SceneInspectTool] INSPEÇÃO DE CENA: {ScenePath} ===");

            var rootGOs = activeScene.GetRootGameObjects();
            sb.AppendLine($"Total de Root GameObjects: {rootGOs.Length}");

            foreach (var go in rootGOs)
            {
                InspectGameObject(go, "", sb);
            }

            sb.AppendLine("==================================================");
            Debug.Log(sb.ToString());
        }

        private static void InspectGameObject(GameObject go, string indent, StringBuilder sb)
        {
            var rt = go.GetComponent<RectTransform>();
            string transformInfo = "";
            if (rt != null)
            {
                transformInfo = $"RectTransform[pos={rt.anchoredPosition}, size={rt.sizeDelta}, scale={rt.localScale}, anchorMin={rt.anchorMin}, anchorMax={rt.anchorMax}]";
            }
            else
            {
                transformInfo = $"Transform[pos={go.transform.localPosition}, scale={go.transform.localScale}]";
            }

            var components = go.GetComponents<Component>();
            var compNames = new List<string>();
            foreach (var c in components)
            {
                if (c == null) compNames.Add("Script Faltando (Missing)");
                else compNames.Add(c.GetType().Name);
            }

            sb.AppendLine($"{indent}- {go.name} [Ativo: {go.activeSelf}] - {transformInfo} - Componentes: {string.Join(", ", compNames)}");

            foreach (Transform child in go.transform)
            {
                InspectGameObject(child.gameObject, indent + "  ", sb);
            }
        }
    }
}
#endif
