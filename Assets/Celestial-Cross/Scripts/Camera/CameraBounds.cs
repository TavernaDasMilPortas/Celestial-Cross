using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    [Header("Map Corners (world space)")]
    public Transform bottomLeft;
    public Transform topRight;

    public Vector3 ClampProjectedPoint(Vector3 point)
    {
        float minX = bottomLeft.position.x;
        float maxX = topRight.position.x;
        float minZ = bottomLeft.position.z;
        float maxZ = topRight.position.z;

        float x = Mathf.Clamp(point.x, minX, maxX);
        float z = Mathf.Clamp(point.z, minZ, maxZ);

        return new Vector3(x, 0f, z);
    }

    void OnDrawGizmos()
    {
        if (bottomLeft == null || topRight == null)
            return;

        Gizmos.color = Color.green;

        Vector3 bl = bottomLeft.position;
        Vector3 tr = topRight.position;
        Vector3 br = new(tr.x, 0, bl.z);
        Vector3 tl = new(bl.x, 0, tr.z);

        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }
}
