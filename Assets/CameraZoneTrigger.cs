using UnityEngine;

public class CameraZoneTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("ENTER " + gameObject.name + " | " + other.name);
    }
}
