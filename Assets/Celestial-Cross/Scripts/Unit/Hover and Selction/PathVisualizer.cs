#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

public class PathVisualizer : MonoBehaviour
{
    private static PathVisualizer _instance;
    private static bool applicationIsQuitting = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic()
    {
        applicationIsQuitting = false;
        _instance = null;
    }

    public static PathVisualizer Instance
    {
        get
        {
            if (applicationIsQuitting) return null;

            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PathVisualizer>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PathVisualizer_Auto");
                    _instance = go.AddComponent<PathVisualizer>();
                    _instance.InitializeDefaultSprites();
                }
            }
            return _instance;
        }
    }

    private void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }

    [Header("Sprites do Caminho")]
    public Sprite straightSprite;
    public Sprite cornerSprite;
    public Sprite arrowSprite;
    public Sprite startSprite; // Nova imagem de início

    [Header("Configurações Globais (Managers)")]
    public float unitWalkSpeed = 8f;
    public bool cameraFollowsMovement = true;
    [Range(0f, 1f)] public float ghostOpacity = 0.5f; // Controle de opacidade do fantasma
    public Color pathColor = new Color(0.2f, 0.6f, 1f, 0.8f);

    private List<GameObject> activePathSegments = new List<GameObject>();
    private List<GameObject> segmentPool = new List<GameObject>();

    public void InitializeDefaultSprites()
    {
#if UNITY_EDITOR
        if (straightSprite == null)
            straightSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/GridPath/Path_Straight.png");
        if (cornerSprite == null)
            cornerSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/GridPath/Path_Corner.png");
        if (arrowSprite == null)
            arrowSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/GridPath/Path_Arrow.png");
#endif
    }

    public void ClearPath()
    {
        foreach (var seg in activePathSegments)
        {
            seg.SetActive(false);
            segmentPool.Add(seg);
        }
        activePathSegments.Clear();
    }

    private GameObject GetSegment()
    {
        if (segmentPool.Count > 0)
        {
            GameObject seg = segmentPool[segmentPool.Count - 1];
            segmentPool.RemoveAt(segmentPool.Count - 1);
            seg.SetActive(true);
            return seg;
        }

        GameObject newSeg = new GameObject($"PathSegment_{activePathSegments.Count + segmentPool.Count}");
        newSeg.transform.SetParent(transform);
        newSeg.AddComponent<SpriteRenderer>();
        return newSeg;
    }

    public void DrawPath(List<Vector2Int> path, Vector2Int startPos)
    {
        ClearPath();

        if (path == null || path.Count == 0) return;
        if (GridMap.Instance == null) return;

        List<Vector2Int> fullPath = new List<Vector2Int>();
        fullPath.Add(startPos);
        fullPath.AddRange(path);

        for (int i = 0; i < fullPath.Count; i++)
        {
            Vector2Int prev = (i > 0) ? fullPath[i - 1] : fullPath[i];
            Vector2Int curr = fullPath[i];
            Vector2Int next = (i < fullPath.Count - 1) ? fullPath[i + 1] : curr; 

            Vector3 worldPos = GridMap.Instance.GridToWorld(curr) + new Vector3(0, 0.05f, 0); 
            
            GameObject segment = GetSegment();
            segment.transform.position = worldPos;
            segment.transform.rotation = Quaternion.Euler(90, 0, 0); 
            
            SpriteRenderer sr = segment.GetComponent<SpriteRenderer>();
            sr.color = pathColor;
            sr.sortingOrder = 10; 

            if (i == fullPath.Count - 1)
            {
                sr.sprite = arrowSprite;
                SetArrowRotation(segment.transform, prev, curr);
                
                // A imagem do Arrow gerada já termina no centro, não precisamos empurrar.
            }
            else if (i == 0)
            {
                // Ponto inicial: aponta para o próximo tile
                sr.sprite = startSprite != null ? startSprite : straightSprite;
                SetArrowRotation(segment.transform, curr, next); // Usa a mesma lógica de direção da seta
                
                // A imagem do Start gerada já começa no centro, não precisamos empurrar.
            }
            else
            {
                Vector2Int dirIn = curr - prev;
                Vector2Int dirOut = next - curr;

                if (dirIn == dirOut)
                {
                    sr.sprite = straightSprite;
                    SetStraightRotation(segment.transform, dirIn);
                }
                else
                {
                    sr.sprite = cornerSprite;
                    SetCornerRotation(segment.transform, dirIn, dirOut);
                }
            }

            activePathSegments.Add(segment);
        }
    }

    private void SetArrowRotation(Transform t, Vector2Int prev, Vector2Int curr)
    {
        Vector2Int dir = curr - prev;
        if (dir == Vector2Int.right) t.localEulerAngles = new Vector3(90, 0, 0);       // Veio da esquerda
        else if (dir == Vector2Int.left) t.localEulerAngles = new Vector3(90, 180, 0);  // Veio da direita
        else if (dir == Vector2Int.up) t.localEulerAngles = new Vector3(90, -90, 0);    // Veio de baixo
        else if (dir == Vector2Int.down) t.localEulerAngles = new Vector3(90, 90, 0);   // Veio de cima
    }

    private void SetStraightRotation(Transform t, Vector2Int dirIn)
    {
        if (dirIn == Vector2Int.right || dirIn == Vector2Int.left)
            t.localEulerAngles = new Vector3(90, 0, 0);
        else
            t.localEulerAngles = new Vector3(90, 90, 0);
    }

    private void SetCornerRotation(Transform t, Vector2Int dirIn, Vector2Int dirOut)
    {
        // DirIn: de onde a unidade ESTÁ VINDO (curr - prev)
        // DirOut: para onde ela VAI (next - curr)
        
        // A nossa imagem Path_Corner padrão entra pela ESQUERDA e sai por BAIXO
        // dirIn = (1, 0) [direita] -> significa que veio da Esquerda
        // dirOut = (0, -1) [baixo] -> significa que vai para Baixo
        // Rotação Padrão = 0

        // 1. Esquerda & Baixo (Quadrante Inferior-Esquerdo) -> 0 graus
        if ((dirIn == Vector2Int.right && dirOut == Vector2Int.down) || (dirIn == Vector2Int.up && dirOut == Vector2Int.left))
            t.localEulerAngles = new Vector3(90, 0, 0);
            
        // 2. Cima & Esquerda (Quadrante Superior-Esquerdo) -> 90 graus
        else if ((dirIn == Vector2Int.down && dirOut == Vector2Int.left) || (dirIn == Vector2Int.right && dirOut == Vector2Int.up))
            t.localEulerAngles = new Vector3(90, 90, 0);
            
        // 3. Cima & Direita (Quadrante Superior-Direito) -> 180 graus
        else if ((dirIn == Vector2Int.down && dirOut == Vector2Int.right) || (dirIn == Vector2Int.left && dirOut == Vector2Int.up))
            t.localEulerAngles = new Vector3(90, 180, 0);
            
        // 4. Baixo & Direita (Quadrante Inferior-Direito) -> 270 graus
        else if ((dirIn == Vector2Int.left && dirOut == Vector2Int.down) || (dirIn == Vector2Int.up && dirOut == Vector2Int.right))
            t.localEulerAngles = new Vector3(90, 270, 0);
    }
}
