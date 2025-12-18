using UnityEngine;

public class TriggerTestLog : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("TRIGGER HIT by: " + other.name);
    }
}
