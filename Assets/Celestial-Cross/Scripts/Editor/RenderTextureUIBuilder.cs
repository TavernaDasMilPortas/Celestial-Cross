using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class RenderTextureUIBuilder : EditorWindow
{
    [MenuItem("Celestial Cross/3. UI Builders/4. Utilities/Setup Render Texture UI")]
    public static void CreateRenderTextureUI()
    {
        // 1. Encontrar ou criar o Canvas Principal
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // 2. Criar a UI (Raw Image)
        GameObject rawImageGO = new GameObject("Game Render View");
        rawImageGO.transform.SetParent(canvas.transform, false);
        RawImage rawImage = rawImageGO.AddComponent<RawImage>();
        RectTransform rt = rawImage.GetComponent<RectTransform>();
        
        // Ajustar o RawImage para preencher a tela, mantendo a proporção (opcional: Preserve Aspect do Raw Image não existe nativo, então ajustamos âncoras ou deixamos esticado como base)
        // Vamos centralizar e deixar num tamanho razoável quadrado.
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(800, 800); // Quadrado
        
        // 3. Encontrar a Câmera Principal
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = Object.FindObjectOfType<Camera>();
        }
        if (mainCamera == null)
        {
            Debug.LogWarning("Nenhuma Câmera encontrada na cena! Setup incompleto.");
            return;
        }

        // 4. Criar e configurar o Manager
        RenderTextureInputManager manager = Object.FindObjectOfType<RenderTextureInputManager>();
        GameObject managerGO = manager != null ? manager.gameObject : new GameObject("RenderTexture Controller");
        
        if (manager == null)
            manager = managerGO.AddComponent<RenderTextureInputManager>();

        if (managerGO.GetComponent<RaycastHoverManager>() == null)
            managerGO.AddComponent<RaycastHoverManager>();

        manager.renderTargetUI = rawImage;
        manager.gameCamera = mainCamera;
        manager.createAndAssignTextureOnStart = true;
        manager.textureResolutionStr = 1024;

        // 5. Criar câmera de fundo e limpar AudioListener da principal
        if (mainCamera.GetComponent<AudioListener>())
        {
            Object.DestroyImmediate(mainCamera.GetComponent<AudioListener>());
        }

        Camera bgCam = null;
        foreach (Camera c in Camera.allCameras)
        {
            if (c.name == "Display Background Camera") bgCam = c;
        }

        if (bgCam == null)
        {
            GameObject bgCamGO = new GameObject("Display Background Camera");
            bgCam = bgCamGO.AddComponent<Camera>();
            bgCam.cullingMask = 0;
            bgCam.clearFlags = CameraClearFlags.SolidColor;
            bgCam.backgroundColor = Color.black; // Cor das bordas
            bgCam.depth = -1;
            bgCamGO.AddComponent<AudioListener>(); // O ouvido agora fica na câmera que o player "sente" as bordas
            Undo.RegisterCreatedObjectUndo(bgCamGO, "Create Background Camera");
        }

        // Registrar para o Undo (Ctrl+Z funcionar)
        Undo.RegisterCreatedObjectUndo(rawImageGO, "Create Game Render View");
        
        Debug.Log("UI de Render Texture criada e configurada com sucesso!");
        Selection.activeGameObject = rawImageGO;
    }
}
