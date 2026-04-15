#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CelestialCross.Editor
{
    public class TilePrefabPreparer : OdinEditorWindow
    {
        [MenuItem("Celestial Cross/Tile Prefab Preparer")]
        private static void OpenWindow()
        {
            var window = GetWindow<TilePrefabPreparer>("Tile Preparer");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        [Title("Configuração de Tiles")]
        [InfoBox("Arraste os Prefabs do seu projeto abaixo para adicionar os componentes necessários (GridTile + SpriteRenderer de topo).")]
        [AssetSelector(Paths = "Assets/")]
        [ListDrawerSettings(Expanded = true, DraggableItems = true)]
        public List<GameObject> prefabsToPrepare = new List<GameObject>();

        [PropertySpace(SpaceBefore = 15f)]
        [Button(ButtonSizes.Large, Name = "Preparar Prefabs Selecionados"), GUIColor(0.4f, 0.8f, 1f)]
        public void PrepareSelectedPrefabs()
        {
            if (prefabsToPrepare.Count == 0)
            {
                EditorUtility.DisplayDialog("Aviso", "A lista de prefabs está vazia!", "OK");
                return;
            }

            int count = 0;
            foreach (var prefab in prefabsToPrepare)
            {
                if (prefab == null) continue;
                if (PreparePrefab(prefab)) count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Concluído", $"{count} Prefabs foram preparados com sucesso!", "Boa!");
        }

        private bool PreparePrefab(GameObject prefab)
        {
            string path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path)) return false;

            GameObject root = PrefabUtility.LoadPrefabContents(path);

            try
            {
                GridTile tile = root.GetComponent<GridTile>();
                if (tile == null) tile = root.AddComponent<GridTile>();

                Transform visualChild = root.transform.Find("VisualSprite");
                if (visualChild == null)
                {
                    GameObject go = new GameObject("VisualSprite");
                    go.transform.SetParent(root.transform);
                    visualChild = go.transform;
                }

                float heightOffset = 0.505f; 
                var mainRenderer = root.GetComponent<Renderer>();
                if (mainRenderer == null) mainRenderer = root.GetComponentInChildren<Renderer>();
                
                if (mainRenderer != null)
                {
                    heightOffset = (mainRenderer.bounds.size.y / 2f) + 0.005f;
                }

                visualChild.localPosition = new Vector3(0, heightOffset, 0);
                visualChild.localRotation = Quaternion.Euler(90, 0, 0); 
                visualChild.localScale = new Vector3(6.25f, 6.25f, 1f);

                SpriteRenderer sr = visualChild.GetComponent<SpriteRenderer>();
                if (sr == null) sr = visualChild.gameObject.AddComponent<SpriteRenderer>();
                
                sr.sortingOrder = 1; 
                sr.receiveShadows = true;

                // Definindo o tamanho em X e Y para 6.25 (pode ser o size ou a escala dependendo do drawMode)
                visualChild.localScale = new Vector3(6.25f, 6.25f, 1f);
                if (sr.drawMode != SpriteDrawMode.Simple)
                {
                    sr.size = new Vector2(6.25f, 6.25f);
                }

                SerializedObject so = new SerializedObject(tile);
                var spriteProp = so.FindProperty("visualSpriteRenderer");
                var renderProp = so.FindProperty("tileRenderer");

                if (spriteProp != null) spriteProp.objectReferenceValue = sr;
                if (renderProp != null && mainRenderer != null) renderProp.objectReferenceValue = mainRenderer;

                so.ApplyModifiedProperties();

                PrefabUtility.SaveAsPrefabAsset(root, path);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TilePreparer] Erro ao preparar '{prefab.name}': {ex.Message}");
                return false;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
#endif
