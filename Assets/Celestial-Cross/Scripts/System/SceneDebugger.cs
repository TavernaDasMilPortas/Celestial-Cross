using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using CelestialCross.Scenes.Inventory;

public class SceneDebugger : MonoBehaviour
{
    private static SceneDebugger _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("[SceneDebugger] Inicializado e inscrito em sceneLoaded.");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"\n=== [SceneDebugger] CENA CARREGADA: {scene.name} (Modo: {mode}) ===");

        // Singleton Status
        sb.AppendLine($"AccountManager.Instance: {(AccountManager.Instance != null ? "Existe" : "Nulo")}");
        if (AccountManager.Instance != null)
        {
            sb.AppendLine($"- PlayerAccount: {(AccountManager.Instance.PlayerAccount != null ? "Carregada" : "Nula")}");
            if (AccountManager.Instance.PlayerAccount != null)
            {
                sb.AppendLine($"- Pets Count: {AccountManager.Instance.PlayerAccount.OwnedRuntimePets?.Count ?? 0}");
                sb.AppendLine($"- Artifacts Count: {AccountManager.Instance.PlayerAccount.OwnedArtifacts?.Count ?? 0}");
            }
        }

        sb.AppendLine($"InventorySceneController.Instance: {(InventorySceneController.Instance != null ? "Existe" : "Nulo")}");

        // Root GameObjects
        sb.AppendLine("Root GameObjects na cena:");
        var rootGOs = scene.GetRootGameObjects();
        foreach (var go in rootGOs)
        {
            sb.AppendLine($"- {go.name} (Ativo: {go.activeSelf}, Componentes: {GetComponentListString(go)})");
            if (go.name.Contains("Canvas"))
            {
                DumpHierarchy(go, "  ", sb);
            }
        }

        sb.AppendLine("==================================================\n");
        Debug.Log(sb.ToString());
    }

    private void DumpHierarchy(GameObject go, string indent, StringBuilder sb)
    {
        sb.AppendLine($"{indent}* {go.name} (Ativo: {go.activeSelf}/{go.activeInHierarchy}, Componentes: {GetComponentListString(go)})");
        foreach (Transform child in go.transform)
        {
            DumpHierarchy(child.gameObject, indent + "  ", sb);
        }
    }

    private string GetComponentListString(GameObject go)
    {
        var components = go.GetComponents<Component>();
        var names = new System.Collections.Generic.List<string>();
        foreach (var c in components)
        {
            if (c == null)
            {
                names.Add("Script Faltando (Missing)");
            }
            else
            {
                names.Add(c.GetType().Name);
            }
        }
        return string.Join(", ", names);
    }
}
