using UnityEngine;

public class CameraSideZone : MonoBehaviour
{
    public enum Side { Left, Right }
    public Side side;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("PLAYER ENTERED " + side);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("PLAYER EXITED " + side);
    }
}
