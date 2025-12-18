using UnityEngine;

public class SideMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 4f;

    // Dieser Wert kommt SPÄTER von Kinect
    // -1 = links | 0 = stehen | 1 = rechts
    [Range(-1f, 1f)]
    public float kinectInputX = 0f;

    void Update()
    {
        Vector3 pos = transform.position;
        pos.x += kinectInputX * speed * Time.deltaTime;
        pos.z = 0f;

        transform.position = pos;
    }
}
