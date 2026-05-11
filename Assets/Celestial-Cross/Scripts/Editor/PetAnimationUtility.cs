using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;

public class PetAnimationUtility : EditorWindow
{
    private List<Sprite> sprites = new List<Sprite>();
    private string animationName = "NewPetAnimation";
    private float frameRate = 12f;
    private bool loop = true;

    [MenuItem("Celestial Cross/Utilities/Pet Animation Generator")]
    public static void ShowWindow()
    {
        GetWindow<PetAnimationUtility>("Pet Anim Gen");
    }

    private void OnGUI()
    {
        GUILayout.Label("Pet Animation Generator", EditorStyles.boldLabel);
        
        animationName = EditorGUILayout.TextField("Animation Name", animationName);
        frameRate = EditorGUILayout.FloatField("Frame Rate", frameRate);
        loop = EditorGUILayout.Toggle("Loop", loop);

        EditorGUILayout.Space();
        GUILayout.Label("Sprites", EditorStyles.label);

        // Drag and drop area
        Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag & Drop Sprites Here");
        HandleDragAndDrop(dropArea);

        // List display
        for (int i = 0; i < sprites.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            sprites[i] = (Sprite)EditorGUILayout.ObjectField(sprites[i], typeof(Sprite), false);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                sprites.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Clear All"))
        {
            sprites.Clear();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Animation Clip"))
        {
            GenerateClip();
        }
    }

    private void HandleDragAndDrop(Rect dropArea)
    {
        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (!dropArea.Contains(evt.mousePosition)) return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (Object draggedObject in DragAndDrop.objectReferences)
                {
                    if (draggedObject is Sprite sprite)
                    {
                        sprites.Add(sprite);
                    }
                    else if (draggedObject is Texture2D texture)
                    {
                        string path = AssetDatabase.GetAssetPath(texture);
                        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                        foreach (Object asset in assets)
                        {
                            if (asset is Sprite s) sprites.Add(s);
                        }
                    }
                }
            }
            Event.current.Use();
        }
    }

    private void GenerateClip()
    {
        if (sprites.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "Add at least one sprite.", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject("Save Animation Clip", animationName, "anim", "Please enter a file name to save the animation clip to");
        if (string.IsNullOrEmpty(path)) return;

        AnimationClip clip = new AnimationClip();
        clip.frameRate = frameRate;

        if (loop)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        spriteBinding.type = typeof(SpriteRenderer);
        spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count + 1];
        float frameTime = 1f / frameRate;

        for (int i = 0; i < sprites.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * frameTime,
                value = sprites[i]
            };
        }

        // Last keyframe to ensure the last frame is visible
        keyframes[sprites.Count] = new ObjectReferenceKeyframe
        {
            time = sprites.Count * frameTime,
            value = sprites[sprites.Count - 1]
        };

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

        AssetDatabase.CreateAsset(clip, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", "Animation Clip created at: " + path, "OK");
    }
}
