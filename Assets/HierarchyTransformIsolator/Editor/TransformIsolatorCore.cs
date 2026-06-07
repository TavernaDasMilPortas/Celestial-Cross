using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TransformIsolator.Editor
{
    [InitializeOnLoad]
    public static class TransformIsolatorCore
    {
        private const string PrefsKey = "TransformIsolator_IsolatedObjects";
        
        // Use instance IDs to store isolated objects
        private static HashSet<int> isolatedObjects = new HashSet<int>();

        // Cache for transforms
        private static Dictionary<Transform, TransformData> selectedTransforms = new Dictionary<Transform, TransformData>();
        private static Dictionary<Transform, TransformData> childTransforms = new Dictionary<Transform, TransformData>();

        private struct TransformData
        {
            public Vector3 position;
            public Quaternion rotation;

            public TransformData(Transform t)
            {
                position = t.position;
                rotation = t.rotation;
            }
        }

        static TransformIsolatorCore()
        {
            LoadState();
            
            EditorApplication.update += OnUpdate;
            Selection.selectionChanged += OnSelectionChanged;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        public static bool IsIsolated(GameObject go)
        {
            if (go == null) return false;
            return isolatedObjects.Contains(go.GetInstanceID());
        }

        public static void SetIsolated(GameObject go, bool isolated)
        {
            if (go == null) return;
            
            int id = go.GetInstanceID();
            if (isolated)
            {
                if (isolatedObjects.Add(id)) SaveState();
            }
            else
            {
                if (isolatedObjects.Remove(id)) SaveState();
            }
            
            // Refresh cache if selection is affected
            if (Selection.Contains(go))
            {
                CacheTransforms();
            }
        }

        private static void OnSelectionChanged()
        {
            CacheTransforms();
        }

        private static void OnUndoRedo()
        {
            CacheTransforms();
        }

        private static void CacheTransforms()
        {
            selectedTransforms.Clear();
            childTransforms.Clear();

            if (Selection.gameObjects.Length == 0) return;

            foreach (var obj in Selection.gameObjects)
            {
                if (!IsIsolated(obj)) continue;

                Transform t = obj.transform;
                selectedTransforms[t] = new TransformData(t);

                foreach (Transform child in t)
                {
                    if (!Selection.Contains(child.gameObject))
                    {
                        childTransforms[child] = new TransformData(child);
                    }
                }
            }
        }

        private static void OnUpdate()
        {
            if (selectedTransforms.Count == 0) return;

            bool anyChanged = false;

            foreach (var obj in Selection.gameObjects)
            {
                if (!IsIsolated(obj)) continue;

                Transform t = obj.transform;
                if (selectedTransforms.TryGetValue(t, out TransformData cachedParent))
                {
                    if (t.position != cachedParent.position || t.rotation != cachedParent.rotation)
                    {
                        anyChanged = true;

                        // Parent has moved. We need to restore children's global transforms.
                        foreach (Transform child in t)
                        {
                            if (childTransforms.TryGetValue(child, out TransformData cachedChild))
                            {
                                // Only record undo if we are actually changing it, 
                                // but we shouldn't spam Undo records during drag.
                                // Actually, Unity's drag operation groups undos automatically.
                                Undo.RecordObject(child, "Transform Isolator Restore");
                                
                                child.position = cachedChild.position;
                                child.rotation = cachedChild.rotation;
                            }
                        }

                        // Update parent cache
                        selectedTransforms[t] = new TransformData(t);
                    }
                }
            }

            if (anyChanged)
            {
                // After moving children, we need to update their cached positions 
                // in case they were modified by something else? No, they stay the same globally.
            }
        }

        private static void SaveState()
        {
            // Convert to comma separated string
            List<string> ids = new List<string>();
            foreach (int id in isolatedObjects)
            {
                ids.Add(id.ToString());
            }
            EditorPrefs.SetString(PrefsKey + "_" + PlayerSettings.productGUID, string.Join(",", ids));
        }

        private static void LoadState()
        {
            isolatedObjects.Clear();
            string saved = EditorPrefs.GetString(PrefsKey + "_" + PlayerSettings.productGUID, "");
            if (!string.IsNullOrEmpty(saved))
            {
                string[] parts = saved.Split(',');
                foreach (string p in parts)
                {
                    if (int.TryParse(p, out int id))
                    {
                        isolatedObjects.Add(id);
                    }
                }
            }
        }
    }
}
