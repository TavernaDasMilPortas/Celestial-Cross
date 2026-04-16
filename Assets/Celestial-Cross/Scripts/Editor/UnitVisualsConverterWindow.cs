#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class UnitVisualsConverterWindow : EditorWindow
{
    private GameObject sourcePrefab;
    private string saveFolder = "Assets/Celestial-Cross/Prefabs/Units/2D";

    [MenuItem("Celestial Cross/Unit 2D Converter")]
    public static void ShowWindow()
    {
        GetWindow<UnitVisualsConverterWindow>("Unit 2D Converter");
    }

    void OnGUI()
    {
        GUILayout.Label("Convert 3D Unit to 2D Animated Unit", EditorStyles.boldLabel);
        sourcePrefab = (GameObject)EditorGUILayout.ObjectField("Original 3D Prefab", sourcePrefab, typeof(GameObject), false);
        saveFolder = EditorGUILayout.TextField("Pasta de Destino", saveFolder);

        if (GUILayout.Button("Converter para 2D"))
        {
            ConvertPrefab();
        }
    }

    void ConvertPrefab()
    {
        if (sourcePrefab == null)
        {
            Debug.LogError("Selecione um prefab 3D original.");
            return;
        }

        if (!System.IO.Directory.Exists(saveFolder))
            System.IO.Directory.CreateDirectory(saveFolder);

        // Clona e quebra ligação
        GameObject clone = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);
        PrefabUtility.UnpackPrefabInstance(clone, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        clone.name = sourcePrefab.name + "_2D";

        // Remove filhos visuais antigos (Spheres, etc)
        var meshRenderers = clone.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in meshRenderers)
        {
            if (mr == null) continue;

            if (mr.gameObject != clone)
            {
                DestroyImmediate(mr.gameObject);
            }
            else
            {
                DestroyImmediate(mr);
                var filter = clone.GetComponent<MeshFilter>();
                if (filter != null) DestroyImmediate(filter);
            }
        }

        // Adiciona Game Object "Visuals"
        Transform existingVis = clone.transform.Find("Visuals");
        if (existingVis != null) DestroyImmediate(existingVis.gameObject);

        GameObject visualsGO = new GameObject("Visuals");
        visualsGO.transform.SetParent(clone.transform);
        visualsGO.transform.localPosition = new Vector3(0, 0.5f, 0);

        var sr = visualsGO.AddComponent<SpriteRenderer>();

        var anim = visualsGO.AddComponent<Animator>();
        
        // Ensure a base generic animator controller exists
        anim.runtimeAnimatorController = GetOrCreateBaseAnimatorController();

        visualsGO.AddComponent<UnitVisualController>();

        // Remove OutlineController que não será mais usado em 3D
        var outline = clone.GetComponent<UnitOutlineController>();
        if (outline != null) DestroyImmediate(outline);

        // Salvar Novo Prefab
        string path = $"{saveFolder}/{clone.name}.prefab";
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        
        PrefabUtility.SaveAsPrefabAsset(clone, path);
        DestroyImmediate(clone);
        
        Debug.Log($"Prefab 2D criado com sucesso em: {path}");
    }

    private AnimatorController GetOrCreateBaseAnimatorController()
    {
        string path = "Assets/Celestial-Cross/Animations/BaseUnitController.controller";
        
        if (!System.IO.Directory.Exists("Assets/Celestial-Cross/Animations"))
            System.IO.Directory.CreateDirectory("Assets/Celestial-Cross/Animations");

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            // Create base states "Idle" and "Combat"
            var rootStateMachine = controller.layers[0].stateMachine;

            var idleState = rootStateMachine.AddState("Idle");
            var combatState = rootStateMachine.AddState("Combat");

            // Add Triggers
            controller.AddParameter("Idle", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Combat", AnimatorControllerParameterType.Trigger);

            // Transitions from Any State
            var transitionToCombat = rootStateMachine.AddAnyStateTransition(combatState);
            transitionToCombat.AddCondition(AnimatorConditionMode.If, 0, "Combat");
            transitionToCombat.duration = 0f;

            var transitionToIdle = rootStateMachine.AddAnyStateTransition(idleState);
            transitionToIdle.AddCondition(AnimatorConditionMode.If, 0, "Idle");
            transitionToIdle.duration = 0f;
            
            // Generate Empty dummy clips to allow overrides
            AnimationClip dummyIdle = new AnimationClip { name = "Base_Idle" };
            AnimationClip dummyCombat = new AnimationClip { name = "Base_Combat" };
            AssetDatabase.CreateAsset(dummyIdle, "Assets/Celestial-Cross/Animations/Base_Idle.anim");
            AssetDatabase.CreateAsset(dummyCombat, "Assets/Celestial-Cross/Animations/Base_Combat.anim");

            idleState.motion = dummyIdle;
            combatState.motion = dummyCombat;

            AssetDatabase.SaveAssets();
        }

        return controller;
    }
}
#endif
