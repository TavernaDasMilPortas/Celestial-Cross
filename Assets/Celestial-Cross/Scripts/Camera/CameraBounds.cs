using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    [Header("Map Corners (world space)")]
    public Transform bottomLeft;
    public Transform topRight;
    
    [Header("Settings")]
    [Tooltip("Adiciona um espaço extra na borda para englobar a extremidade do tile e não o centro do transform")]
    public float tileExtensao = 0.5f;

    public Vector3 ClampProjectedPoint(Vector3 point)
    {
        float minX = bottomLeft.position.x - tileExtensao;
        float maxX = topRight.position.x + tileExtensao;
        float minZ = bottomLeft.position.z - tileExtensao;
        float maxZ = topRight.position.z + tileExtensao;

        float x = Mathf.Clamp(point.x, minX, maxX);
        float z = Mathf.Clamp(point.z, minZ, maxZ);

        return new Vector3(x, 0f, z);
    }

    void OnDrawGizmos()
    {
        if (bottomLeft == null || topRight == null)
            return;

        Gizmos.color = Color.green;

        // Desenha o gizmo já considerando a extensão para ser fácil visualizar as quinas na Unity
        Vector3 bl = new Vector3(bottomLeft.position.x - tileExtensao, 0, bottomLeft.position.z - tileExtensao);
        Vector3 tr = new Vector3(topRight.position.x + tileExtensao, 0, topRight.position.z + tileExtensao);
        Vector3 br = new(tr.x, 0, bl.z);
        Vector3 tl = new(bl.x, 0, tr.z);

        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }
}
