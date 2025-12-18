using UnityEngine;

public class DecisionBox : MonoBehaviour
{
    [SerializeField]
    private bool isLeft;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log(isLeft ? "Entered LEFT box" : "Entered RIGHT box", this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log(isLeft ? "In LEFT box" : "In RIGHT box", this);
    }
}