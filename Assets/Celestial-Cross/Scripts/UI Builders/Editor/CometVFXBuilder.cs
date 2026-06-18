using UnityEngine;
using UnityEditor;
using CelestialCross.VFX;
using System.IO;

namespace CelestialCross.Editor
{
    public class CometVFXBuilder : EditorWindow
    {
        [MenuItem("Celestial Cross/VFX/Build Comet Death Prefab")]
        public static void BuildCometPrefab()
        {
            string resourcesPath = "Assets/Celestial-Cross/Resources/VFX";
            string prefabPath = resourcesPath + "/CometDeathVFX.prefab";

            if (!Directory.Exists(resourcesPath))
            {
                Directory.CreateDirectory(resourcesPath);
            }

            // Create GameObject
            GameObject cometObj = new GameObject("CometDeathVFX");
            
            // Add CometDeathVFX component
            CometDeathVFX vfxScript = cometObj.AddComponent<CometDeathVFX>();

            // Setup Head Sprite
            GameObject headObj = new GameObject("HeadSprite");
            headObj.transform.SetParent(cometObj.transform);
            headObj.transform.localPosition = Vector3.zero;
            headObj.transform.localScale = Vector3.one * 3f; // Cabeça 3x maior
            SpriteRenderer sr = headObj.AddComponent<SpriteRenderer>();
            
            // Tenta achar o sprite nativo Knob (a bolinha branca padrão da Unity)
            Sprite knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.sprite = knobSprite;
            sr.sortingOrder = 50; // Renderiza por cima dos personagens
            
            vfxScript.headSprite = sr;

            // Setup Trail
            GameObject trailObj = new GameObject("Trail");
            trailObj.transform.SetParent(cometObj.transform);
            trailObj.transform.localPosition = Vector3.zero;
            TrailRenderer trail = trailObj.AddComponent<TrailRenderer>();
            
            // Material padrão de sprite para a luz funcionar sem texturas esticadas
            Material spriteMat = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(spriteMat, resourcesPath + "/CometTrailMaterial.mat");
            trail.material = spriteMat;

            // Configuração da curva do trail
            trail.time = 0.8f; // Cauda dura o dobro do tempo antes de apagar
            trail.startWidth = 1.8f; // Largura 3x maior para acompanhar a cabeça
            trail.endWidth = 0f;
            trail.numCornerVertices = 5;
            trail.numCapVertices = 5;
            trail.sortingOrder = 49;

            vfxScript.trail = trail;

            // Save Prefab
            PrefabUtility.SaveAsPrefabAsset(cometObj, prefabPath);
            DestroyImmediate(cometObj);

            Debug.Log($"[CometVFXBuilder] Prefab construído com sucesso em: {prefabPath}");
            AssetDatabase.Refresh();
        }
    }
}
