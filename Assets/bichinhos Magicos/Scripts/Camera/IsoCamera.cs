using UnityEngine;

public class IsoCamera : MonoBehaviour
{
    void Start()
    {
        transform.rotation = Quaternion.Euler(45f, 45f, 0f);
    }
}